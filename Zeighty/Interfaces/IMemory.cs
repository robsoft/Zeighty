using System;
using System.Collections.Generic;
using System.Text;

namespace Zeighty.Interfaces;

public interface IMemory
{
    byte IE { get; set; }
    byte IF { get; set; }

    byte ReadByte(ushort address);
    void WriteByte(ushort address, byte value);

    ushort ReadUWord(ushort address);
    void WriteUWord(ushort address, ushort value);
    uint ReadDoubleUWord(ushort address);
    void WriteDoubleUWord(ushort address, uint value);
    int ReadDoubleWord(ushort address);
    void WriteDoubleWord(ushort address, int value);
}

