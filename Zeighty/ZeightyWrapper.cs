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
    private int _scaleFactor = 2;
    private RenderTarget2D _mainRenderTarget;
    private Rectangle _screenDestinationRectangle;

    private bool _havePressed = false;

    public ZeightyGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.IsFullScreen = false; // Usually default, but good to be explicit
        //_graphics.HardwareModeSwitch = false; // Only relevant if IsFullScreen is true
        _graphics.ApplyChanges();

    }

    protected override void Initialize()
    {
        // we are building to an 800x600 area but want to scale up to double this for display
        
        // --- Initialize RenderTarget2D and screen rectangle ---
        // Create the RenderTarget2D with our internal resolution
        _mainRenderTarget = new RenderTarget2D(
            GraphicsDevice,
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight,
            false, // MipMap
            SurfaceFormat.Color,
            DepthFormat.None,
            0, // MultiSampleCount
            RenderTargetUsage.PreserveContents // Essential if you draw piecemeal
        );

        // Calculate the destination rectangle on the *actual* screen
        int scaledWidth = _graphics.PreferredBackBufferWidth * _scaleFactor;
        int scaledHeight = _graphics.PreferredBackBufferHeight * _scaleFactor;

        // Update the actual back buffer size to accommodate the scaled content
        _graphics.PreferredBackBufferWidth = scaledWidth;
        _graphics.PreferredBackBufferHeight = scaledHeight;
        _graphics.ApplyChanges();

        _screenDestinationRectangle = new Rectangle( 0, 0, scaledWidth, scaledHeight );

        int pixelScaleFactor = 2; // Scale factor for doubling

        // Original GB is 160x144. Doubled is 320x288.
        int gbDisplayWidth = 160 * pixelScaleFactor;
        int gbDisplayHeight = 144 * pixelScaleFactor;

        int gbDisplayX = 0;
        int gbDisplayY = 0;

        Rectangle gameBoyScreenRectangle = new Rectangle(gbDisplayX, gbDisplayY, gbDisplayWidth, gbDisplayHeight);

        // Debugger Console Area - bottom of the screen, full width
        int debugConsoleHeight = _graphics.PreferredBackBufferHeight - (gbDisplayY + gbDisplayHeight);
        if (debugConsoleHeight < 100) debugConsoleHeight = 100; // Ensure min height
        Rectangle debugConsoleRectangle = new Rectangle(0, gbDisplayY + gbDisplayHeight, 800, debugConsoleHeight);
        
        // the tilemap viewer is over to the right of the gameboy display
        Rectangle tilemapRectangle = new Rectangle(gbDisplayWidth, 0, 800 - gbDisplayWidth, gbDisplayHeight);

        base.Initialize();

        _debugState = new GameBoyDebugState(); // Create debug state
        _emulator = new GameBoyEmulator(GraphicsDevice, _debugFont, gameBoyScreenRectangle, _debugState);
        _debugConsole = new DebugConsole(GraphicsDevice, _debugFont, debugConsoleRectangle, 
            tilemapRectangle, pixelScaleFactor, _emulator, _debugState);


        // prepare for the main emulation loop
        _debugState.Reset();
        _emulator.Cpu.Reset();
        _emulator.Cpu.FetchInstructions();
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
    }


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
        // set the render target to our main off-screen buffer
        GraphicsDevice.SetRenderTarget(_mainRenderTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // do all of our drawing to the off-screen buffer here
        _debugConsole.Draw(_spriteBatch, gameTime);
        _emulator.Draw(_spriteBatch, gameTime);

        _spriteBatch.End();


        // switch back to the default back buffer (the screen) ---
        GraphicsDevice.SetRenderTarget(null);

        // clear the actual screen ---
        GraphicsDevice.Clear(Color.Black);

        // draw the RenderTarget to the screen, scaled ---
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp); // Use PointClamp for pixel-perfect scaling!
        _spriteBatch.Draw(_mainRenderTarget, _screenDestinationRectangle, Color.White);
        _spriteBatch.End();

        // done
        base.Draw(gameTime);
    }
}

