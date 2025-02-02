using UnityEngine;

public class ImageConverter
{
    public static byte[] ConvertToPeriPageFormat(Texture2D image)
    {
        int width = image.width;
        int height = image.height;
        byte[] data = new byte[(width * height) / 8];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                int bit = (pixel.grayscale > 0.5f) ? 1 : 0;
                int index = (y * width + x) / 8;
                data[index] |= (byte)(bit << (7 - (x % 8)));
            }
        }

        return data;
    }
}
