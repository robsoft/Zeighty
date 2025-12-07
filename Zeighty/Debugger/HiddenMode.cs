using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class HiddenMode : BaseMode
{
    private const int CAPTION_ID = 1;

    public HiddenMode(DebugConsole console) : base(console)
    {
    }

    public override void Init()
    {
        ResetKeys();
        _debounce = true;
    }

    public override void Update(GameTime gameTime)
    {
        DefaultUpdate(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(_console.BackgroundTexture, _baseArea, Color.Black);
    }

    public override void KeyHandler()
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            if (_keyIsDown[(int)Keys.Escape] == false)
            {
                _debugState.Mode = Mode.Debug;
                _keyIsDown[(int)Keys.Escape] = true;
            }
        }
        else _keyIsDown[(int)Keys.Escape] = false;

    }
}
