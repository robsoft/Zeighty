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

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTexture, _area, Color.Black); // Solid black

        foreach (var item in _items.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_area.X+item.X, _area.Y+item.Y), item.Color);
        }
    }
}
