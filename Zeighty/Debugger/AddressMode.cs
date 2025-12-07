using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Emulator;
using Zeighty.Interfaces;

namespace Zeighty.Debugger
{
    public class AddressMode : BaseMode
    {
        private const int CAPTION_ID = 1;

        public AddressMode(DebugConsole console) : base(console)
        {
            
        }

        public override void Init()
        {
            Items.Clear();
            Items.Add(200, 120, "Enter new address:", CAPTION_ID);
            _debounce = true;
        }
        public override void Update(GameTime gameTime)
        {
            if (_debounce && NeedDebounce())
                return;
            _debounce = false;

            KeyHandler();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_console.BackgroundTexture, _baseArea, Color.Black); // Solid black

            foreach (var item in Items.GetItems())
            {
                spriteBatch.DrawString(_spritefont, item.Text, new Vector2(_baseArea.X + item.X, _baseArea.Y + item.Y), item.Color);
            }
        }

        public override void KeyHandler()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter)) // manual edit of memory address content 
            {
                if (_keyIsDown[(int)Keys.Enter] == false)
                {
                    // should have an memory address input dialog here
                    // just fix MemoryAddress to 0C00 for now
                    _debugState.Mode = Mode.Debug;
                    _debugState.MemoryAddress = 0xC000;
                    _keyIsDown[(int)Keys.Enter] = true;
                }
            }
            else _keyIsDown[(int)Keys.Enter] = false;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // manual edit of memory address content 
            {
                if (_keyIsDown[(int)Keys.Escape] == false)
                {
                    // should have an memory address input dialog here
                    // just fix MemoryAddress to 0C00 for now
                    _debugState.Mode = Mode.Debug;
                    _keyIsDown[(int)Keys.Escape] = true;
                }
            }
            else _keyIsDown[(int)Keys.Escape] = false;


        }

    }
}
