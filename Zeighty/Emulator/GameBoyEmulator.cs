using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Debugger;
using Zeighty.Interfaces;

namespace Zeighty.Emulator;


public class GameBoyEmulator : IEmulator
{
    private GraphicsDevice _graphicsDevice;
    private SpriteFont _spritefont;
    private Rectangle _area;
    private Texture2D _backgroundTexture;
    private Color[] _gameBoyPalette; // Our mapping from 2-bit color to MonoGame Color

    private GameBoyDebugState _debugState;

    public GameBoyMemory Memory;

    private GameBoyCpu _cpu;
    public ICpu Cpu => _cpu;


    private byte[] testRom = new byte[]
    {
        0x3E, 0x11,       // LD A, $11
        0xCD, 0x10, 0x01, // CALL $0110 (Subroutine)
        0x3E, 0x22,       // LD A, $22
        0xCD, 0x10, 0x01, // CALL $0110 (Subroutine)
        0x76,             // HALT
        0x00, 0x00, 0x00, // NOP NOP NOP (padding)
        0x00, 0x00,       // NOP NOP (padding)
        // Subroutine at 0x0110
        0xEA, 0x00, 0xC0, // LD [$C000], A
        0xC9              // RET
    };

    /*
        private byte[] testRom = new byte[]
        {
            0xF3,             // DI
            0x3E,             // LD A, (next byte)
            0x01,             //   0x01 (Source high byte for DMA: 0x0100)
            0xE0,             // LDH (0xFF00 + next byte), A
            0x46,             //   0x46 (DMA register 0xFF46)
            0x18,             // JR (next byte)
            0xFE                //   -2 (loop indefinitely at 0x0105)
        };
    */
    private byte[] fakeRomData;


    public GameBoyEmulator(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Rectangle area,
        GameBoyDebugState debugState)
    {
        _graphicsDevice = graphicsDevice;
        _spritefont = spriteFont;
        _area = area;

        _backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });

        fakeRomData = GameBoyHelpers.FakeCartridge(testRom, 0x0100);
        // Data to be copied (starting at 0x0107 within the ROM)
        // This will be copied to OAM (0xFE00-0xFE9F)
        /*
        for (int i = 0; i < 0xA0; i++) // 160 bytes
        {
            fakeRomData[0x0107 + i]=(byte)i; // Simple sequential data 0x00, 0x01, ..., 0x9F
        }
        */

        // Initialize a simple palette for testing
        _gameBoyPalette = new Color[4];
        _gameBoyPalette[0] = Color.White;      // Color 0: Lightest
        _gameBoyPalette[1] = Color.LightGray; // Color 1: Lighter gray
        _gameBoyPalette[2] = Color.DarkGray;   // Color 2: Darker gray
        _gameBoyPalette[3] = Color.Black;      // Color 3: Darkest

        Memory = new GameBoyMemory(fakeRomData);

        Memory.FillVRAM(); // Fill VRAM with test data
        //Memory.FillIO();

        _cpu = new GameBoyCpu(Memory);
        _debugState = debugState;

        // fake some breakpoint/debugger stuff
        debugState.Memory.AddEntry(0x0110, "Subroutine", BreakpointType.None);
        debugState.Memory.AddEntry(0x010A, "end", BreakpointType.None);
        debugState.Memory.AddEntry(0x0100, "start", BreakpointType.None);

    }



    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTexture, _area, Color.Gray);

    }


}

