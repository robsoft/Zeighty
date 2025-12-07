using System;
using System.Collections.Generic;
using System.Text;

namespace Zeighty.Interfaces;

public enum Mode { None, Hidden, Debug, AddressEntry, FileLoad, FileSave, Settings };

public interface IDebugState
{

    public IDebugMemory Memory { get; }
    public string LoadedFileName { get; set; }
    public ushort MemoryAddress { get; set; }
    public ushort VRAMAddress { get; set; }
    public ushort VRAMWidth { get; set; }
    public bool IOVisible { get; set; }
    public bool IsRunning { get; set; }
    public bool NextStep { get; set; }
    public bool SingleStep { get; set; }
    public bool NeedReset { get; set; }
    public bool InBreakpoint { get; set; }
    public Mode Mode { get; set; }

    public void Reset();
}
