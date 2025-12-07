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
    public const int CPU_STATE_ID = 3;
    public const int DEBUGGER_STATE_ID = 4;

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
    public const int INSTR_06_ID = 25;
    public const int INSTR_07_ID = 26;

    public const int IO_JOY_ID = 30;
    public const int IO_SER_ID = 31;
    public const int IO_TIM_ID = 32;
    public const int IO_INT_ID = 33;
    
    public const int IO_AUD1_ID = 34;
    public const int IO_AUD2_ID = 35;
    public const int IO_AUD3_ID = 36;
    public const int IO_AUD4_ID = 37;

    public const int IO_LCD1_ID = 38;
    public const int IO_LCD2_ID = 39;
    public const int IO_LCD3_ID = 40;

    public const int MEM1_ID = 50;
    public const int MEM2_ID = 51;
    public const int MEM3_ID = 52;
    public const int MEM4_ID = 53;
    public const int MEM5_ID = 54;

    public const int MOUSE_ID = 60;

}
