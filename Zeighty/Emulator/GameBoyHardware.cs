namespace Zeighty.Emulator
{
    internal static class GameBoyHardware
    {
        public static int MAX_TILES = 384;

        public static ushort IO_Joy = 0xFF00;
        public static ushort IO_Ser = 0xFF01;
        public static ushort IO_Tim = 0xFF04;
        public static ushort IO_Int = 0xFF0F;
        public static ushort IO_Aud = 0xFF10;
        public static ushort IO_Lcd = 0xFF40;
        public static ushort IO_End = 0xFF7F;

        public static ushort ROM_StartAddr = 0x0000;
        public static ushort ROM_EndAddr = 0x7FFF;
        public static ushort VRAM_StartAddr = 0x8000;
        public static ushort VRAM_EndAddr = 0x9FFF;
        public static ushort WRAM_StartAddr = 0xC000;
        public static ushort WRAM_EndAddr = 0xDFFF;
        public static ushort ERAM_StartAddr = 0xE000;
        public static ushort ERAM_EndAddr = 0xFDFF;
        public static ushort OAM_StartAddr = 0xFE00;
        public static ushort OAM_EndAddr = 0xFE9F;
        public static ushort IO_StartAddr = 0xFF00;
        public static ushort IO_EndAddr = 0xFF7F;
        public static ushort HRAM_StartAddr = 0xFF80;
        public static ushort HRAM_EndAddr = 0xFFFE;
        public static ushort END_OF_MEMORY = 0xFFFF;
    }
}