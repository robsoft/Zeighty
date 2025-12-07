using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Data;
using System.Linq;
using System.Text;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public abstract class BaseMode
{
    protected DebugConsole _console;
    protected GraphicsDevice _graphicsDevice;
    protected IEmulator _emulator;
    protected GameBoyDebugState _debugState;
    protected Rectangle _baseArea;
    protected SpriteFont _spritefont;

    private DebugConsoleItems _items;

    protected bool[] _keyIsDown;
    protected bool _debounce = true;

    public DebugConsoleItems Items => _items;

    public BaseMode(DebugConsole console)
    {
        _console = console;
        _graphicsDevice = console.GraphicsDevice;
        _emulator = console.Emulator;
        _debugState = console.DebugState;
        _spritefont = console.SpriteFont;
        _baseArea = console.Area;

        _items = new DebugConsoleItems();

        SetupKeyHandler();
    }

    public void DefaultDraw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_console.BackgroundTexture, _baseArea, Color.Black); // Solid black
        foreach (var item in Items.GetItems())
        {
            spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_baseArea.X + item.X, _baseArea.Y + item.Y), item.Color);
        }
    }
    public void DefaultUpdate(GameTime gameTime)
    {
        if (_debounce && NeedDebounce()) return;
        _debounce = false;
        KeyHandler();
    }
    private void SetupKeyHandler()
    {
        Array allKeys = Enum.GetValues(typeof(Keys));

        // Find the maximum integer value among the Keys enum.
        // This ensures our array is large enough to hold all possible key states
        // by index. If the enum values were very sparse, this would waste memory.
        // Fortunately, Keys enum is fairly dense.
        int maxKeyValue = allKeys.Cast<int>().Max();

        // Initialize the arrays
        _keyIsDown = new bool[maxKeyValue + 1];
    }

    public void ResetKeys()
    {
        for (int i = 0; i < _keyIsDown.Length; i++)
        {
            _keyIsDown[i] = false;
        }
    }

    public bool NeedDebounce()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.GetPressedKeys().Length == 0)
        {
            // No keys are currently pressed, reset all states
            ResetKeys();
            return false; // No new key presses detected
        }
        return true;
    }

    public abstract void Init();

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

    public abstract void KeyHandler();


    public string FormatBytesString(ushort startAddress, int length)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"${startAddress:X4}: ");
        for (ushort i = 0; i < length; i++)
        {
            sb.Append($"{_emulator.Cpu.Memory.ReadByte((ushort)(startAddress + i)):X2} ");
        }
        return sb.ToString().TrimEnd();
    }

}
