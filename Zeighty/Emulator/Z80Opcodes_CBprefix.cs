using System;
using System.Collections.Generic;
using System.Text;

namespace Zeighty.Emulator;

public static partial class Z80Opcodes
{
    public static void InitialiseCBOpcodeTable(Z80Instruction[] _opcodeTable)
    {
        CBOpCodes_00_3F(_opcodeTable);
        CBOpCodes_40_7F(_opcodeTable);
        CBOpCodes_80_BF(_opcodeTable);
        CBOpCodes_C0_FF(_opcodeTable);

        var wait = false;

        // --- Important: Initialize all other 256 entries too! ---
        // For opcodes we haven't implemented yet, use a default
        // instruction that likely throws an exception
        for (int i = 0; i < 256; i++)
        {
            if (_opcodeTable[i] == null)
            {
                Console.WriteLine($"CB opcode 0x{i:X2} is unimplemented, assigning default handler.");
                wait = true;
                _opcodeTable[i] = new Z80Instruction(
                    Mnemonic: $"UNIMPLEMENTED 0x{i:X2}",
                    Opcode: (byte)i,
                    InstructionSize: 1, // Assume 1 byte for unimplemented for safety
                    TCycles: 4, // Default cycles
                    AffectsFlags: false,
                    Execute: (cpu, memory, cycles) =>
                    {
                        throw new NotSupportedException($"Attempted to execute unimplemented CB opcode: 0x{i:X2} at PC 0x{cpu.PC - 1:X4}");
                        //return cycles; 
                    });
            }
        }

        if (wait) { Console.WriteLine("Extended CB OpCode table has errors ok"); Console.ReadLine(); }
    }


    private static void CBOpCodes_00_3F(Z80Instruction[] _cbOpcodeTable)
    {
        #region 0x00-0x0F 

        _cbOpcodeTable[0x00] = new Z80Instruction(
            Mnemonic: "RLC B",
            Opcode: 0x00,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RLC(cpu.B); return cycles; });

        _cbOpcodeTable[0x01] = new Z80Instruction(
            Mnemonic: "RLC C",
            Opcode: 0x01,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RLC(cpu.C); return cycles; });

        _cbOpcodeTable[0x02] = new Z80Instruction(
            Mnemonic: "RLC D",
            Opcode: 0x02,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RLC(cpu.D); return cycles; });

        _cbOpcodeTable[0x03] = new Z80Instruction(
            Mnemonic: "RLC E",
            Opcode: 0x03,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RLC(cpu.E); return cycles; });

        _cbOpcodeTable[0x04] = new Z80Instruction(
            Mnemonic: "RLC H",
            Opcode: 0x04,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RLC(cpu.H); return cycles; });

        _cbOpcodeTable[0x05] = new Z80Instruction(
            Mnemonic: "RLC L",
            Opcode: 0x05,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RLC(cpu.L); return cycles; });

        _cbOpcodeTable[0x06] = new Z80Instruction(
            Mnemonic: "RLC [HL]",
            Opcode: 0x06,
            InstructionSize: 2,
            TCycles: 16, // Memory access takes longer
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RLC(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x07] = new Z80Instruction(
            Mnemonic: "RLC A",
            Opcode: 0x07,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RLC(cpu.A); return cycles; });

        _cbOpcodeTable[0x08] = new Z80Instruction(
            Mnemonic: "RRC B",
            Opcode: 0x08,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RRC(cpu.B); return cycles; });

        _cbOpcodeTable[0x09] = new Z80Instruction(
            Mnemonic: "RRC C",
            Opcode: 0x09,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RRC(cpu.C); return cycles; });

        _cbOpcodeTable[0x0A] = new Z80Instruction(
            Mnemonic: "RRC D",
            Opcode: 0x0A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RRC(cpu.D); return cycles; });

        _cbOpcodeTable[0x0B] = new Z80Instruction(
            Mnemonic: "RRC E",
            Opcode: 0x0B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RRC(cpu.E); return cycles; });

        _cbOpcodeTable[0x0C] = new Z80Instruction(
            Mnemonic: "RRC H",
            Opcode: 0x0C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RRC(cpu.H); return cycles; });

        _cbOpcodeTable[0x0D] = new Z80Instruction(
            Mnemonic: "RRC L",
            Opcode: 0x0D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RRC(cpu.L); return cycles; });

        _cbOpcodeTable[0x0E] = new Z80Instruction(
            Mnemonic: "RRC [HL]",
            Opcode: 0x0E,
            InstructionSize: 2,
            TCycles: 16, // Memory access takes longer
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RRC(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x0F] = new Z80Instruction(
            Mnemonic: "RRC A",
            Opcode: 0x0F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RRC(cpu.A); return cycles; });
        #endregion

        #region 0x10 - 0x1F  

        // RL r (Rotate Left through Carry)
        _cbOpcodeTable[0x10] = new Z80Instruction(
            Mnemonic: "RL B",
            Opcode: 0x10,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RL(cpu.B); return cycles; });

        _cbOpcodeTable[0x11] = new Z80Instruction(
            Mnemonic: "RL C",
            Opcode: 0x11,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RL(cpu.C); return cycles; });

        _cbOpcodeTable[0x12] = new Z80Instruction(
            Mnemonic: "RL D",
            Opcode: 0x12,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RL(cpu.D); return cycles; });

        _cbOpcodeTable[0x13] = new Z80Instruction(
            Mnemonic: "RL E",
            Opcode: 0x13,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RL(cpu.E); return cycles; });

        _cbOpcodeTable[0x14] = new Z80Instruction(
            Mnemonic: "RL H",
            Opcode: 0x14,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RL(cpu.H); return cycles; });

        _cbOpcodeTable[0x15] = new Z80Instruction(
            Mnemonic: "RL L",
            Opcode: 0x15,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RL(cpu.L); return cycles; });

        _cbOpcodeTable[0x16] = new Z80Instruction(
            Mnemonic: "RL [HL]",
            Opcode: 0x16,
            InstructionSize: 2,
            TCycles: 16, // Memory access takes longer
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RL(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x17] = new Z80Instruction(
            Mnemonic: "RL A",
            Opcode: 0x15,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RL(cpu.A); return cycles; });

        _cbOpcodeTable[0x18] = new Z80Instruction(
            Mnemonic: "RR B",
            Opcode: 0x18,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RR(cpu.B); return cycles; });

        _cbOpcodeTable[0x19] = new Z80Instruction(
            Mnemonic: "RR C",
            Opcode: 0x19,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RR(cpu.C); return cycles; });

        _cbOpcodeTable[0x1A] = new Z80Instruction(
            Mnemonic: "RR D",
            Opcode: 0x1A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RR(cpu.D); return cycles; });

        _cbOpcodeTable[0x1B] = new Z80Instruction(
            Mnemonic: "RR E",
            Opcode: 0x1B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RR(cpu.E); return cycles; });

        _cbOpcodeTable[0x1C] = new Z80Instruction(
            Mnemonic: "RR H",
            Opcode: 0x1C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RR(cpu.H); return cycles; });

        _cbOpcodeTable[0x1D] = new Z80Instruction(
            Mnemonic: "RR L",
            Opcode: 0x1D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RR(cpu.L); return cycles; });

        // RR [HL] (Rotate Right through Carry memory at HL)
        _cbOpcodeTable[0x1E] = new Z80Instruction(
            Mnemonic: "RR [HL]",
            Opcode: 0x1E,
            InstructionSize: 2,
            TCycles: 16, // Memory access takes longer
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RR(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x1F] = new Z80Instruction(
            Mnemonic: "RR A",
            Opcode: 0x1F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RR(cpu.A); return cycles; });

        #endregion

        #region 0x20 - 0x2F  

        // SLA r (Shift Left Arithmetic)
        _cbOpcodeTable[0x20] = new Z80Instruction(
            Mnemonic: "SLA B",
            Opcode: 0x20,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SLA(cpu.B); return cycles; });

        _cbOpcodeTable[0x21] = new Z80Instruction(
            Mnemonic: "SLA C",
            Opcode: 0x21,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SLA(cpu.C); return cycles; });

        _cbOpcodeTable[0x22] = new Z80Instruction(
            Mnemonic: "SLA D",
            Opcode: 0x22,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SLA(cpu.D); return cycles; });

        _cbOpcodeTable[0x23] = new Z80Instruction(
            Mnemonic: "SLA E",
            Opcode: 0x23,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SLA(cpu.E); return cycles; });

        _cbOpcodeTable[0x24] = new Z80Instruction(
            Mnemonic: "SLA H",
            Opcode: 0x24,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SLA(cpu.H); return cycles; });

        _cbOpcodeTable[0x25] = new Z80Instruction(
            Mnemonic: "SLA L",
            Opcode: 0x25,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SLA(cpu.L); return cycles; });

        // SLA [HL] (Shift Left Arithmetic memory at HL)
        _cbOpcodeTable[0x26] = new Z80Instruction(
            Mnemonic: "SLA [HL]",
            Opcode: 0x26,
            InstructionSize: 2,
            TCycles: 16,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) =>
            {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SLA(value);
                memory.WriteByte(cpu.HL, result);
                return cycles;
            }
        );

        _cbOpcodeTable[0x27] = new Z80Instruction(
            Mnemonic: "SLA A",
            Opcode: 0x27,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SLA(cpu.A); return cycles; });

        // SRA r (Shift Right Arithmetic)
        _cbOpcodeTable[0x28] = new Z80Instruction(
            Mnemonic: "SRA B",
            Opcode: 0x28,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SRA(cpu.B); return cycles; });

        _cbOpcodeTable[0x29] = new Z80Instruction(
            Mnemonic: "SRA C",
            Opcode: 0x29,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SRA(cpu.C); return cycles; });

        _cbOpcodeTable[0x2A] = new Z80Instruction(
            Mnemonic: "SRA D",
            Opcode: 0x2A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SRA(cpu.D); return cycles; });

        _cbOpcodeTable[0x2B] = new Z80Instruction(
            Mnemonic: "SRA E",
            Opcode: 0x2B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SRA(cpu.E); return cycles; });

        _cbOpcodeTable[0x2C] = new Z80Instruction(
            Mnemonic: "SRA H",
            Opcode: 0x2C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SRA(cpu.H); return cycles; });

        _cbOpcodeTable[0x2D] = new Z80Instruction(
            Mnemonic: "SRA L",
            Opcode: 0x2D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SRA(cpu.L); return cycles; });

        // SRA [HL] (Shift Right Arithmetic memory at HL)
        _cbOpcodeTable[0x2E] = new Z80Instruction(
            Mnemonic: "SRA [HL]",
            Opcode: 0x2E,
            InstructionSize: 2,
            TCycles: 16,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SRA(value);
                memory.WriteByte(cpu.HL, result);
                return cycles;
            });

        _cbOpcodeTable[0x2F] = new Z80Instruction(
            Mnemonic: "SRA A",
            Opcode: 0x2F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SRA(cpu.A); return cycles; });
        #endregion

        #region 0x30 - 0x3F  

        // SWAP r (Swap Nibbles)
        _cbOpcodeTable[0x30] = new Z80Instruction(
            Mnemonic: "SWAP B",
            Opcode: 0x30,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SWAP(cpu.B); return cycles; });

        _cbOpcodeTable[0x31] = new Z80Instruction(
            Mnemonic: "SWAP C",
            Opcode: 0x31,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SWAP(cpu.C); return cycles; });

        _cbOpcodeTable[0x32] = new Z80Instruction(
            Mnemonic: "SWAP D",
            Opcode: 0x32,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SWAP(cpu.D); return cycles; });

        _cbOpcodeTable[0x33] = new Z80Instruction(
            Mnemonic: "SWAP E",
            Opcode: 0x33,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SWAP(cpu.E); return cycles; });

        _cbOpcodeTable[0x34] = new Z80Instruction(
            Mnemonic: "SWAP H",
            Opcode: 0x34,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SWAP(cpu.H); return cycles; });

        _cbOpcodeTable[0x35] = new Z80Instruction(
            Mnemonic: "SWAP L",
            Opcode: 0x35,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SWAP(cpu.L); return cycles; });

        // SWAP [HL] (Swap Nibbles memory at HL)
        _cbOpcodeTable[0x36] = new Z80Instruction(
            Mnemonic: "SWAP [HL]",
            Opcode: 0x36,
            InstructionSize: 2,
            TCycles: 16,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SWAP(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x37] = new Z80Instruction(
            Mnemonic: "SWAP A",
            Opcode: 0x37,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SWAP(cpu.A); return cycles; });


        // SRL r (Shift Right Logical)
        _cbOpcodeTable[0x38] = new Z80Instruction(
            Mnemonic: "SRL B",
            Opcode: 0x38,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SRL(cpu.B); return cycles; });

        _cbOpcodeTable[0x39] = new Z80Instruction(
            Mnemonic: "SRL C",
            Opcode: 0x39,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SRL(cpu.C); return cycles; });

        _cbOpcodeTable[0x3A] = new Z80Instruction(
            Mnemonic: "SRL D",
            Opcode: 0x3A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SRL(cpu.D); return cycles; });

        _cbOpcodeTable[0x3B] = new Z80Instruction(
            Mnemonic: "SRL E",
            Opcode: 0x3B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SRL(cpu.E); return cycles; });

        _cbOpcodeTable[0x3C] = new Z80Instruction(
            Mnemonic: "SRL H",
            Opcode: 0x3C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SRL(cpu.H); return cycles; });

        _cbOpcodeTable[0x3D] = new Z80Instruction(
            Mnemonic: "SRL L",
            Opcode: 0x3D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SRL(cpu.L); return cycles; });


        // SRL [HL] (Shift Right Logical memory at HL)
        _cbOpcodeTable[0x3E] = new Z80Instruction(
            Mnemonic: "SRL [HL]",
            Opcode: 0x3E,
            InstructionSize: 2,
            TCycles: 16,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SRL(value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x3F] = new Z80Instruction(
            Mnemonic: "SRL A",
            Opcode: 0x3F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SRL(cpu.A); return cycles; });
        #endregion

    }

    private static void CBOpCodes_40_7F(Z80Instruction[] _cbOpcodeTable)
    {
        #region 0x40-0x4F  
        _cbOpcodeTable[0x40] = new Z80Instruction(
            Mnemonic: "BIT 0, B",
            Opcode: 0x40,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.B); return cycles; } );

        _cbOpcodeTable[0x41] = new Z80Instruction(
            Mnemonic: "BIT 0, C",
            Opcode: 0x41,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.C); return cycles; });

        _cbOpcodeTable[0x42] = new Z80Instruction(
            Mnemonic: "BIT 0, D",
            Opcode: 0x42,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.D); return cycles; });

        _cbOpcodeTable[0x43] = new Z80Instruction(
            Mnemonic: "BIT 0, E",
            Opcode: 0x43,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.E); return cycles; });

        _cbOpcodeTable[0x44] = new Z80Instruction(
            Mnemonic: "BIT 0, H",
            Opcode: 0x44,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.H); return cycles; });

        _cbOpcodeTable[0x45] = new Z80Instruction(
            Mnemonic: "BIT 0, L",
            Opcode: 0x45,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0x46)
        _cbOpcodeTable[0x46] = new Z80Instruction(
            Mnemonic: "BIT 0, [HL]",
            Opcode: 0x46,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(0, value);
                return cycles;
            });

        _cbOpcodeTable[0x47] = new Z80Instruction(
            Mnemonic: "BIT 0, A",
            Opcode: 0x47,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(0, cpu.A); return cycles; });

        _cbOpcodeTable[0x48] = new Z80Instruction(
            Mnemonic: "BIT 1, B",
            Opcode: 0x48,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.B); return cycles; });

        _cbOpcodeTable[0x49] = new Z80Instruction(
            Mnemonic: "BIT 1, C",
            Opcode: 0x49,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.C); return cycles; });

        _cbOpcodeTable[0x4A] = new Z80Instruction(
            Mnemonic: "BIT 1, D",
            Opcode: 0x4A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.D); return cycles; });

        _cbOpcodeTable[0x4B] = new Z80Instruction(
            Mnemonic: "BIT 1, E",
            Opcode: 0x4B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.E); return cycles; });

        _cbOpcodeTable[0x4C] = new Z80Instruction(
            Mnemonic: "BIT 1, H",
            Opcode: 0x4C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.H); return cycles; });

        _cbOpcodeTable[0x4D] = new Z80Instruction(
            Mnemonic: "BIT 1, L",
            Opcode: 0x4D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0x46)
        _cbOpcodeTable[0x4E] = new Z80Instruction(
            Mnemonic: "BIT 1, [HL]",
            Opcode: 0x4E,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(1, value);
            return cycles; });

        _cbOpcodeTable[0x4F] = new Z80Instruction(
            Mnemonic: "BIT 1, A",
            Opcode: 0x4F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(1, cpu.A); return cycles; });
        #endregion

        #region 0x50-0x5F  
        _cbOpcodeTable[0x50] = new Z80Instruction(
            Mnemonic: "BIT 2, B",
            Opcode: 0x50,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.B); return cycles; });

        _cbOpcodeTable[0x51] = new Z80Instruction(
            Mnemonic: "BIT 2, C",
            Opcode: 0x51,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.C); return cycles; });

        _cbOpcodeTable[0x52] = new Z80Instruction(
            Mnemonic: "BIT 2, D",
            Opcode: 0x52,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.D); return cycles; });

        _cbOpcodeTable[0x53] = new Z80Instruction(
            Mnemonic: "BIT 2, E",
            Opcode: 0x43,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.E); return cycles; });

        _cbOpcodeTable[0x54] = new Z80Instruction(
            Mnemonic: "BIT 2, H",
            Opcode: 0x54,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.H); return cycles; });

        _cbOpcodeTable[0x55] = new Z80Instruction(
            Mnemonic: "BIT 2, L",
            Opcode: 0x55,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.L); return cycles; });

        _cbOpcodeTable[0x56] = new Z80Instruction(
            Mnemonic: "BIT 2, [HL]",
            Opcode: 0x56,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(2, value);
            return cycles; });

        _cbOpcodeTable[0x57] = new Z80Instruction(
            Mnemonic: "BIT 2, A",
            Opcode: 0x57,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(2, cpu.A); return cycles; });

        _cbOpcodeTable[0x58] = new Z80Instruction(
            Mnemonic: "BIT 3, B",
            Opcode: 0x58,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.B); return cycles; });

        _cbOpcodeTable[0x59] = new Z80Instruction(
            Mnemonic: "BIT 3, C",
            Opcode: 0x59,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.C); return cycles; });

        _cbOpcodeTable[0x5A] = new Z80Instruction(
            Mnemonic: "BIT 3, D",
            Opcode: 0x5A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.D); return cycles; });

        _cbOpcodeTable[0x5B] = new Z80Instruction(
            Mnemonic: "BIT 3, E",
            Opcode: 0x5B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.E); return cycles; });

        _cbOpcodeTable[0x5C] = new Z80Instruction(
            Mnemonic: "BIT 3, H",
            Opcode: 0x5C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.H); return cycles; });

        _cbOpcodeTable[0x5D] = new Z80Instruction(
            Mnemonic: "BIT 3, L",
            Opcode: 0x5D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.L); return cycles; });

        _cbOpcodeTable[0x5E] = new Z80Instruction(
            Mnemonic: "BIT 3, [HL]",
            Opcode: 0x5E,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(3, value);
            return cycles; });

        _cbOpcodeTable[0x5F] = new Z80Instruction(
            Mnemonic: "BIT 3, A",
            Opcode: 0x5F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(3, cpu.A); return cycles; });
        #endregion

        #region 0x60-0x6F  
        _cbOpcodeTable[0x60] = new Z80Instruction(
            Mnemonic: "BIT 4, B",
            Opcode: 0x60,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.B); return cycles; });

        _cbOpcodeTable[0x61] = new Z80Instruction(
            Mnemonic: "BIT 4, C",
            Opcode: 0x61,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.C); return cycles; });

        _cbOpcodeTable[0x62] = new Z80Instruction(
            Mnemonic: "BIT 4, D",
            Opcode: 0x62,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.D); return cycles; });

        _cbOpcodeTable[0x63] = new Z80Instruction(
            Mnemonic: "BIT 4, E",
            Opcode: 0x63,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.E); return cycles; });

        _cbOpcodeTable[0x64] = new Z80Instruction(
            Mnemonic: "BIT 4, H",
            Opcode: 0x64,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.H); return cycles; });

        _cbOpcodeTable[0x65] = new Z80Instruction(
            Mnemonic: "BIT 4, L",
            Opcode: 0x65,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.L); return cycles; });

        _cbOpcodeTable[0x66] = new Z80Instruction(
            Mnemonic: "BIT 4, [HL]",
            Opcode: 0x66,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(4, value);
            return cycles; });

        _cbOpcodeTable[0x67] = new Z80Instruction(
            Mnemonic: "BIT 4, A",
            Opcode: 0x67,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(4, cpu.A); return cycles; });

        _cbOpcodeTable[0x68] = new Z80Instruction(
            Mnemonic: "BIT 5, B",
            Opcode: 0x68,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.B); return cycles; });

        _cbOpcodeTable[0x69] = new Z80Instruction(
            Mnemonic: "BIT 5, C",
            Opcode: 0x69,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.C); return cycles; });

        _cbOpcodeTable[0x6A] = new Z80Instruction(
            Mnemonic: "BIT 5, D",
            Opcode: 0x6A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.D); return cycles; });

        _cbOpcodeTable[0x6B] = new Z80Instruction(
            Mnemonic: "BIT 5, E",
            Opcode: 0x6B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.E); return cycles; });

        _cbOpcodeTable[0x6C] = new Z80Instruction(
            Mnemonic: "BIT 5, H",
            Opcode: 0x6C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.H); return cycles; });

        _cbOpcodeTable[0x6D] = new Z80Instruction(
            Mnemonic: "BIT 5, L",
            Opcode: 0x6D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.L); return cycles; });

        _cbOpcodeTable[0x6E] = new Z80Instruction(
            Mnemonic: "BIT 5, [HL]",
            Opcode: 0x6E,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(5, value);
            return cycles; });

        _cbOpcodeTable[0x6F] = new Z80Instruction(
            Mnemonic: "BIT 5, A",
            Opcode: 0x6F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(5, cpu.A); return cycles; });
        #endregion

        #region 0x70-0x7F
        
        _cbOpcodeTable[0x70] = new Z80Instruction(
            Mnemonic: "BIT 6, B",
            Opcode: 0x70,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.B); return cycles; });

        _cbOpcodeTable[0x71] = new Z80Instruction(
            Mnemonic: "BIT 6, C",
            Opcode: 0x71,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.C); return cycles; });

        _cbOpcodeTable[0x72] = new Z80Instruction(
            Mnemonic: "BIT 6, D",
            Opcode: 0x72,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.D); return cycles; });

        _cbOpcodeTable[0x73] = new Z80Instruction(
            Mnemonic: "BIT 6, E",
            Opcode: 0x73,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.E); return cycles; });

        _cbOpcodeTable[0x74] = new Z80Instruction(
            Mnemonic: "BIT 6, H",
            Opcode: 0x74,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.H); return cycles; });

        _cbOpcodeTable[0x75] = new Z80Instruction(
            Mnemonic: "BIT 6, L",
            Opcode: 0x75,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.L); return cycles; });

        _cbOpcodeTable[0x76] = new Z80Instruction(
            Mnemonic: "BIT 6, [HL]",
            Opcode: 0x76,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(6, value);
            return cycles; });

        _cbOpcodeTable[0x77] = new Z80Instruction(
            Mnemonic: "BIT 6, A",
            Opcode: 0x77,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(6, cpu.A); return cycles; });

        _cbOpcodeTable[0x78] = new Z80Instruction(
            Mnemonic: "BIT 7, B",
            Opcode: 0x78,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.B); return cycles; });

        _cbOpcodeTable[0x79] = new Z80Instruction(
            Mnemonic: "BIT 7, C",
            Opcode: 0x79,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.C); return cycles; });

        _cbOpcodeTable[0x7A] = new Z80Instruction(
            Mnemonic: "BIT 7, D",
            Opcode: 0x7A,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.D); return cycles; });

        _cbOpcodeTable[0x7B] = new Z80Instruction(
            Mnemonic: "BIT 7, E",
            Opcode: 0x7B,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.E); return cycles; });

        _cbOpcodeTable[0x7C] = new Z80Instruction(
            Mnemonic: "BIT 7, H",
            Opcode: 0x7C,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.H); return cycles; });

        _cbOpcodeTable[0x7D] = new Z80Instruction(
            Mnemonic: "BIT 7, L",
            Opcode: 0x7D,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.L); return cycles; });

        _cbOpcodeTable[0x7E] = new Z80Instruction(
            Mnemonic: "BIT 7, [HL]",
            Opcode: 0x7E,
            InstructionSize: 2,
            TCycles: 12, // Memory read for BIT [HL] takes 12 cycles, not 16.
            AffectsFlags: true,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                cpu.BIT(7, value);
            return cycles; });

        _cbOpcodeTable[0x7F] = new Z80Instruction(
            Mnemonic: "BIT 7, A",
            Opcode: 0x7F,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: true, // Specifically Z, N, H flags
            Execute: (cpu, memory, cycles) => { cpu.BIT(7, cpu.A); return cycles; });
        #endregion
    }

    private static void CBOpCodes_80_BF(Z80Instruction[] _cbOpcodeTable)
    {
        #region 0x80-0x8F  
        _cbOpcodeTable[0x80] = new Z80Instruction(
            Mnemonic: "RES 0, B",
            Opcode: 0x80,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(0, cpu.B); return cycles; });

        _cbOpcodeTable[0x81] = new Z80Instruction(
            Mnemonic: "RES 0, C",
            Opcode: 0x81,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(0, cpu.C); return cycles; });

        _cbOpcodeTable[0x82] = new Z80Instruction(
            Mnemonic: "RES 0, D",
            Opcode: 0x82,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(0, cpu.D); return cycles; });

        _cbOpcodeTable[0x83] = new Z80Instruction(
            Mnemonic: "RES 0, E",
            Opcode: 0x83,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(0, cpu.E); return cycles; });

        _cbOpcodeTable[0x84] = new Z80Instruction(
            Mnemonic: "RES 0, H",
            Opcode: 0x84,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(0, cpu.H); return cycles; });

        _cbOpcodeTable[0x85] = new Z80Instruction(
            Mnemonic: "RES 0, L",
            Opcode: 0x85,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(0, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0x86)
        _cbOpcodeTable[0x86] = new Z80Instruction(
            Mnemonic: "RES 0, [HL]",
            Opcode: 0x86,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(0, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x87] = new Z80Instruction(
            Mnemonic: "RES 0, A",
            Opcode: 0x87,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(0, cpu.A); return cycles; });

        _cbOpcodeTable[0x88] = new Z80Instruction(
            Mnemonic: "RES 1, B",
            Opcode: 0x88,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(1, cpu.B); return cycles; });

        _cbOpcodeTable[0x89] = new Z80Instruction(
            Mnemonic: "RES 1, C",
            Opcode: 0x89,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(1, cpu.C); return cycles; });

        _cbOpcodeTable[0x8A] = new Z80Instruction(
            Mnemonic: "RES 1, D",
            Opcode: 0x8A,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(1, cpu.D); return cycles; });

        _cbOpcodeTable[0x8B] = new Z80Instruction(
            Mnemonic: "RES 1, E",
            Opcode: 0x8B,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(1, cpu.E); return cycles; });

        _cbOpcodeTable[0x8C] = new Z80Instruction(
            Mnemonic: "RES 1, H",
            Opcode: 0x8C,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(1, cpu.H); return cycles; });

        _cbOpcodeTable[0x8D] = new Z80Instruction(
            Mnemonic: "RES 1, L",
            Opcode: 0x8D,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(1, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0x86)
        _cbOpcodeTable[0x8E] = new Z80Instruction(
            Mnemonic: "RES 1, [HL]",
            Opcode: 0x8E,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(1, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x8F] = new Z80Instruction(
            Mnemonic: "RES 1, A",
            Opcode: 0x8F,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(1, cpu.A); return cycles; });
        #endregion

        #region 0x90-0x9F  
        _cbOpcodeTable[0x90] = new Z80Instruction(
            Mnemonic: "RES 2, B",
            Opcode: 0x90,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(2, cpu.B); return cycles; });

        _cbOpcodeTable[0x91] = new Z80Instruction(
            Mnemonic: "RES 2, C",
            Opcode: 0x91,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(2, cpu.C); return cycles; });

        _cbOpcodeTable[0x92] = new Z80Instruction(
            Mnemonic: "RES 2, D",
            Opcode: 0x92,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(2, cpu.D); return cycles; });

        _cbOpcodeTable[0x93] = new Z80Instruction(
            Mnemonic: "RES 2, E",
            Opcode: 0x83,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(2, cpu.E); return cycles; });

        _cbOpcodeTable[0x94] = new Z80Instruction(
            Mnemonic: "RES 2, H",
            Opcode: 0x94,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(2, cpu.H); return cycles; });

        _cbOpcodeTable[0x95] = new Z80Instruction(
            Mnemonic: "RES 2, L",
            Opcode: 0x95,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(2, cpu.L); return cycles; });

        _cbOpcodeTable[0x96] = new Z80Instruction(
            Mnemonic: "RES 2, [HL]",
            Opcode: 0x96,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(2, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x97] = new Z80Instruction(
            Mnemonic: "RES 2, A",
            Opcode: 0x97,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(2, cpu.A); return cycles; });

        _cbOpcodeTable[0x98] = new Z80Instruction(
            Mnemonic: "RES 3, B",
            Opcode: 0x98,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(3, cpu.B); return cycles; });

        _cbOpcodeTable[0x99] = new Z80Instruction(
            Mnemonic: "RES 3, C",
            Opcode: 0x99,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(3, cpu.C); return cycles; });

        _cbOpcodeTable[0x9A] = new Z80Instruction(
            Mnemonic: "RES 3, D",
            Opcode: 0x9A,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(3, cpu.D); return cycles; });

        _cbOpcodeTable[0x9B] = new Z80Instruction(
            Mnemonic: "RES 3, E",
            Opcode: 0x9B,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(3, cpu.E); return cycles; });

        _cbOpcodeTable[0x9C] = new Z80Instruction(
            Mnemonic: "RES 3, H",
            Opcode: 0x9C,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(3, cpu.H); return cycles; });

        _cbOpcodeTable[0x9D] = new Z80Instruction(
            Mnemonic: "RES 3, L",
            Opcode: 0x9D,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(3, cpu.L); return cycles; });

        _cbOpcodeTable[0x9E] = new Z80Instruction(
            Mnemonic: "RES 3, [HL]",
            Opcode: 0x9E,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(3, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0x9F] = new Z80Instruction(
            Mnemonic: "RES 3, A",
            Opcode: 0x9F,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(3, cpu.A); return cycles; });
        #endregion

        #region 0xA0-0xAF  
        _cbOpcodeTable[0xA0] = new Z80Instruction(
            Mnemonic: "RES 4, B",
            Opcode: 0xA0,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(4, cpu.B); return cycles; });

        _cbOpcodeTable[0xA1] = new Z80Instruction(
            Mnemonic: "RES 4, C",
            Opcode: 0xA1,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(4, cpu.C); return cycles; });

        _cbOpcodeTable[0xA2] = new Z80Instruction(
            Mnemonic: "RES 4, D",
            Opcode: 0xA2,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(4, cpu.D); return cycles; });

        _cbOpcodeTable[0xA3] = new Z80Instruction(
            Mnemonic: "RES 4, E",
            Opcode: 0xA3,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(4, cpu.E); return cycles; });

        _cbOpcodeTable[0xA4] = new Z80Instruction(
            Mnemonic: "RES 4, H",
            Opcode: 0xA4,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(4, cpu.H); return cycles; });

        _cbOpcodeTable[0xA5] = new Z80Instruction(
            Mnemonic: "RES 4, L",
            Opcode: 0xA5,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(4, cpu.L); return cycles; });

        _cbOpcodeTable[0xA6] = new Z80Instruction(
            Mnemonic: "RES 4, [HL]",
            Opcode: 0xA6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(4, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xA7] = new Z80Instruction(
            Mnemonic: "RES 4, A",
            Opcode: 0xA7,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(4, cpu.A); return cycles; });

        _cbOpcodeTable[0xA8] = new Z80Instruction(
            Mnemonic: "RES 5, B",
            Opcode: 0xA8,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(5, cpu.B); return cycles; });

        _cbOpcodeTable[0xA9] = new Z80Instruction(
            Mnemonic: "RES 5, C",
            Opcode: 0xA9,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(5, cpu.C); return cycles; });

        _cbOpcodeTable[0xAA] = new Z80Instruction(
            Mnemonic: "RES 5, D",
            Opcode: 0xAA,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(5, cpu.D); return cycles; });

        _cbOpcodeTable[0xAB] = new Z80Instruction(
            Mnemonic: "RES 5, E",
            Opcode: 0xAB,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(5, cpu.E); return cycles; });

        _cbOpcodeTable[0xAC] = new Z80Instruction(
            Mnemonic: "RES 5, H",
            Opcode: 0xAC,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(5, cpu.H); return cycles; });

        _cbOpcodeTable[0xAD] = new Z80Instruction(
            Mnemonic: "RES 5, L",
            Opcode: 0xAD,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(5, cpu.L); return cycles; });

        _cbOpcodeTable[0xAE] = new Z80Instruction(
            Mnemonic: "RES 5, [HL]",
            Opcode: 0xAE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(5, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xAF] = new Z80Instruction(
            Mnemonic: "RES 5, A",
            Opcode: 0xAF,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(5, cpu.A); return cycles; });
        #endregion


        #region 0xB0-0xBF

        _cbOpcodeTable[0xB0] = new Z80Instruction(
            Mnemonic: "RES 6, B",
            Opcode: 0xB0,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(6, cpu.B); return cycles; });

        _cbOpcodeTable[0xB1] = new Z80Instruction(
            Mnemonic: "RES 6, C",
            Opcode: 0xB1,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(6, cpu.C); return cycles; });

        _cbOpcodeTable[0xB2] = new Z80Instruction(
            Mnemonic: "RES 6, D",
            Opcode: 0xB2,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(6, cpu.D); return cycles; });

        _cbOpcodeTable[0xB3] = new Z80Instruction(
            Mnemonic: "RES 6, E",
            Opcode: 0xB3,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(6, cpu.E); return cycles; });

        _cbOpcodeTable[0xB4] = new Z80Instruction(
            Mnemonic: "RES 6, H",
            Opcode: 0xB4,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(6, cpu.H); return cycles; });

        _cbOpcodeTable[0xB5] = new Z80Instruction(
            Mnemonic: "RES 6, L",
            Opcode: 0xB5,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(6, cpu.L); return cycles; });

        _cbOpcodeTable[0xB6] = new Z80Instruction(
            Mnemonic: "RES 6, [HL]",
            Opcode: 0xB6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(6, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xB7] = new Z80Instruction(
            Mnemonic: "RES 6, A",
            Opcode: 0xB7,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(6, cpu.A); return cycles; });

        _cbOpcodeTable[0xB8] = new Z80Instruction(
            Mnemonic: "RES 7, B",
            Opcode: 0xB8,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.RES(7, cpu.B); return cycles; });

        _cbOpcodeTable[0xB9] = new Z80Instruction(
            Mnemonic: "RES 7, C",
            Opcode: 0xB9,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.RES(7, cpu.C); return cycles; });

        _cbOpcodeTable[0xBA] = new Z80Instruction(
            Mnemonic: "RES 7, D",
            Opcode: 0xBA,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.RES(7, cpu.D); return cycles; });

        _cbOpcodeTable[0xBB] = new Z80Instruction(
            Mnemonic: "RES 7, E",
            Opcode: 0xBB,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.RES(7, cpu.E); return cycles; });

        _cbOpcodeTable[0xBC] = new Z80Instruction(
            Mnemonic: "RES 7, H",
            Opcode: 0xBC,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.RES(7, cpu.H); return cycles; });

        _cbOpcodeTable[0xBD] = new Z80Instruction(
            Mnemonic: "RES 7, L",
            Opcode: 0xBD,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.RES(7, cpu.L); return cycles; });

        _cbOpcodeTable[0xBE] = new Z80Instruction(
            Mnemonic: "RES 7, [HL]",
            Opcode: 0xBE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.RES(7, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xBF] = new Z80Instruction(
            Mnemonic: "RES 7, A",
            Opcode: 0xBF,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.RES(7, cpu.A); return cycles; });
        #endregion


    }

    private static void CBOpCodes_C0_FF(Z80Instruction[] _cbOpcodeTable)
    {
        #region 0xC0-0xCF  
        _cbOpcodeTable[0xC0] = new Z80Instruction(
            Mnemonic: "SET 0, B",
            Opcode: 0xC0,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(0, cpu.B); return cycles; });

        _cbOpcodeTable[0xC1] = new Z80Instruction(
            Mnemonic: "SET 0, C",
            Opcode: 0xC1,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(0, cpu.C); return cycles; });

        _cbOpcodeTable[0xC2] = new Z80Instruction(
            Mnemonic: "SET 0, D",
            Opcode: 0xC2,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(0, cpu.D); return cycles; });

        _cbOpcodeTable[0xC3] = new Z80Instruction(
            Mnemonic: "SET 0, E",
            Opcode: 0xC3,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(0, cpu.E); return cycles; });

        _cbOpcodeTable[0xC4] = new Z80Instruction(
            Mnemonic: "SET 0, H",
            Opcode: 0xC4,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(0, cpu.H); return cycles; });

        _cbOpcodeTable[0xC5] = new Z80Instruction(
            Mnemonic: "SET 0, L",
            Opcode: 0xC5,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(0, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0xC6)
        _cbOpcodeTable[0xC6] = new Z80Instruction(
            Mnemonic: "SET 0, [HL]",
            Opcode: 0xC6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(0, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xC7] = new Z80Instruction(
            Mnemonic: "SET 0, A",
            Opcode: 0xC7,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(0, cpu.A); return cycles; });

        _cbOpcodeTable[0xC8] = new Z80Instruction(
            Mnemonic: "SET 1, B",
            Opcode: 0xC8,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(1, cpu.B); return cycles; });

        _cbOpcodeTable[0xC9] = new Z80Instruction(
            Mnemonic: "SET 1, C",
            Opcode: 0xC9,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(1, cpu.C); return cycles; });

        _cbOpcodeTable[0xCA] = new Z80Instruction(
            Mnemonic: "SET 1, D",
            Opcode: 0xCA,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(1, cpu.D); return cycles; });

        _cbOpcodeTable[0xCB] = new Z80Instruction(
            Mnemonic: "SET 1, E",
            Opcode: 0xCB,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(1, cpu.E); return cycles; });

        _cbOpcodeTable[0xCC] = new Z80Instruction(
            Mnemonic: "SET 1, H",
            Opcode: 0xCC,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(1, cpu.H); return cycles; });

        _cbOpcodeTable[0xCD] = new Z80Instruction(
            Mnemonic: "SET 1, L",
            Opcode: 0xCD,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(1, cpu.L); return cycles; });

        // BIT 0, [HL] (Opcode 0xC6)
        _cbOpcodeTable[0xCE] = new Z80Instruction(
            Mnemonic: "SET 1, [HL]",
            Opcode: 0xCE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(1, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xCF] = new Z80Instruction(
            Mnemonic: "SET 1, A",
            Opcode: 0xCF,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(1, cpu.A); return cycles; });
        #endregion

        #region 0xD0-0xDF  
        _cbOpcodeTable[0xD0] = new Z80Instruction(
            Mnemonic: "SET 2, B",
            Opcode: 0xD0,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(2, cpu.B); return cycles; });

        _cbOpcodeTable[0xD1] = new Z80Instruction(
            Mnemonic: "SET 2, C",
            Opcode: 0xD1,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(2, cpu.C); return cycles; });

        _cbOpcodeTable[0xD2] = new Z80Instruction(
            Mnemonic: "SET 2, D",
            Opcode: 0xD2,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(2, cpu.D); return cycles; });

        _cbOpcodeTable[0xD3] = new Z80Instruction(
            Mnemonic: "SET 2, E",
            Opcode: 0xC3,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(2, cpu.E); return cycles; });

        _cbOpcodeTable[0xD4] = new Z80Instruction(
            Mnemonic: "SET 2, H",
            Opcode: 0xD4,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(2, cpu.H); return cycles; });

        _cbOpcodeTable[0xD5] = new Z80Instruction(
            Mnemonic: "SET 2, L",
            Opcode: 0xD5,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(2, cpu.L); return cycles; });

        _cbOpcodeTable[0xD6] = new Z80Instruction(
            Mnemonic: "SET 2, [HL]",
            Opcode: 0xD6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(2, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xD7] = new Z80Instruction(
            Mnemonic: "SET 2, A",
            Opcode: 0xD7,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(2, cpu.A); return cycles; });

        _cbOpcodeTable[0xD8] = new Z80Instruction(
            Mnemonic: "SET 3, B",
            Opcode: 0xD8,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(3, cpu.B); return cycles; });

        _cbOpcodeTable[0xD9] = new Z80Instruction(
            Mnemonic: "SET 3, C",
            Opcode: 0xD9,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(3, cpu.C); return cycles; });

        _cbOpcodeTable[0xDA] = new Z80Instruction(
            Mnemonic: "SET 3, D",
            Opcode: 0xDA,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(3, cpu.D); return cycles; });

        _cbOpcodeTable[0xDB] = new Z80Instruction(
            Mnemonic: "SET 3, E",
            Opcode: 0xDB,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(3, cpu.E); return cycles; });

        _cbOpcodeTable[0xDC] = new Z80Instruction(
            Mnemonic: "SET 3, H",
            Opcode: 0xDC,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(3, cpu.H); return cycles; });

        _cbOpcodeTable[0xDD] = new Z80Instruction(
            Mnemonic: "SET 3, L",
            Opcode: 0xDD,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(3, cpu.L); return cycles; });

        _cbOpcodeTable[0xDE] = new Z80Instruction(
            Mnemonic: "SET 3, [HL]",
            Opcode: 0xDE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(3, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xDF] = new Z80Instruction(
            Mnemonic: "SET 3, A",
            Opcode: 0xDF,
            InstructionSize: 2,
            TCycles: 8,
            AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(3, cpu.A); return cycles; });
        #endregion

        #region 0xE0-0xEF  
        _cbOpcodeTable[0xE0] = new Z80Instruction(
            Mnemonic: "SET 4, B",
            Opcode: 0xE0,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(4, cpu.B); return cycles; });

        _cbOpcodeTable[0xE1] = new Z80Instruction(
            Mnemonic: "SET 4, C",
            Opcode: 0xE1,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(4, cpu.C); return cycles; });

        _cbOpcodeTable[0xE2] = new Z80Instruction(
            Mnemonic: "SET 4, D",
            Opcode: 0xE2,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(4, cpu.D); return cycles; });

        _cbOpcodeTable[0xE3] = new Z80Instruction(
            Mnemonic: "SET 4, E",
            Opcode: 0xE3,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(4, cpu.E); return cycles; });

        _cbOpcodeTable[0xE4] = new Z80Instruction(
            Mnemonic: "SET 4, H",
            Opcode: 0xE4,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(4, cpu.H); return cycles; });

        _cbOpcodeTable[0xE5] = new Z80Instruction(
            Mnemonic: "SET 4, L",
            Opcode: 0xE5,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(4, cpu.L); return cycles; });

        _cbOpcodeTable[0xE6] = new Z80Instruction(
            Mnemonic: "SET 4, [HL]",
            Opcode: 0xE6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(4, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xE7] = new Z80Instruction(
            Mnemonic: "SET 4, A",
            Opcode: 0xE7,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(4, cpu.A); return cycles; });

        _cbOpcodeTable[0xE8] = new Z80Instruction(
            Mnemonic: "SET 5, B",
            Opcode: 0xE8,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(5, cpu.B); return cycles; });

        _cbOpcodeTable[0xE9] = new Z80Instruction(
            Mnemonic: "SET 5, C",
            Opcode: 0xE9,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(5, cpu.C); return cycles; });

        _cbOpcodeTable[0xEA] = new Z80Instruction(
            Mnemonic: "SET 5, D",
            Opcode: 0xEA,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(5, cpu.D); return cycles; });

        _cbOpcodeTable[0xEB] = new Z80Instruction(
            Mnemonic: "SET 5, E",
            Opcode: 0xEB,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(5, cpu.E); return cycles; });

        _cbOpcodeTable[0xEC] = new Z80Instruction(
            Mnemonic: "SET 5, H",
            Opcode: 0xEC,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(5, cpu.H); return cycles; });

        _cbOpcodeTable[0xED] = new Z80Instruction(
            Mnemonic: "SET 5, L",
            Opcode: 0xED,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(5, cpu.L); return cycles; });

        _cbOpcodeTable[0xEE] = new Z80Instruction(
            Mnemonic: "SET 5, [HL]",
            Opcode: 0xEE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(5, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xEF] = new Z80Instruction(
            Mnemonic: "SET 5, A",
            Opcode: 0xEF,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(5, cpu.A); return cycles; });
        #endregion


        #region 0xF0-0xFF

        _cbOpcodeTable[0xF0] = new Z80Instruction(
            Mnemonic: "SET 6, B",
            Opcode: 0xF0,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(6, cpu.B); return cycles; });

        _cbOpcodeTable[0xF1] = new Z80Instruction(
            Mnemonic: "SET 6, C",
            Opcode: 0xF1,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(6, cpu.C); return cycles; });

        _cbOpcodeTable[0xF2] = new Z80Instruction(
            Mnemonic: "SET 6, D",
            Opcode: 0xF2,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(6, cpu.D); return cycles; });

        _cbOpcodeTable[0xF3] = new Z80Instruction(
            Mnemonic: "SET 6, E",
            Opcode: 0xF3,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(6, cpu.E); return cycles; });

        _cbOpcodeTable[0xF4] = new Z80Instruction(
            Mnemonic: "SET 6, H",
            Opcode: 0xF4,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(6, cpu.H); return cycles; });

        _cbOpcodeTable[0xF5] = new Z80Instruction(
            Mnemonic: "SET 6, L",
            Opcode: 0xF5,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(6, cpu.L); return cycles; });

        _cbOpcodeTable[0xF6] = new Z80Instruction(
            Mnemonic: "SET 6, [HL]",
            Opcode: 0xF6,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(6, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xF7] = new Z80Instruction(
            Mnemonic: "SET 6, A",
            Opcode: 0xF7,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(6, cpu.A); return cycles; });

        _cbOpcodeTable[0xF8] = new Z80Instruction(
            Mnemonic: "SET 7, B",
            Opcode: 0xF8,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.B = cpu.SET(7, cpu.B); return cycles;  });

        _cbOpcodeTable[0xF9] = new Z80Instruction(
            Mnemonic: "SET 7, C",
            Opcode: 0xF9,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.C = cpu.SET(7, cpu.C); return cycles; });

        _cbOpcodeTable[0xFA] = new Z80Instruction(
            Mnemonic: "SET 7, D",
            Opcode: 0xFA,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.D = cpu.SET(7, cpu.D); return cycles; });

        _cbOpcodeTable[0xFB] = new Z80Instruction(
            Mnemonic: "SET 7, E",
            Opcode: 0xFB,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.E = cpu.SET(7, cpu.E); return cycles; });

        _cbOpcodeTable[0xFC] = new Z80Instruction(
            Mnemonic: "SET 7, H",
            Opcode: 0xFC,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.H = cpu.SET(7, cpu.H); return cycles; });

        _cbOpcodeTable[0xFD] = new Z80Instruction(
            Mnemonic: "SET 7, L",
            Opcode: 0xFD,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.L = cpu.SET(7, cpu.L); return cycles; });

        _cbOpcodeTable[0xFE] = new Z80Instruction(
            Mnemonic: "SET 7, [HL]",
            Opcode: 0xFE,
            InstructionSize: 2,
            TCycles: 16, // Memory read/write for RES [HL] takes 16 cycles
            AffectsFlags: false,
            Execute: (cpu, memory, cycles) => {
                byte value = memory.ReadByte(cpu.HL);
                byte result = cpu.SET(7, value);
                memory.WriteByte(cpu.HL, result);
            return cycles; });

        _cbOpcodeTable[0xFF] = new Z80Instruction(
            Mnemonic: "SET 7, A",
            Opcode: 0xFF,
            InstructionSize: 2,
            TCycles: 8,
AffectsFlags: false, // RES doesn't affect flags
            Execute: (cpu, memory, cycles) => { cpu.A = cpu.SET(7, cpu.A); return cycles; });
        #endregion


    }
}