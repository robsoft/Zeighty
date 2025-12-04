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

    private GameBoyDebugState _debugState;

    public GameBoyMemory Memory;
    
    private GameBoyCpu _cpu;
    public ICpu Cpu => _cpu; // Shorthand for { get { return _cpu; } }

    //public GameBoyCpu Cpu;

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
        Memory = new GameBoyMemory(fakeRomData); // Create instance of memory
        Memory.FillVRAM(); // Fill VRAM with test data
        Memory.FillIO();

        _cpu = new GameBoyCpu(Memory); // Create instance of CPU
        _debugState = debugState;


        debugState.Memory.AddEntry(0x0110, "Subroutine", BreakpointType.Normal);
        debugState.Memory.AddEntry(0x010A, "end", BreakpointType.None);
        debugState.Memory.AddEntry(0x0100, "start", BreakpointType.None);

    }



    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_backgroundTexture, _area, Color.Gray);

    }

}

