using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class FileLoadMode : BaseMode
{
    private const int CAPTION_ID = 1;

    public FileLoadMode(DebugConsole console) : base(console)
    {
        Items.Clear();
        Items.Add(200, 120, "Enter file name:", CAPTION_ID);
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
        DefaultDraw(spriteBatch, gameTime);
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
