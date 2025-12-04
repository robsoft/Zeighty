using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace Zeighty.Emulator;

public static partial class Z80Opcodes
{
    public static void InitialiseOpcodeTable(Z80Instruction[] _opcodeTable)
    {
        OpCodes_00_3F(_opcodeTable);
        OpCodes_40_7F(_opcodeTable);
        OpCodes_80_BF(_opcodeTable);
        OpCodes_C0_FF(_opcodeTable);

        var wait = false;

        // --- Important: Initialize all other 256 entries too! ---
        // For opcodes we haven't implemented yet, use a default
        // instruction that likely throws an exception
        for (int i = 0; i < 256; i++)
        {
            if (_opcodeTable[i] == null)
            {
                Console.WriteLine($"opcode 0x{i:X2} is unimplemented, assigning default handler.");
                wait = true;
                _opcodeTable[i] = new Z80Instruction(
                    Mnemonic: $"UNIMPLEMENTED 0x{i:X2}",
                    Opcode: (byte)i,
                    InstructionSize: 1, // Assume 1 byte for unimplemented for safety
                    TCycles: 4, // Default cycles
                    AffectsFlags: false,
                    Execute: (cpu, memory, cycles) =>
                    {
                        throw new NotSupportedException($"Attempted to execute unimplemented opcode: 0x{i:X2} at PC 0x{cpu.PC - 1:X4}");
                        //return cycles;
                        });
            }
        }

        if (wait) { Console.WriteLine("Basic CB OpCode table has errors ok"); Console.ReadLine(); }
    }

    private static void OpCodes_00_3F(Z80Instruction[] _opcodeTable)
    {
        #region 0x00-0x0F 
        _opcodeTable[0x00] = new Z80Instruction(
            Mnemonic: "NOP",
            Opcode: 0x00,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { 
                /* NOP does nothing */
            return cycles; });

        _opcodeTable[0x01] = new Z80Instruction(
            Mnemonic: "LD BC, nn",
            Opcode: 0x01,
            InstructionSize: 3,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                // Reconstruct the 16-bit value from the Little-Endian bytes.
                ushort value = (ushort)((highByte << 8) | lowByte);
                // Load this 16-bit value into the BC register pair.
                // This will use the 'set' accessor of the cpu.BC property, which in turn updates cpu.B and cpu.C automatically.
                cpu.BC = value;
                return cycles; });

        _opcodeTable[0x02] = new Z80Instruction(
            Mnemonic: "LD (BC), A",
            Opcode: 0x02,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Get the 16-bit target address from the BC register pair, uses the 'get' accessor of the cpu.BC property.
                ushort address = cpu.BC;
                // Get the byte value from the Accumulator (A).
                byte value = cpu.A;
                // Write the value from the A register into memory at the calculated address.
                memory.WriteByte(address, value);
                return cycles; });

        _opcodeTable[0x03] = new Z80Instruction(
            Mnemonic: "INC BC",
            Opcode: 0x03,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Simply increment the 16-bit BC register pair.
                // The 'set' accessor of cpu.BC will handle splitting into B and C.
                cpu.BC = (ushort)(cpu.BC + 1);
            return cycles; });

        _opcodeTable[0x04] = new Z80Instruction(
            Mnemonic: "INC B",
            Opcode: 0x04,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte oldB = cpu.B; // Capture B's value BEFORE increment
                cpu.B = (byte)(cpu.B + 1); // Increment B (C# handles 8-bit overflow naturally)
                // --- FLAG UPDATES for INC B ---
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.B == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // oldB:   xxxx0111
                // oldB+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if oldB's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((oldB & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x05] = new Z80Instruction(
            Mnemonic: "DEC B",
            Opcode: 0x05,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte oldB = cpu.B; // Capture B's value BEFORE decrement
                cpu.B = (byte)(cpu.B - 1); // Decrement B (C# handles 8-bit underflow naturally)
                // --- FLAG UPDATES for DEC B ---
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.B == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // oldB:   xxxx0000
                // oldB-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if oldB's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((oldB & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });
    
        _opcodeTable[0x06] = new Z80Instruction(
            Mnemonic: "LD B, n",
            Opcode: 0x06,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.B = value;
            return cycles; });

        _opcodeTable[0x07] = new Z80Instruction(
            Mnemonic: "RLCA",
            Opcode: 0x07,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A; // Capture A before modification
                // 1. Get the value of bit 7 (the MSB) before rotation. This will be the new Carry flag.
                bool oldBit7 = (oldA & 0x80) != 0;
                // 2. Rotate A left by 1.
                //    (oldA << 1) shifts all bits left, bit 7 goes into bit 8 (overflow).
                //    If oldBit7 was true, set bit 0 of the result to 1.
                cpu.A = (byte)((oldA << 1) | (oldBit7 ? 1 : 0));
                // --- FLAG UPDATES ---
                cpu.SetFlagZ(false);      // GB-specific: Z flag is ALWAYS cleared.
                cpu.SetFlagN(false);      // N flag is ALWAYS cleared.
                cpu.SetFlagH(false);      // H flag is ALWAYS cleared.
                cpu.SetFlagC(oldBit7);    // C flag receives the old bit 7.
            return cycles; });

        _opcodeTable[0x08] = new Z80Instruction(
            Mnemonic: "LD (nn), SP",
            Opcode: 0x08,
            InstructionSize: 3,
            TCycles: 20,
            AffectsFlags: false, // This instruction does NOT affect any flags
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 16-bit immediate address 'nn'.
                //    The opcode is at (PC - 3)
                //    The low byte (nnL) is at (PC - 2)
                //    The high byte (nnH) is at (PC - 1)
                byte lowByte_a16 = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte_a16 = memory.ReadByte((ushort)(cpu.PC - 1));

                // Reconstruct the 16-bit target address (nn) from the Little-Endian bytes.
                ushort targetAddress = (ushort)((highByte_a16 << 8) | lowByte_a16);

                // 2. Get the 16-bit value from the Stack Pointer (SP).
                ushort spValue = cpu.SP;

                // 3. Store the low byte of SP at targetAddress.
                //    Little-Endian: LSB goes to lowest address.
                memory.WriteByte(targetAddress, (byte)(spValue & 0xFF)); // Low byte of SP

                // 4. Store the high byte of SP at targetAddress + 1.
                memory.WriteByte((ushort)(targetAddress + 1), (byte)(spValue >> 8)); // High byte of SP

                // 5. Flags: This instruction does NOT affect any flags.
                //    So, we do nothing here.
            return cycles; });

        _opcodeTable[0x09] = new Z80Instruction(
            Mnemonic: "ADD HL, BC",
            Opcode: 0x09,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // N, H, C are affected. Z is NOT.
            Execute: (cpu, memory, cycles) => {
                ushort oldHL = cpu.HL; // Capture HL's value BEFORE addition
                ushort operandBC = cpu.BC; // Get BC's value
                // Perform addition using 'int' to safely detect 16-bit overflow
                int tempResult = oldHL + operandBC;
                cpu.HL = (ushort)tempResult; // Store the 16-bit result back into HL

                // --- FLAG UPDATES for ADD HL, BC ---
                // Z Flag (Zero): UNCHANGED.
                // cpu.SetFlagZ(cpu.GetFlagZ()); // Or simply omit any change
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 11 to bit 12.
                // Similar to 8-bit H-flag, but for the upper nibble of the low byte.
                // Check if the sum of the lower 12 bits (or bit 11 carry) resulted in overflow.
                // A common way: (oldHL & 0xFFF) + (operandBC & 0xFFF) > 0xFFF
                // Even more precisely: ((oldHL ^ operandBC ^ tempResult) & 0x1000) != 0; (bit 12)
                // Or for simpler logic specific to Z80 16-bit H flag:
                // check if (oldHL & 0x0FFF) + (operandBC & 0x0FFF) resulted in a value > 0x0FFF
                cpu.SetFlagH(((oldHL & 0x0FFF) + (operandBC & 0x0FFF)) > 0x0FFF);
                // C Flag (Carry): Set if the 16-bit sum overflows (i.e., result > 0xFFFF).
                // tempResult (the 'int' sum) already tells us if it went over 65535.
                cpu.SetFlagC(tempResult > 0xFFFF);
            return cycles; });

        _opcodeTable[0x0A] = new Z80Instruction(
            Mnemonic: "LD A, (BC)",
            Opcode: 0x0A,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 16-bit address from the BC register pair.
                //    This uses the 'get' accessor of your cpu.BC property.
                ushort address = cpu.BC;
                // 2. Read the byte from memory at that address.
                byte value = memory.ReadByte(address);
                // 3. Store the read value into the Accumulator (A).
                cpu.A = value;
                // 4. Flags: This instruction does NOT affect any flags.
                //    So, we do nothing here.
            return cycles; });

        _opcodeTable[0x0B] = new Z80Instruction(
            Mnemonic: "DEC BC",
            Opcode: 0x0B,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Simply decrement the 16-bit BC register pair.
                cpu.BC = (ushort)(cpu.BC - 1);
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x0C] = new Z80Instruction(
            Mnemonic: "INC C",
            Opcode: 0x0C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.C; // Capture B's value BEFORE increment
                cpu.C = (byte)(old + 1); // Increment B (C# handles 8-bit overflow naturally)
                                   // --- FLAG UPDATES for INC B ---
                                   // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.C == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // oldB:   xxxx0111
                // oldB+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if oldB's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((old & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                                             // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                                             // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x0D] = new Z80Instruction(
            Mnemonic: "DEC C",
            Opcode: 0x0D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.C; // Capture B's value BEFORE decrement
                cpu.C = (byte)(old - 1); // Decrement B (C# handles 8-bit underflow naturally)
                // --- FLAG UPDATES for DEC B ---
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.C == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // oldB:   xxxx0000
                // oldB-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if oldB's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((old & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x0E] = new Z80Instruction(
            Mnemonic: "LD C, n",
            Opcode: 0x0E,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.C = value;
            return cycles; });

        _opcodeTable[0x0F] = new Z80Instruction(
            Mnemonic: "RRCA",
            Opcode: 0x0F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A; // Capture A before modification
                // 1. Get the value of bit 0 (the LSB) before rotation. This will be the new Carry flag.
                bool oldBit0 = (oldA & 0x01) != 0;
                // 2. Rotate A right by 1.
                //    (oldA >> 1) shifts all bits right, bit 0 goes into bit -1 (overflow).
                //    If oldBit0 was true, set bit 7 of the result to 1.
                cpu.A = (byte)((oldA >> 1) | (oldBit0 ? 0x80 : 0x00));
                // --- FLAG UPDATES ---
                cpu.SetFlagZ(false);      // GB-specific: Z flag is ALWAYS cleared.
                cpu.SetFlagN(false);      // N flag is ALWAYS cleared.
                cpu.SetFlagH(false);      // H flag is ALWAYS cleared.
                cpu.SetFlagC(oldBit0);    // C flag receives the old bit 0.
            return cycles; });

        #endregion

        #region 0x10-0x1F

        _opcodeTable[0x10] = new Z80Instruction(
            Mnemonic: "STOP n",
            Opcode: 0x10,
            InstructionSize: 2, // It consumes 2 bytes, even if the second is ignored
            TCycles: 4,
            AffectsFlags: false, // This instruction does NOT affect any flags
            Execute: (cpu, memory, cycles) => {
                // 1. The n operand is ignored, but we still read it to advance PC correctly.
                //    For `STOP`, the byte at `PC-1` is just junk.
                // byte ignoredOperand = memory.ReadByte((ushort)(cpu.PC - 1)); // You can read it or just ignore.
                // 2. Set the CPU to a stopped state.
                //    This requires a flag in your GameBoyCpu class.
                //TODO cpu.IsStopped = true;
                // 3. For Game Boy Color, this instruction also triggers a CPU speed switch.
                //    This is where your emulator would respond to a read from the KEY1 register (0xFF4D)
                //    being set, then execute STOP to actually perform the speed change.
                //    For DMG emulation, you don't need to worry about speed switching here.
                //    If (cpu.IsGBC && cpu.Key1SpeedSwitchRequested) { cpu.PerformSpeedSwitch(); }
                // 4. Flags: This instruction does NOT affect any flags.
            return cycles; });

        _opcodeTable[0x11] = new Z80Instruction(
            Mnemonic: "LD DE, nn",
            Opcode: 0x11,
            InstructionSize: 3,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                // Reconstruct the 16-bit value from the Little-Endian bytes.
                ushort value = (ushort)((highByte << 8) | lowByte);
                cpu.DE = value;
            return cycles; });

        _opcodeTable[0x12] = new Z80Instruction(
            Mnemonic: "LD (DE), A",
            Opcode: 0x12,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.DE;
                // Get the byte value from the Accumulator (A).
                byte value = cpu.A;
                // Write the value from the A register into memory at the calculated address.
                memory.WriteByte(address, value);
            return cycles; });

        _opcodeTable[0x13] = new Z80Instruction(
            Mnemonic: "INC DE",
            Opcode: 0x13,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.DE = (ushort)(cpu.DE + 1);
            return cycles; });

        _opcodeTable[0x14] = new Z80Instruction(
            Mnemonic: "INC D",
            Opcode: 0x14,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.D; // Capture value BEFORE increment
                cpu.D = (byte)(cpu.D + 1); // Increment (C# handles 8-bit overflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.D == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // old:   xxxx0111
                // old+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if old's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((old & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x15] = new Z80Instruction(
            Mnemonic: "DEC D",
            Opcode: 0x15,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.D; // Capture value BEFORE decrement
                cpu.D = (byte)(old - 1); // Decrement (C# handles 8-bit underflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.D == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // old:   xxxx0000
                // old-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if old's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((old & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x16] = new Z80Instruction(
            Mnemonic: "LD D, n",
            Opcode: 0x16,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.D = value;
            return cycles; });

        _opcodeTable[0x17] = new Z80Instruction(
            Mnemonic: "RLA",
            Opcode: 0x17,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                bool oldCarry = cpu.GetFlagC(); // Get the current Carry flag value

                // 1. Get the value of bit 7 (MSB) before rotation. This will be the new Carry flag.
                bool oldBit7 = (oldA & 0x80) != 0;

                // 2. Rotate A left by 1, with the old Carry flag moving into bit 0.
                cpu.A = (byte)((oldA << 1) | (oldCarry ? 1 : 0));

                // --- FLAG UPDATES ---
                cpu.SetFlagZ(false);      // GB-specific: Z flag is ALWAYS cleared.
                cpu.SetFlagN(false);      // N flag is ALWAYS cleared.
                cpu.SetFlagH(false);      // H flag is ALWAYS cleared.
                cpu.SetFlagC(oldBit7);    // C flag receives the old bit 7.
            return cycles; });


        _opcodeTable[0x18] = new Z80Instruction(
            Mnemonic: "JR e", // Using 'e' for the 8-bit signed offset
            Opcode: 0x18,
            InstructionSize: 2,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 8-bit immediate operand 'e8'.
                //    Since cpu.PC is already advanced to PC_of_next_instruction,
                //    the operand e8 is at (PC - 1).
                byte relativeOffsetByte = memory.ReadByte((ushort)(cpu.PC - 1));
                // 2. Convert the unsigned byte to a signed 8-bit integer (sbyte).
                //    This is crucial for correctly handling negative (backward) jumps.
                sbyte signedOffset = (sbyte)relativeOffsetByte;
                // 3. Calculate the new PC address.
                //    The jump is relative to the *address of the instruction following the JR*.
                //    Since cpu.PC is *already* pointing to the address after the JR,
                //    we simply add the signedOffset to the current PC.
                cpu.PC = (ushort)(cpu.PC + signedOffset);
                // 4. Flags: This instruction does NOT affect any flags.
            return cycles; });



        _opcodeTable[0x19] = new Z80Instruction(
            Mnemonic: "ADD HL, DE",
            Opcode: 0x19,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // N, H, C are affected. Z is NOT.
            Execute: (cpu, memory, cycles) => {
                ushort old = cpu.HL; // Capture HL's value BEFORE addition
                ushort operand = cpu.DE;
                // Perform addition using 'int' to safely detect 16-bit overflow
                int tempResult = old + operand;
                cpu.HL = (ushort)tempResult; // Store the 16-bit result back into HL

                // Z Flag (Zero): UNCHANGED.
                // cpu.SetFlagZ(cpu.GetFlagZ()); // Or simply omit any change
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 11 to bit 12.
                // Similar to 8-bit H-flag, but for the upper nibble of the low byte.
                // Check if the sum of the lower 12 bits (or bit 11 carry) resulted in overflow.
                // A common way: (old & 0xFFF) + (operand & 0xFFF) > 0xFFF
                // Even more precisely: ((old ^ operand ^ tempResult) & 0x1000) != 0; (bit 12)
                // Or for simpler logic specific to Z80 16-bit H flag:
                // check if (old & 0x0FFF) + (operand & 0x0FFF) resulted in a value > 0x0FFF
                cpu.SetFlagH(((old & 0x0FFF) + (operand & 0x0FFF)) > 0x0FFF);
                // C Flag (Carry): Set if the 16-bit sum overflows (i.e., result > 0xFFFF).
                // tempResult (the 'int' sum) already tells us if it went over 65535.
                cpu.SetFlagC(tempResult > 0xFFFF);
            return cycles; });

        _opcodeTable[0x1A] = new Z80Instruction(
            Mnemonic: "LD A, (DE)",
            Opcode: 0x1A,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.DE;
                byte value = memory.ReadByte(address);
                cpu.A = value;
                // Flags: This instruction does NOT affect any flags.
                //    So, we do nothing here.
            return cycles; });

        _opcodeTable[0x1B] = new Z80Instruction(
            Mnemonic: "DEC DE",
            Opcode: 0x1B,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Simply decrement the 16-bit register pair.
                cpu.DE = (ushort)(cpu.DE - 1);
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x1C] = new Z80Instruction(
            Mnemonic: "INC E",
            Opcode: 0x1C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.E; // Capture B's value BEFORE increment
                cpu.E = (byte)(old + 1); // Increment B (C# handles 8-bit overflow naturally)
                                         // --- FLAG UPDATES for INC B ---
                                         // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.E == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // oldB:   xxxx0111
                // oldB+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if oldB's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((old & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                                                     // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                                                     // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x1D] = new Z80Instruction(
            Mnemonic: "DEC E",
            Opcode: 0x1D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.E;
                cpu.E = (byte)(old - 1);
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.E == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // oldB:   xxxx0000
                // oldB-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if oldB's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((old & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x1E] = new Z80Instruction(
            Mnemonic: "LD E, n",
            Opcode: 0x1E,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.E = value;
            return cycles; });

        _opcodeTable[0x1F] = new Z80Instruction(
            Mnemonic: "RRA",
            Opcode: 0x1F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                bool oldCarry = cpu.GetFlagC(); // Get the current Carry flag value
                // 1. Get the value of bit 0 (LSB) before rotation. This will be the new Carry flag.
                bool oldBit0 = (oldA & 0x01) != 0;
                // 2. Rotate A right by 1, with the old Carry flag moving into bit 7.
                cpu.A = (byte)((oldA >> 1) | (oldCarry ? 0x80 : 0x00));
                // --- FLAG UPDATES ---
                cpu.SetFlagZ(false);      // GB-specific: Z flag is ALWAYS cleared.
                cpu.SetFlagN(false);      // N flag is ALWAYS cleared.
                cpu.SetFlagH(false);      // H flag is ALWAYS cleared.
                cpu.SetFlagC(oldBit0);    // C flag receives the old bit 0.
            return cycles; });
        #endregion

        #region 0x20-0x2F
        _opcodeTable[0x20] = new Z80Instruction(
            Mnemonic: "JR NZ, e",
            Opcode: 0x20,
            InstructionSize: 2,
            TCycles: 12, // Default to longer T-cycles if condition depends on it, will adjust
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte relativeOffsetByte = memory.ReadByte((ushort)(cpu.PC - 1));
                sbyte signedOffset = (sbyte)relativeOffsetByte;
                // Condition: Jump if Z flag is CLEAR (NZ = Not Zero)
                if (!cpu.GetFlagZ()) // If Z flag is 0
                {
                    cpu.PC = (ushort)(cpu.PC + signedOffset); // Jump is taken
                    // T-cycles are already set to 12. No change needed here.
                }
                else
                {
                    // Jump not taken. PC already advanced by InstructionSize (2 bytes).
                    // But T-cycles need to be adjusted down.
                    return 8;
                }
                // Flags: None affected.
            return cycles; });


        _opcodeTable[0x21] = new Z80Instruction(
            Mnemonic: "LD HL, nn",
            Opcode: 0x21,
            InstructionSize: 3,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                // Reconstruct the 16-bit value from the Little-Endian bytes.
                ushort value = (ushort)((highByte << 8) | lowByte);
                cpu.HL = value;
            return cycles; });

        //TODO check this
        _opcodeTable[0x22] = new Z80Instruction(
            Mnemonic: "LDI/LD(HL+), A",
            Opcode: 0x22,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                // Get the byte value from the Accumulator (A).
                byte value = cpu.A;
                // Write the value from the A register into memory at the calculated address.
                memory.WriteByte(address, value);
                cpu.HL = (ushort)(cpu.HL + 1);
            return cycles; });


        _opcodeTable[0x23] = new Z80Instruction(
            Mnemonic: "INC HL",
            Opcode: 0x23,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.HL = (ushort)(cpu.HL + 1);
            return cycles; });

        _opcodeTable[0x24] = new Z80Instruction(
            Mnemonic: "INC H",
            Opcode: 0x24,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.H; // Capture value BEFORE increment
                cpu.H = (byte)(cpu.H + 1); // Increment (C# handles 8-bit overflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.H == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // old:   xxxx0111
                // old+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if old's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((old & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x25] = new Z80Instruction(
            Mnemonic: "DEC H",
            Opcode: 0x25,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.H; // Capture value BEFORE decrement
                cpu.H = (byte)(old - 1); // Decrement (C# handles 8-bit underflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.H == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // old:   xxxx0000
                // old-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if old's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((old & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x26] = new Z80Instruction(
            Mnemonic: "LD H, n",
            Opcode: 0x26,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.H = value;
            return cycles; });

        _opcodeTable[0x27] = new Z80Instruction(
            Mnemonic: "DAA",
            Opcode: 0x27,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte a = cpu.A;
                bool nFlag = cpu.GetFlagN();
                bool hFlag = cpu.GetFlagH();
                bool cFlag = cpu.GetFlagC();
                byte correction = 0x00;
                if (hFlag || (!nFlag && (a & 0x0F) > 0x09))
                {
                    correction = (byte)(correction | 0x06);
                }
                if (cFlag || (!nFlag && a > 0x99))
                {
                    correction = (byte)(correction | 0x60);
                    cpu.SetFlagC(true); // If upper nibble adjustment, Carry is always set or propagated
                }
                else
                {
                    // If no correction to upper nibble due to C flag or A>0x99, C flag is NOT set.
                    cpu.SetFlagC(false); // Make sure C is cleared if not explicitly set
                }
                if (nFlag) // Subtraction
                {
                    cpu.A = (byte)(a - correction);
                }
                else // Addition
                {
                    cpu.A = (byte)(a + correction);
                }
        
                cpu.SetFlagZ(cpu.A == 0); // Z flag based on final A
                // N flag is UNCHANGED
                cpu.SetFlagH(false);      // H flag is ALWAYS cleared

                // The DAA instruction specifically has this rule for C flag:
                // IF old C was set OR (no N flag and old A > 0x99 (before first adjustment)) THEN C is set
                // ELSE C is cleared
                // The `correction` logic above often correctly manages this if written carefully.
                // A common way to implement C is to hold onto the old C state and set it if `(oldC || a_was_over_99_and_no_subtract)`.
                // Let's refine the C flag logic:
                if (cFlag || (!nFlag && a > 0x99)) // Check if original C was set OR (was add and A needed 0x60 adjustment)
                {
                    cpu.SetFlagC(true);
                }
                else
                {
                    cpu.SetFlagC(false);
                }
            return cycles; });

        _opcodeTable[0x28] = new Z80Instruction(
            Mnemonic: "JR Z, e",
            Opcode: 0x28,
            InstructionSize: 2,
            TCycles: 12, // Default to longer T-cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte relativeOffsetByte = memory.ReadByte((ushort)(cpu.PC - 1));
                sbyte signedOffset = (sbyte)relativeOffsetByte;
                // Condition: Jump if Z flag is SET (Z = Zero)
                if (cpu.GetFlagZ()) // If Z flag is 1
                {
                    cpu.PC = (ushort)(cpu.PC + signedOffset); // Jump is taken
                }
                // else: Jump not taken. PC already advanced.
                // Flags: None affected.
            return cycles; });



        _opcodeTable[0x29] = new Z80Instruction(
            Mnemonic: "ADD HL, HL",
            Opcode: 0x19,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // N, H, C are affected. Z is NOT.
            Execute: (cpu, memory, cycles) => {
                ushort old = cpu.HL; // Capture HL's value BEFORE addition
                ushort operand = cpu.HL;
                // Perform addition using 'int' to safely detect 16-bit overflow
                int tempResult = old + operand;
                cpu.HL = (ushort)tempResult; // Store the 16-bit result back into HL
                // Z Flag (Zero): UNCHANGED.
                // cpu.SetFlagZ(cpu.GetFlagZ()); // Or simply omit any change
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 11 to bit 12.
                // Similar to 8-bit H-flag, but for the upper nibble of the low byte.
                // Check if the sum of the lower 12 bits (or bit 11 carry) resulted in overflow.
                // A common way: (old & 0xFFF) + (operand & 0xFFF) > 0xFFF
                // Even more precisely: ((old ^ operand ^ tempResult) & 0x1000) != 0; (bit 12)
                // Or for simpler logic specific to Z80 16-bit H flag:
                // check if (old & 0x0FFF) + (operand & 0x0FFF) resulted in a value > 0x0FFF
                cpu.SetFlagH(((old & 0x0FFF) + (operand & 0x0FFF)) > 0x0FFF);
                // C Flag (Carry): Set if the 16-bit sum overflows (i.e., result > 0xFFFF).
                // tempResult (the 'int' sum) already tells us if it went over 65535.
                cpu.SetFlagC(tempResult > 0xFFFF);
            return cycles; });

        _opcodeTable[0x2A] = new Z80Instruction(
            Mnemonic: "LD A, (HL+)",
            Opcode: 0x2A,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.DE;
                byte value = memory.ReadByte(address);
                cpu.A = value;
                cpu.HL = (ushort)(cpu.HL + 1);
                // Flags: This instruction does NOT affect any flags.
                //    So, we do nothing here.
            return cycles; });

        _opcodeTable[0x2B] = new Z80Instruction(
            Mnemonic: "DEC HL",
            Opcode: 0x2B,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Simply decrement the 16-bit register pair.
                cpu.HL = (ushort)(cpu.HL - 1);
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x2C] = new Z80Instruction(
            Mnemonic: "INC L",
            Opcode: 0x2C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.L; // Capture B's value BEFORE increment
                cpu.L = (byte)(old + 1); // Increment B (C# handles 8-bit overflow naturally)
                                         // --- FLAG UPDATES for INC B ---
                                         // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.L == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This means the lower nibble (0-F) overflowed.
                // oldB:   xxxx0111
                // oldB+1: xxxx1000  (carry from bit 3 to 4 means H=1)
                // Check if oldB's bit 3 was 1, and the result's bit 3 is 0, implying a carry.
                cpu.SetFlagH((old & 0x0F) == 0x0F); // Simpler check: if low nibble was 0xF, then it half-carried
                                                    // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                                                    // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x2D] = new Z80Instruction(
            Mnemonic: "DEC L",
            Opcode: 0x2D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Flags Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte old = cpu.L;
                cpu.L = (byte)(old - 1);
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.L == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble (0-F) underflowed from 0x00 to 0x0F.
                // oldB:   xxxx0000
                // oldB-1: xxxx1111  (borrow from bit 4 to 3 means H=1)
                // Check if oldB's bit 3 was 0, and the result's bit 3 is 1, implying a borrow.
                cpu.SetFlagH((old & 0x0F) == 0x00); // Simpler check: if low nibble was 0x0, then it half-borrowed
                // C Flag (Carry): NOT affected by 8-bit INC/DEC.
                // It retains its previous value. So, explicitly do nothing here.
            return cycles; });

        _opcodeTable[0x2E] = new Z80Instruction(
            Mnemonic: "LD L, n",
            Opcode: 0x2E,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1));
                cpu.L = value;
            return cycles; });

        _opcodeTable[0x2F] = new Z80Instruction(
            Mnemonic: "CPL",
            Opcode: 0x2F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // N and H flags are affected
            Execute: (cpu, memory, cycles) => {
                cpu.A = (byte)(~cpu.A); // Perform the bitwise NOT operation
                // N Flag (Subtract): ALWAYS SET.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): ALWAYS SET.
                cpu.SetFlagH(true);
            return cycles; });
        #endregion

        #region 0x30-0x3F
        _opcodeTable[0x30] = new Z80Instruction(
            Mnemonic: "JR NC, e",
            Opcode: 0x30,
            InstructionSize: 2,
            TCycles: 12, // Max T-cycles for now, pending conditional T-cycle handling
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte relativeOffsetByte = memory.ReadByte((ushort)(cpu.PC - 1));
                sbyte signedOffset = (sbyte)relativeOffsetByte;
                // Condition: Jump if C flag is CLEAR (NC = No Carry)
                if (!cpu.GetFlagC())
                {
                    cpu.PC = (ushort)(cpu.PC + signedOffset); // Jump is taken
                }
                // else: Jump not taken. PC already advanced.
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x31] = new Z80Instruction(
            Mnemonic: "LD SP, nn", // Using nn for 16-bit immediate for consistency
            Opcode: 0x31,
            InstructionSize: 3,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 16-bit immediate operand 'n16' (nn).
                //    The opcode is at (PC - 3)
                //    The low byte is at (PC - 2)
                //    The high byte is at (PC - 1)
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                // Reconstruct the 16-bit value from the Little-Endian bytes.
                ushort value = (ushort)((highByte << 8) | lowByte);
               // 2. Load this 16-bit value into the Stack Pointer (SP).
                cpu.SP = value;
                // 3. Flags: None affected.
            return cycles; });

        _opcodeTable[0x32] = new Z80Instruction(
            Mnemonic: "LD (HL-), A",
            Opcode: 0x32,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 16-bit target address from the HL register pair.
                ushort address = cpu.HL;
                // 2. Get the byte value from the Accumulator (A).
                byte value = cpu.A;
                // 3. Write the value from A into memory at the calculated address.
                memory.WriteByte(address, value);
                // 4. --- CRUCIAL STEP --- Decrement HL by 1 AFTER the write.
                cpu.HL = (ushort)(cpu.HL - 1);
                // 5. Flags: This instruction does NOT affect any flags.
            return cycles; });

        _opcodeTable[0x33] = new Z80Instruction(
            Mnemonic: "INC SP",
            Opcode: 0x33,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false, // IMPORTANT: No flags affected by 16-bit INC/DEC
            Execute: (cpu, memory, cycles) => {
                // Simply increment the 16-bit SP register.
                cpu.SP = (ushort)(cpu.SP + 1);
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x34] = new Z80Instruction(
            Mnemonic: "INC (HL)",
            Opcode: 0x34,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: true, // Z, N, H flags are affected
            Execute: (cpu, memory, cycles) =>
            {
                ushort address = cpu.HL;
                byte oldValue = memory.ReadByte(address); // Read the current value from memory
                byte newValue = (byte)(oldValue + 1); // Increment (C# handles 8-bit overflow naturally)
                memory.WriteByte(address, newValue); // Write the new value back to memory
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(newValue == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This happens if the lower nibble was 0x0F before incrementing.
                cpu.SetFlagH((oldValue & 0x0F) == 0x0F);
            return cycles; });

        _opcodeTable[0x35] = new Z80Instruction(
            Mnemonic: "DEC (HL)",
            Opcode: 0x35,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: true, // Z, N, H flags are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte oldValue = memory.ReadByte(address); // Read the current value from memory
                byte newValue = (byte)(oldValue - 1); // Decrement (C# handles 8-bit underflow naturally)
                memory.WriteByte(address, newValue); // Write the new value back to memory
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(newValue == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This happens if the lower nibble was 0x00 before decrementing.
                cpu.SetFlagH((oldValue & 0x0F) == 0x00);
            return cycles; });

        _opcodeTable[0x36] = new Z80Instruction(
            Mnemonic: "LD (HL), n",
            Opcode: 0x36,
            InstructionSize: 2,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 8-bit immediate operand 'n'.
                //    Since cpu.PC is already advanced to PC_of_next_instruction,
                //    the operand 'n' is at (PC - 1).
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                // 2. Get the 16-bit target address from the HL register pair.
                ushort address = cpu.HL;
                // 3. Write the operand 'n' into memory at the calculated address.
                memory.WriteByte(address, operand);
                // 4. Flags: None affected.
            return cycles; });

        // 0x37: SCF
        _opcodeTable[0x37] = new Z80Instruction(
            Mnemonic: "SCF",
            Opcode: 0x37,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) =>
            {
                cpu.SetFlagZ(cpu.GetFlagZ()); // Z flag: UNCHANGED
                cpu.SetFlagN(false);          // N flag: ALWAYS cleared.
                cpu.SetFlagH(false);          // H flag: ALWAYS cleared.
                cpu.SetFlagC(true);           // C flag: ALWAYS set.
                return cycles;
            }
        );

        _opcodeTable[0x38] = new Z80Instruction(
            Mnemonic: "JR C, e",
            Opcode: 0x38,
            InstructionSize: 2,
            TCycles: 12, // Max T-cycles for now, pending conditional T-cycle handling
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte relativeOffsetByte = memory.ReadByte((ushort)(cpu.PC - 1));
                sbyte signedOffset = (sbyte)relativeOffsetByte;
                // Condition: Jump if C flag is SET (C = Carry)
                if (cpu.GetFlagC())
                {
                    cpu.PC = (ushort)(cpu.PC + signedOffset); // Jump is taken
                }
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x39] = new Z80Instruction(
            Mnemonic: "ADD HL, SP",
            Opcode: 0x39,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // N, H, C are affected. Z is NOT.
            Execute: (cpu, memory, cycles) => {
                ushort oldHL = cpu.HL; // Capture HL's value BEFORE addition
                ushort operandSP = cpu.SP; // Get SP's value
                // Perform addition using 'int' to safely detect 16-bit overflow
                int tempResult = oldHL + operandSP;
                cpu.HL = (ushort)tempResult; // Store the 16-bit result back into HL
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 11 to bit 12.
                cpu.SetFlagH(((oldHL & 0x0FFF) + (operandSP & 0x0FFF)) > 0x0FFF);
                // C Flag (Carry): Set if the 16-bit sum overflows (i.e., result > 0xFFFF).
                cpu.SetFlagC(tempResult > 0xFFFF);
            return cycles; });

        _opcodeTable[0x3A] = new Z80Instruction(
            Mnemonic: "LD A, (HL-)",
            Opcode: 0x3A,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // 1. Get the 16-bit source address from the HL register pair.
                ushort address = cpu.HL;
                // 2. Read the byte from memory at that address.
                byte value = memory.ReadByte(address);
                // 3. Store the read value into the Accumulator (A).
                cpu.A = value;
                // 4. --- CRUCIAL STEP --- Decrement HL by 1 AFTER the read.
                cpu.HL = (ushort)(cpu.HL - 1);
                // 5. Flags: This instruction does NOT affect any flags.
            return cycles; });

        _opcodeTable[0x3B] = new Z80Instruction(
            Mnemonic: "DEC SP",
            Opcode: 0x3B,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Simply decrement the 16-bit SP register.
                cpu.SP = (ushort)(cpu.SP - 1);
                // Flags: None affected.
            return cycles; });

        _opcodeTable[0x3C] = new Z80Instruction(
            Mnemonic: "INC A",
            Opcode: 0x3C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A; // Capture A's value BEFORE increment
                cpu.A = (byte)(cpu.A + 1); // Increment A (C# handles 8-bit overflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for INC operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This happens if the lower nibble was 0x0F before incrementing.
                cpu.SetFlagH((oldA & 0x0F) == 0x0F);
            return cycles; });

        _opcodeTable[0x3D] = new Z80Instruction(
            Mnemonic: "DEC A",
            Opcode: 0x3D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A; // Capture A's value BEFORE decrement
                cpu.A = (byte)(cpu.A - 1); // Decrement A (C# handles 8-bit underflow naturally)
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always SET for DEC operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This happens if the lower nibble was 0x00 before decrementing.
                cpu.SetFlagH((oldA & 0x0F) == 0x00);
            return cycles; });

        _opcodeTable[0x3E] = new Z80Instruction(
            Mnemonic: "LD A, n",
            Opcode: 0x3E,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte((ushort)(cpu.PC - 1)); // PC was incremented after fetch
                cpu.A = value;
                return cycles; });

        _opcodeTable[0x3F] = new Z80Instruction(
            Mnemonic: "CCF",
            Opcode: 0x3F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // N, H, C flags are affected
            Execute: (cpu, memory, cycles) => {
                // 1. Get the current state of the Carry flag.
                bool oldCarry = cpu.GetFlagC();
                // N Flag (Subtract): ALWAYS CLEARED.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): ALWAYS CLEARED.
                cpu.SetFlagH(false);
                // C Flag (Carry): FLIPPED.
                cpu.SetFlagC(!oldCarry); // Set C to the opposite of its previous state
                return cycles; });
        #endregion
    }


    private static void OpCodes_40_7F(Z80Instruction[] _opcodeTable)
    {

        #region 0x40-0x4F
        _opcodeTable[0x40] = new Z80Instruction(
            Mnemonic: "LD B, B",
            Opcode: 0x40,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.B; return cycles; });

        _opcodeTable[0x41] = new Z80Instruction(
            Mnemonic: "LD B, C",
            Opcode: 0x41,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.C; return cycles; });

        _opcodeTable[0x42] = new Z80Instruction(
            Mnemonic: "LD B, D",
            Opcode: 0x42,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.D; return cycles; });

        _opcodeTable[0x43] = new Z80Instruction(
            Mnemonic: "LD B, E",
            Opcode: 0x43,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.E; return cycles; });

        _opcodeTable[0x44] = new Z80Instruction(
            Mnemonic: "LD B, H",
            Opcode: 0x44,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.H; return cycles; });

        _opcodeTable[0x45] = new Z80Instruction(
            Mnemonic: "LD B, L",
            Opcode: 0x45,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.L; return cycles; });

        _opcodeTable[0x46] = new Z80Instruction(
            Mnemonic: "LD B, (HL)",
            Opcode: 0x46,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.B = value; return cycles; });

        _opcodeTable[0x47] = new Z80Instruction(
            Mnemonic: "LD B, A",
            Opcode: 0x47,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.A; return cycles; });

        _opcodeTable[0x48] = new Z80Instruction(
            Mnemonic: "LD C, B",
            Opcode: 0x48,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.B; return cycles; });

        _opcodeTable[0x49] = new Z80Instruction(
            Mnemonic: "LD C, C",
            Opcode: 0x49,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.C; return cycles; });

        _opcodeTable[0x4A] = new Z80Instruction(
            Mnemonic: "LD C, D",
            Opcode: 0x4A,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.D; return cycles; });

        _opcodeTable[0x4B] = new Z80Instruction(
            Mnemonic: "LD C, E",
            Opcode: 0x4B,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.E; return cycles; });

        _opcodeTable[0x4C] = new Z80Instruction(
            Mnemonic: "LD C, H",
            Opcode: 0x4C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.H; return cycles; });

        _opcodeTable[0x4D] = new Z80Instruction(
            Mnemonic: "LD C, L",
            Opcode: 0x4D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.L; return cycles; });

        _opcodeTable[0x4E] = new Z80Instruction(
            Mnemonic: "LD C, (HL)",
            Opcode: 0x4E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.C = value;
            return cycles; });

        _opcodeTable[0x4F] = new Z80Instruction(
            Mnemonic: "LD C, A",
            Opcode: 0x4F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.A; return cycles; });
        #endregion

        #region 0x50-0x5F
        _opcodeTable[0x50] = new Z80Instruction(
            Mnemonic: "LD D, B",
            Opcode: 0x50,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.B; return cycles; });

        _opcodeTable[0x51] = new Z80Instruction(
            Mnemonic: "LD D, C",
            Opcode: 0x51,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.C; return cycles; });

        _opcodeTable[0x52] = new Z80Instruction(
            Mnemonic: "LD D, D",
            Opcode: 0x52,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.D; return cycles; });

        _opcodeTable[0x53] = new Z80Instruction(
            Mnemonic: "LD D, E",
            Opcode: 0x53,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.E; return cycles; });

        _opcodeTable[0x54] = new Z80Instruction(
            Mnemonic: "LD D, H",
            Opcode: 0x54,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.H; return cycles; });

        _opcodeTable[0x55] = new Z80Instruction(
            Mnemonic: "LD D, L",
            Opcode: 0x55,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.L; return cycles; });

        _opcodeTable[0x56] = new Z80Instruction(
            Mnemonic: "LD D, (HL)",
            Opcode: 0x56,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.D = value;
            return cycles; });

        _opcodeTable[0x57] = new Z80Instruction(
            Mnemonic: "LD D, A",
            Opcode: 0x57,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.A; return cycles; });

        _opcodeTable[0x58] = new Z80Instruction(
            Mnemonic: "LD E, B",
            Opcode: 0x58,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.B; return cycles; });

        _opcodeTable[0x59] = new Z80Instruction(
            Mnemonic: "LD E, C",
            Opcode: 0x59,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.C; return cycles; });

        _opcodeTable[0x5A] = new Z80Instruction(
            Mnemonic: "LD E, D",
            Opcode: 0x5A,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.D; return cycles; });

        _opcodeTable[0x5B] = new Z80Instruction(
            Mnemonic: "LD E, E",
            Opcode: 0x5B,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.E; return cycles; });

        _opcodeTable[0x5C] = new Z80Instruction(
            Mnemonic: "LD E, H",
            Opcode: 0x5C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.H; return cycles; });

        _opcodeTable[0x5D] = new Z80Instruction(
            Mnemonic: "LD E, L",
            Opcode: 0x5D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.L; return cycles; });

        _opcodeTable[0x5E] = new Z80Instruction(
            Mnemonic: "LD E, (HL)",
            Opcode: 0x5E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.E = value;
            return cycles; });

        _opcodeTable[0x5F] = new Z80Instruction(
            Mnemonic: "LD E, A",
            Opcode: 0x5F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.A; return cycles; });
        #endregion

        #region 0x60-0x6F
        _opcodeTable[0x60] = new Z80Instruction(
            Mnemonic: "LD H, B",
            Opcode: 0x60,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.B; return cycles; });

        _opcodeTable[0x61] = new Z80Instruction(
            Mnemonic: "LD H, C",
            Opcode: 0x61,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.C; return cycles; });

        _opcodeTable[0x62] = new Z80Instruction(
            Mnemonic: "LD H, D",
            Opcode: 0x62,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.D; return cycles; });

        _opcodeTable[0x63] = new Z80Instruction(
            Mnemonic: "LD H, E",
            Opcode: 0x63,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.E; return cycles; });

        _opcodeTable[0x64] = new Z80Instruction(
            Mnemonic: "LD H, H",
            Opcode: 0x64,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.H; return cycles; });

        _opcodeTable[0x65] = new Z80Instruction(
            Mnemonic: "LD H, L",
            Opcode: 0x65,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.L; return cycles; });

        _opcodeTable[0x66] = new Z80Instruction(
            Mnemonic: "LD H, (HL)",
            Opcode: 0x66,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.H = value;
            return cycles; });

        _opcodeTable[0x67] = new Z80Instruction(
            Mnemonic: "LD H, A",
            Opcode: 0x67,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.A; return cycles; });

        _opcodeTable[0x68] = new Z80Instruction(
            Mnemonic: "LD L, B",
            Opcode: 0x68,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.B; return cycles; });

        _opcodeTable[0x69] = new Z80Instruction(
            Mnemonic: "LD L, C",
            Opcode: 0x69,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.C; return cycles; });

        _opcodeTable[0x6A] = new Z80Instruction(
            Mnemonic: "LD L, D",
            Opcode: 0x6A,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.D; return cycles; });

        _opcodeTable[0x6B] = new Z80Instruction(
            Mnemonic: "LD L, E",
            Opcode: 0x6B,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.E; return cycles; });

        _opcodeTable[0x6C] = new Z80Instruction(
            Mnemonic: "LD L, H",
            Opcode: 0x6C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.H; return cycles; });

        _opcodeTable[0x6D] = new Z80Instruction(
            Mnemonic: "LD L, L",
            Opcode: 0x6D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.L; return cycles; });

        _opcodeTable[0x6E] = new Z80Instruction(
            Mnemonic: "LD L, (HL)",
            Opcode: 0x6E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.L = value;
            return cycles; });

        _opcodeTable[0x6F] = new Z80Instruction(
            Mnemonic: "LD L, A",
            Opcode: 0x6F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.A; return cycles; });
        #endregion

        #region 0x70-0x7F
        _opcodeTable[0x70] = new Z80Instruction(
            Mnemonic: "LD (HL), B",
            Opcode: 0x70,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.B;
                memory.WriteByte(addr, value); return cycles; });

        _opcodeTable[0x71] = new Z80Instruction(
            Mnemonic: "LD (HL), C",
            Opcode: 0x71,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.C;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x72] = new Z80Instruction(
            Mnemonic: "LD (HL), D",
            Opcode: 0x72,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.D;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x73] = new Z80Instruction(
            Mnemonic: "LD (HL), E",
            Opcode: 0x73,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.E;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x74] = new Z80Instruction(
            Mnemonic: "LD (HL), H",
            Opcode: 0x74,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.H;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x75] = new Z80Instruction(
            Mnemonic: "LD (HL), L",
            Opcode: 0x75,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.L;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x76] = new Z80Instruction(
            Mnemonic: "HALT",
            Opcode: 0x76,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.IsHalted = true; return cycles; });

        _opcodeTable[0x77] = new Z80Instruction(
            Mnemonic: "LD (HL), A",
            Opcode: 0x77,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = cpu.A;
                memory.WriteByte(addr, value);
            return cycles; });

        _opcodeTable[0x78] = new Z80Instruction(
            Mnemonic: "LD A, B",
            Opcode: 0x78,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.B; return cycles; });

        _opcodeTable[0x79] = new Z80Instruction(
            Mnemonic: "LD A, C",
            Opcode: 0x79,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.C; return cycles; });

        _opcodeTable[0x7A] = new Z80Instruction(
            Mnemonic: "LD A, D",
            Opcode: 0x7A,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.D; return cycles; });

        _opcodeTable[0x7B] = new Z80Instruction(
            Mnemonic: "LD A, E",
            Opcode: 0x7B,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.E; return cycles; });

        _opcodeTable[0x7C] = new Z80Instruction(
            Mnemonic: "LD A, H",
            Opcode: 0x7C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.H; return cycles; });

        _opcodeTable[0x7D] = new Z80Instruction(
            Mnemonic: "LD A, L",
            Opcode: 0x7D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.L; return cycles; });

        _opcodeTable[0x7E] = new Z80Instruction(
            Mnemonic: "LD A, (HL)",
            Opcode: 0x7E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort addr = cpu.HL;
                byte value = memory.ReadByte(addr);
                cpu.A = value;
            return cycles; });

        _opcodeTable[0x7F] = new Z80Instruction(
            Mnemonic: "LD A, A",
            Opcode: 0x7F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.A; return cycles; });

        #endregion
    }


    private static void OpCodes_80_BF(Z80Instruction[] _opcodeTable)
    {

        #region 0x80 - 0x8F  

        _opcodeTable[0x80] = new Z80Instruction(
            Mnemonic: "ADD A, B",
            Opcode: 0x80,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.B;
                int tempResult = oldA + operand; // Use int for detecting overflow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for ADD
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
                return cycles;
            }
        );

        _opcodeTable[0x81] = new Z80Instruction(
            Mnemonic: "ADD A, C",
            Opcode: 0x81,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.C;
                int tempResult = oldA + operand; // Use int for detecting overflow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for ADD
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
                return cycles;
            }
        );

        _opcodeTable[0x82] = new Z80Instruction(
            Mnemonic: "ADD A, D",
            Opcode: 0x82,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.D;
                int tempResult = oldA + operand; // Use int for detecting overflow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for ADD
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
                return cycles;
            }
        );

        _opcodeTable[0x83] = new Z80Instruction(
            Mnemonic: "ADD A, E",
            Opcode: 0x83,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.E;
                int tempResult = oldA + operand; // Use int for detecting overflow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for ADD
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
                return cycles;
            }
        );

        _opcodeTable[0x84] = new Z80Instruction(
    Mnemonic: "ADD A, H",
    Opcode: 0x84,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.H;
        int tempResult = oldA + operand; // Use int for detecting overflow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(false); // Clear N for ADD
        cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
        cpu.SetFlagC(tempResult > 0xFF);
        return cycles;
    }
);

        _opcodeTable[0x85] = new Z80Instruction(
    Mnemonic: "ADD A, L",
    Opcode: 0x85,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.L;
        int tempResult = oldA + operand; // Use int for detecting overflow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(false); // Clear N for ADD
        cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
        cpu.SetFlagC(tempResult > 0xFF);
        return cycles;
    }
);

        _opcodeTable[0x86] = new Z80Instruction(
            Mnemonic: "ADD A, (HL)",
            Opcode: 0x86,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                 byte oldA = cpu.A;
                int tempResult = oldA + operandHL;
                 cpu.A = (byte)tempResult;
                 cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false);
                cpu.SetFlagH(((oldA & 0x0F) + (operandHL & 0x0F)) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
            return cycles; });

        _opcodeTable[0x87] = new Z80Instruction(
    Mnemonic: "ADD A, A",
    Opcode: 0x87,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.A;
        int tempResult = oldA + operand; // Use int for detecting overflow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(false); // Clear N for ADD
        cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F)) > 0x0F);
        cpu.SetFlagC(tempResult > 0xFF);
        return cycles;
    });


        _opcodeTable[0x88] = new Z80Instruction(
         Mnemonic: "ADC A, B",
         Opcode: 0x88,
         InstructionSize: 1,
         TCycles: 4,
         AffectsFlags: true, // Z, N, H, C are affected
         Execute: (cpu, memory, cycles) => {
             byte oldA = cpu.A;
             byte operand = cpu.B;
             byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
             int tempResult = oldA + operand + carryIn; // Add with carry
             cpu.A = (byte)tempResult; // Store 8-bit result
             cpu.SetFlagZ(cpu.A == 0);
             cpu.SetFlagN(false); // Clear N for ADC
             cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
             cpu.SetFlagC(tempResult > 0xFF);
             return cycles;
         }
     );

        _opcodeTable[0x89] = new Z80Instruction(
         Mnemonic: "ADC A, C",
         Opcode: 0x89,
         InstructionSize: 1,
         TCycles: 4,
         AffectsFlags: true, // Z, N, H, C are affected
         Execute: (cpu, memory, cycles) => {
             byte oldA = cpu.A;
             byte operand = cpu.C;
             byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
             int tempResult = oldA + operand + carryIn; // Add with carry
             cpu.A = (byte)tempResult; // Store 8-bit result
             cpu.SetFlagZ(cpu.A == 0);
             cpu.SetFlagN(false); // Clear N for ADC
             cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
             cpu.SetFlagC(tempResult > 0xFF);
             return cycles;
         }
     );

        _opcodeTable[0x8A] = new Z80Instruction(
         Mnemonic: "ADC A, D",
         Opcode: 0x8A,
         InstructionSize: 1,
         TCycles: 4,
         AffectsFlags: true, // Z, N, H, C are affected
         Execute: (cpu, memory, cycles) => {
             byte oldA = cpu.A;
             byte operand = cpu.D;
             byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
             int tempResult = oldA + operand + carryIn; // Add with carry
             cpu.A = (byte)tempResult; // Store 8-bit result
             cpu.SetFlagZ(cpu.A == 0);
             cpu.SetFlagN(false); // Clear N for ADC
             cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
             cpu.SetFlagC(tempResult > 0xFF);
             return cycles;
         }
     );

        _opcodeTable[0x8B] = new Z80Instruction(
         Mnemonic: "ADC A, E",
         Opcode: 0x8B,
         InstructionSize: 1,
         TCycles: 4,
         AffectsFlags: true, // Z, N, H, C are affected
         Execute: (cpu, memory, cycles) => {
             byte oldA = cpu.A;
             byte operand = cpu.E;
             byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
             int tempResult = oldA + operand + carryIn; // Add with carry
             cpu.A = (byte)tempResult; // Store 8-bit result
             cpu.SetFlagZ(cpu.A == 0);
             cpu.SetFlagN(false); // Clear N for ADC
             cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
             cpu.SetFlagC(tempResult > 0xFF);
             return cycles;
         }
     );

        _opcodeTable[0x8C] = new Z80Instruction(
         Mnemonic: "ADC A, H",
         Opcode: 0x8C,
         InstructionSize: 1,
         TCycles: 4,
         AffectsFlags: true, // Z, N, H, C are affected
         Execute: (cpu, memory, cycles) => {
             byte oldA = cpu.A;
             byte operand = cpu.H;
             byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
             int tempResult = oldA + operand + carryIn; // Add with carry
             cpu.A = (byte)tempResult; // Store 8-bit result
             cpu.SetFlagZ(cpu.A == 0);
             cpu.SetFlagN(false); // Clear N for ADC
             cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
             cpu.SetFlagC(tempResult > 0xFF);
             return cycles;
         }
     );

        _opcodeTable[0x8D] = new Z80Instruction(
 Mnemonic: "ADC A, L",
 Opcode: 0x8D,
 InstructionSize: 1,
 TCycles: 4,
 AffectsFlags: true, // Z, N, H, C are affected
 Execute: (cpu, memory, cycles) => {
     byte oldA = cpu.A;
     byte operand = cpu.L;
     byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
     int tempResult = oldA + operand + carryIn; // Add with carry
     cpu.A = (byte)tempResult; // Store 8-bit result
     cpu.SetFlagZ(cpu.A == 0);
     cpu.SetFlagN(false); // Clear N for ADC
     cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
     cpu.SetFlagC(tempResult > 0xFF);
     return cycles;
 }
);

        _opcodeTable[0x8E] = new Z80Instruction(
            Mnemonic: "ADC A, (HL)",
            Opcode: 0x8E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0;
                byte oldA = cpu.A;
                int tempResult = oldA + operandHL + carryIn;
                cpu.A = (byte)tempResult;
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false);
                cpu.SetFlagH(((oldA & 0x0F) + (operandHL & 0x0F) + carryIn) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
                return cycles; 
            });

        _opcodeTable[0x8F] = new Z80Instruction(
            Mnemonic: "ADC A, A",
            Opcode: 0x8F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.A;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag
                int tempResult = oldA + operand + carryIn; // Add with carry
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for ADC
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carryIn) > 0x0F);
                cpu.SetFlagC(tempResult > 0xFF);
            return cycles; });

        #endregion

        #region 0x90 - 0x9F  

        _opcodeTable[0x90] = new Z80Instruction(
            Mnemonic: "SUB A, B",
            Opcode: 0x90,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.B;
                int tempResult = oldA - operand; // Use int for detecting underflow/borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SUBTRACTION
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
                cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
                return cycles;
            });
    

        _opcodeTable[0x91] = new Z80Instruction(
            Mnemonic: "SUB A, C",
            Opcode: 0x91,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.C;
                int tempResult = oldA - operand; // Use int for detecting underflow/borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SUBTRACTION
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
                cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
                return cycles; });

        _opcodeTable[0x92] = new Z80Instruction(
    Mnemonic: "SUB A, D",
    Opcode: 0x92,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.D;
        int tempResult = oldA - operand; // Use int for detecting underflow/borrow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(true); // Set N for SUBTRACTION
        cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
        cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
        return cycles;
    });

        _opcodeTable[0x93] = new Z80Instruction(
    Mnemonic: "SUB A, E",
    Opcode: 0x93,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.E;
        int tempResult = oldA - operand; // Use int for detecting underflow/borrow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(true); // Set N for SUBTRACTION
        cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
        cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
        return cycles;
    });


        _opcodeTable[0x94] = new Z80Instruction(
    Mnemonic: "SUB A, H",
    Opcode: 0x94,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.H;
        int tempResult = oldA - operand; // Use int for detecting underflow/borrow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(true); // Set N for SUBTRACTION
        cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
        cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
        return cycles;
    });

        _opcodeTable[0x95] = new Z80Instruction(
    Mnemonic: "SUB A, L",
    Opcode: 0x95,
    InstructionSize: 1,
    TCycles: 4,
    AffectsFlags: true, // Z, N, H, C are affected
    Execute: (cpu, memory, cycles) => {
        byte oldA = cpu.A;
        byte operand = cpu.L;
        int tempResult = oldA - operand; // Use int for detecting underflow/borrow
        cpu.A = (byte)tempResult; // Store 8-bit result
        cpu.SetFlagZ(cpu.A == 0);
        cpu.SetFlagN(true); // Set N for SUBTRACTION
        cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
        cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
        return cycles;
    });

        _opcodeTable[0x96] = new Z80Instruction(
            Mnemonic: "SUB A, (HL)",
            Opcode: 0x96,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                byte oldA = cpu.A;
                int tempResult = oldA - operandHL;
                cpu.A = (byte)tempResult;
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true);
                cpu.SetFlagH(((oldA & 0x0F) - (operandHL & 0x0F)) < 0x00);
                cpu.SetFlagC(oldA < operandHL);
            return cycles; });


        _opcodeTable[0x97] = new Z80Instruction(
            Mnemonic: "SUB A, A",
            Opcode: 0x97,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.A;
                int tempResult = oldA - operand; // Use int for detecting underflow/borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SUBTRACTION
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00); // Half-borrow: from bit 4 to 3
                cpu.SetFlagC(oldA < operand); // Carry (Borrow) flag: Set if oldA < operand
            return cycles; });


        _opcodeTable[0x98] = new Z80Instruction(
            Mnemonic: "SBC A, B",
            Opcode: 0x98,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.B;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
        return cycles;
    } );

        _opcodeTable[0x99] = new Z80Instruction(
            Mnemonic: "SBC A, C",
            Opcode: 0x99,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.C;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });
        _opcodeTable[0x9A] = new Z80Instruction(
            Mnemonic: "SBC A, D",
            Opcode: 0x9A,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.D;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });
        _opcodeTable[0x9B] = new Z80Instruction(
            Mnemonic: "SBC A, E",
            Opcode: 0x9B,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.E;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });
        _opcodeTable[0x9C] = new Z80Instruction(
            Mnemonic: "SBC A, H",
            Opcode: 0x9C,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.H;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });
        _opcodeTable[0x9D] = new Z80Instruction(
            Mnemonic: "SBC A, L",
            Opcode: 0x9D,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.L;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });

        _opcodeTable[0x9E] = new Z80Instruction(
            Mnemonic: "SBC A, (HL)",
            Opcode: 0x9E,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0;
                byte oldA = cpu.A;
                int tempResult = oldA - operandHL - carryIn;
                cpu.A = (byte)tempResult;
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true);
                cpu.SetFlagH(((oldA & 0x0F) - (operandHL & 0x0F) - carryIn) < 0x00);
                cpu.SetFlagC(tempResult < 0x00);
            return cycles; });


        _opcodeTable[0x9F] = new Z80Instruction(
            Mnemonic: "SBC A, A",
            Opcode: 0x9F,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;
                byte operand = cpu.A;
                byte carryIn = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get current C flag (acts as borrow in)
                int tempResult = oldA - operand - carryIn; // Subtract with borrow
                cpu.A = (byte)tempResult; // Store 8-bit result
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(true); // Set N for SBC
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F) - carryIn) < 0x00); // Half-borrow
                cpu.SetFlagC(tempResult < 0x00); // Carry (Borrow) flag: Set if oldA - operandB - carryIn < 0
            return cycles; });

#endregion

        #region 0xA0 - 0xAF  

        _opcodeTable[0xA0] = new Z80Instruction(
            Mnemonic: "AND A, B",
            Opcode: 0xA0,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) =>    {
                byte operand = cpu.B;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });


        _opcodeTable[0xA1] = new Z80Instruction(
            Mnemonic: "AND A, C",
            Opcode: 0xA1,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.C;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });
        _opcodeTable[0xA2] = new Z80Instruction(
            Mnemonic: "AND A, D",
            Opcode: 0xA2,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.D;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });
        _opcodeTable[0xA3] = new Z80Instruction(
            Mnemonic: "AND A, E",
            Opcode: 0xA3,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.E;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });
        _opcodeTable[0xA4] = new Z80Instruction(
            Mnemonic: "AND A, H",
            Opcode: 0xA4,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.H;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });
        _opcodeTable[0xA5] = new Z80Instruction(
            Mnemonic: "AND A, L",
            Opcode: 0xA5,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.L;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });

        _opcodeTable[0xA6] = new Z80Instruction(
            Mnemonic: "AND A, (HL)",
            Opcode: 0xA6,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                cpu.A = (byte)(cpu.A & operandHL); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false);
                cpu.SetFlagH(true); // H flag is ALWAYS SET for AND
                cpu.SetFlagC(false);
            return cycles; });

        _opcodeTable[0xA7] = new Z80Instruction(
            Mnemonic: "AND A, A",
            Opcode: 0xA7,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.A;
                cpu.A = (byte)(cpu.A & operand); // Perform bitwise AND
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for AND
                cpu.SetFlagH(true); // Set H for AND
                cpu.SetFlagC(false); // Clear C for AND
            return cycles; });

        _opcodeTable[0xA8] = new Z80Instruction(
            Mnemonic: "XOR A, B",
            Opcode: 0xA8,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.B;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });

        _opcodeTable[0xA9] = new Z80Instruction(
            Mnemonic: "XOR A, C",
            Opcode: 0xA9,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.C;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });

        _opcodeTable[0xAA] = new Z80Instruction(
            Mnemonic: "XOR A, D",
            Opcode: 0xAA,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.D;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
                return cycles;
        });

        _opcodeTable[0xAB] = new Z80Instruction(
            Mnemonic: "XOR A, E",
            Opcode: 0xAB,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.E;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });

        _opcodeTable[0xAC] = new Z80Instruction(
            Mnemonic: "XOR A, H",
            Opcode: 0xAC,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.H;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });


        _opcodeTable[0xAD] = new Z80Instruction(
            Mnemonic: "XOR A, L",
            Opcode: 0xAD,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.L;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });

        _opcodeTable[0xAE] = new Z80Instruction(
            Mnemonic: "XOR A, (HL)",
            Opcode: 0xAE,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                cpu.A = (byte)(cpu.A ^ operandHL); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false);
                cpu.SetFlagH(false);
                cpu.SetFlagC(false);
            return cycles; });

        _opcodeTable[0xAF] = new Z80Instruction(
            Mnemonic: "XOR A, A",
            Opcode: 0xAF,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.A;
                cpu.A = (byte)(cpu.A ^ operand); // Perform bitwise XOR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for XOR
                cpu.SetFlagH(false); // Clear H for XOR
                cpu.SetFlagC(false); // Clear C for XOR
            return cycles; });
#endregion

        #region 0xB0 - 0xBF  

        _opcodeTable[0xB0] = new Z80Instruction(
            Mnemonic: "OR A, B",
            Opcode: 0xB0,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.B;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });

        _opcodeTable[0xB1] = new Z80Instruction(
            Mnemonic: "OR A, C",
            Opcode: 0xB1,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.C;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });

        _opcodeTable[0xB2] = new Z80Instruction(
            Mnemonic: "OR A, D",
            Opcode: 0xB2,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.D;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });


        _opcodeTable[0xB3] = new Z80Instruction(
            Mnemonic: "OR A, E",
            Opcode: 0xB3,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.E;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });

        _opcodeTable[0xB4] = new Z80Instruction(
            Mnemonic: "OR A, H",
            Opcode: 0xB4,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.H;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });

        _opcodeTable[0xB5] = new Z80Instruction(
            Mnemonic: "OR A, L",
            Opcode: 0xB5,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.L;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });


        _opcodeTable[0xB6] = new Z80Instruction(
            Mnemonic: "OR A, (HL)",
            Opcode: 0xB6,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) =>
            {
                // 1. Get the 16-bit address from HL.
                ushort address = cpu.HL;
                // 2. Read the operand from memory.
                byte operandHL = memory.ReadByte(address);
                cpu.A = (byte)(cpu.A | operandHL); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false);
                cpu.SetFlagH(false);
                cpu.SetFlagC(false);
            return cycles; });


        _opcodeTable[0xB7] = new Z80Instruction(
            Mnemonic: "OR A, A",
            Opcode: 0xB7,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte operand = cpu.A;
                cpu.A = (byte)(cpu.A | operand); // Perform bitwise OR
                cpu.SetFlagZ(cpu.A == 0);
                cpu.SetFlagN(false); // Clear N for OR
                cpu.SetFlagH(false); // Clear H for OR
                cpu.SetFlagC(false); // Clear C for OR
            return cycles; });


        _opcodeTable[0xB8] = new Z80Instruction(
            Mnemonic: "CP A, B",
            Opcode: 0xB8,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.B;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                // Note: cpu.A is NOT modified by this instruction.
                // Z Flag (Zero): Set if the (imaginary) result is 0.
                // This means A == B.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });

        _opcodeTable[0xB9] = new Z80Instruction(
            Mnemonic: "CP A, C",
            Opcode: 0xB9,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.C;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                                                // Note: cpu.A is NOT modified by this instruction.
                                                // Z Flag (Zero): Set if the (imaginary) result is 0.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                                                    // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });

        _opcodeTable[0xBA] = new Z80Instruction(
            Mnemonic: "CP A, D",
            Opcode: 0xBA,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.D;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                                                 // Note: cpu.A is NOT modified by this instruction.
                                                 // Z Flag (Zero): Set if the (imaginary) result is 0.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                                                 // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });


        _opcodeTable[0xBB] = new Z80Instruction(
            Mnemonic: "CP A, E",
            Opcode: 0xBB,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.E;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                                         // Note: cpu.A is NOT modified by this instruction.
                                         // Z Flag (Zero): Set if the (imaginary) result is 0.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                                             // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });

        _opcodeTable[0xBC] = new Z80Instruction(
            Mnemonic: "CP A, H",
            Opcode: 0xBC,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.H;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                                         // Note: cpu.A is NOT modified by this instruction.
                                         // Z Flag (Zero): Set if the (imaginary) result is 0.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                                             // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });

        _opcodeTable[0xBD] = new Z80Instruction(
            Mnemonic: "CP A, L",
            Opcode: 0xBD,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.L;
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // Use int to correctly detect borrow/underflow
                                         // Note: cpu.A is NOT modified by this instruction.
                                         // Z Flag (Zero): Set if the (imaginary) result is 0.
                cpu.SetFlagZ((byte)tempResult == 0); // Check the 8-bit result
                                             // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operand & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7.
                // This means oldA < operand.
                cpu.SetFlagC(oldA < operand);
            return cycles; });

        _opcodeTable[0xBE] = new Z80Instruction(
            Mnemonic: "CP A, (HL)",
            Opcode: 0xBE,
            InstructionSize: 1,
            TCycles: 8,
            AffectsFlags: true, // Z, N, H, C are affected (like SUB)
            Execute: (cpu, memory, cycles) =>
            {
                ushort address = cpu.HL;
                byte operandHL = memory.ReadByte(address);
                byte oldA = cpu.A; // The value of A before the (imaginary) subtraction
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operandHL; // Use int to correctly detect borrow/underflow
                // Z Flag (Zero): Set if the (imaginary) result is 0. This means A == (HL).
                cpu.SetFlagZ((byte)tempResult == 0);
                // N Flag (Subtract): Always SET for SUBTRACT-like operations (CP is like SUB).
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH(((oldA & 0x0F) - (operandHL & 0x0F)) < 0x00);
                // C Flag (Carry/Borrow): Set if the subtraction required a borrow from bit 7. This means A < (HL).
                cpu.SetFlagC(oldA < operandHL);
            return cycles; });

        _opcodeTable[0xBF] = new Z80Instruction(
            Mnemonic: "CP A, A",
            Opcode: 0xBF,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: true, // Z, N, H, C are affected
            Execute: (cpu, memory, cycles) => {
                byte oldA = cpu.A;      // The value of A before the (imaginary) subtraction
                byte operand = cpu.A;  // The operand is A itself
                // Perform the subtraction for flag purposes, but DO NOT store the result in cpu.A
                int tempResult = oldA - operand; // This will always be 0
                // Z Flag (Zero): Set if the (imaginary) result is 0.
                // A - A is always 0, so Z is always set for CP A.
                cpu.SetFlagZ(true);
                // N Flag (Subtract): Always SET for SUBTRACT-like operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a half-borrow.
                // A - A never half-borrows if A is non-zero (0x0F - 0x0F = 0).
                // If A was 0x00, then 0x00 - 0x00 = 0.
                // So H is always cleared for CP A.
                cpu.SetFlagH(false); // Can be complex, but for A-A, it's false
                // C Flag (Carry/Borrow): Set if oldA < operandA.
                // oldA is never less than operandA when they are the same.
                // So C is always cleared for CP A.
                cpu.SetFlagC(false);
            return cycles; });
        #endregion

    }



    private static void OpCodes_C0_FF(Z80Instruction[] _opcodeTable)
    {
        #region 0xC0 - 0xCF  

        _opcodeTable[0xC0] = new Z80Instruction(
            Mnemonic: "RET NZ",
            Opcode: 0xC0,
            InstructionSize: 1,
            TCycles: 20, // 20 T-cycles if condition met, 8 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RET NZ: Return if Z flag is not set.
                if (!cpu.GetFlagZ())
                {
                    // Pop the program counter from the stack
                    ushort address = cpu.PopWordFromStack();
                    cpu.PC = address;
                }
                else
                {
                    return 8;
                }
            return cycles; });

        _opcodeTable[0xC1] = new Z80Instruction(
            Mnemonic: "POP BC",
            Opcode: 0xC1,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort value = cpu.PopWordFromStack();
                cpu.BC = value;
            return cycles; });

        _opcodeTable[0xC2] = new Z80Instruction(
            Mnemonic: "JP NZ, nn",
            Opcode: 0xC2,
            InstructionSize: 3,
            TCycles: 16, // 16 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // The PC has already advanced past the opcode and operand bytes
                // by the time Execute is called.
                // The operand 'nn' (16-bit address) is at PC - 2 and PC - 1.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte);
                // JP NZ: Jump to address 'nn' if Z flag is not set.
                if (!cpu.GetFlagZ())
                {
                    cpu.PC = address;
                    // TCycles for conditional jumps often depend on whether the jump is taken.
                    // If the jump is taken: 16 cycles. If not taken: 12 cycles.
                    // Our static table typically stores the 'taken' cycles.
                }
                else
                {
                    // If the condition is not met, PC remains as incremented by the main loop.
                    // No additional action needed here, the 'return' from execute will continue
                    // the normal PC flow.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xC3] = new Z80Instruction(
            Mnemonic: "JP $nn",
            Opcode: 0xC3,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // The PC has already advanced past the opcode and operand bytes.
                // The operand 'nn' (16-bit address) is at PC - 2 and PC - 1.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte);
                // JP nn: Unconditionally jump to address 'nn'.
                cpu.PC = address;
            return cycles; });

        _opcodeTable[0xC4] = new Z80Instruction(
            Mnemonic: "CALL NZ, $nn",
            Opcode: 0xC4,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 24, // 24 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn' from the two bytes following the opcode.
                // PC has advanced by InstructionSize (3) by this point, so 'nn' is at PC-2 and PC-1.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort targetAddress = (ushort)((highByte << 8) | lowByte);
                // CALL NZ: Call address 'nn' if Z flag is not set.
                if (!cpu.GetFlagZ())
                {
                    // Push the address of the instruction *after* this CALL onto the stack.
                    // This is the current value of PC, which has already been advanced by 3 bytes.
                    cpu.PushWordToStack(cpu.PC);
                    // Then, jump to the target address.
                    cpu.PC = targetAddress;
                    // TCycles: 24 if call is taken. If not taken, it's 12 cycles.
                }
                else
                {
                    // If condition not met, PC remains unchanged from its auto-increment.
                    // No stack operation, no jump.
                    return 12;
                }
            return cycles; });


        _opcodeTable[0xC5] = new Z80Instruction(
            Mnemonic: "PUSH BC",
            Opcode: 0xC5,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.PushWordToStack(cpu.BC);
            return cycles; });

        _opcodeTable[0xC6] = new Z80Instruction(
            Mnemonic: "ADD A, n",
            Opcode: 0xC6,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                byte old = cpu.A;
                int temp = old + operand;
                cpu.A = (byte)temp;
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This is checked by seeing if the sum of the lower nibbles (0-F) exceeds F.
                cpu.SetFlagH(((old & 0x0F) + (operand & 0x0F)) > 0x0F);
                // C Flag (Carry): Set if the 8-bit sum overflows (i.e., result > 0xFF).
                // temp (the 'int' sum) already tells us if it went over 255.
                cpu.SetFlagC(temp > 0xFF);
            return cycles; });

        _opcodeTable[0xC7] = new Z80Instruction(
            Mnemonic: "RST $00",
            Opcode: 0xC7,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // The fixed address is determined by the opcode itself.
                // For RST $00 (Opcode C7), the target address is 0x0000.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0000;
            return cycles; });


        _opcodeTable[0xC8] = new Z80Instruction(
            Mnemonic: "RET Z",
            Opcode: 0xC8,
            InstructionSize: 1,
            TCycles: 20, // 20 T-cycles if condition met, 8 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RET Z: Return if Z flag is set.
                if (cpu.GetFlagZ())
                {
                    // Pop the program counter from the stack
                    ushort address = cpu.PopWordFromStack();
                    cpu.PC = address;
                }
                else return 8;
                    // If condition not met, PC advances normally.
                return cycles; });

        _opcodeTable[0xC9] = new Z80Instruction(
            Mnemonic: "RET",
            Opcode: 0xC9,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RET: Unconditionally return from subroutine.
                ushort address = cpu.PopWordFromStack();
                cpu.PC = address;
            return cycles; });

        _opcodeTable[0xCA] = new Z80Instruction(
            Mnemonic: "JP Z, $nn",
            Opcode: 0xCA,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 16, // 16 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn' from the two bytes following the opcode.
                // PC has advanced by InstructionSize (3) by this point.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte);
                // JP Z: Jump to address 'nn' if Z flag is set.
                if (cpu.GetFlagZ())
                {
                    cpu.PC = address;
                    // TCycles for conditional jumps: 16 if taken.
                }
                else
                {
                    // If condition not met, PC continues to the next instruction.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xCB] = new Z80Instruction(
            Mnemonic: "CB-PREFIX",
            Opcode: 0xCB,
            InstructionSize: 0,
            TCycles: 0,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => { /* gets pounced ion by the instruction processor */ return cycles; });

        _opcodeTable[0xCC] = new Z80Instruction(
            Mnemonic: "CALL Z, $nn",
            Opcode: 0xCC,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 24, // 24 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn'.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort targetAddress = (ushort)((highByte << 8) | lowByte);
                // CALL Z: Call address 'nn' if Z flag is set.
                if (cpu.GetFlagZ())
                {
                    // Push the return address (current PC value) onto the stack.
                    cpu.PushWordToStack(cpu.PC);
                    // Jump to the target address.
                    cpu.PC = targetAddress;
                    // TCycles: 24 if call is taken.
                }
                else
                {
                    // If condition not met, PC remains unchanged.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xCD] = new Z80Instruction(
         Mnemonic: "CALL $nn",
         Opcode: 0xCD,
         InstructionSize: 3, // Opcode + 2 bytes for address
         TCycles: 24,
         AffectsFlags: false,
         Execute: (cpu, memory, cycles) => {
             // Read the 16-bit address 'nn'.
             byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
             byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
             ushort targetAddress = (ushort)((highByte << 8) | lowByte);
             // Push the return address (current PC value) onto the stack.
             cpu.PushWordToStack(cpu.PC);
             // Jump to the target address.
             cpu.PC = targetAddress;
             return cycles;
         });

        _opcodeTable[0xCE] = new Z80Instruction(
            Mnemonic: "ADC A, n",
            Opcode: 0xCE,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                byte oldA = cpu.A;
                byte carry = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get the current Carry flag state
                // Perform the addition: A + operand + CARRY
                int temp = oldA + operand + carry;
                cpu.A = (byte)temp;
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for ADD operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This is checked by seeing if the sum of the lower nibbles (0-F) exceeds F.
                cpu.SetFlagH(((oldA & 0x0F) + (operand & 0x0F) + carry) > 0x0F);
                // C Flag (Carry): Set if the 8-bit sum overflows (i.e., result > 0xFF).
                cpu.SetFlagC(temp > 0xFF);
            return cycles; });

        _opcodeTable[0xCF] = new Z80Instruction(
            Mnemonic: "RST $08",
            Opcode: 0xCF,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0008;
            return cycles; });
        #endregion

        #region 0xD0 - 0xDF  

        _opcodeTable[0xD0] = new Z80Instruction(
            Mnemonic: "RET NC",
            Opcode: 0xD0,
            InstructionSize: 1,
            TCycles: 20, // 20 T-cycles if condition met, 8 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RET NC: Return if C flag is not set.
                if (!cpu.GetFlagC())
                {
                    // Pop the program counter from the stack
                    ushort address = cpu.PopWordFromStack();
                    cpu.PC = address;
                }
                else return 8;
                    // If condition not met, PC advances normally.
                    return cycles; });

        _opcodeTable[0xD1] = new Z80Instruction(
            Mnemonic: "POP DE",
            Opcode: 0xD1,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort value = cpu.PopWordFromStack();
                cpu.DE = value;
            return cycles; });

        _opcodeTable[0xD2] = new Z80Instruction(
            Mnemonic: "JP NC, $nn",
            Opcode: 0xD2,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 16, // 16 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn' from the two bytes following the opcode.
                // PC has advanced by InstructionSize (3) by this point.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte);
                // JP NC: Jump to address 'nn' if C flag is NOT set.
                if (!cpu.GetFlagC())
                {
                    cpu.PC = address;
                    // TCycles for conditional jumps: 16 if taken.
                }
                else
                {
                    // If condition not met, PC continues to the next instruction.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xD3] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xD3,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xD3:X2} at PC: {cpu.PC:X4}");
                //return cycles;
                });

        _opcodeTable[0xD4] = new Z80Instruction(
            Mnemonic: "CALL NC, nn",
            Opcode: 0xD4,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 24, // 24 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn'.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort targetAddress = (ushort)((highByte << 8) | lowByte);
                // CALL NC: Call address 'nn' if C flag is not set.
                if (!cpu.GetFlagC())
                {
                    // Push the return address (PC after this CALL) onto the stack.
                    cpu.PushWordToStack(cpu.PC);
                    // Jump to the target address.
                    cpu.PC = targetAddress;
                    // TCycles: 24 if call is taken. If not taken, it's 12 cycles.
                }
                else
                {
                    // If condition not met, PC remains unchanged.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xD5] = new Z80Instruction(
            Mnemonic: "PUSH DE",
            Opcode: 0xD5,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.PushWordToStack(cpu.DE);
            return cycles; });

        _opcodeTable[0xD6] = new Z80Instruction(
            Mnemonic: "SUB A, n",
            Opcode: 0xD6,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                byte old = cpu.A;
                // Perform the subtraction. Using an int to detect underflow.
                int temp = old - operand;
                cpu.A = (byte)temp;
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always SET for SUB operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This is equivalent to checking if the lower nibble of 'old'
                // is less than the lower nibble of 'operand'.
                cpu.SetFlagH((old & 0x0F) < (operand & 0x0F));
                // C Flag (Carry): Set if the 8-bit result underflows (i.e., result < 0).
                // temp (the 'int' result) already tells us if it went below 0.
                cpu.SetFlagC(temp < 0);
            return cycles; });

        _opcodeTable[0xD7] = new Z80Instruction(
            Mnemonic: "RST $10",
            Opcode: 0xD7,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0010;
            return cycles; });


        _opcodeTable[0xD8] = new Z80Instruction(
            Mnemonic: "RET C",
            Opcode: 0xD8,
            InstructionSize: 1,
            TCycles: 20, // 20 T-cycles if condition met, 8 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RET Z: Return if Z flag is set.
                if (cpu.GetFlagC())
                {
                    // Pop the program counter from the stack
                    ushort address = cpu.PopWordFromStack();
                    cpu.PC = address;
                }
                else return 8;
                    // If condition not met, PC advances normally.
                    return cycles; });

        _opcodeTable[0xD9] = new Z80Instruction(
            Mnemonic: "RETI",
            Opcode: 0xD9,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) =>
            {
                // 1. Pop the return address from the stack (like a normal RET)
                ushort address = cpu.PopWordFromStack();
                cpu.PC = address;
                // 2. Enable interrupts (set the Master Interrupt Enable Flag)
                cpu.EnableInterrupts();
            return cycles; });

        _opcodeTable[0xDA] = new Z80Instruction(
            Mnemonic: "JP C, nn",
            Opcode: 0xDA,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 16, // 16 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn' from the two bytes following the opcode.
                // PC has advanced by InstructionSize (3) by this point.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte);
                // JP C: Jump to address 'nn' if C flag is set.
                if (cpu.GetFlagC())
                {
                    cpu.PC = address;
                    // TCycles for conditional jumps: 16 if taken.
                }
                else
                {
                    // If condition not met, PC continues to the next instruction.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xDB] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xDB,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xDB:X2} at PC: {cpu.PC:X4}");
            //return cycles;
            });


        _opcodeTable[0xDC] = new Z80Instruction(
            Mnemonic: "CALL C, nn",
            Opcode: 0xDC,
            InstructionSize: 3, // Opcode + 2 bytes for address
            TCycles: 24, // 24 T-cycles if condition met, 12 T-cycles if not
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 16-bit address 'nn'.
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort targetAddress = (ushort)((highByte << 8) | lowByte);
                if (cpu.GetFlagC())
                {
                    // Push the return address (current PC value) onto the stack.
                    cpu.PushWordToStack(cpu.PC);
                    // Jump to the target address.
                    cpu.PC = targetAddress;
                    // TCycles: 24 if call is taken.
                }
                else
                {
                    // If condition not met, PC remains unchanged.
                    return 12;
                }
            return cycles; });

        _opcodeTable[0xDD] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xDD,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xDD:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });

        _opcodeTable[0xDE] = new Z80Instruction(
            Mnemonic: "SBC A, n",
            Opcode: 0xDE,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                byte oldA = cpu.A;
                byte carry = cpu.GetFlagC() ? (byte)1 : (byte)0; // Get the current Carry flag state
                // Perform the subtraction: A - (operand + CARRY)
                // Effectively, A - operand - CARRY
                int temp = oldA - operand - carry;
                cpu.A = (byte)temp;
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always SET for SUB operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                // This means the lower nibble of A was less than the lower nibble of (operand + carry).
                cpu.SetFlagH(((oldA & 0x0F) - ((operand & 0x0F) + carry)) < 0);
                // C Flag (Carry): Set if the 8-bit result underflows (i.e., result < 0).
                cpu.SetFlagC(temp < 0);
            return cycles; });

        _opcodeTable[0xDF] = new Z80Instruction(
                Mnemonic: "RST $18",
                Opcode: 0xDF,
                InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
                TCycles: 16,
                AffectsFlags: false,
                Execute: (cpu, memory, cycles) => {
                    // RST instruction: Push current PC to stack, then jump to a fixed address.
                    // First, push the current PC (return address) onto the stack.
                    // cpu.PC already points to the instruction *after* this RST.
                    cpu.PushWordToStack(cpu.PC);
                    // Then, jump to the specific restart vector address.
                    cpu.PC = 0x0018;
                return cycles; });
        #endregion

        #region 0xE0 - 0xEF  

        _opcodeTable[0xE0] = new Z80Instruction(
            Mnemonic: "LDH (n), A",
            Opcode: 0xE0,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit offset (a8/n)
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit offset (a8/n) from memory.
                // PC has advanced by InstructionSize (2) by this point, so 'n' is at PC - 1.
                byte offset = memory.ReadByte((ushort)(cpu.PC - 1));
                // The full 16-bit address is 0xFF00 + offset.
                ushort targetAddress = (ushort)(0xFF00 + offset);
                // Write the value from Accumulator (A) to the target address.
                memory.WriteByte(targetAddress, cpu.A);
            return cycles; });

        _opcodeTable[0xE1] = new Z80Instruction(
            Mnemonic: "POP HL",
            Opcode: 0xE1,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                ushort value = cpu.PopWordFromStack();
                cpu.HL = value;
            return cycles; });

        _opcodeTable[0xE2] = new Z80Instruction(
            Mnemonic: "LDH [C], A",
            Opcode: 0xE2,
            InstructionSize: 1, // Opcode only, C register is implicit
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // The 8-bit value in C forms the offset.
                byte offset = cpu.C;
                // The full 16-bit address is 0xFF00 + C.
                ushort targetAddress = (ushort)(0xFF00 + offset);
                // Write the value from Accumulator (A) to the target address.
                memory.WriteByte(targetAddress, cpu.A);
            return cycles; });

        _opcodeTable[0xE3] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xE3,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xE3:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });

        _opcodeTable[0xE4] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xE4,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xE4:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });


        _opcodeTable[0xE5] = new Z80Instruction(
            Mnemonic: "PUSH HL",
            Opcode: 0xE5,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.PushWordToStack(cpu.HL);
            return cycles; });

        _opcodeTable[0xE6] = new Z80Instruction(
            Mnemonic: "AND A, n",
            Opcode: 0xE6,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit operand (n)
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit operand (n).
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                // Perform bitwise AND with A.
                cpu.A = (byte)(cpu.A & operand);
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for AND operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Always SET for AND operations.
                cpu.SetFlagH(true);
                // C Flag (Carry): Always CLEAR for AND operations.
                cpu.SetFlagC(false);
            return cycles; });

        _opcodeTable[0xE7] = new Z80Instruction(
            Mnemonic: "RST $20",
            Opcode: 0xE7,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0020;
            return cycles; });

        _opcodeTable[0xE8] = new Z80Instruction(
            Mnemonic: "ADD SP, e",
            Opcode: 0xE8,
            InstructionSize: 2, // Opcode + 1 byte for the signed 8-bit operand (e)
            TCycles: 16,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the signed 8-bit operand (e).
                // PC has advanced by InstructionSize (2), so 'e' is at PC - 1.
                sbyte signedOperand = (sbyte)memory.ReadByte((ushort)(cpu.PC - 1));
                ushort oldSP = cpu.SP; // Store old SP for flag calculations
                // Perform the addition. Use int for intermediate calculation to handle overflow/underflow correctly.
                int result = oldSP + signedOperand;
                cpu.SP = (ushort)result;
                // Flags for ADD SP, e are peculiar and treat it somewhat like 16-bit math with signed offset.
                // N Flag (Subtract): Always CLEAR.
                cpu.SetFlagN(false);
                // Z Flag (Zero): Always CLEAR for ADD SP, e. (It's almost never 0 after this, regardless).
                cpu.SetFlagZ(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                // This is usually tricky with signed numbers. A common way is to check 0x0F boundary.
                // (oldSP & 0x0F) + (signedOperand & 0x0F) > 0x0F
                // Since 'signedOperand' is sbyte, we need to cast to int/ushort for correct bitwise arithmetic.
                cpu.SetFlagH(((oldSP & 0x0F) + (signedOperand & 0x0F)) > 0x0F);

                // C Flag (Carry): Set if there's a carry from bit 7 to bit 8.
                // (oldSP & 0xFF) + (signedOperand & 0xFF) > 0xFF
                cpu.SetFlagC(((oldSP & 0xFF) + (signedOperand & 0xFF)) > 0xFF);
            return cycles; });

        _opcodeTable[0xE9] = new Z80Instruction(
            Mnemonic: "JP HL",
            Opcode: 0xE9,
            InstructionSize: 1, // Opcode only, HL register is implicit
            TCycles: 4, // Very fast, as it's just loading a register
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Unconditionally jump to the address stored in the HL register.
                cpu.PC = cpu.HL;
            return cycles; });

        _opcodeTable[0xEA] = new Z80Instruction(
            Mnemonic: "LD (nn), A",
            Opcode: 0xEA,
            InstructionSize: 3,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) =>
            {
                // The 'nn' operand is two bytes after the opcode, Little-Endian
                // Opcode is at PC - 3
                // Low byte is at PC - 2
                // High byte is at PC - 1
                byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                ushort address = (ushort)((highByte << 8) | lowByte); // Reconstruct 16-bit address

                memory.WriteByte(address, cpu.A);
                // No flag changes for LD (nn), A
            return cycles; });

        _opcodeTable[0xEB] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xEB,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xEB:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });

        _opcodeTable[0xEC] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xEC,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xEC:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });

        _opcodeTable[0xED] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xED,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xED:X2} at PC: {cpu.PC:X4}");
                //return cycles; 
            });


        _opcodeTable[0xEE] = new Z80Instruction(
            Mnemonic: "XOR A, n",
            Opcode: 0xEE,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit operand (n)
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit operand (n).
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                // Perform bitwise XOR with A.
                cpu.A = (byte)(cpu.A ^ operand);
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for XOR operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Always CLEAR for XOR operations.
                cpu.SetFlagH(false);
                // C Flag (Carry): Always CLEAR for XOR operations.
                cpu.SetFlagC(false);
            return cycles; });

        _opcodeTable[0xEF] = new Z80Instruction(
            Mnemonic: "RST $28",
            Opcode: 0xEF,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0028;
            return cycles; });

        #endregion

        #region 0xF0 - 0xFF  

        _opcodeTable[0xF0] = new Z80Instruction(
            Mnemonic: "LDH A, (n)",
            Opcode: 0xF0,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit offset (a8/n)
            TCycles: 12,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit offset (a8/n) from memory.
                // PC has advanced by InstructionSize (2) by this point, so 'n' is at PC - 1.
                byte offset = memory.ReadByte((ushort)(cpu.PC - 1));
                // The full 16-bit address is 0xFF00 + offset.
                ushort sourceAddress = (ushort)(0xFF00 + offset);
                // Read the value from the source address into Accumulator (A).
                cpu.A = memory.ReadByte(sourceAddress);
            return cycles; });

        _opcodeTable[0xF1] = new Z80Instruction(
            Mnemonic: "POP AF",
            Opcode: 0xF1,
            InstructionSize: 1,
            TCycles: 12,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                ushort value = cpu.PopWordFromStack();
                // The 'A' register gets the high byte.
                cpu.A = (byte)(value >> 8);
                // The 'F' register gets the low byte, but its lower 4 bits are always 0.
                // We ensure this by masking off the lower 4 bits.
                cpu.F = (byte)(value & 0xF0); // Mask: 1111 0000
            return cycles; });

        _opcodeTable[0xF2] = new Z80Instruction(
            Mnemonic: "LDH A, [C]",
            Opcode: 0xF2,
            InstructionSize: 1, // Opcode only, C register is implicit
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // The 8-bit value in C forms the offset.
                byte offset = cpu.C;
                // The full 16-bit address is 0xFF00 + C.
                ushort sourceAddress = (ushort)(0xFF00 + offset);
                // Read the value from the source address into Accumulator (A).
                cpu.A = memory.ReadByte(sourceAddress);
            return cycles; });

        _opcodeTable[0xF3] = new Z80Instruction(
            Mnemonic: "DI",
            Opcode: 0xF3,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.DisableInterrupts();
            return cycles; });

        _opcodeTable[0xF4] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xF4,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xF4:X2} at PC: {cpu.PC:X4}");
                //return cycles;
                });


        _opcodeTable[0xF5] = new Z80Instruction(
            Mnemonic: "PUSH AF",
            Opcode: 0xF5,
            InstructionSize: 1,
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // When pushing AF, we construct the 16-bit word.
                // A is the high byte, F is the low byte.
                // The lower 4 bits of F should always be 0 when pushed.
                // cpu.F should already be correctly maintained by flag setting methods (0xF0 mask applied there).
                ushort value = (ushort)((cpu.A << 8) | (cpu.F & 0xF0)); // Ensure lower 4 F bits are 0
                cpu.PushWordToStack(value);
            return cycles; });

        _opcodeTable[0xF6] = new Z80Instruction(
            Mnemonic: "OR A, n",
            Opcode: 0xF6,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit operand (n)
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit operand (n).
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                // Perform bitwise OR with A.
                cpu.A = (byte)(cpu.A | operand);
                // Z Flag (Zero): Set if the 8-bit result is 0.
                cpu.SetFlagZ(cpu.A == 0);
                // N Flag (Subtract): Always CLEAR for OR operations.
                cpu.SetFlagN(false);
                // H Flag (Half-Carry): Always CLEAR for OR operations.
                cpu.SetFlagH(false);
                // C Flag (Carry): Always CLEAR for OR operations.
                cpu.SetFlagC(false);
            return cycles; });

        _opcodeTable[0xF7] = new Z80Instruction(
            Mnemonic: "RST $30",
            Opcode: 0xF7,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0030;
            return cycles; });

        _opcodeTable[0xF8] = new Z80Instruction(
            Mnemonic: "LD HL, SP + e",
            Opcode: 0xF8,
            InstructionSize: 2, // Opcode + 1 byte for the signed 8-bit operand (e)
            TCycles: 12,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the signed 8-bit operand (e).
                sbyte signedOperand = (sbyte)memory.ReadByte((ushort)(cpu.PC - 1));
                ushort oldSP = cpu.SP; // Store old SP for flag calculations
                // Perform the addition. Use int for intermediate calculation.
                int result = oldSP + signedOperand;
                cpu.HL = (ushort)result;
                // Flags are set in a peculiar way, similar to ADD SP, e.
                // N Flag (Subtract): Always CLEAR.
                cpu.SetFlagN(false);
                // Z Flag (Zero): Always CLEAR.
                cpu.SetFlagZ(false);
                // H Flag (Half-Carry): Set if there's a carry from bit 3 to bit 4.
                cpu.SetFlagH(((oldSP & 0x0F) + (signedOperand & 0x0F)) > 0x0F);
                // C Flag (Carry): Set if there's a carry from bit 7 to bit 8.
                cpu.SetFlagC(((oldSP & 0xFF) + (signedOperand & 0xFF)) > 0xFF);
            return cycles; });

        _opcodeTable[0xF9] = new Z80Instruction(
            Mnemonic: "LD SP, HL",
            Opcode: 0xF9,
            InstructionSize: 1, // Opcode only, HL and SP are implicit registers
            TCycles: 8,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // Load the 16-bit value from HL into the Stack Pointer (SP).
                cpu.SP = cpu.HL;
            return cycles; });


        // 0xFA: LD A, (nn)
        _opcodeTable[0xFA] = new Z80Instruction(
                Mnemonic: "LD A, (nn)",
                Opcode: 0xFA,
                InstructionSize: 3,
                TCycles: 16,
                AffectsFlags: false,
                Execute: (cpu, memory, cycles) =>
                {
                    byte lowByte = memory.ReadByte((ushort)(cpu.PC - 2));
                    byte highByte = memory.ReadByte((ushort)(cpu.PC - 1));
                    ushort address = (ushort)((highByte << 8) | lowByte);

                    cpu.A = memory.ReadByte(address);
                    // No flag changes for LD A, (nn)
                return cycles; });

        _opcodeTable[0xFB] = new Z80Instruction(
            Mnemonic: "EI",
            Opcode: 0xFB,
            InstructionSize: 1,
            TCycles: 4,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                cpu.EnableInterruptsPending = true;
                return cycles; 
            });

        _opcodeTable[0xFC] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xFC,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xFC:X2} at PC: {cpu.PC:X4}");
                //return cycles;
            });

        _opcodeTable[0xFD] = new Z80Instruction(
            Mnemonic: "UNDEFINED", // Or "ILL_OPCODE"
            Opcode: 0xFD,
            InstructionSize: 1, // Or potentially 0 if you want to explicitly not advance PC, but 1 is safer.
            TCycles: 4, // Minimal cycles, or 0, or an average. It doesn't really matter for an undefined instruction.
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // For development, throw an exception
                throw new InvalidOperationException($"Encountered undefined opcode {0xFD:X2} at PC: {cpu.PC:X4}");
                //return cycles;
            });

        _opcodeTable[0xFE] = new Z80Instruction(
            Mnemonic: "CP A, n",
            Opcode: 0xFE,
            InstructionSize: 2, // Opcode + 1 byte for the 8-bit operand (n)
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                // Read the 8-bit operand (n).
                byte operand = memory.ReadByte((ushort)(cpu.PC - 1));
                byte oldA = cpu.A;
                // Perform the subtraction conceptually (A - operand), but don't store the result in A.
                int temp = oldA - operand;
                // Flags are set based on the result of this subtraction.
                // Z Flag (Zero): Set if the 8-bit result of (A - operand) is 0.
                cpu.SetFlagZ((byte)temp == 0); // Cast to byte to check for 8-bit zero
                                               // N Flag (Subtract): Always SET for CP operations.
                cpu.SetFlagN(true);
                // H Flag (Half-Carry): Set if there's a borrow from bit 4 to bit 3.
                cpu.SetFlagH((oldA & 0x0F) < (operand & 0x0F));
                // C Flag (Carry): Set if the 8-bit result underflows (i.e., A < operand).
                cpu.SetFlagC(temp < 0);
            return cycles; });

        _opcodeTable[0xFF] = new Z80Instruction(
            Mnemonic: "RST $38",
            Opcode: 0xFF,
            InstructionSize: 1, // RST is a 1-byte instruction, the address is implicit
            TCycles: 16,
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                // RST instruction: Push current PC to stack, then jump to a fixed address.
                // First, push the current PC (return address) onto the stack.
                // cpu.PC already points to the instruction *after* this RST.
                cpu.PushWordToStack(cpu.PC);
                // Then, jump to the specific restart vector address.
                cpu.PC = 0x0038;
            return cycles; });
        #endregion


    }


}
