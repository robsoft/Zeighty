using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zeighty.Emulator;

namespace Zeighty.Interfaces
{
    public interface IEmulator
    {
        ICpu Cpu { get; }
        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}