using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Emulator;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public partial class DebugConsole
{
    private GraphicsDevice _graphicsDevice;
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    private IEmulator _emulator;
    public IEmulator Emulator => _emulator;

    private SpriteFont _spritefont;
    public SpriteFont SpriteFont => _spritefont;

    private Rectangle _area;
    public Rectangle Area => _area;

//    private DebugConsoleItems _items;
    private Texture2D _backgroundTexture;
    public Texture2D BackgroundTexture => _backgroundTexture;

    private GameBoyDebugState _debugState;
    public GameBoyDebugState DebugState => _debugState;

    private int _scaleFactor;
    private Rectangle _tileArea;

    private DebugMode _debugMode;
    private FileLoadMode _fileLoadMode;
    private FileSaveMode _fileSaveMode;
    private SettingsMode _settingsMode;
    private AddressMode _addressMode;
    private HiddenMode _hiddenMode;
    private Mode _lastMode = Mode.None;

//    private bool[] _keyIsDown; // indexed by Keys enum integer value
//    private bool _debounce;

    //public DebugConsoleItems Items => _items;

    public DebugConsole(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Rectangle debugArea,
        Rectangle tilemapArea, int scaleFactor,
        IEmulator emulator, GameBoyDebugState debugState)
    {
        _graphicsDevice = graphicsDevice;
        _spritefont = spriteFont;
        _scaleFactor = scaleFactor;
        _area = debugArea;
        _tileArea = tilemapArea;
        _scaleFactor = scaleFactor;

        //_items = new DebugConsoleItems();
        _emulator = emulator;
        _debugState = debugState;
        _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });

        _debugState.Mode = Mode.Debug;
        SwitchMode();

    }
/*
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
*/
    public void SwitchMode()
    {
        _lastMode = _debugState.Mode;

        // When switching modes, initialize the new mode
        switch (_debugState.Mode)
        {
            case Mode.Debug:
                if (_debugMode == null)
                {
                    _debugMode = new DebugMode(this, _tileArea, _scaleFactor);
                }
                _debugMode.Init();
                break;
            case Mode.FileLoad:
                if (_fileLoadMode == null) {
                    _fileLoadMode = new FileLoadMode(this);
                }
                _fileLoadMode.Init();
                break;
            case Mode.FileSave:
                if (_fileSaveMode == null) {
                    _fileSaveMode = new FileSaveMode(this);
                }
                _fileSaveMode.Init();
                break;
            case Mode.Settings:
                if (_settingsMode == null) {
                    _settingsMode = new SettingsMode(this);
                }
                _settingsMode.Init();
                break;
            case Mode.AddressEntry:
                if (_addressMode == null) {
                    _addressMode = new AddressMode(this);
                }
                _addressMode.Init();
                break;
            case Mode.Hidden:
                if (_hiddenMode == null) {                    
                    _hiddenMode = new HiddenMode(this);
                }
                _hiddenMode.Init();
                break;
        }
    }

    public void Update(GameTime gameTime)
    {

        switch (_debugState.Mode)
        {
            case Mode.Debug:
                _debugMode.Update(gameTime);
                break;
                //return;
            case Mode.FileLoad:
                _fileLoadMode.Update(gameTime);
                break;
                //return;
            case Mode.FileSave:
                _fileSaveMode.Update(gameTime);
                break;
                //return;
            case Mode.Settings:
                _settingsMode.Update(gameTime);
                break;
                //return;
            case Mode.AddressEntry:
                _addressMode.Update(gameTime);
                break;
                //return;
            case Mode.Hidden:
                _hiddenMode.Update(gameTime);
                break;
                //return;
        }

        if (_debugState.Mode != _lastMode)
        {
            SwitchMode();
        }
    }


    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        switch (_debugState.Mode)
        {
            case Mode.Debug:
                _debugMode.Draw(spriteBatch, gameTime);
                return;
            case Mode.FileLoad:
                _fileLoadMode.Draw(spriteBatch, gameTime);
                return;
            case Mode.FileSave:
                _fileSaveMode.Draw(spriteBatch, gameTime);
                return;
            case Mode.Settings:
                _settingsMode.Draw(spriteBatch, gameTime);
                return;
            case Mode.AddressEntry:
                _addressMode.Draw(spriteBatch, gameTime);
                return;
            case Mode.Hidden:
                _hiddenMode.Draw(spriteBatch, gameTime);
                return;
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
