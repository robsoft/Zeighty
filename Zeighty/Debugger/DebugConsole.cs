using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    private Mode _lastMode = Mode.None;

    private DebugMode _debugMode;
    private FileLoadMode _fileLoadMode;
    private FileSaveMode _fileSaveMode;
    private SettingsMode _settingsMode;
    private AddressMode _addressMode;
    private HiddenMode _hiddenMode;

    private BaseMode _currentMode;

    public DebugConsole(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Rectangle debugArea,
        Rectangle tilemapArea, int scaleFactor, IEmulator emulator, GameBoyDebugState debugState)
    {
        _graphicsDevice = graphicsDevice;
        _spritefont = spriteFont;
        _area = debugArea;
        _emulator = emulator;
        _debugState = debugState;

        _tileArea = tilemapArea;
        _scaleFactor = scaleFactor;

        _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });

        _debugState.Mode = Mode.Debug;
        SwitchMode();
    }

    public void SwitchMode()
    {
        _lastMode = _debugState.Mode;

        // When switching modes, initialize the new mode
        switch (_debugState.Mode)
        {
            case Mode.Debug:
                if (_debugMode == null) {
                    _debugMode = new DebugMode(this, _tileArea, _scaleFactor);
                }
                _currentMode = _debugMode;
                break;
            case Mode.FileLoad:
                if (_fileLoadMode == null) {
                    _fileLoadMode = new FileLoadMode(this);
                }
                _currentMode = _fileLoadMode;
                break;
            case Mode.FileSave:
                if (_fileSaveMode == null) {
                    _fileSaveMode = new FileSaveMode(this);
                }
                _currentMode = _fileSaveMode;
                break;
            case Mode.Settings:
                if (_settingsMode == null) {
                    _settingsMode = new SettingsMode(this);
                }
                _currentMode = _settingsMode;
                break;
            case Mode.AddressEntry:
                if (_addressMode == null) {
                    _addressMode = new AddressMode(this);
                }
                _currentMode = _addressMode;
                break;
            case Mode.Hidden:
                if (_hiddenMode == null) {                    
                    _hiddenMode = new HiddenMode(this);
                }
                _currentMode = _hiddenMode;
                break;
        }
        _currentMode.Init();
    }

    public void Update(GameTime gameTime)
    {
        _currentMode.Update(gameTime);

        if (_debugState.Mode != _lastMode)
        {
            SwitchMode();
        }
    }


    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _currentMode.Draw(spriteBatch, gameTime);
    }


}
