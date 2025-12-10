using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using Zeighty.Emulator;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public partial class DebugMode : BaseMode
{
    public const int TITLE_ID = 1;
    public const int KEYBOARD_ID = 2;
    public const int CPU_STATE_ID = 3;
    public const int DEBUGGER_STATE_ID = 4;

    public const int REG_A_ID = 10;
    public const int REG_BC_ID = 11;
    public const int REG_DE_ID = 12;
    public const int REG_HL_ID = 13;
    public const int REG_F_ID = 14;
    public const int REG_SP_ID = 15;
    public const int REG_PC_ID = 16;

    public const int INSTR_01_ID = 20;
    public const int INSTR_02_ID = 21;
    public const int INSTR_03_ID = 22;
    public const int INSTR_04_ID = 23;
    public const int INSTR_05_ID = 24;
    public const int INSTR_06_ID = 25;
    public const int INSTR_07_ID = 26;

    public const int IO_JOY_ID = 30;
    public const int IO_SER_ID = 31;
    public const int IO_TIM_ID = 32;
    public const int IO_INT_ID = 33;

    public const int IO_AUD1_ID = 34;
    public const int IO_AUD2_ID = 35;
    public const int IO_AUD3_ID = 36;
    public const int IO_AUD4_ID = 37;

    public const int IO_LCD1_ID = 38;
    public const int IO_LCD2_ID = 39;
    public const int IO_LCD3_ID = 40;

    public const int MEM1_ID = 50;
    public const int MEM2_ID = 51;
    public const int MEM3_ID = 52;
    public const int MEM4_ID = 53;
    public const int MEM5_ID = 54;

    public const int MOUSE_ID = 60;

    private Rectangle _instrRect;
    private Color[] _gameBoyPalette; // mapping from 2-bit color to MonoGame Color
    private Texture2D[] _debugTileTextures;
    private bool[] _tileDataChanged = new bool[GameBoyHardware.MAX_TILES];
    private Rectangle _tileArea;
    private DebugConsoleItems TileItems = new DebugConsoleItems();
    private int _scaleFactor;
    private int _tilesPerRow;
    //private int _tilesPerColumn;

    public DebugMode(DebugConsole console, Rectangle tileArea, int scaleFactor) : 
        base(console)
    {
        _scaleFactor = scaleFactor;
        _instrRect = new Rectangle(_baseArea.X + 100, _baseArea.Y + 20, 480, 150);
        _tileArea = tileArea;
        _tilesPerRow = _tileArea.Width / (8 * _scaleFactor);

        DebugPalette();
        FillVRAMTest();

        _emulator.Cpu.Memory.OnVRAMWrite += HandleVRAMWrite;
        InitializeDebugTiles(); // Create the texture objects
        UpdateAllDebugTiles();  // Populate them initially
        SetupConsoleItems();
    }


    public override void Init()
    {
        ResetKeys();
        _debounce = true;
    }


    public override void Update(GameTime gameTime)
    {
        // ok, back to normal operation
        if (_debounce && NeedDebounce()) return;
        _debounce = false;

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

        KeyHandler();
        UpdateMouse();
    }


    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_console.BackgroundTexture, _baseArea, Color.Black);
        spriteBatch.Draw(_console.BackgroundTexture, _instrRect, Color.CornflowerBlue);

        // draw normal items
        foreach (var item in Items.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_baseArea.X + item.X, _baseArea.Y + item.Y), item.Color);
        }

        // draw tiles
        for (int i = 0; i < GameBoyHardware.MAX_TILES; i++)
        {
            {
                DrawDebugTileByIndex(spriteBatch, i);
            }
        }

        // draw tile info
        foreach (var item in TileItems.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_tileArea.X + item.X, _tileArea.Y + item.Y), item.Color);
        }

    }

    public override void KeyHandler()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Space)) // single-step this instruction
        {
            if (_keyIsDown[(int)Keys.Space] == false)
                _debugState.NextStep = true;
            _keyIsDown[(int)Keys.Space] = true;
        }
        else _keyIsDown[(int)Keys.Space] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F1))
        {
            if (_keyIsDown[(int)Keys.F1] == false)
            {
                _debugState.Mode = Mode.FileLoad;
                _debounce = true;
                _keyIsDown[(int)Keys.F1] = true;
            }
        }
        else _keyIsDown[(int)Keys.F1] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F2))
        {
            if (_keyIsDown[(int)Keys.F2] == false)
            {
                _debugState.Mode = Mode.FileSave;
                _debounce = true;
                _keyIsDown[(int)Keys.F2] = true;
            }
        }
        else _keyIsDown[(int)Keys.F2] = false;


        if (Keyboard.GetState().IsKeyDown(Keys.F5)) // 'run' to the next breakpoint
        {
            if (_keyIsDown[(int)Keys.F5] == false)
            {
                _debugState.SingleStep = false;
                _debugState.NextStep = true;
                _keyIsDown[(int)Keys.F5] = true;
            }
        }
        else _keyIsDown[(int)Keys.F5] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F4)) // edit/toggle breakpoint
        {
            if (_keyIsDown[(int)Keys.F4] == false)
            {
                _keyIsDown[(int)Keys.F4] = true;
                //EditBreakpoint(_emulator.Cpu.PC);
            }
        }
        else _keyIsDown[(int)Keys.F4] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F12)) // reset the CPU, PC & debugger state (does NOT reset ram though)
        {
            if (_keyIsDown[(int)Keys.F12] == false)
            {
                _debugState.NeedReset = true;
                _keyIsDown[(int)Keys.F12] = true;
            }
        }
        else _keyIsDown[(int)Keys.F12] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.F11)) // settings
        {
            if (_keyIsDown[(int)Keys.F11] == false)
            {
                _debugState.Mode = Mode.Settings;
                _keyIsDown[(int)Keys.F11] = true;
            }
        }
        else _keyIsDown[(int)Keys.F11] = false;


        if (Keyboard.GetState().IsKeyDown(Keys.PageDown)) // move downwards (towards 0xFFFF) in memory
        {
            if (_keyIsDown[(int)Keys.PageDown] == false)
            {
                if (_debugState.MemoryAddress < (GameBoyHardware.END_OF_MEMORY - 1))
                {
                    _debugState.MemoryAddress = (ushort)(_debugState.MemoryAddress + 16);
                }
                _keyIsDown[(int)Keys.PageDown] = true;
            }
        }
        else _keyIsDown[(int)Keys.PageDown] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.PageUp)) // move upwards (towards 0x0000) in memory
        {
            if (_keyIsDown[(int)Keys.PageUp] == false)
            {
                if (_debugState.MemoryAddress > (GameBoyHardware.ROM_StartAddr + 15))
                {
                    _debugState.MemoryAddress = (ushort)(_debugState.MemoryAddress - 16);
                }
                _keyIsDown[(int)Keys.PageUp] = true;
            }
        }
        else _keyIsDown[(int)Keys.PageUp] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Tab)) // manual change of memory address
        {
            if (_keyIsDown[(int)Keys.Tab] == false)
            {
                _debugState.Mode = Mode.AddressEntry;
                _debounce = true;   // enforce debounce to avoid immediate re-entry etc
                _keyIsDown[(int)Keys.Tab] = true;
            }
        }
        else _keyIsDown[(int)Keys.Tab] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Enter)) // manual edit of memory address content 
        {
            if (_keyIsDown[(int)Keys.Enter] == false)
            {
                //_debugState.Mode = Mode.AddressEntry;
                //_debounce = true;   // enforce debounce to avoid immediate re-entry etc
                // should have an memory address input dialog here
                // just fix MemoryAddress to 0C00 for now
                //_debugState.MemoryAddress = 0xC000;
                //EditMemoryViewContent();
                _keyIsDown[(int)Keys.Enter] = true;
            }
        }
        else _keyIsDown[(int)Keys.Enter] = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // toggle hidden
        {
            if (_keyIsDown[(int)Keys.Escape] == false)
            {
                _keyIsDown[(int)Keys.Escape] = true;
                _debugState.Mode = Mode.Hidden;
                _debounce = true;   // enforce debounce to avoid immediate re-entry etc
            }
        }
        else _keyIsDown[(int)Keys.Escape] = false;

    }


    private void DebugPalette()
    {
        // Initialize a simple palette for testing
        _gameBoyPalette = new Color[4];
        _gameBoyPalette[0] = Color.White;      // Color 0: Lightest
        _gameBoyPalette[1] = Color.LightGray; // Color 1: Lighter gray
        _gameBoyPalette[2] = Color.DarkGray;   // Color 2: Darker gray
        _gameBoyPalette[3] = Color.Black;      // Color 3: Darkest

    }
    private void InitializeDebugTiles()
    {
        // Allocate space for all possible Game Boy tiles (384 of them)
        // We'll create 8x8 textures for each.
        _debugTileTextures = new Texture2D[GameBoyHardware.MAX_TILES];
        for (int i = 0; i < _debugTileTextures.Length; i++)
        {
            _debugTileTextures[i] = new Texture2D(_graphicsDevice, 8, 8);
            _tileDataChanged[i] = true;
        }
    }

    private void HandleVRAMWrite(ushort address)
    {
        // When VRAM changes, we need to refresh the relevant tile texture
        // Each tile is 16 bytes, starting at 0x8000
        // So, address 0x8000 is tile 0, 0x8010 is tile 1, etc.
        // (address - 0x8000) / 16 gives us the tile index
        if (address >= GameBoyHardware.VRAM_StartAddr && address <= GameBoyHardware.VRAM_EndAddr) //0x97FF)
        {
            int tileIndex = (address - GameBoyHardware.VRAM_StartAddr) / 16;
            if (tileIndex >= 0 && tileIndex < _debugTileTextures.Length)
            {
                // Only update the specific tile that changed
                UpdateDebugTile(tileIndex);
            }
        }
    }


    private void FillVRAMTest()
    {

        byte[] debugTileBox = new byte[]
        {
            0xFF, 0xFF, // Row 0: All ones (color 3 - black)
            0x81, 0x81, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x81, 0x81, // Row 2
            0x81, 0x81, // Row 3
            0x81, 0x81, // Row 4
            0x81, 0x81, // Row 5
            0x81, 0x81, // Row 6
            0xFF, 0xFF  // Row 7
        };
        byte[] debugTileFlipBox = new byte[]
        {
            0x00, 0x00, // Row 0: All ones (color 3 - black)
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x00, 0x00  // Row 7
        };
        byte[] debugTileDataDiagonal = new byte[]
        {
            0x80, 0x80, // Row 0
            0x40, 0x40, // Row 1
            0x20, 0x20, // Row 2
            0x10, 0x10, // Row 3
            0x08, 0x08, // Row 4
            0x04, 0x04, // Row 5
            0x02, 0x02, // Row 6
            0x01, 0x01  // Row 7
        };
        byte[] debugTileDataDiagonalFlip = new byte[]
        {
            0x01, 0x01,  // Row 7
            0x02, 0x02, // Row 6
            0x04, 0x04, // Row 5
            0x08, 0x08, // Row 4
            0x10, 0x10, // Row 3
            0x20, 0x20, // Row 2
            0x40, 0x40, // Row 1
            0x80, 0x80, // Row 0
        };
        for (int i = 0; i < GameBoyHardware.MAX_TILES; i = i + 4)
        {
            ushort baseAddr = (ushort)(GameBoyHardware.VRAM_StartAddr + (i * 16));
            for (ushort offs = 0; offs < 16; offs++)
            {
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs), debugTileBox[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 16), debugTileDataDiagonal[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 32), debugTileFlipBox[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 48), debugTileDataDiagonalFlip[offs]);
            }
        }
    }



}
