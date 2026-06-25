using System.Text;
using ImageMagick;

class ConsoleApp
{
  static void Main(string[] args)
  {
    using var imagesFromFile = new MagickImageCollection("1RtT7h3L.png");
    //using var imagesFromFile = new MagickImageCollection("EKD3bBZP.png");
    //using var imagesFromFile = new MagickImageCollection("bmDwolWU.png");
    //using var imagesFromFile = new MagickImageCollection("MBlXyTSp.png"); //cat
    //using var imagesFromFile = new MagickImageCollection("mCeETXDs.png"); //butterfly
    foreach (var image in imagesFromFile)
    {
      var pixels = image.GetPixels();

      var offset = GetHeaderOffset(image.Width, image.Height, pixels);
      var length = GetLength(image.Width, offset, pixels);
      Console.WriteLine($"{length}");

      var message = GetXCharString(length, image.Width, offset, pixels);
      Console.WriteLine(message);

      // for (int x = 0; x < image.Width; x++)
      // {
      //   for (int y = 0; y < image.Height; y++)
      //   {
      //     var pixel = pixels[x, y];
      //     Console.Write($"{pixel[0]}-{pixel[1]}-{pixel[2]}, ");
      //   }
      //   Console.WriteLine();
      // }
    }


  }

  private static uint GetHeaderOffset(uint width, uint height, IPixelCollection<byte> pixels)
  {
    uint offset = 0;
    int zeroCount = 0;
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        for (uint channel = 0; channel < 3; channel++)
        {
          var component = pixels[x, y][channel];
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
    //var bitCount = 0;
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
    return pixels[x, y][(uint)(index % 3)];
  }

}
