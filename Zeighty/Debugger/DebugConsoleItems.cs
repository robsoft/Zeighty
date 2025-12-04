using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Zeighty.Debugger;

public class DebugConsoleItems()
{
    private List<DebugConsoleItem> _items = new List<DebugConsoleItem>();

    public void Add(int x, int y, string text, int id)
    {
        if (_items.Any(i => i.ID == id))
        {
            // Update existing item
            var existingItem = _items.First(i => i.ID == id);
            existingItem.X = x;
            existingItem.Y = y;
            existingItem.Text = text;
        }
        else
        {
            // Add new item
            _items.Add(new DebugConsoleItem() { X = x, Y = y, Text = text, ID = id });
        }
    }
    public void Add(int x, int y, string text, int id, Color color)
    {
        if (_items.Any(i => i.ID == id))
        {
            // Update existing item
            var existingItem = _items.First(i => i.ID == id);
            existingItem.X = x;
            existingItem.Y = y;
            existingItem.Text = text;
            existingItem.Color = color;
        }
        else
        {
            // Add new item
            _items.Add(new DebugConsoleItem() { X = x, Y = y, Text = text, ID = id, Color = color });
        }
    }

    public void Remove(int id)
    {
        var item = _items.FirstOrDefault(i => i.ID == id);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public List<DebugConsoleItem> GetItems()
    {
        return _items;
    }
    public DebugConsoleItem? GetItemById(int ID)
    {             
        return _items.FirstOrDefault(i => i.ID == ID);
    }

}
