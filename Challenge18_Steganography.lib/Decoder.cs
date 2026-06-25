using System.Text;
using ImageMagick;

namespace Challenge18_Steganography.lib;

public static class Decoder
{
  public static string Decode(string filename)
  {
    using var imagesFromFile = new MagickImageCollection(filename);
    var image = imagesFromFile[0];
    var pixels = image.GetPixels();

    var width = (uint)image.Width;
    var height = (uint)image.Height;

    var offset = GetHeaderOffset(width, height, pixels);
    var length = GetLength(width, offset, pixels);
    Console.WriteLine($"{length}");

    return GetXCharString(length, width, offset, pixels);
  }

  private static uint GetHeaderOffset(uint width, uint height, IPixelCollection<byte> pixels)
  {
    uint offset = 0;
    int zeroCount = 0;
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        var pixel = pixels[x, y];
        if (pixel is null)
        {
          throw new IndexOutOfRangeException($"Pixel at ({x},{y}) is null");
        }

        for (uint channel = 0; channel < 3; channel++)
        {
          var component = pixel[channel];
          if ((component & 1) == 0)
          //if (component == 0)
          {
            zeroCount++;
            if (zeroCount == 2)
            {
              return offset - 1;
            }
          }
          else
          {
            zeroCount = 0;
          }
          offset++;
        }
      }
    }
    throw new Exception("Failed to find header.");
  }

  private static string GetXCharString(uint length, uint width, uint offset, IPixelCollection<byte> pixels)
  {
    var value = 0;
    var sb = new StringBuilder();
    for (int i = 0; i < length; i++)
    {
      var component = GetByteComponentAtIndex((uint)(offset + 18 + i), width, pixels);
      value += (component & 1) << (8 - (i % 8) - 1);

      if (i > 0 && i % 8 == 0)
      {
        sb.Append((char)value);
        value = 0;
      }
    }
    return sb.ToString();
  }
  private static uint GetLength(uint width, uint offset, IPixelCollection<byte> pixels)
  {
    var length = 0;
    for (int i = 0; i < 16; i++)
    {
      var component = GetByteComponentAtIndex((uint)(offset + 2 + i), width, pixels);
      length += (component & 1) << (16 - i - 1);
    }
    return (uint)length / 8;
  }

  private static byte GetByteComponentAtIndex(uint index, uint width, IPixelCollection<byte> pixels)
  {
    int i = (int)(index / 3);
    int x = (int)(i % width);

    int y = (int)(i / width);
    var pixel = pixels[x, y];
    if (pixel == null)
      throw new IndexOutOfRangeException($"Pixel at ({x},{y}) is null");
    return pixel[(uint)(index % 3)];
  }
}
