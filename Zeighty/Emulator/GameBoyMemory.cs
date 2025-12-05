using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Emulator;

public class GameBoyMemory : IMemory
{

    //private byte[] _rom;
    private byte[] _cartridgeRom; // Stores the *entire* loaded ROM file
    private byte[] _wram;         // 8KB for 0xC000-0xDFFF
    private byte[] _vram;         // 8KB for 0x8000-0x9FFF
    private byte[] _oam;          // 160 bytes for 0xFE00-0xFE9F
    private byte[] _hram;         // 127 bytes for 0xFF80-0xFFFE
    private byte[] _ioram = new byte[0x80]; // 128 bytes for 0xFF00-0xFF7F
    private byte _ieRegister;     // Single byte for 0xFFFF
    private byte _ifRegister;
    // ... potentially a byte[] for I/O registers 0xFF00-0xFF7F or separate byte fields for each

    public event Action<ushort> OnVRAMWrite; // Event to signal VRAM writes

    public void FillVRAM()
    {
        for (int i = 0; i < _vram.Length; i=i+2)
        {
            _vram[i] = (byte)(i & 0xFF); // Fill with some pattern for testing
            _vram[i+1] = (byte)(i & 0xFF); // Fill with some pattern for testing
        }
    }

    public void FillIO()
    {
        for (ushort i = GameBoyHardware.IO_Joy; i < GameBoyHardware.IO_End; i++)
        {
            WriteByte(i, (byte)(i & 0xFF));
        }
        WriteByte(GameBoyHardware.IO_Int, 0xE0); // Set default IF register state
    }

    public GameBoyMemory(byte[] romData) // Constructor takes ROM data
    {
        _cartridgeRom = romData; // This holds your entire fakearom.gb file (8KB in your example)
        _wram = new byte[0x2000]; // 8KB
        _vram = new byte[0x2000]; // 8KB
        _oam = new byte[0xA0];    // 160 bytes (0xFE00 to 0xFE9F)
        _hram = new byte[0x7F];   // 127 bytes (0xFF80 to 0xFFFE)
        _ieRegister = 0x00;       // Default value
        _ifRegister = 0xE0; // Default state for unused upper bits (0b11100000) for a DMG,
                            // although PanDocs states 0xFF0F's lower 3 bits are 0 and upper 3 (5,6,7) are 1.
                            // The writable bits (0-4) are 0 initially.
                            // For simplicity and to reflect initial hardware state, 0xE0 (1110 0000) for initial upper bit values seems appropriate.

        // ... initialize other memory regions
    }

    public byte IE
    {
        get => _ieRegister;
        set => _ieRegister = value;
    }

    public byte IF
    {
        get => _ifRegister;
        set => _ifRegister = (byte)(value | 0xE0); // Mask to ensure bits 5-7 are always 1 (read-only for hardware status)
                                                   // Only bits 0-4 are writable/clearable by CPU.
    }

    public byte ReadByte(ushort address)
    {
        if (address >= GameBoyHardware.ROM_StartAddr && address <= GameBoyHardware.ROM_EndAddr) // ROM Area (Fixed bank 0 and Switchable banks)
        {
            // For now, no MBC, so just directly access the ROM data.
            // Later, if MBC1 is implemented, this would change to:
            // return _cartridgeRom[MBC.CalculateRomAddress(address)];
            // handle tiuny debugger type roms
            if (address < _cartridgeRom.Length)
                return _cartridgeRom[address];
            else return 0;
        }
        else if (address >= GameBoyHardware.VRAM_StartAddr && address <= GameBoyHardware.VRAM_EndAddr) // VRAM
        {
            return _vram[address - GameBoyHardware.VRAM_StartAddr];
        }
        else if (address >= GameBoyHardware.WRAM_StartAddr && address <= GameBoyHardware.WRAM_EndAddr) // WRAM
        {
            return _wram[address - GameBoyHardware.WRAM_StartAddr];
        }
        else if (address >= GameBoyHardware.ERAM_StartAddr && address <= GameBoyHardware.ERAM_EndAddr) // Echo RAM (Mirror of C000-DDFF)
        {
            // Just redirect to WRAM
            return _wram[address - GameBoyHardware.ERAM_StartAddr];
        }
        else if (address >= GameBoyHardware.OAM_StartAddr && address <= GameBoyHardware.OAM_EndAddr) // OAM
        {
            return _oam[address - GameBoyHardware.OAM_StartAddr];
        }
        else if (address >= GameBoyHardware.IO_StartAddr && address <= GameBoyHardware.IO_EndAddr) // I/O Registers
        {
            if (address == 0xFF0F) return IF; // Interrupt Flag Register
            // ... handle other I/O registers
            //return 0xFF; // Placeholder for unhandled I/O
            return _ioram[address - GameBoyHardware.IO_StartAddr]; // Temporary: map to HRAM for now
        }
        else if (address >= GameBoyHardware.HRAM_StartAddr && address <= GameBoyHardware.HRAM_EndAddr) // HRAM
        {
            return _hram[address - GameBoyHardware.HRAM_StartAddr];
        }
        else if (address == 0xFFFF) // Interrupt Enable Register
        {
            return IE; // _ieRegister;
        }
        // ... any other unhandled regions ...
        return 0xFF; // Default for unmapped/unhandled areas
    }

    public void WriteByte(ushort address, byte value)
    {
        if (address >= GameBoyHardware.ROM_StartAddr && address <= GameBoyHardware.ROM_EndAddr) // ROM is generally not writable
        {
            // If MBC is implemented, writes to this range control banking.
            // Example: MBC.HandleRomBankWrite(address, value);
            return; // For now, ignore writes to ROM
        }
        else if (address >= GameBoyHardware.VRAM_StartAddr && address <= GameBoyHardware.VRAM_EndAddr) // VRAM
        {
            _vram[address - GameBoyHardware.VRAM_StartAddr] = value;
            // Later, notify PPU of VRAM change for rendering updates
            OnVRAMWrite?.Invoke(address);
        }
        else if (address >= GameBoyHardware.WRAM_StartAddr && address <= GameBoyHardware.WRAM_EndAddr) // WRAM
        {
            _wram[address - GameBoyHardware.WRAM_StartAddr] = value;
        }
        else if (address >= GameBoyHardware.ERAM_StartAddr && address <= GameBoyHardware.ERAM_EndAddr) // Echo RAM (Mirror of C000-DDFF)
        {
            // Write also goes to WRAM
            _wram[address - GameBoyHardware.ERAM_StartAddr] = value;
        }
        else if (address >= GameBoyHardware.OAM_StartAddr && address <= GameBoyHardware.OAM_EndAddr) // OAM
        {
            _oam[address - GameBoyHardware.OAM_StartAddr] = value;
            // Later, notify PPU of OAM change for sprite updates
        }
        else if (address >= GameBoyHardware.IO_StartAddr && address <= GameBoyHardware.IO_EndAddr) // I/O Registers
        {
            if (address == 0xFF0F) { IF = value; return; } // Interrupt Flag Register
            _ioram[address - GameBoyHardware.IO_StartAddr] = value; // Temporary: map to HRAM for now

            // ... handle other I/O registers
            // Example: PPU.WriteIO(address, value);
        }
        else if (address >= GameBoyHardware.HRAM_StartAddr && address <= GameBoyHardware.HRAM_EndAddr) // HRAM
        {
            _hram[address - GameBoyHardware.HRAM_StartAddr] = value;
        }
        else if (address == 0xFFFF) // Interrupt Enable Register
        {
            //_ieRegister = value;
            IE = value;
        }
        // ... ignore writes to unhandled regions
    }

    public ushort ReadUWord(ushort address)
    {
        byte low = ReadByte(address);
        byte high = ReadByte((ushort)(address + 1));
        return (ushort)((high << 8) | low);
    }

    public void WriteUWord(ushort address, ushort value)
    {
        byte low = (byte)(value & 0x00FF);
        byte high = (byte)((value >> 8) & 0x00FF);
        WriteByte(address, low);
        WriteByte((ushort)(address + 1), high);
    }

    public uint ReadDoubleUWord(ushort address)
    {
        ushort low = ReadUWord(address);
        ushort high = ReadUWord((ushort)(address + 2));
        return (uint)((high << 16) | low);
    }

    public void WriteDoubleUWord(ushort address, uint value)
    {
        ushort low = (ushort)(value & 0x0000FFFF);
        ushort high = (ushort)((value >> 16) & 0x0000FFFF);
        WriteUWord(address, low);
        WriteUWord((ushort)(address + 2), high);
    }

    public int ReadDoubleWord(ushort address)
    {
        uint uvalue = ReadDoubleUWord(address);
        return (int)uvalue;
    }

    public void WriteDoubleWord(ushort address, int value)
    {
        uint uvalue = (uint)value;
        WriteDoubleUWord(address, uvalue);
    }

}
