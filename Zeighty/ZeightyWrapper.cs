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
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();

        _debugState = new GameBoyDebugState(); // Create debug state
        _emulator = new GameBoyEmulator(GraphicsDevice, _debugFont, new Rectangle(0, 0, 320, 240), 
            _debugState);
        _debugConsole = new DebugConsole(GraphicsDevice, _debugFont, new Rectangle(0, 240, 640, 240),
            _emulator, _debugState);

        _debugConsole.Items.Add(0, 0, "Debug Console Initialized", 
         Zeighty.Debugger.DebugUIConstants.TITLE_ID, Color.LimeGreen);
   

        // Main emulation loop
        _debugState.Reset();

        _emulator.Cpu.Reset();

        _emulator.Cpu.FetchInstructions();

        //debugUI.StaticUI();


    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

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
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _debugConsole.Items.Add(10, 10, "Pressing a key", Zeighty.Debugger.DebugUIConstants.KEYBOARD_ID);
            _havePressed = true;
        }
        else
        {
            if (_havePressed == true)
            {
                _debugConsole.Items.Remove(Zeighty.Debugger.DebugUIConstants.KEYBOARD_ID);
                _havePressed = false;
            }
        }


        _emulator.Cpu.FetchInstructions();
        _debugState.NextStep = false;
        _debugConsole.Update(gameTime);
        if (_debugState.NextStep && !_emulator.Cpu.IsHalted)
        {
            _emulator.Cpu.ExecuteInstruction();
        }

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(); // Or use the same Begin/End from GameBoyScreen if layers are fine
                          // Draw a semi-transparent rectangle for the console background
                          // (You'll need a 1x1 white texture loaded/created for this)
    /*
                _spriteBatch.Draw(
                    _onePixelTexture, // A 1x1 white texture
                    new Rectangle(0, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2),
                    Color.Black * 0.7f // Make it semi-transparent black
                );
    */

        _debugConsole.Draw(_spriteBatch, gameTime);
        _emulator.Draw(_spriteBatch, gameTime);

    /*
                // Draw text
                Vector2 textPosition = new Vector2(10, GraphicsDevice.Viewport.Height / 2 + 10);
                _spriteBatch.DrawString(_debugFont, "Hello Debugger!", textPosition, Color.White);

                textPosition.Y += _debugFont.LineSpacing; // Move down for next line
                _spriteBatch.DrawString(_debugFont, $"PC: {0x1234:X4}", textPosition, Color.Yellow);
    */
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

