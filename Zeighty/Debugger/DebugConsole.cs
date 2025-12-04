using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class DebugConsole
{
    private GraphicsDevice _graphicsDevice;
    private IEmulator _emulator;
    private SpriteFont _spritefont;
    private Rectangle _area;
    private DebugConsoleItems _items;
    private Texture2D _backgroundTexture;
    private GameBoyDebugState _debugState;

    private bool _spaceDown = false;
    public DebugConsoleItems Items => _items;

    public DebugConsole(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Rectangle area,
        IEmulator emulator, GameBoyDebugState debugState)
    {
        _graphicsDevice = graphicsDevice;
        _spritefont = spriteFont;
        _area = area;
        _items = new DebugConsoleItems();
        _emulator = emulator;
        _debugState = debugState;
        _backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });
        // pre-set up our disaply items
        Items.Add(10,30,"F: 00", DebugUIConstants.REG_F_ID);
        Items.Add(10, 30 + _spritefont.LineSpacing,"A: 00", DebugUIConstants.REG_A_ID);
        Items.Add(10, 30 + (2 * _spritefont.LineSpacing), "BC: 00", DebugUIConstants.REG_BC_ID);
        Items.Add(10, 30 + (3 * _spritefont.LineSpacing), "DE: 00", DebugUIConstants.REG_DE_ID);
        Items.Add(10, 30 + (4 * _spritefont.LineSpacing), "HL: 00", DebugUIConstants.REG_HL_ID);
        Items.Add(10, 30 + (5 * _spritefont.LineSpacing), "SP: 00", DebugUIConstants.REG_SP_ID);
        Items.Add(10, 30 + (6 * _spritefont.LineSpacing), "PC: 00", DebugUIConstants.REG_PC_ID);

        Items.Add(120, 30, "", DebugUIConstants.INSTR_01_ID);
        Items.Add(120, 30 + _spritefont.LineSpacing, "", DebugUIConstants.INSTR_02_ID);
        Items.Add(120, 30 + (2 * _spritefont.LineSpacing), "", DebugUIConstants.INSTR_03_ID);
        Items.Add(120, 30 + (3 * _spritefont.LineSpacing), "", DebugUIConstants.INSTR_04_ID);
        Items.Add(120, 30 + (4 * _spritefont.LineSpacing), "", DebugUIConstants.INSTR_05_ID);
    }

    public void Update(GameTime gameTime)
    {

        var entry = _debugState.Memory.GetEntry(_emulator.Cpu.PC);
        var newState = (entry != null && entry.BreakpointType != BreakpointType.None);
        if (newState != _debugState.InBreakpoint)
        {
            _debugState.InBreakpoint = newState;
            _debugState.SingleStep = true; // prevent us from haring off if we step on
            //UpdateHalt();
        }

        // if we're running normally, and not halted, and no breakpoint, just continue
        if (!_debugState.SingleStep && !_emulator.Cpu.IsHalted && !_debugState.InBreakpoint)
        {
            _debugState.NextStep = true;
            return;
        }


        string[] instr = new string[5];
        for (int i = 0; i < 5; i++)
        {
            var addr = $"${_emulator.Cpu.Instructions[i].Address:X4}";
            var decoded = _emulator.Cpu.Instructions[i].Decoded;
            var desc = _debugState.Memory.GetAddressDescription(_emulator.Cpu.Instructions[i].Address);
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
            instr[i] = $"{desc,10} {addr} : {decoded,-20} {_emulator.Cpu.Instructions[i].DecodedBytes,-12}";
        }

        Items.GetItemById(DebugUIConstants.REG_A_ID).Text = $"A: {_emulator.Cpu.A:X2}";
        Items.GetItemById(DebugUIConstants.REG_BC_ID).Text = $"BC: {_emulator.Cpu.B:X2} {_emulator.Cpu.C:X2}";
        Items.GetItemById(DebugUIConstants.REG_DE_ID).Text = $"DE: {_emulator.Cpu.D:X2} {_emulator.Cpu.E:X2}";
        Items.GetItemById(DebugUIConstants.REG_HL_ID).Text = $"HL: {_emulator.Cpu.H:X2} {_emulator.Cpu.L:X2}";
        Items.GetItemById(DebugUIConstants.REG_PC_ID).Text = $"PC: {_emulator.Cpu.PC:X4}";
        Items.GetItemById(DebugUIConstants.REG_SP_ID).Text = $"SP: {_emulator.Cpu.SP:X4}";
        Items.GetItemById(DebugUIConstants.INSTR_01_ID).Text = instr[0];
        Items.GetItemById(DebugUIConstants.INSTR_02_ID).Text = instr[1];
        Items.GetItemById(DebugUIConstants.INSTR_03_ID).Text = instr[2];
        Items.GetItemById(DebugUIConstants.INSTR_04_ID).Text = instr[3];
        Items.GetItemById(DebugUIConstants.INSTR_05_ID).Text = instr[4];



        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            if (_spaceDown == false)
                _debugState.NextStep = true;
            _spaceDown = true;
        }
        else _spaceDown = false;
    }

    /*
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
    */

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTexture, _area, Color.Black); // Solid black

        foreach (var item in _items.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_area.X+item.X, _area.Y+item.Y), item.Color);
        }
    }
}
