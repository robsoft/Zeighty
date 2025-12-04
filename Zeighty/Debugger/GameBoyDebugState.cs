using System;
using System.Collections.Generic;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class GameBoyDebugState : IDebugState
{
    private bool isRunning = false;
    public ushort VRAMAddress { get; set; } = 0x8000;
    public string LoadedFileName { get; set; } = "(no file)";
    public ushort MemoryAddress { get; set; } = 0xC000;
    public ushort MemorySize { get; set; } = 0;
    public bool IsRunning { get => isRunning; set { if (isRunning != value) { value = isRunning; } } }
    public bool NeedReset { get; set; } = false;
    public bool NextStep { get; set; } = false;
    public bool InBreakpoint { get; set; } = false;

    public bool IOVisible { get; set; } = true;
    public bool SingleStep { get; set; } = true;
    public ushort VRAMWidth { get; set; } = 16;

    public IDebugMemory Memory { get; private set; } = new GameBoyDebugMemory();
    public void Reset()
    {
        MemoryAddress = 0xC000;
        VRAMAddress = 0x8000;
        VRAMWidth = 16;
        IsRunning = false;
        SingleStep = true;
        NeedReset = false;
        NextStep = false;
        InBreakpoint = false;
    }

    public GameBoyDebugState()
    {
        Memory.Clear();
        //LoadedFileName = "(no file)";
        IOVisible = true;
        Reset();
    }


}

