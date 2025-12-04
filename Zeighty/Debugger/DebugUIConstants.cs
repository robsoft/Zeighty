using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeighty.Debugger;

public static class DebugUIConstants
{
    public const int DEFAULT_WIDTH = 320;
    public const int DEFAULT_HEIGHT = 240;

    public static Color BACKGROUND_COLOR = Color.Black;
    public static Color FOREGROUND_NORMAL_COLOR = Color.White;
    public static Color FOREGROUND_HIGHLIGHT_COLOR = Color.Yellow;
    public static Color FOREGROUND_LABEL_COLOR = Color.LightBlue;

    // IDs to help keep DisplayConsoleItems 'unique' for each kind of item
    public const int TITLE_ID = 1;
    public const int KEYBOARD_ID = 2;
    public const int REG_A_ID = 10;
    public const int REG_BC_ID = 11;
    public const int REG_DE_ID = 12;
    public const int REG_HL_ID = 13;
    public const int REG_F_ID = 14;
    public const int REG_SP_ID = 15;
    public const int REG_PC_ID = 16;

    public const int INSTR_01_ID = 20;
    public const int INSTR_02_ID = 21;
    public const int INSTR_03_ID = 22;
    public const int INSTR_04_ID = 23;
    public const int INSTR_05_ID = 24;



}
