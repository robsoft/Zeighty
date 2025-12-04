using System;
using System.Collections.Generic;
using System.Text;

namespace Zeighty.Interfaces;

public interface IInstruction
{
    byte Opcode { get; }
    string Mnemonic { get; }
    ushort TCycles { get; }
    ushort InstructionSize { get; }
    bool AffectsFlags { get; }

}
