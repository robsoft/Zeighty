using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zeighty.Interfaces;

namespace Zeighty.Debugger;

public class AddressMode : BaseMode
{
    private const int CAPTION_ID = 1;

    public AddressMode(DebugConsole console) : base(console)
    {
        Items.Clear();
        Items.Add(200, 120, "Enter new address:", CAPTION_ID);
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
*/


}
