using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Emulator;

public class GameBoyCpu : ICpu
{
    private DecodedInstruction _nopInstr;
    private GameBoyMemory _memory;
    private Z80Instruction[] _opcodeTable = new Z80Instruction[256];
    private Z80Instruction[] _cbOpcodeTable = new Z80Instruction[256];

    public bool IsHalted { get; set; } = false;
    public long CyclesThisFrame { get; set; } // Cycles accumulated in the current emulated frame
    public long TotalCycles { get; set; }     // Total cycles since the CPU was reset/booted

    public DecodedInstruction[] Instructions { get; } = new DecodedInstruction[5];

    #region 'Registers'
    public byte A { get; set; }
    public byte F { get; set; }
    public byte B { get; set; }
    public byte C { get; set; }
    public byte D { get; set; }
    public byte E { get; set; }
    public byte H { get; set; }
    public byte L { get; set; }

    public ushort PC { get; set; }
    public ushort SP { get; set; }

    // AF pair (Accumulator and Flags)
    public ushort AF
    {
        get => (ushort)((A << 8) | F); // Combine A (MSB) and F (LSB)
        set
        {
            A = (byte)(value >> 8); // Extract A from MSB
            F = (byte)(value & 0xFF); // Extract F from LSB
            F = (byte)(F & 0xF0);     // IMPORTANT: Game Boy's F register only uses upper 4 bits
                                      // The lower 4 bits are always 0. Mask them off.
        }
    }

    // BC pair
    public ushort BC
    {
        get => (ushort)((B << 8) | C); // Combine B (MSB) and C (LSB)
        set
        {
            B = (byte)(value >> 8);
            C = (byte)(value & 0xFF);
        }
    }

    // DE pair
    public ushort DE
    {
        get => (ushort)((D << 8) | E); // Combine D (MSB) and E (LSB)
        set
        {
            D = (byte)(value >> 8);
            E = (byte)(value & 0xFF);
        }
    }

    // HL pair
    public ushort HL
    {
        get => (ushort)((H << 8) | L); // Combine H (MSB) and L (LSB)
        set
        {
            H = (byte)(value >> 8);
            L = (byte)(value & 0xFF);
        }
    }
    #endregion

    // --- Flag Bit Positions ---
    private const byte FLAG_Z_BIT = 0x80; // Bit 7
    private const byte FLAG_N_BIT = 0x40; // Bit 6
    private const byte FLAG_H_BIT = 0x20; // Bit 5
    private const byte FLAG_C_BIT = 0x10; // Bit 4

    // --- Helper methods for Flags ---
    public bool GetFlagZ() => (F & FLAG_Z_BIT) != 0;
    public void SetFlagZ(bool value) => F = value ? (byte)(F | FLAG_Z_BIT) : (byte)(F & ~FLAG_Z_BIT);
    public bool GetFlagN() => (F & FLAG_N_BIT) != 0;
    public void SetFlagN(bool value) => F = value ? (byte)(F | FLAG_N_BIT) : (byte)(F & ~FLAG_N_BIT);
    public bool GetFlagH() => (F & FLAG_H_BIT) != 0;
    public void SetFlagH(bool value) => F = value ? (byte)(F | FLAG_H_BIT) : (byte)(F & ~FLAG_H_BIT);
    public bool GetFlagC() => (F & FLAG_C_BIT) != 0;
    public void SetFlagC(bool value) => F = value ? (byte)(F | FLAG_C_BIT) : (byte)(F & ~FLAG_C_BIT);


    public GameBoyCpu(GameBoyMemory memory)
    {
        Z80Opcodes.InitialiseOpcodeTable(_opcodeTable);
        _nopInstr = new DecodedInstruction((ushort)0x00, _opcodeTable[0x00], _opcodeTable[0x00].Mnemonic, "00"); // NOP

        Z80Opcodes.InitialiseCBOpcodeTable(_cbOpcodeTable);

        _memory = memory;

        Reset();
    }

    public void ClearInstructions()
    {
        for (int i = 0; i < 5; i++)
        {
            Instructions[i] = _nopInstr;
        }
    }

    public void FetchInstructions()
    {
        var addr = PC;
        for (int i=0; i < 5; i++)
        {
            Instructions[i] = FetchInstruction(addr);
            addr += (ushort)Instructions[i].Instruction.InstructionSize;
        }
    }


    public DecodedInstruction FetchInstruction(ushort Addr)
    {
        // local copy of Addr as we might mess with it
        ushort address = Addr;

        // Fetch Opcode
        byte opcode = _memory.ReadByte(address);
        // Resolve Instruction
        var currentInst = _opcodeTable[opcode];

        if (opcode == 0xCB) // CB-prefixed instruction
        {
            address = (ushort)(address + 1);
            opcode = _memory.ReadByte((ushort)(address) );
            currentInst = _cbOpcodeTable[opcode];
        }


        var decodedInst = currentInst.Mnemonic;
        if (currentInst.InstructionSize > 1)
        {
            // Fetch operand bytes and replace operands in mnemonic
            byte lowByte = _memory.ReadByte((ushort)(address + 1));
            if (currentInst.InstructionSize == 2)
            {
                decodedInst = decodedInst.Replace("n", $"{lowByte:X2}");
            }
            else if (currentInst.InstructionSize == 3)
            {
                byte highByte = _memory.ReadByte((ushort)(address + 2));
                decodedInst = decodedInst.Replace("nn", $"{highByte:X2}{lowByte:X2}");
            }
        }

        return new DecodedInstruction
        (
            Address: Addr,
            Instruction: currentInst,
            Decoded: decodedInst,
            DecodedBytes: string.Join(" ",
                Enumerable.Range(0, currentInst.InstructionSize)
                          .Select(i => _memory.ReadByte((ushort)(Addr + i)).ToString("X2"))
            )
        );
    }

    private void CheckForInterrupts()
    {
        // The CPU should check for interrupts only AFTER an instruction has fully executed.
        // Also, the Game Boy handles an interrupt after 5 M-cycles (20 T-cycles) of delay once a condition is met.
        // This simple check will occur immediately. For super-accurate timing, a pending interrupt flag with a cycle counter might be needed.

        byte ie = _memory.IE; // Get the enabled interrupts
        byte requestedInterrupts = _memory.IF; // Get the currently requested interrupts

        byte pendingAndEnabledInterrupts = (byte)(ie & requestedInterrupts);

        if (pendingAndEnabledInterrupts == 0)
        {
            // No interrupts are both requested AND enabled, so nothing to do.
            return;
        }

        // --- HALT handling (simplified for now) ---
        // If your CPU has a 'Halted' state:
        // if (IsHalted)
        // {
        //     IsHalted = false; // Wake up from HALT
        //     if (!IME) {
        //         // If HALTed with IME disabled, and an interrupt is pending,
        //         // the CPU simply continues after HALT. The interrupt isn't taken
        //         // until IME is re-enabled. There's also a 'HALT bug' which
        //         // involves re-fetching the next instruction. This is an advanced topic.
        //         return;
        //     }
        // }
        // --- End HALT handling ---


        if (!IME)
        {
            // Interrupts are globally disabled (IME is false), so don't take any.
            // The CPU will continue executing regular instructions.
            return;
        }

        // Find the highest priority interrupt that is both pending and enabled (bits 0-4)
        // V-Blank (0x01), LCD STAT (0x02), Timer (0x04), Serial (0x08), Joypad (0x10)
        for (int i = 0; i < 5; i++) // Loop through interrupt bits 0 to 4
        {
            if (((pendingAndEnabledInterrupts >> i) & 1) == 1) // If this specific interrupt bit is set
            {
                ushort interruptVectorAddress = (ushort)(0x0040 + (i * 8));

                // INTERRUPT SEQUENCE:
                // 1. Disable all interrupts (IME = false)
                DisableInterrupts();

                // 2. Push current PC to stack
                PushWordToStack(PC); // This should be a method on your CPU class

                // 3. Jump to the interrupt vector address
                PC = interruptVectorAddress;

                // 4. Clear the corresponding flag in IF (acknowledge the interrupt)
                _memory.IF = (byte)(requestedInterrupts & ~(1 << i)); // Clear only the bit that was just serviced

                // 5. Add T-cycles for interrupt handling. A typical interrupt takes 20 T-cycles (5 M-cycles)
                // Your main execution loop needs to consume these cycles.
                // You might have a method like cpu.AddTCycles(20); or similar.
                // For now, let's just note this timing requirement.

                return; // Only one interrupt is handled at a time (the highest priority)
            }
        }
    }

    public bool IME { get; set; }
    public bool EnableInterruptsPending { get; set; } = false; // Initialize to false
    public void EnableInterrupts()
    {
        IME = true;
    }
    public void DisableInterrupts()
    {
        IME = false;
    }

    // Implementation of PopWordFromStack
    public ushort PopWordFromStack()
    {
        // Read low byte
        byte lowByte = _memory.ReadByte(SP);
        SP++; // Increment SP after reading

        // Read high byte
        byte highByte = _memory.ReadByte(SP);
        SP++; // Increment SP again

        // Combine into a 16-bit word (high byte << 8 | low byte)
        // Game Boy is little-endian for stack pushes/pops
        return (ushort)((highByte << 8) | lowByte);
    }

    // Implementation of PushWordToStack
    public void PushWordToStack(ushort value)
    {
        // Get high byte (upper 8 bits)
        byte highByte = (byte)(value >> 8);
        // Get low byte (lower 8 bits)
        byte lowByte = (byte)(value & 0x00FF);

        // Decrement SP and write high byte
        SP--;
        _memory.WriteByte(SP, highByte);

        // Decrement SP and write low byte
        SP--;
        _memory.WriteByte(SP, lowByte);
    }

    // todo - sort out what to do with 'e' type operands (signed byte)
    /*
     * // Example in your disassembler logic (Conceptual)

    // Assume:
    ushort currentPcAddress = 0x012A; // Address of the JR instruction
    byte opcode = 0x18;
    byte relativeOffsetByte = memory.ReadByte((ushort)(currentPcAddress + 1)); // The e8 byte

    // Convert to signed offset for display
    sbyte signedOffset = (sbyte)relativeOffsetByte;

    // Calculate target address for display (relative to PC *after* instruction)
    // The instruction's length is 2 (opcode + e8).
    // So, the "address after the instruction" is currentPcAddress + 2.
    ushort targetAddress = (ushort)(currentPcAddress + 2 + signedOffset);


    // Now, format the string for display
    string disassemblyLine = $"0x{currentPcAddress:X4}: 0x{opcode:X2} 0x{relativeOffsetByte:X2}  JR {signedOffset:+0;-0} (-> 0x{targetAddress:X4})";

    // Example outputs:
    // If e8 is 0x05 (signed +5):
    // 0x012A: 18 05  JR +5 (-> 0x0131)

    // If e8 is 0xFB (signed -5):
    // 0x012A: 18 FB  JR -5 (-> 0x0127)

        and

           case 2: // Opcode followed by 1 byte (e.g., LD A, n; JR e)
                byte n = memory.ReadByte((ushort)(address + 1));
                operandBytes = $"{n:X2}";
                if (opcode == 0x18) // Special handling for JR e8 to show signed value
                {
                    sbyte e8 = (sbyte)n;
                    mnemonic = $"JR {e8:+0;-0}";
                    ushort targetAddress = (ushort)(address + currentInstruction.Length + e8);
                    mnemonic += $" (-> 0x{targetAddress:X4})";
                }
                else // Regular 8-bit immediate operand
                {
                    mnemonic = mnemonic.Replace("n", $"{n:X2}");
                }
                break;
            case 3: // Opcode followed by 2 bytes (e.g., LD BC, nn; JP nn)
                byte nnL = memory.ReadByte((ushort)(address + 1));
                byte nnH = memory.ReadByte((ushort)(address + 2));
                ushort nn = (ushort)((nnH << 8) | nnL); // Reconstruct 16-bit operand
                operandBytes = $"{nnL:X2} {nnH:X2}";

                // For instructions like LD (nn), A or JP nn, replace 'nn' in mnemonic with the actual address
                mnemonic = mnemonic.Replace("nn", $"0x{nn:X4}");
                break;
        }
    */


public void ExecuteInstruction()
{
        if (EnableInterruptsPending)
        {
            IME = true; // Finally enable interrupts
            EnableInterruptsPending = false; // Clear the flag
        }

        // Increment PC
        PC += (ushort)Instructions[0].Instruction.InstructionSize; // Advance PC first

        // Execute the Instruction
        int cyclesUsed = Instructions[0].Instruction.Execute(this, _memory,
                Instructions[0].Instruction.TCycles); // Execute the logic assigned to the delegate

        // Accumulate T-states
        CyclesThisFrame += cyclesUsed;
        TotalCycles += cyclesUsed;

        CheckForInterrupts();

    // ... cycle tracking, PPU/APU advance (later) ...
}



public void Reset()
{ 
    // Initialize registers to default values if needed
    A = 0x01; // Specific value after boot ROM in real GB
    F = 0xB0; // ZNHC flags (Z=1, N=0, H=1, C=1)
    B = 0x00;
    C = 0x13;
    D = 0x00;
    E = 0xD8;
    H = 0x01;
    L = 0x4D;
    PC = 0x0100; // start of game rom
    SP = 0xFFFE; // Initial stack pointer
    ResetCycles();
    ResetTotalCycles();
    IsHalted = false;

        ClearInstructions();

    FetchInstructions();
}

public void ResetCycles()
{
    CyclesThisFrame = 0;
}

public void ResetTotalCycles()
{
    TotalCycles = 0;
}

public void AddCycles(int cycles)
{
    CyclesThisFrame += cycles;
    TotalCycles += cycles;
}


    // RLC (Rotate Left Circular)
    // C flag set to old bit 7. Z set if result is 0. N, H reset.
    public byte RLC(byte value)
    {
        bool oldBit7 = ((value >> 7) & 1) == 1; // Store original bit 7
        byte result = (byte)((value << 1) | (oldBit7 ? 1 : 0)); // Shift left, bit 7 moves to bit 0

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit7);     // C flag set to original bit 7

        return result;
    }

    // RRC (Rotate Right Circular)
    // C flag set to old bit 0. Z set if result is 0. N, H reset.
    public byte RRC(byte value)
    {
        bool oldBit0 = (value & 1) == 1; // Store original bit 0
        byte result = (byte)((value >> 1) | (oldBit0 ? 0x80 : 0)); // Shift right, bit 0 moves to bit 7

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit0);     // C flag set to original bit 0

        return result;
    }

    // RL (Rotate Left through Carry)
    // Old C flag goes into bit 0. Old bit 7 goes into C flag. Z set if result is 0. N, H reset.
    public byte RL(byte value)
    {
        bool oldCarry = GetFlagC(); // Capture current C flag
        bool oldBit7 = ((value >> 7) & 1) == 1; // Store original bit 7

        // Shift left, bit 0 gets the old C flag
        byte result = (byte)((value << 1) | (oldCarry ? 1 : 0));

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit7);     // C flag set to original bit 7

        return result;
    }

    // RR (Rotate Right through Carry)
    // Old C flag goes into bit 7. Old bit 0 goes into C flag. Z set if result is 0. N, H reset.
    public byte RR(byte value)
    {
        bool oldCarry = GetFlagC(); // Capture current C flag
        bool oldBit0 = (value & 1) == 1; // Store original bit 0

        // Shift right, bit 7 gets the old C flag
        byte result = (byte)((value >> 1) | (oldCarry ? 0x80 : 0));

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit0);     // C flag set to original bit 0

        return result;
    }


    // SLA (Shift Left Arithmetic)
    // Bit 0 is always set to 0. Old bit 7 goes into C flag. Z set if result is 0. N, H reset.
    public byte SLA(byte value)
    {
        bool oldBit7 = ((value >> 7) & 1) == 1; // Store original bit 7 (becomes C flag)
        byte result = (byte)(value << 1); // Shift left, bit 0 becomes 0

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit7);     // C flag set to original bit 7

        return result;
    }

    // SRA (Shift Right Arithmetic)
    // Bit 7 (sign bit) is preserved. Bit 0 goes into C flag. Z set if result is 0. N, H reset.
    public byte SRA(byte value)
    {
        bool oldBit0 = (value & 1) == 1;     // Store original bit 0 (becomes C flag)
        bool oldBit7 = ((value >> 7) & 1) == 1; // Store original bit 7 (sign bit)

        byte result = (byte)(value >> 1); // Shift right
        if (oldBit7)
        {
            result = (byte)(result | 0x80); // If original bit 7 was 1, set it back to 1
        }

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit0);     // C flag set to original bit 0

        return result;
    }

    // SWAP (Swap Nibbles)
    // Swaps the upper and lower 4-bit nibbles. Z set if result is 0. N, H, C reset.
    public byte SWAP(byte value)
    {
        // Get upper nibble (bits 4-7) and shift to lower nibble position.
        byte upperNibble = (byte)((value >> 4) & 0x0F);
        // Get lower nibble (bits 0-3) and shift to upper nibble position.
        byte lowerNibble = (byte)((value & 0x0F) << 4);

        byte result = (byte)(lowerNibble | upperNibble);

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(false);       // C flag always cleared

        return result;
    }

    // SRL (Shift Right Logical)
    // Bit 0 goes into C flag. Bit 7 is always set to 0. Z set if result is 0. N, H reset.
    public byte SRL(byte value)
    {
        bool oldBit0 = (value & 1) == 1; // Store original bit 0 (becomes C flag)

        byte result = (byte)(value >> 1); // Shift right, bit 7 becomes 0

        SetFlagZ(result == 0); // Z flag based on result
        SetFlagN(false);       // N flag always cleared
        SetFlagH(false);       // H flag always cleared
        SetFlagC(oldBit0);     // C flag set to original bit 0

        return result;
    }

    // BIT (Test bit)
    // Tests bit 'bitIndex' in 'value'. Z set if bit is 0. N reset. H set. C unchanged.
    public void BIT(byte bitIndex, byte value)
    {
        // Check if the specified bit is 0
        bool isBitZero = !(((value >> bitIndex) & 1) == 1);

        SetFlagZ(isBitZero); // Z flag based on whether the bit is 0
        SetFlagN(false);     // N flag always cleared
        SetFlagH(true);      // H flag always set
        // C flag is NOT affected
    }

    // RES (Reset bit)
    // Resets (clears to 0) bit 'bitIndex' in 'value'. Returns new value. Flags unaffected.
    public byte RES(byte bitIndex, byte value)
    {
        // Create a mask with a 0 at the bitIndex position and 1s everywhere else
        byte mask = (byte)(~(1 << bitIndex));
        byte result = (byte)(value & mask);
        // Flags are NOT affected by RES operation.
        return result;
    }

    // SET (Set bit)
    // Sets (to 1) bit 'bitIndex' in 'value'. Returns new value. Flags unaffected.
    public byte SET(byte bitIndex, byte value)
    {
        // Create a mask with a 1 at the bitIndex position and 0s everywhere else
        byte mask = (byte)(1 << bitIndex);
        byte result = (byte)(value | mask);
        // Flags are NOT affected by SET operation.
        return result;
    }

}
