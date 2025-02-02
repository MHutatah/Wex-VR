using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class CatPrinterProtocol
{
    // Example ESC/POS commands
    public static byte[] AlignCenter() => new byte[] { 0x1B, 0x61, 0x01 };
    public static byte[] AlignLeft()   => new byte[] { 0x1B, 0x61, 0x00 };
    public static byte[] AlignRight()  => new byte[] { 0x1B, 0x61, 0x02 };
    public static byte[] LineFeed()    => new byte[] { 0x0A };

    /// <summary>
    /// Set line spacing: ESC 3 n
    /// </summary>
    public static byte[] SetLineSpace(byte n) => new byte[] { 0x1B, 0x33, n };

    /// <summary>
    /// Convert a text string to ASCII bytes. 
    /// The second parameter is optional; defaults to false.
    /// </summary>
    public static byte[] TextToBytes(string text, bool appendLineFeed = false)
    {
        List<byte> data = new List<byte>(Encoding.ASCII.GetBytes(text));
        if (appendLineFeed)
            data.Add(0x0A); // LF
        return data.ToArray();
    }

    /// <summary>
    /// Helper method to combine two byte arrays
    /// </summary>
    public static byte[] Combine(byte[] first, byte[] second)
    {
        if (first == null) return second;
        if (second == null) return first;

        byte[] combined = new byte[first.Length + second.Length];
        System.Buffer.BlockCopy(first, 0, combined, 0, first.Length);
        System.Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
        return combined;
    }

    // ---------------------------------------------------------------------
    // Image methods (optional) — if you want to replicate catprinter's 
    // image logic, you'd implement them here.
    // ---------------------------------------------------------------------

    public static byte[] ConvertTextureToMonochromeBytes(Texture2D tex)
    {
        // Placeholder: replicate your threshold logic from catprinter
        return new byte[0];
    }

    public static byte[] CreateBitImageCommand(byte[] imageData, int width, int height)
    {
        // Placeholder: build ESC * commands from catprinter
        return new byte[0];
    }
}
