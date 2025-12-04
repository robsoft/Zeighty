using Microsoft.Xna.Framework;

namespace Zeighty.Debugger;

public class DebugConsoleItem()
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Text { get; set; }
    public int ID { get; set; }
    public Color Color { get; set; } = Color.White;
}
