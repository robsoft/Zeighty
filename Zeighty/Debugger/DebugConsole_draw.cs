using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeighty.Emulator;

namespace Zeighty.Debugger;

public partial class DebugConsole
{
    private void SetupConsoleItems()
    {
        Items.Add(0, 0, $"Cart: {_debugState.LoadedFileName}", DebugUIConstants.TITLE_ID, Color.LimeGreen);

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

    }

    private void DebugPalette()
    {
        // Initialize a simple palette for testing
        _gameBoyPalette = new Color[4];
        _gameBoyPalette[0] = Color.White;      // Color 0: Lightest
        _gameBoyPalette[1] = Color.LightGray; // Color 1: Lighter gray
        _gameBoyPalette[2] = Color.DarkGray;   // Color 2: Darker gray
        _gameBoyPalette[3] = Color.Black;      // Color 3: Darkest

    }
    private void InitializeDebugTiles()
    {
        // Allocate space for all possible Game Boy tiles (384 of them)
        // We'll create 8x8 textures for each.
        _debugTileTextures = new Texture2D[GameBoyHardware.MAX_TILES];
        for (int i = 0; i < _debugTileTextures.Length; i++)
        {
            _debugTileTextures[i] = new Texture2D(_graphicsDevice, 8, 8);
            _tileDataChanged[i] = true;
        }
    }

    private void HandleVRAMWrite(ushort address)
    {
        // When VRAM changes, we need to refresh the relevant tile texture
        // Each tile is 16 bytes, starting at 0x8000
        // So, address 0x8000 is tile 0, 0x8010 is tile 1, etc.
        // (address - 0x8000) / 16 gives us the tile index
        if (address >= GameBoyHardware.VRAM_StartAddr && address <= GameBoyHardware.VRAM_EndAddr) //0x97FF)
        {
            int tileIndex = (address - GameBoyHardware.VRAM_StartAddr) / 16;
            if (tileIndex >= 0 && tileIndex < _debugTileTextures.Length)
            {
                // Only update the specific tile that changed
                UpdateDebugTile(tileIndex);
            }
        }
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


    private void FillVRAMTest()
    {

        byte[] debugTileBox = new byte[]
        {
            0xFF, 0xFF, // Row 0: All ones (color 3 - black)
            0x81, 0x81, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x81, 0x81, // Row 2
            0x81, 0x81, // Row 3
            0x81, 0x81, // Row 4
            0x81, 0x81, // Row 5
            0x81, 0x81, // Row 6
            0xFF, 0xFF  // Row 7
        };
        byte[] debugTileFlipBox = new byte[]
        {
            0x00, 0x00, // Row 0: All ones (color 3 - black)
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x7E, 0x7E, // Row 1: Leftmost and rightmost pixels are color 3, rest color 0
            0x00, 0x00  // Row 7
        };
        byte[] debugTileDataDiagonal = new byte[]
        {
            0x80, 0x80, // Row 0
            0x40, 0x40, // Row 1
            0x20, 0x20, // Row 2
            0x10, 0x10, // Row 3
            0x08, 0x08, // Row 4
            0x04, 0x04, // Row 5
            0x02, 0x02, // Row 6
            0x01, 0x01  // Row 7
        };
        byte[] debugTileDataDiagonalFlip = new byte[]
        {
            0x01, 0x01,  // Row 7
            0x02, 0x02, // Row 6
            0x04, 0x04, // Row 5
            0x08, 0x08, // Row 4
            0x10, 0x10, // Row 3
            0x20, 0x20, // Row 2
            0x40, 0x40, // Row 1
            0x80, 0x80, // Row 0
        };
        for (int i = 0; i < GameBoyHardware.MAX_TILES; i = i + 4)
        {
            ushort baseAddr = (ushort)(GameBoyHardware.VRAM_StartAddr + (i * 16));
            for (ushort offs = 0; offs < 16; offs++)
            {
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs), debugTileBox[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 16), debugTileDataDiagonal[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 32), debugTileFlipBox[offs]);
                _emulator.Cpu.Memory.WriteByte((ushort)(baseAddr + offs + 48), debugTileDataDiagonalFlip[offs]);
            }
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
