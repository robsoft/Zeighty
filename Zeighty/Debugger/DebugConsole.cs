using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Emulator;
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
    private Rectangle _instrRect;

    private bool _spaceDown = false;
    private bool _F1Down = false;
    private bool _F4Down = false;
    private bool _F12Down = false;
    private bool _PgUpDown = false;
    private bool _PgDnDown = false;
    private bool _TabDown = false;
    private bool _RtnDown = false;
    private bool _EscDown = false;

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
        _instrRect = new Rectangle(_area.X + 100, _area.Y + 20, 480, 150);

        Items.Add(0, 0, $"Cart: {_debugState.LoadedFileName}", DebugUIConstants.TITLE_ID, Color.LimeGreen);

        int lineHeight = _spritefont.LineSpacing;

        // pre-set up our display items
        int Regx = 10; int Regy = 30;
        Items.Add(Regx, Regy + (0 * lineHeight), " A: 00", DebugUIConstants.REG_A_ID);
        Items.Add(Regx, Regy + (1 * lineHeight), "BC: 00", DebugUIConstants.REG_BC_ID);
        Items.Add(Regx, Regy + (2 * lineHeight), "DE: 00", DebugUIConstants.REG_DE_ID);
        Items.Add(Regx, Regy + (3 * lineHeight), "HL: 00", DebugUIConstants.REG_HL_ID);
        Items.Add(Regx, Regy + (4 * lineHeight), "SP: 00", DebugUIConstants.REG_SP_ID);
        Items.Add(Regx, Regy + (5 * lineHeight), "PC: 00", DebugUIConstants.REG_PC_ID);
        Items.Add(Regx, Regy + (6 * lineHeight), " F: 00", DebugUIConstants.REG_F_ID);

        int Instrx = 110; int Instry = 30; Color instrColor = Color.Black;
        Items.Add(Instrx, Instry + (0 * lineHeight), "", DebugUIConstants.INSTR_01_ID, instrColor);
        Items.Add(Instrx, Instry + (1 * lineHeight), "", DebugUIConstants.INSTR_02_ID, instrColor);
        Items.Add(Instrx, Instry + (2 * lineHeight), "", DebugUIConstants.INSTR_03_ID, instrColor);
        Items.Add(Instrx, Instry + (3 * lineHeight), "", DebugUIConstants.INSTR_04_ID, instrColor);
        Items.Add(Instrx, Instry + (4 * lineHeight), "", DebugUIConstants.INSTR_05_ID, instrColor);
        Items.Add(Instrx, Instry + (5 * lineHeight), "", DebugUIConstants.INSTR_06_ID, instrColor);
        Items.Add(Instrx, Instry + (6 * lineHeight), "", DebugUIConstants.INSTR_07_ID, instrColor);

        Items.Add(320, 0, "CPU", DebugUIConstants.CPU_STATE_ID, Color.Yellow);
        Items.Add(590, 0, "DEBUG", DebugUIConstants.DEBUGGER_STATE_ID, Color.Yellow);

        int Iox = 590; int Ioy = 30;
        Items.Add(Iox, Ioy + (0 * lineHeight), $"${GameBoyMemory.IO_Joy:X4} JOY", DebugUIConstants.IO_JOY_ID);
        Items.Add(Iox, Ioy + (1 * lineHeight), $"${GameBoyMemory.IO_Ser:X4} SER", DebugUIConstants.IO_SER_ID);
        Items.Add(Iox, Ioy + (2 * lineHeight), $"${GameBoyMemory.IO_Tim:X4} TIM", DebugUIConstants.IO_TIM_ID);
        Items.Add(Iox, Ioy + (3 * lineHeight), $"${GameBoyMemory.IO_Int:X4} INT", DebugUIConstants.IO_INT_ID);
        Items.Add(Iox, Ioy + (4 * lineHeight), $"${GameBoyMemory.IO_Aud:X4} AUD", DebugUIConstants.IO_AUD1_ID);
        Items.Add(Iox, Ioy + (5 * lineHeight), $"${GameBoyMemory.IO_Aud + 4:X4}    ", DebugUIConstants.IO_AUD2_ID);
        Items.Add(Iox, Ioy + (6 * lineHeight), $"${GameBoyMemory.IO_Aud + 8:X4}    ", DebugUIConstants.IO_AUD3_ID);
        Items.Add(Iox, Ioy + (7 * lineHeight), $"${GameBoyMemory.IO_Aud + 12:X4}    ", DebugUIConstants.IO_AUD4_ID);
        Items.Add(Iox, Ioy + (8 * lineHeight), $"${GameBoyMemory.IO_Lcd:X4} LCD", DebugUIConstants.IO_LCD1_ID);
        Items.Add(Iox, Ioy + (9 * lineHeight), $"${GameBoyMemory.IO_Lcd + 4:X4}    ", DebugUIConstants.IO_LCD2_ID);
        Items.Add(Iox, Ioy + (10 * lineHeight), $"${GameBoyMemory.IO_Lcd + 8:X4}    ", DebugUIConstants.IO_LCD3_ID);

        int Memx = 10; int Memy = 200;
        Items.Add(Memx, Memy + (0 * lineHeight), $"${(ushort)(_debugState.MemoryAddress):X4} ", DebugUIConstants.MEM1_ID);
        Items.Add(Memx, Memy + (1 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 16):X4} ", DebugUIConstants.MEM2_ID);
        Items.Add(Memx, Memy + (2 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 32):X4} ", DebugUIConstants.MEM3_ID);
        Items.Add(Memx, Memy + (3 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 48):X4} ", DebugUIConstants.MEM4_ID);
        Items.Add(Memx, Memy + (4 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 64):X4} ", DebugUIConstants.MEM5_ID);

    }

    public void Update(GameTime gameTime)
    {
        // do we have any special condition regarding the current address?
        var entry = _debugState.Memory.GetEntry(_emulator.Cpu.PC);
        var newBPState = (entry != null && entry.BreakpointType != BreakpointType.None);
        // have we hit a breakpoint?
        if (newBPState != _debugState.InBreakpoint)
        {
            _debugState.InBreakpoint = newBPState;
            _debugState.SingleStep = true; // prevent us from haring off upon hitting bp
        }

        // if we're running normally, and not halted, and not on a breakpoint, just continue -- don't update UI
        if (!_debugState.SingleStep && !_emulator.Cpu.IsHalted && !_debugState.InBreakpoint)
        {
            _debugState.NextStep = true;
            return;
        }

        UpdateRegView();
        UpdateDisassembly();
        UpdateFlagView();
        UpdateIOView();
        UpdateStateView();
        UpdateMemoryView();
        // would update VRAM here once converted from the console app versions

        HandleDebuggerKeyboard();
    }

    private void HandleDebuggerKeyboard()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) // single-step this instruction
        {
            if (_spaceDown == false)
                _debugState.NextStep = true;
            _spaceDown = true;
        }
        else _spaceDown = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F1)) // 'run' to the next breakpoint
        {
            if (_F1Down == false)
            {
                _debugState.SingleStep = false;
                _debugState.NextStep = true;
                _F1Down = true;
            }
        }
        else _F1Down = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F4)) // edit/toggle breakpoint
        {
            if (_F4Down == false)
            {
                _F4Down = true;
                EditBreakpoint(_emulator.Cpu.PC);
            }
        }
        else _F4Down = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F12)) // reset the CPU, PC & debugger state (does NOT reset ram though)
        {
            if (_F12Down == false)
            {
                _debugState.NeedReset = true;
                _F12Down = true;
            }
        }
        else _F12Down = false;

        if (Keyboard.GetState().IsKeyDown(Keys.PageDown)) // move downwards (towards 0xFFFF) in memory
        {
            if (_PgDnDown == false)
            {
                if (_debugState.MemoryAddress < (0xFFF0))
                {
                    _debugState.MemoryAddress = (ushort)(_debugState.MemoryAddress + 16);
                }
                _PgDnDown = true;
            }
        }
        else _PgDnDown = false;

        if (Keyboard.GetState().IsKeyDown(Keys.PageUp)) // move upwards (towards 0x0000) in memory
        {
            if (_PgUpDown == false)
            {
                if (_debugState.MemoryAddress > (0x000F))
                {
                    _debugState.MemoryAddress = (ushort)(_debugState.MemoryAddress - 16);
                }
                _PgUpDown = true;
            }
        }
        else _PgUpDown = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Tab)) // manual change of memory address
        {
            if (_TabDown == false)
            {
                // should have an memory address input dialog here
                // just fix MemoryAddress to 0C00 for now
                EditMemoryViewAddress();
                _debugState.MemoryAddress = 0xC000;
                _TabDown = true;
            }
        }
        else _TabDown = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Enter)) // manual edit of memory address content 
        {
            if (_RtnDown == false)
            {
                // should have an memory address input dialog here
                // just fix MemoryAddress to 0C00 for now
                //_debugState.MemoryAddress = 0xC000;
                EditMemoryViewContent();
                _RtnDown = true;
            }
        }
        else _RtnDown = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // reserved for now
        {
            if (_EscDown == false)
            {
                _EscDown = true;
            }
        }
        else _EscDown = false;
    }

    private void EditMemoryViewContent()
    { }

    private void EditMemoryViewAddress()
    { }

    private void EditBreakpoint(ushort Addr)
    { }

    private void UpdateRegView()
    {
        Items.GetItemById(DebugUIConstants.REG_A_ID).Text = $" A: {_emulator.Cpu.A:X2}";
        Items.GetItemById(DebugUIConstants.REG_BC_ID).Text = $"BC: {_emulator.Cpu.B:X2} {_emulator.Cpu.C:X2}";
        Items.GetItemById(DebugUIConstants.REG_DE_ID).Text = $"DE: {_emulator.Cpu.D:X2} {_emulator.Cpu.E:X2}";
        Items.GetItemById(DebugUIConstants.REG_HL_ID).Text = $"HL: {_emulator.Cpu.H:X2} {_emulator.Cpu.L:X2}";
        Items.GetItemById(DebugUIConstants.REG_PC_ID).Text = $"PC: {_emulator.Cpu.PC:X4}";
        Items.GetItemById(DebugUIConstants.REG_SP_ID).Text = $"SP: {_emulator.Cpu.SP:X4}";

    }

    private void UpdateDisassembly()
    {
        string[] instr = new string[7];
        for (int i = 0; i < 7; i++)
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

        Items.GetItemById(DebugUIConstants.INSTR_01_ID).Text = instr[0];
        Items.GetItemById(DebugUIConstants.INSTR_02_ID).Text = instr[1];
        Items.GetItemById(DebugUIConstants.INSTR_03_ID).Text = instr[2];
        Items.GetItemById(DebugUIConstants.INSTR_04_ID).Text = instr[3];
        Items.GetItemById(DebugUIConstants.INSTR_05_ID).Text = instr[4];
        Items.GetItemById(DebugUIConstants.INSTR_06_ID).Text = instr[5];
        Items.GetItemById(DebugUIConstants.INSTR_07_ID).Text = instr[6];
    }

    private void UpdateMemoryView()
    {
        ushort addr = _debugState.MemoryAddress;
        Items.GetItemById(DebugUIConstants.MEM1_ID).Text = FormatBytesString(addr, 16);
        Items.GetItemById(DebugUIConstants.MEM2_ID).Text = FormatBytesString((ushort)(addr + 16), 16);
        Items.GetItemById(DebugUIConstants.MEM3_ID).Text = FormatBytesString((ushort)(addr + 32), 16);
        Items.GetItemById(DebugUIConstants.MEM4_ID).Text = FormatBytesString((ushort)(addr + 48), 16);
        Items.GetItemById(DebugUIConstants.MEM5_ID).Text = FormatBytesString((ushort)(addr + 64), 16);
    }

    private string FormatBytesString(ushort startAddress, int length)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"${startAddress:X4}: ");
        for (ushort i = 0; i < length; i++)
        {
            sb.Append($"{_emulator.Cpu.Memory.ReadByte((ushort)(startAddress + i)):X2} ");
        }
        return sb.ToString().TrimEnd();
    }

    private void UpdateIOView()
    {
        // joy is the byte and then the bits, easier to read presses
        byte JOY = _emulator.Cpu.Memory.ReadByte(GameBoyMemory.IO_Joy);

        Items.GetItemById(DebugUIConstants.IO_JOY_ID).Text = $"${GameBoyMemory.IO_Joy:X4} JOY {JOY:X2} {Convert.ToString(JOY, 2).PadLeft(8, '0')}";

        byte SER1 = _emulator.Cpu.Memory.ReadByte(GameBoyMemory.IO_Ser);
        byte SER2 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyMemory.IO_Ser + 1));
        Items.GetItemById(DebugUIConstants.IO_SER_ID).Text = $"${GameBoyMemory.IO_Ser:X4} SER {SER1:X2} {SER2:X2}";

        byte TIM1 = _emulator.Cpu.Memory.ReadByte(GameBoyMemory.IO_Tim);
        byte TIM2 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyMemory.IO_Tim + 1));
        byte TIM3 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyMemory.IO_Tim + 2));
        Items.GetItemById(DebugUIConstants.IO_TIM_ID).Text = $"${GameBoyMemory.IO_Tim:X4} TIM {TIM1:X2} {TIM2:X2} {TIM3:X2}";

        byte INT = _emulator.Cpu.Memory.ReadByte(GameBoyMemory.IO_Int);
        Items.GetItemById(DebugUIConstants.IO_INT_ID).Text = $"${GameBoyMemory.IO_Int:X4} INT {INT:X2}";

        uint value = _emulator.Cpu.Memory.ReadDoubleUWord(GameBoyMemory.IO_Aud);
        byte byte0 = (byte)(value & 0xFF);
        byte byte1 = (byte)((value >> 8) & 0xFF);
        byte byte2 = (byte)((value >> 16) & 0xFF);
        byte byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD1_ID).Text = $"${GameBoyMemory.IO_Aud:X4} AUD {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 4));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD2_ID).Text = $"${GameBoyMemory.IO_Aud + 4:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 8));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD3_ID).Text = $"${GameBoyMemory.IO_Aud + 8:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Aud + 12));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD4_ID).Text = $"${GameBoyMemory.IO_Aud + 12:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD1_ID).Text = $"${GameBoyMemory.IO_Lcd:X4} LCD {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd + 4));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD2_ID).Text = $"${GameBoyMemory.IO_Lcd + 4:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyMemory.IO_Lcd + 8));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        //byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD3_ID).Text = $"${GameBoyMemory.IO_Lcd + 8:X4}     {byte0:X2} {byte1:X2} {byte2:X2}";
    }


    private void UpdateFlagView()
    {
        // Access the F register value
        byte flags = _emulator.Cpu.F;

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
        //return $" F: {flags:X2} {zeroFlag}{subtractFlag}{halfCarryFlag}{carryFlag}{bit3}{bit2}{bit1}{bit0} {flagIndicators}";
        // B0:1 0 1 1 0 0 0 0:ZHC-
        Items.GetItemById(DebugUIConstants.REG_F_ID).Text = $" F: {flagIndicators}";
    }

    private void UpdateStateView()
    {
        string cpuText = "READY";
        if (_emulator.Cpu.IsHalted)
        {
            cpuText = "HALTED";
            _debugState.SingleStep = true; // force single-step when halted
        }

        string stateText = "  RUNNING  ";
        if (_debugState.InBreakpoint)
            stateText = " BREAKPOINT";
        else if (_debugState.SingleStep)
            stateText = "SINGLE-STEP";

        Items.GetItemById(DebugUIConstants.CPU_STATE_ID).Text = $"CPU: {cpuText}";
        Items.GetItemById(DebugUIConstants.DEBUGGER_STATE_ID).Text = $"DEBUG: {stateText}";
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTexture, _area, Color.Black); // Solid black

        spriteBatch.Draw(_backgroundTexture, _instrRect, Color.CornflowerBlue); // Solid black

        foreach (var item in _items.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_area.X + item.X, _area.Y + item.Y), item.Color);
        }
    }


    /*
  
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

}
