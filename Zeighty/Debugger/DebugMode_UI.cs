using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using Zeighty.Emulator;

namespace Zeighty.Debugger;

public partial class DebugMode : BaseMode
{
    private void SetupConsoleItems()
    {
        Items.Add(0, 0, $"CART: {_debugState.LoadedFileName.ToUpper()}", DebugUIConstants.TITLE_ID, Color.LimeGreen);

        int lineHeight = _spritefont.LineSpacing;

        // pre-set up our display items
        int Regx = 10; int Regy = 30;
        Items.Add(Regx, Regy + (0 * lineHeight), " A: 00", DebugUIConstants.REG_A_ID);
        Items.Add(Regx, Regy + (1 * lineHeight), "BC: 00", DebugUIConstants.REG_BC_ID);
        Items.Add(Regx, Regy + (2 * lineHeight), "DE: 00", DebugUIConstants.REG_DE_ID);
        Items.Add(Regx, Regy + (3 * lineHeight), "HL: 00", DebugUIConstants.REG_HL_ID);
        Items.Add(Regx, Regy + (4 * lineHeight), "SP: 00", DebugUIConstants.REG_SP_ID);
        Items.Add(Regx, Regy + (5 * lineHeight), "PC: 00", DebugUIConstants.REG_PC_ID);
        Items.Add(Regx, Regy + (6 * lineHeight), " F: 00", DebugUIConstants.REG_F_ID);

        int Instrx = 110; int Instry = 30; Color instrColor = Color.Black;
        Items.Add(Instrx, Instry + (0 * lineHeight), "", DebugUIConstants.INSTR_01_ID, instrColor);
        Items.Add(Instrx, Instry + (1 * lineHeight), "", DebugUIConstants.INSTR_02_ID, instrColor);
        Items.Add(Instrx, Instry + (2 * lineHeight), "", DebugUIConstants.INSTR_03_ID, instrColor);
        Items.Add(Instrx, Instry + (3 * lineHeight), "", DebugUIConstants.INSTR_04_ID, instrColor);
        Items.Add(Instrx, Instry + (4 * lineHeight), "", DebugUIConstants.INSTR_05_ID, instrColor);
        Items.Add(Instrx, Instry + (5 * lineHeight), "", DebugUIConstants.INSTR_06_ID, instrColor);
        Items.Add(Instrx, Instry + (6 * lineHeight), "", DebugUIConstants.INSTR_07_ID, instrColor);

        Items.Add(320, 0, "CPU", DebugUIConstants.CPU_STATE_ID, Color.Yellow);
        Items.Add(590, 0, "DEBUG", DebugUIConstants.DEBUGGER_STATE_ID, Color.Yellow);

        int Iox = 590; int Ioy = 30;
        Items.Add(Iox, Ioy + (0 * lineHeight), $"${GameBoyHardware.IO_Joy:X4} JOY", DebugUIConstants.IO_JOY_ID);
        Items.Add(Iox, Ioy + (1 * lineHeight), $"${GameBoyHardware.IO_Ser:X4} SER", DebugUIConstants.IO_SER_ID);
        Items.Add(Iox, Ioy + (2 * lineHeight), $"${GameBoyHardware.IO_Tim:X4} TIM", DebugUIConstants.IO_TIM_ID);
        Items.Add(Iox, Ioy + (3 * lineHeight), $"${GameBoyHardware.IO_Int:X4} INT", DebugUIConstants.IO_INT_ID);
        Items.Add(Iox, Ioy + (4 * lineHeight), $"${GameBoyHardware.IO_Aud:X4} AUD", DebugUIConstants.IO_AUD1_ID);
        Items.Add(Iox, Ioy + (5 * lineHeight), $"${GameBoyHardware.IO_Aud + 4:X4}    ", DebugUIConstants.IO_AUD2_ID);
        Items.Add(Iox, Ioy + (6 * lineHeight), $"${GameBoyHardware.IO_Aud + 8:X4}    ", DebugUIConstants.IO_AUD3_ID);
        Items.Add(Iox, Ioy + (7 * lineHeight), $"${GameBoyHardware.IO_Aud + 12:X4}    ", DebugUIConstants.IO_AUD4_ID);
        Items.Add(Iox, Ioy + (8 * lineHeight), $"${GameBoyHardware.IO_Lcd:X4} LCD", DebugUIConstants.IO_LCD1_ID);
        Items.Add(Iox, Ioy + (9 * lineHeight), $"${GameBoyHardware.IO_Lcd + 4:X4}    ", DebugUIConstants.IO_LCD2_ID);
        Items.Add(Iox, Ioy + (10 * lineHeight), $"${GameBoyHardware.IO_Lcd + 8:X4}    ", DebugUIConstants.IO_LCD3_ID);

        int Memx = 10; int Memy = 200;
        Items.Add(Memx, Memy + (0 * lineHeight), $"${(ushort)(_debugState.MemoryAddress):X4} ", DebugUIConstants.MEM1_ID);
        Items.Add(Memx, Memy + (1 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 16):X4} ", DebugUIConstants.MEM2_ID);
        Items.Add(Memx, Memy + (2 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 32):X4} ", DebugUIConstants.MEM3_ID);
        Items.Add(Memx, Memy + (3 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 48):X4} ", DebugUIConstants.MEM4_ID);
        Items.Add(Memx, Memy + (4 * lineHeight), $"${(ushort)(_debugState.MemoryAddress + 64):X4} ", DebugUIConstants.MEM5_ID);

        TileItems.Add(10, _tileArea.Height - (2 * lineHeight), $"MOUSE ", DebugUIConstants.MOUSE_ID);
    }

    private void UpdateMouse()
    {
        string MouseText = "";
        
        // do we have anything to try and decode?
        if (_debugState.MouseX >= 0 && _debugState.MouseY >= 0)
        {
            int i = 0;
            while (i < GameBoyHardware.MAX_TILES)
            {
                // Calculate position in the tile area
                int row = i / _tilesPerRow;
                int col = i % _tilesPerRow;
                Rectangle tileRect = new Rectangle(
                    _tileArea.X + (col * 8 * _scaleFactor),
                    _tileArea.Y + (row * 8 * _scaleFactor),
                    8 * _scaleFactor,
                    8 * _scaleFactor
                );
                if (tileRect.Contains(_debugState.MouseX, _debugState.MouseY))
                {
                    // Mouse is over this tile
                    MouseText = $"TILE:{i}";
                    // todo - more debug info of the tile
                    break; // Exit loop once we've found the tile
                }
                i++;
            }
        }

        TileItems.GetItemById(DebugUIConstants.MOUSE_ID).Text = MouseText;
    }

    private void UpdateRegView()
    {
        Items.GetItemById(DebugUIConstants.REG_A_ID).Text = $" A: {_emulator.Cpu.A:X2}";
        Items.GetItemById(DebugUIConstants.REG_BC_ID).Text = $"BC: {_emulator.Cpu.B:X2} {_emulator.Cpu.C:X2}";
        Items.GetItemById(DebugUIConstants.REG_DE_ID).Text = $"DE: {_emulator.Cpu.D:X2} {_emulator.Cpu.E:X2}";
        Items.GetItemById(DebugUIConstants.REG_HL_ID).Text = $"HL: {_emulator.Cpu.H:X2} {_emulator.Cpu.L:X2}";
        Items.GetItemById(DebugUIConstants.REG_PC_ID).Text = $"PC: {_emulator.Cpu.PC:X4}";
        Items.GetItemById(DebugUIConstants.REG_SP_ID).Text = $"SP: {_emulator.Cpu.SP:X4}";

    }

    private void UpdateDisassembly()
    {
        string[] instr = new string[7];
        for (int i = 0; i < 7; i++)
        {
            var addr = $"${_emulator.Cpu.Instructions[i].Address:X4}";
            var decoded = _emulator.Cpu.Instructions[i].Decoded;
            var desc = _debugState.Memory.GetAddressDescription(_emulator.Cpu.Instructions[i].Address);
            if (decoded.Contains('$'))
            {
                // get a ushort from the 4 chars following the $
                var dollarIndex = decoded.IndexOf('$');
                if (dollarIndex >= 0)
                {
                    var hexPart = decoded.Substring(dollarIndex + 1, 4);
                    if (ushort.TryParse(hexPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort destAddr))
                    {
                        var resolvedDest = _debugState.Memory.GetAddressDescription(destAddr);
                        if (!string.IsNullOrEmpty(resolvedDest))
                        {
                            decoded = decoded.Replace($"${hexPart}", resolvedDest);
                        }
                    }
                }
            }
            instr[i] = $"{desc,10} {addr} : {decoded,-20} {_emulator.Cpu.Instructions[i].DecodedBytes,-12}";
        }

        Items.GetItemById(DebugUIConstants.INSTR_01_ID).Text = instr[0];
        Items.GetItemById(DebugUIConstants.INSTR_02_ID).Text = instr[1];
        Items.GetItemById(DebugUIConstants.INSTR_03_ID).Text = instr[2];
        Items.GetItemById(DebugUIConstants.INSTR_04_ID).Text = instr[3];
        Items.GetItemById(DebugUIConstants.INSTR_05_ID).Text = instr[4];
        Items.GetItemById(DebugUIConstants.INSTR_06_ID).Text = instr[5];
        Items.GetItemById(DebugUIConstants.INSTR_07_ID).Text = instr[6];
    }

    private void UpdateMemoryView()
    {
        ushort addr = _debugState.MemoryAddress;
        Items.GetItemById(DebugUIConstants.MEM1_ID).Text = FormatBytesString(addr, 16);
        Items.GetItemById(DebugUIConstants.MEM2_ID).Text = FormatBytesString((ushort)(addr + 16), 16);
        Items.GetItemById(DebugUIConstants.MEM3_ID).Text = FormatBytesString((ushort)(addr + 32), 16);
        Items.GetItemById(DebugUIConstants.MEM4_ID).Text = FormatBytesString((ushort)(addr + 48), 16);
        Items.GetItemById(DebugUIConstants.MEM5_ID).Text = FormatBytesString((ushort)(addr + 64), 16);
    }
    private void UpdateIOView()
    {
        // joy is the byte and then the bits, easier to read presses
        byte JOY = _emulator.Cpu.Memory.ReadByte(GameBoyHardware.IO_Joy);

        Items.GetItemById(DebugUIConstants.IO_JOY_ID).Text = $"${GameBoyHardware.IO_Joy:X4} JOY {JOY:X2} {Convert.ToString(JOY, 2).PadLeft(8, '0')}";

        byte SER1 = _emulator.Cpu.Memory.ReadByte(GameBoyHardware.IO_Ser);
        byte SER2 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyHardware.IO_Ser + 1));
        Items.GetItemById(DebugUIConstants.IO_SER_ID).Text = $"${GameBoyHardware.IO_Ser:X4} SER {SER1:X2} {SER2:X2}";

        byte TIM1 = _emulator.Cpu.Memory.ReadByte(GameBoyHardware.IO_Tim);
        byte TIM2 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyHardware.IO_Tim + 1));
        byte TIM3 = _emulator.Cpu.Memory.ReadByte((ushort)(GameBoyHardware.IO_Tim + 2));
        Items.GetItemById(DebugUIConstants.IO_TIM_ID).Text = $"${GameBoyHardware.IO_Tim:X4} TIM {TIM1:X2} {TIM2:X2} {TIM3:X2}";

        byte INT = _emulator.Cpu.Memory.ReadByte(GameBoyHardware.IO_Int);
        Items.GetItemById(DebugUIConstants.IO_INT_ID).Text = $"${GameBoyHardware.IO_Int:X4} INT {INT:X2}";

        uint value = _emulator.Cpu.Memory.ReadDoubleUWord(GameBoyHardware.IO_Aud);
        byte byte0 = (byte)(value & 0xFF);
        byte byte1 = (byte)((value >> 8) & 0xFF);
        byte byte2 = (byte)((value >> 16) & 0xFF);
        byte byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD1_ID).Text = $"${GameBoyHardware.IO_Aud:X4} AUD {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Aud + 4));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD2_ID).Text = $"${GameBoyHardware.IO_Aud + 4:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Aud + 8));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD3_ID).Text = $"${GameBoyHardware.IO_Aud + 8:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Aud + 12));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_AUD4_ID).Text = $"${GameBoyHardware.IO_Aud + 12:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Lcd));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD1_ID).Text = $"${GameBoyHardware.IO_Lcd:X4} LCD {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Lcd + 4));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD2_ID).Text = $"${GameBoyHardware.IO_Lcd + 4:X4}     {byte0:X2} {byte1:X2} {byte2:X2} {byte3:X2}";

        value = _emulator.Cpu.Memory.ReadDoubleUWord((ushort)(GameBoyHardware.IO_Lcd + 8));
        byte0 = (byte)(value & 0xFF);
        byte1 = (byte)((value >> 8) & 0xFF);
        byte2 = (byte)((value >> 16) & 0xFF);
        //byte3 = (byte)((value >> 24) & 0xFF);
        Items.GetItemById(DebugUIConstants.IO_LCD3_ID).Text = $"${GameBoyHardware.IO_Lcd + 8:X4}     {byte0:X2} {byte1:X2} {byte2:X2}";
    }


    private void UpdateFlagView()
    {
        // Access the F register value
        byte flags = _emulator.Cpu.F;

        // Use bitwise operations to check each flag bit
        // (flags & (1 << bitPosition)) checks if the bit at bitPosition is set
        // Then use a conditional operator (ternary operator) to output '1' or '0'

        char zeroFlag = ((flags & (1 << 7)) != 0) ? '1' : '0'; // Bit 7
        char subtractFlag = ((flags & (1 << 6)) != 0) ? '1' : '0'; // Bit 6
        char halfCarryFlag = ((flags & (1 << 5)) != 0) ? '1' : '0'; // Bit 5
        char carryFlag = ((flags & (1 << 4)) != 0) ? '1' : '0'; // Bit 4

        // The lower 4 bits are always 0 on Game Boy, so we can just hardcode '0' for them.
        char bit3 = '0';
        char bit2 = '0';
        char bit1 = '0';
        char bit0 = '0';

        // Now, use string interpolation to display them
        // Example: F: 0xFX (Z N H C 0 0 0 0)
        // You might also want a quick text indicator of which flags are set
        string flagIndicators = $"{(zeroFlag == '1' ? "Z" : "-")}" +
                                $"{(subtractFlag == '1' ? "N" : "-")}" +
                                $"{(halfCarryFlag == '1' ? "H" : "-")}" +
                                $"{(carryFlag == '1' ? "C" : "-")}";
        //return $" F: {flags:X2} {zeroFlag}{subtractFlag}{halfCarryFlag}{carryFlag}{bit3}{bit2}{bit1}{bit0} {flagIndicators}";
        // B0:1 0 1 1 0 0 0 0:ZHC-
        Items.GetItemById(DebugUIConstants.REG_F_ID).Text = $" F: {flagIndicators}";
    }

    private void UpdateStateView()
    {
        string cpuText = "READY";
        if (_emulator.Cpu.IsHalted)
        {
            cpuText = "HALTED";
            _debugState.SingleStep = true; // force single-step when halted
        }

        string stateText = "  RUNNING  ";
        if (_debugState.InBreakpoint)
            stateText = " BREAKPOINT";
        else if (_debugState.SingleStep)
            stateText = "SINGLE-STEP";

        Items.GetItemById(DebugUIConstants.CPU_STATE_ID).Text = $"CPU: {cpuText}";
        Items.GetItemById(DebugUIConstants.DEBUGGER_STATE_ID).Text = $"DEBUG: {stateText}";
    }

    // New method to update a single specific debug tile texture
    private void UpdateDebugTile(int tileIndex)
    {
        ushort tileAddress = (ushort)(GameBoyHardware.VRAM_StartAddr + (tileIndex * 16));
        Color[] pixelData = new Color[8 * 8];

        // Decode tile data from memory (0x8000 + tileIndex * 16)
        for (int y = 0; y < 8; y++)
        {
            byte lsbByte = _emulator.Cpu.Memory.ReadByte((ushort)(tileAddress + (y * 2)));
            byte msbByte = _emulator.Cpu.Memory.ReadByte((ushort)(tileAddress + (y * 2) + 1));

            for (int x = 0; x < 8; x++)
            {
                int lsb = (lsbByte >> (7 - x)) & 1;
                int msb = (msbByte >> (7 - x)) & 1;
                int colorIndex = (msb << 1) | lsb;
                pixelData[(y * 8) + x] = _gameBoyPalette[colorIndex];
            }
        }
        _debugTileTextures[tileIndex].SetData(pixelData);
        _tileDataChanged[tileIndex] = true;
    }


    // Method to update all tiles (e.g., on initial load)
    public void UpdateAllDebugTiles()
    {
        for (int i = 0; i < _debugTileTextures.Length; i++)
        {
            UpdateDebugTile(i);
        }
    }

    public void DrawDebugTileByIndex(
    SpriteBatch spriteBatch,
    int tileIndex)
    {
        if (tileIndex >= 0 && tileIndex < _debugTileTextures.Length)
        {
            // Calculate position in the tile area
            int tilesPerRow = _tileArea.Width / (8 * _scaleFactor);
            int tilesPerColumn = _tileArea.Height / (8 * _scaleFactor);
            int row = tileIndex / tilesPerRow;
            int col = tileIndex % tilesPerRow;
            Vector2 screenPosition = new Vector2(
                _tileArea.X + (col * 8 * _scaleFactor),
                _tileArea.Y + (row * 8 * _scaleFactor)
            );
            spriteBatch.Draw(
                _debugTileTextures[tileIndex],
                screenPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                _scaleFactor * 1f,
                SpriteEffects.None,
                0f
            );
        }
    }



    /// <summary>
    /// Decodes and draws a single Game Boy tile to a specific screen position.
    /// </summary>
    /// <param name="spriteBatch">The MonoGame SpriteBatch for drawing.</param>
    /// <param name="tileAddress">The starting memory address of the 16-byte tile data.</param>
    /// <param name="screenPosition">The top-left screen coordinate to draw the tile.</param>
    /// <param name="scale">How much to scale the 8x8 tile (e.g., 4 for a 32x32 pixel tile).</param>
    public void DrawTile(
        SpriteBatch spriteBatch,
        ushort tileAddress,
        Vector2 screenPosition,
        float scale)
    {
        // 1. Create an 8x8 pixel texture for this tile
        Texture2D tileTexture = new Texture2D(_graphicsDevice, 8, 8);
        Color[] pixelData = new Color[8 * 8]; // Array to hold the 64 MonoGame colors

        // 2. Decode the 16 bytes of tile data
        for (int y = 0; y < 8; y++) // 8 rows
        {
            // Each row uses 2 bytes
            byte lsbByte = _emulator.Cpu.Memory.ReadByte((ushort)(tileAddress + (y * 2)));
            byte msbByte = _emulator.Cpu.Memory.ReadByte((ushort)(tileAddress + (y * 2) + 1));

            for (int x = 0; x < 8; x++) // 8 pixels per row
            {
                // Extract the corresponding bits for the current pixel (x)
                // We read bits from left to right (bit 7 is leftmost pixel, bit 0 is rightmost)
                int lsb = (lsbByte >> (7 - x)) & 1; // Get the LSB bit for pixel x
                int msb = (msbByte >> (7 - x)) & 1; // Get the MSB bit for pixel x

                // Combine them to get the 2-bit color index (0-3)
                int colorIndex = (msb << 1) | lsb;

                // Map the 2-bit color index to our MonoGame palette color
                pixelData[(y * 8) + x] = _gameBoyPalette[colorIndex];
            }
        }

        // 3. Set the texture data
        tileTexture.SetData(pixelData);



        // 4. Draw the texture using SpriteBatch
        spriteBatch.Draw(
            tileTexture,
            screenPosition,
            null, // No source rectangle, draw the whole texture
            Color.White, // Tint color (we want the original pixel colors)
            0f,          // Rotation
            Vector2.Zero, // Origin
            scale,       // Scale
            SpriteEffects.None, // No flipping
            0f           // Depth
        );


        // Don't forget to dispose of the texture if it's created per-frame
        // For a debug view, you might create a few textures and reuse them
        // rather than creating new ones every frame. But for a single tile, this is fine for now.
        // tileTexture.Dispose(); // Uncomment if you create and dispose frequently
    }

}
