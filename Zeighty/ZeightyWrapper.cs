using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zeighty.Debugger;
using Zeighty.Emulator;

namespace Zeighty;


public class ZeightyGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _debugFont;

    private DebugConsole _debugConsole;
    private GameBoyEmulator _emulator;
    private GameBoyDebugState _debugState;

    private RenderTarget2D _mainRenderTarget;
    private Rectangle _screenDestinationRectangle;

    // this is the relationship between our design (800x600) and actual screen resolution (eg 1600x1200)
    private int _designResToScreenResFactor = 1; // we could make this a float, but we might introduce some weird rounding issues

    private const int DEFAULT_SCREEN_WIDTH = 800;
    private const int DEFAULT_SCREEN_HEIGHT = 600;

    private readonly ILogger<ZeightyGame> _log;

    public ZeightyGame(ILogger<ZeightyGame> log)
    {
        _log = log;
        _log.LogInformation("Starting Zeighty...");

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = DEFAULT_SCREEN_WIDTH;
        _graphics.PreferredBackBufferHeight = DEFAULT_SCREEN_HEIGHT;
        _graphics.IsFullScreen = false; // Usually default, but good to be explicit
        //_graphics.HardwareModeSwitch = false; // Only relevant if IsFullScreen is true
        _graphics.ApplyChanges();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        _log.LogInformation("Zeighty exiting");
        base.OnExiting(sender, args);
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
        int scaledWidth = _graphics.PreferredBackBufferWidth * _designResToScreenResFactor;
        int scaledHeight = _graphics.PreferredBackBufferHeight * _designResToScreenResFactor;

        // Update the actual back buffer size to accommodate the scaled content
        _graphics.PreferredBackBufferWidth = scaledWidth;
        _graphics.PreferredBackBufferHeight = scaledHeight;
        _graphics.ApplyChanges();

        _screenDestinationRectangle = new Rectangle(0, 0, scaledWidth, scaledHeight);

        int pixelScaleFactor = 2; // Scale factor for doubling

        // Original GB is 160x144. Doubled is 320x288.
        int gbDisplayWidth = 160 * pixelScaleFactor;
        int gbDisplayHeight = 144 * pixelScaleFactor;

        // this is the 'offset' down into the overall window
        int marginX = 4;
        int marginY = 4;

        Rectangle gameBoyScreenRectangle = new Rectangle(marginX, marginY, gbDisplayWidth, gbDisplayHeight);

        // Debugger Console Area - bottom of the screen, full width, with a similar offset
        int debugConsoleHeight = _graphics.PreferredBackBufferHeight - gbDisplayHeight - marginY - marginY - marginY;
        if (debugConsoleHeight < 100) debugConsoleHeight = 100; // Ensure min height
        Rectangle debugConsoleRectangle = new Rectangle(marginX, gbDisplayHeight + marginY + marginY,
            DEFAULT_SCREEN_WIDTH - marginX - marginX, debugConsoleHeight);

        // the tilemap viewer is over to the right of the gameboy display; we have a margin on all sides, but only
        // a single margin between the right of the gameboy display and the tilemap viewer, and the bottom of the
        // gameboy display and the debug output
        Rectangle tilemapRectangle = new Rectangle(marginX + gbDisplayWidth + marginX, marginY,
            DEFAULT_SCREEN_WIDTH - gbDisplayWidth - marginX - marginX - marginX, gbDisplayHeight);

        base.Initialize();

        _debugState = new GameBoyDebugState();
        _emulator = new GameBoyEmulator(GraphicsDevice, _debugFont, gameBoyScreenRectangle, _debugState);

        _debugConsole = new DebugConsole(GraphicsDevice, _debugFont, debugConsoleRectangle,
            tilemapRectangle, pixelScaleFactor, _emulator, _debugState);

        // the console needs to know about the overall screen and position, for decoding mouse input
        _debugConsole.SetScreenInfo(new Rectangle(0, 0, scaledWidth, scaledHeight), _designResToScreenResFactor);

        // prepare for the main emulation loop - will need to refactor some of this when we start loading proper carts etc
        _debugState.Reset();
        _debugState.MemoryAddress = GameBoyHardware.WRAM_StartAddr;

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
        bool skipCpu = false;

        //TODO: DMA not working!
        if (_emulator.Memory.IsDmaTransferActive)
        {
            _emulator.Memory.DmaCyclesRemaining--;
            if (_emulator.Memory.DmaCyclesRemaining <= 0)
            {
                _emulator.Memory.IsDmaTransferActive = false;
            }
            else
                skipCpu = true;
            // CPU effectively does nothing during this time, just decrements counter.
            // We need to make sure the PPU/other components still get their cycles.
        }

        //TODO: see ChipAte - we need to execute as many instructions as we can in a slice of time,
        // based on the original clock speed of the GameBoy (4.19MHz) vs how much time has passed
        // and then move on everything else to fit that number of cycles.
        if (!skipCpu)
        {
            // find out our next instructions (we will need this in the debugger if the view is going to refresh)
            _emulator.Cpu.FetchInstructions();

            // assume we're in full-step (not single-step) mode
            _debugState.NextStep = false;

            // can we grab the mouse
            if ((_debugConsole.DebugState.Mode == Interfaces.Mode.Debug) && _debugConsole.DebugState.SingleStep)
            {
                MouseState currentMouseState = Mouse.GetState();
                int mouseX = currentMouseState.X;
                int mouseY = currentMouseState.Y;

                int renderTargetMouseX = -1; // Default to outside
                int renderTargetMouseY = -1;

                if (mouseX >= _screenDestinationRectangle.X && mouseX < _screenDestinationRectangle.X + _screenDestinationRectangle.Width &&
                    mouseY >= _screenDestinationRectangle.Y && mouseY < _screenDestinationRectangle.Y + _screenDestinationRectangle.Height)
                {
                    int relativeMouseX = mouseX - _screenDestinationRectangle.X;
                    int relativeMouseY = mouseY - _screenDestinationRectangle.Y;
                    renderTargetMouseX = (int)(relativeMouseX / (float)_designResToScreenResFactor);
                    renderTargetMouseY = (int)(relativeMouseY / (float)_designResToScreenResFactor);
                }
                // --- End Mouse Transform ---
                _debugConsole.DebugState.MouseX = renderTargetMouseX;
                _debugConsole.DebugState.MouseY = renderTargetMouseY;
            }

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

