using System;
using System.Collections.Generic;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Emulator;

// Define a delegate type for an instruction's execution logic.
// It takes a GameBoyCpu instance (the 'this' for the CPU)
// and an IGameBoyMemory instance (for memory access) as parameters.
public delegate void CpuInstructionExecute(ICpu cpu, IMemory memory);

public class Z80Instruction : IInstruction
{
    private byte _opcode;
    private string _mnemonic = "";
    private ushort _tCycles = 1;
    private ushort _instructionSize = 1;
    private bool _affectsFlags = false;

    public Func<ICpu, IMemory, int, int> Execute { get; init; } // receives baseTCycles, returns actual TCycles

    public byte Opcode => _opcode;

    public string Mnemonic => _mnemonic;

    public ushort TCycles => _tCycles;

    public ushort InstructionSize => _instructionSize;

    public bool AffectsFlags => _affectsFlags;

    //public CpuInstructionExecute Execute => _execute;


    public Z80Instruction (string Mnemonic, byte Opcode, ushort TCycles, ushort InstructionSize, bool AffectsFlags,
        Func<ICpu, IMemory, int, int> Execute) //CpuInstructionExecute Execute)
    {
        _mnemonic = Mnemonic;
        _opcode = Opcode;
        _tCycles = TCycles;
        _instructionSize = InstructionSize;
        _affectsFlags = AffectsFlags;
        //_execute = Execute;
        this.Execute = Execute;
    }
}
