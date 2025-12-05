using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeighty.Emulator;

public static class GameBoyHelpers
{
    public static byte[] FakeCartridge(byte[] romData, ushort offset = 0x0000)
    {
        // Fake the cartridge by padding the ROM data
        var paddedRom = new byte[GameBoyHardware.VRAM_StartAddr];
        Array.Copy(romData, 0, paddedRom, offset, romData.Length);
        return paddedRom;
    }
}
