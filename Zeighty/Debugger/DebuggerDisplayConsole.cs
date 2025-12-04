using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

/*
public class DebuggerDisplayConsole
{
    private ICpu _cpu;
    private IMemory _memory;
    private IDebugState _debugState;

    private int ScreenWidth = 80;
    private int ScreenHeight = 25;

    private int IOX = 56;
    private int IOY = 9;
    private bool IOVisible = true;
    private int VRAMX = 0;
    private int VRAMY = 16;
    private int MEMX = 0;
    private int MEMY = 09;
    private int REGX = 0;
    private int REGY = 2;
    private int FILEX = 20;
    private int FILEY = 0;
    private int DISX = 25;
    private int DISY = 2;

    private int curVRAMWidth = 16;

    private int STATUSX = 68;
    private int STATUSY = 0;
    private int FLAGX = 0;
    private int FLAGY = 6;

    public DebuggerDisplayConsole(ICpu cpu, IMemory memory, IDebugState debugState)
    {
        _cpu = cpu;
        _memory = memory;
        _debugState = debugState;
        _debugState.IOVisible = true;
        _debugState.SingleStep = true;
        _debugState.MemoryAddress = 0xC000;
        _debugState.VRAMAddress = 0x8000;
        _debugState.LoadedFileName = "(no file)";
        _debugState.VRAMWidth = 16;
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.Clear();
    }

    public bool WaitStep()
    {
        var entry = _debugState.Memory.GetEntry(_cpu.PC);
        var newState = (entry != null && entry.BreakpointType != BreakpointType.None);
        if (newState != _debugState.InBreakpoint)
        {
            _debugState.InBreakpoint = newState;
            _debugState.SingleStep = true; // prevent us from haring off if we step on
            UpdateHalt();
        }

        // if we're running normally, and not halted, and no breakpoint, just continue
        if (!_debugState.SingleStep && !_cpu.IsHalted && !_debugState.InBreakpoint)
        {
            return true;
        }

        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.PageUp)
        {
            _debugState.MemoryAddress--;
            return false;
        }
        else if (key.Key == ConsoleKey.PageDown)
        {
            _debugState.MemoryAddress++;
            return false;
        }
        else if (key.Key == ConsoleKey.Tab)
        {
            UpdateMemoryAddress();
            StaticUI();
            UpdateScreen();
            return false;
        }
        else if (key.Key == ConsoleKey.F12)
        {
            // todo: confirm prompt first
            _debugState.NeedReset = true;
            return true;
        }
        else if (key.Key == ConsoleKey.F2)
        {
            _debugState.IOVisible = !_debugState.IOVisible;
            _debugState.VRAMWidth = _debugState.IOVisible ? (ushort)16 : (ushort)24;
            StaticUI();
            return false;
        }
        else if (key.Key == ConsoleKey.F1)
        {
            // todo: confirm prompt first
            _debugState.SingleStep = false;
            return true;
        }
        else if (key.Key == ConsoleKey.Spacebar)
        {
            // this is single-step
            return true;
        }
        return false;
        // PageUp, PageDown, LeftArrow, UpArrow, RightArrow, DownArrow, Escape, Enter, Tab
    }

    private void Blank(int x, int y, int length)
    {
        Console.SetCursorPosition(x, y);
        var str = " ";
        AnsiConsole.MarkupLine($"[white on navy]{str:length}[/]");
    }

    private void UpdateMemoryAddress()
    {
        Blank(0, MEMY, 60);
        AnsiConsole.Markup("[black on yellow]New Addr: $[/]");

        string? addrInput = Console.ReadLine()?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(addrInput) || addrInput.Length > 4)
        {
            return;
            //Console.WriteLine("Invalid input: Address must be 1 to 4 hex digits.");
            //return null; // Return null to indicate invalid input
        }

        ushort address;
        // ushort.TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        // NumberStyles.HexNumber: Allows "A-F", "a-f", "0-9". Doesn't allow '0x' prefix.
        // CultureInfo.InvariantCulture: Ensures consistent parsing regardless of user's locale.
        if (ushort.TryParse(addrInput, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
        {
            // Conversion successful, and it's a valid ushort (0-65535)
            _debugState.MemoryAddress = address;
        }
    }

    public void UpdateScreen()
    {
        UpdateRegisters();
        UpdateFlags();
        DisplayMemoryView(_debugState.MemoryAddress);
        DisplayVRAMView(_debugState.VRAMAddress);
        DisplayIOView();
        UpdateInstructions();
        UpdateHalt();
        AnsiConsole.Cursor.Hide();
    }


    public void UpdateRegisters()
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        // want to break-out flags, and show cycles here too
        Console.SetCursorPosition(REGX + 15, REGY); AnsiConsole.Write($"{_cpu.A:X2}");
        Console.SetCursorPosition(REGX + 4, REGY); AnsiConsole.Write($"{_cpu.B:X2} {_cpu.C:X2}");
        Console.SetCursorPosition(REGX + 4, REGY + 1); AnsiConsole.Write($"{_cpu.D:X2} {_cpu.E:X2}");
        Console.SetCursorPosition(REGX + 4, REGY + 2); AnsiConsole.Write($"{_cpu.H:X2} {_cpu.L:X2}");

        Console.SetCursorPosition(REGX + 4, REGY + 3); AnsiConsole.Write($"${_cpu.SP:X4}");
        Console.SetCursorPosition(REGX + 15, REGY + 3); AnsiConsole.Write($"${_cpu.PC:X4}");
    }


    public void StaticUI()
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.Clear();

        Console.SetCursorPosition(0, 0);
        AnsiConsole.Write("Zeemu Debugger v0.1");
        Console.SetCursorPosition(FILEX, FILEY);
        AnsiConsole.Write("File: " + _debugState.LoadedFileName);

        AnsiConsole.Write("Registers:");
        Console.SetCursorPosition(REGX + 12, REGY);
        AnsiConsole.Write("A:");
        Console.SetCursorPosition(REGX, REGY);
        AnsiConsole.Write("BC:");
        Console.SetCursorPosition(REGX, REGY + 1);
        AnsiConsole.Write("DE:");
        Console.SetCursorPosition(REGX, REGY + 2);
        AnsiConsole.Write("HL:");
        Console.SetCursorPosition(REGX, REGY + 3);
        AnsiConsole.Write("SP:");
        Console.SetCursorPosition(REGX + 11, REGY + 3);
        AnsiConsole.Write("PC:");

        Console.SetCursorPosition(FLAGX, FLAGY);
        AnsiConsole.Write(" F:");

        Console.SetCursorPosition(MEMX, MEMY);
        AnsiConsole.Write("MEM:");

        Console.SetCursorPosition(VRAMX, VRAMY);
        AnsiConsole.Write("VRAM:");

        if (_debugState.IOVisible)
        {
            Console.SetCursorPosition(IOX, IOY);
            AnsiConsole.Write("I/O:");
            Console.SetCursorPosition(IOX + 1, IOY + 1);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            AnsiConsole.Write("$FF00 JOY");
            Console.SetCursorPosition(IOX + 1, IOY + 2);
            AnsiConsole.Write("$FF01 SER");
            Console.SetCursorPosition(IOX + 1, IOY + 3);
            AnsiConsole.Write("$FF04 TIM");
            Console.SetCursorPosition(IOX + 1, IOY + 4);
            AnsiConsole.Write("$FF0F INT");
            Console.SetCursorPosition(IOX + 1, IOY + 5);
            AnsiConsole.Write("$FF10 AUD");
            Console.SetCursorPosition(IOX + 1, IOY + 9);
            AnsiConsole.Write("$FF40 LCD");
        }

        UpdateScreen();
    }

    public void UpdateHalt()
    {
        Console.SetCursorPosition(STATUSX, STATUSY);
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        if (_cpu.IsHalted)
        {
            AnsiConsole.Write("  HALTED!  ");
        }
        else
        {
            if (_debugState.InBreakpoint)
                AnsiConsole.Write(" BREAKPOINT");
            else
            if (_debugState.SingleStep)
                AnsiConsole.Write("SINGLE-STEP");
            else
                AnsiConsole.Write("  RUNNING  ");
        }
    }


    public void UpdateInstructions()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        for (int i = 0; i < 5; i++)
        {
            Console.SetCursorPosition(DISX, DISY + i);
            var addr = $"${_cpu.Instructions[i].Address:X4}";
            var decoded = _cpu.Instructions[i].Decoded;
            var desc = _debugState.Memory.GetAddressDescription(_cpu.Instructions[i].Address);
            if (decoded.Contains('$'))
            {
                // get a ushort from the 4 chars following the $
                var dollarIndex = decoded.IndexOf('$');
                if (dollarIndex >= 0)
                {
                    var hexPart = decoded.Substring(dollarIndex + 1, 4);
                    if (ushort.TryParse(hexPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort destAddr))
                    {
                        var resolvedDest = _debugState.Memory.GetAddressDescription(destAddr);
                        if (!string.IsNullOrEmpty(resolvedDest))
                        {
                            decoded = decoded.Replace($"${hexPart}", resolvedDest);
                        }
                    }
                }
            }
            AnsiConsole.Markup($"[grey]{desc,10}[/] {addr} : {decoded,-20} [grey]{_cpu.Instructions[i].DecodedBytes,-12}[/]"); 
        }
    }

    public void UpdateFlags()
    {
        // Access the F register value
        byte flags = _cpu.F;

        // Use bitwise operations to check each flag bit
        // (flags & (1 << bitPosition)) checks if the bit at bitPosition is set
        // Then use a conditional operator (ternary operator) to output '1' or '0'

        char zeroFlag = ((flags & (1 << 7)) != 0) ? '1' : '0'; // Bit 7
        char subtractFlag = ((flags & (1 << 6)) != 0) ? '1' : '0'; // Bit 6
        char halfCarryFlag = ((flags & (1 << 5)) != 0) ? '1' : '0'; // Bit 5
        char carryFlag = ((flags & (1 << 4)) != 0) ? '1' : '0'; // Bit 4

        // The lower 4 bits are always 0 on Game Boy, so we can just hardcode '0' for them.
        char bit3 = '0';
        char bit2 = '0';
        char bit1 = '0';
        char bit0 = '0';

        // Now, use string interpolation to display them
        // Example: F: 0xFX (Z N H C 0 0 0 0)
        // You might also want a quick text indicator of which flags are set
        string flagIndicators = $"{(zeroFlag == '1' ? "Z" : "-")}" +
                                $"{(subtractFlag == '1' ? "N" : "-")}" +
                                $"{(halfCarryFlag == '1' ? "H" : "-")}" +
                                $"{(carryFlag == '1' ? "C" : "-")}";
        Console.SetCursorPosition(FLAGX + 4, FLAGY);
        AnsiConsole.Write($"{flags:X2} {zeroFlag}{subtractFlag}{halfCarryFlag}{carryFlag}{bit3}{bit2}{bit1}{bit0} {flagIndicators}");
        // Example Output:
        // B0:1 0 1 1 0 0 0 0:ZHC-
        // 00:0 0 0 0 0 0 0 0:----
        // 80:1 0 0 0 0 0 0 0:Z---
    }

    public void DisplayIOView()
    {
        // joy is the byte and then the bits, easier to read presses
        byte JOY = _memory.ReadByte(GameBoyMemory.IO_Joy);
        Console.SetCursorPosition(IOX + 11, IOY + 1);
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.Yellow;
        AnsiConsole.Write($"{JOY:X2} {Convert.ToString(JOY, 2).PadLeft(8, '0')}");
        
        byte SER1 = _memory.ReadByte(GameBoyMemory.IO_Ser);
        byte SER2 = _memory.ReadByte((ushort)(GameBoyMemory.IO_Ser+1));
        Console.SetCursorPosition(IOX + 11, IOY + 2);
        AnsiConsole.Write($"{SER1:X2} {SER2:X2}");
        
        byte TIM1 = _memory.ReadByte(GameBoyMemory.IO_Tim);
        byte TIM2 = _memory.ReadByte((ushort)(GameBoyMemory.IO_Tim + 1));
        byte TIM3 = _memory.ReadByte((ushort)(GameBoyMemory.IO_Tim + 2));
        Console.SetCursorPosition(IOX + 11, IOY + 3);
        AnsiConsole.Write($"{TIM1:X2} {TIM2:X2} {TIM3:X2}");
        
        byte INT = _memory.ReadByte(GameBoyMemory.IO_Int);
        Console.SetCursorPosition(IOX + 11, IOY + 4);
        AnsiConsole.Write($"{INT:X2}");

        uint value = _memory.ReadDoubleUWord(GameBoyMemory.IO_Aud);
        byte byte0 = (byte)(value & 0xFF);     
        byte byte1 = (byte)((value >> 8) & 0xFF);
        byte byte2 = (byte)((value >> 16) & 0xFF);
        byte byte3 = (byte)((value >> 24) & 0xFF);    
        Console.SetCursorPosition(IOX + 11, IOY + 5);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");

        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 4));
        byte0 = (byte)(value & 0xFF);      
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);     
        Console.SetCursorPosition(IOX + 11, IOY + 6);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");

        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 8));
        byte0 = (byte)(value & 0xFF);       
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);     
        Console.SetCursorPosition(IOX + 11, IOY + 7);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");

        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 12));
        byte0 = (byte)(value & 0xFF);         
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);     
        Console.SetCursorPosition(IOX + 11, IOY + 8);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");

        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd));
        byte0 = (byte)(value & 0xFF);             
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);   
        Console.SetCursorPosition(IOX + 11, IOY + 9);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");
        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd + 4));
        byte0 = (byte)(value & 0xFF);             
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);   
        Console.SetCursorPosition(IOX + 11, IOY + 10);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}");
        value = _memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd + 8));
        byte0 = (byte)(value & 0xFF);           
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        //byte3 = (byte)((value >> 24) & 0xFF);
        Console.SetCursorPosition(IOX + 11, IOY + 11);
        AnsiConsole.Write($"{byte0:X2} {byte1:X2} {byte2:X2}");
    }


    public void DisplayMemoryView(ushort startAddress)
    {
        Console.SetCursorPosition(MEMX + 5, MEMY);
        AnsiConsole.Write($"${startAddress:X4}");

        DisplayMemory(MEMX, MEMY + 1, (ushort)(startAddress - 32), 16);
        DisplayMemory(MEMX, MEMY + 2, (ushort)(startAddress - 16), 16);
        DisplayMemory(MEMX, MEMY + 3, startAddress, 16, true);
        DisplayMemory(MEMX, MEMY + 4, (ushort)(startAddress + 16), 16);
        DisplayMemory(MEMX, MEMY + 5, (ushort)(startAddress + 32), 16);
    }

    public void DisplayVRAMView(ushort startAddress)
    {
        Console.SetCursorPosition(VRAMX + 5, VRAMY);
        AnsiConsole.Write($"${startAddress:X4}");
        ushort addr = (ushort)startAddress;
        ushort offs = _debugState.VRAMWidth;
        DisplayMemory(VRAMX, VRAMY + 1, (ushort)addr, offs);
        addr = (ushort)(addr + offs);
        DisplayMemory(VRAMX, VRAMY + 2, (ushort)addr, offs);
        addr = (ushort)(addr + offs);
        DisplayMemory(VRAMX, VRAMY + 3, (ushort)addr, offs);
        addr = (ushort)(addr + offs);
        DisplayMemory(VRAMX, VRAMY + 4, (ushort)addr, offs);
        addr = (ushort)(addr + offs);
        DisplayMemory(VRAMX, VRAMY + 5, (ushort)addr, offs);
    }

    private void DisplayMemory(int x, int y, ushort startAddress, int length, bool highlight = false)
    {
        Console.SetCursorPosition(x, y);
        StringBuilder sb = new StringBuilder();
        sb.Append(highlight ? "[yellow on navy]>" : "[white on navy] ");
        sb.Append($"${startAddress:X4}: ");
        for (ushort i = 0; i < length; i++)
        {
            sb.Append($"{_memory.ReadByte((ushort)(startAddress + i)):X2} ");
        }
        AnsiConsole.MarkupLine(sb.ToString() + "[/]");
    }
}
*/