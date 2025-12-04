using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using Zeighty.Debugger;
using Zeighty.Emulator;
using Zeighty.Interfaces;

namespace Zeighty;


public class ZeightyGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _debugFont;

    private DebugConsole _debugConsole;
    private GameBoyEmulator _emulator;
    private GameBoyDebugState _debugState;

    private bool _havePressed = false;

    public ZeightyGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 800; // Set desired width
        _graphics.PreferredBackBufferHeight = 600; // Set desired height
        // _graphics.IsFullScreen = false; // Usually default, but good to be explicit
        // _graphics.HardwareModeSwitch = false; // Only relevant if IsFullScreen is true
    }

    protected override void Initialize()
    {
        _graphics.ApplyChanges();

        // Now that GraphicsDevice is set up, you can calculate positions for your UI elements
        // Game Boy Display Area (top, centered horizontally)
        // Original GB is 160x144. Doubled is 320x288.
        const int gbDisplayWidth = 160 * 2; // 320
        const int gbDisplayHeight = 144 * 2; // 288

        // Centered horizontally
        //int gbDisplayX = (_graphics.PreferredBackBufferWidth - gbDisplayWidth) / 2;
        // want this to the left because we'll show tilemaps at the right
        int gbDisplayX = 0;
        int gbDisplayY = 0; // Starts at the top

        Rectangle gameBoyScreenRectangle = new Rectangle(gbDisplayX, gbDisplayY, gbDisplayWidth, gbDisplayHeight);

        // Debugger Console Area (bottom)
        // Let's say it takes up the bottom half of the remaining space (or a fixed height)
        int debugConsoleHeight = _graphics.PreferredBackBufferHeight - (gbDisplayY + gbDisplayHeight);
        if (debugConsoleHeight < 100) debugConsoleHeight = 100; // Ensure min height
        
        Rectangle debugConsoleRectangle = new Rectangle(0, gbDisplayY + gbDisplayHeight, _graphics.PreferredBackBufferWidth, debugConsoleHeight);

        base.Initialize();

        _debugState = new GameBoyDebugState(); // Create debug state

        _emulator = new GameBoyEmulator(GraphicsDevice, _debugFont, gameBoyScreenRectangle, _debugState);
        _debugConsole = new DebugConsole(GraphicsDevice, _debugFont, debugConsoleRectangle, _emulator, _debugState);

        // Main emulation loop
        _debugState.Reset();
        _emulator.Cpu.Reset();
        _emulator.Cpu.FetchInstructions();


    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont"); // Matches the name of your .spritefont file

    }


    /*
     * 
     while (true)
    {
        // Main emulation loop
        debugState.Reset();
    
        cpu.Reset();

        cpu.FetchInstructions();

        debugUI.StaticUI();

        while (true)
        {
            cpu.FetchInstructions();
            debugUI.UpdateScreen();
            if (debugUI.WaitStep())
            {
                if (!cpu.IsHalted)
                {
                    cpu.ExecuteInstruction();
                }
                else
                {
                    debugState.SingleStep = true;
                }

                debugUI.UpdateScreen();

                if (debugState.NeedReset)
                { break; }
            }
        }
    }


*/

    protected override void Update(GameTime gameTime)
    {
        // find out our next instructions (we will need this in the debugger if the view is going to refresh)
        _emulator.Cpu.FetchInstructions();

        // assume we're in full-step (not single-step) mode
        _debugState.NextStep = false;
        // this will adjust _debugState flags accordingly
        _debugConsole.Update(gameTime);

        // are we going to execute the next instruction, or not?
        if (_debugState.NextStep && !_emulator.Cpu.IsHalted)
        {
            _emulator.Cpu.ExecuteInstruction();
        }

        // have we signalled we need to reset?
        if (_debugState.NeedReset)
        {
            _debugState.Reset();
            _emulator.Cpu.Reset();
        }

        // any other housekeeping
        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        _debugConsole.Draw(_spriteBatch, gameTime);
        _emulator.Draw(_spriteBatch, gameTime);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

