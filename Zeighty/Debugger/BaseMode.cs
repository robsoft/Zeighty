using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeighty.Debugger
{
    public abstract class BaseMode
    {
        protected GraphicsDevice _graphicsDevice;
        protected GameBoyDebugState _debugState;
        protected Texture2D _backgroundTexture;
        protected Rectangle _baseArea;
        protected SpriteFont _spritefont;

        private int _scaleFactor = 2;
        private Rectangle _area;
        private DebugConsoleItems _items;

        protected bool[] _keyIsDown; // indexed by Keys enum integer value
        protected bool _debounce = true;

        public DebugConsoleItems Items => _items;

        public BaseMode(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Rectangle area, GameBoyDebugState debugState)
        {
            _graphicsDevice = graphicsDevice;
            _debugState = debugState;
            _spritefont = spriteFont;
            //_scaleFactor = scaleFactor;
            _baseArea = area;

            _items = new DebugConsoleItems();
            _backgroundTexture = new Texture2D(_graphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] { Color.White });

            SetupKeyHandler();

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
       
    }
}
