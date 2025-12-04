using System.Collections.Generic;

namespace Zeighty.Interfaces;

public enum BreakpointType
{
    None,
    Normal,
    Trigger
};


public class DebugMemoryEntry
{
    public ushort Address { get; set; }
    public byte TriggerValue { get; set; } = 0;
    public string Description { get; set; } = "";
    public BreakpointType BreakpointType { get; set; } = BreakpointType.None;
    public bool Watch { get; set; } = false;

    public DebugMemoryEntry(ushort address, string description = "")
    {
        Address = address;
        Description = (string.IsNullOrEmpty(description) ? $"${address:X4}" : description);
    }
}


public interface IDebugMemory
{
    List<DebugMemoryEntry> Entries { get; set; }
    DebugMemoryEntry? GetEntryByName(string name);
    DebugMemoryEntry? GetEntry(ushort address);
    void RemoveEntry(ushort address);
    void Clear();
    bool AddEntry(ushort address, string description = "", BreakpointType type = BreakpointType.None);
    string GetAddressDescription(ushort address);
}