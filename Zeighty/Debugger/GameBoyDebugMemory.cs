using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class GameBoyDebugMemory : IDebugMemory
{
    public List<DebugMemoryEntry> Entries { get; set; } = new List<DebugMemoryEntry>();

    List<DebugMemoryEntry> IDebugMemory.Entries { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public DebugMemoryEntry? GetEntryByName(string name)
    {
        return Entries.FirstOrDefault(entry => entry.Description == name);
    }

    public DebugMemoryEntry? GetEntry(ushort address)
    {
        return Entries.FirstOrDefault(entry => entry.Address == address);
    }

    public void RemoveEntry(ushort address)
    {
        var entry = GetEntry(address);
        if (entry != null)
        {
            Entries.Remove(entry);
        }
    }

    public void Clear()
    {
        Entries.Clear();
    }

    public string GetAddressDescription(ushort address)
    {
        var entry = GetEntry(address);
        return entry != null ? entry.Description : "";
    }

    public bool AddEntry(ushort address, string description = "", BreakpointType type = BreakpointType.None)
    {
        if (GetEntry(address) != null)
        {
            return false;
        }

        if (GetEntryByName(description) != null) {
            return false;
        }

        Entries.Add(new DebugMemoryEntry(address, description)
        {
            BreakpointType = type
        });
        return true;
    }
}
