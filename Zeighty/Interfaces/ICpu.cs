using System;
using System.Collections.Generic;
using System.Text;
using Zeighty.Emulator;

namespace Zeighty.Interfaces;

public interface ICpu
{
    bool IsHalted { get; set; }
    long CyclesThisFrame { get; set; } // Cycles accumulated in the current emulated frame
    long TotalCycles { get; set; }     // Total cycles since the CPU was reset/booted

    DecodedInstruction[] Instructions { get; }
 
    IMemory Memory { get; }

    byte A { get; set; }
    byte F { get; set; }
    byte B { get; set; }
    byte C { get; set; }
    byte D { get; set; }
    byte E { get; set; }
    byte H { get; set; }
    byte L { get; set; }

    ushort AF { get; set; }
    ushort BC { get; set; }
    ushort DE { get; set; }
    ushort HL { get; set; }

    ushort PC { get; set; }
    ushort SP { get; set; }

    bool GetFlagZ();
    void SetFlagZ(bool value);

    bool GetFlagN();
    void SetFlagN(bool value);

    bool GetFlagH();
    void SetFlagH(bool value);

    bool GetFlagC();
    void SetFlagC(bool value);

    bool IME { get; set; } // Interrupt Master Enable flag
    bool EnableInterruptsPending { get; set; }
    void EnableInterrupts();
    void DisableInterrupts();

    ushort PopWordFromStack();
    void PushWordToStack(ushort value);

    void Reset();
    void ExecuteInstruction();
    void FetchInstructions();

    byte RLC(byte value);
    byte RRC(byte value);
    byte RL(byte value);
    byte RR(byte value);
    byte SLA(byte value);
    byte SRA(byte value);
    byte SWAP(byte value);
    byte SRL(byte value);

    // bitIndex should be 0-7
    void BIT(byte bitIndex, byte value);
    byte RES(byte bitIndex, byte value);
    byte SET(byte bitIndex, byte value);
}
