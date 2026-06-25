using System.Text;
using ImageMagick;

class ConsoleApp
{
  static void Main(string[] args)
  {
    //using var imagesFromFile = new MagickImageCollection("1RtT7h3L.png");
    //using var imagesFromFile = new MagickImageCollection("EKD3bBZP.png");
    //using var imagesFromFile = new MagickImageCollection("bmDwolWU.png");
    //using var imagesFromFile = new MagickImageCollection("MBlXyTSp.png"); //cat
    using var imagesFromFile = new MagickImageCollection("mCeETXDs.png"); //butterfly
    foreach (var image in imagesFromFile)
    {
      var pixels = image.GetPixels();

      uint offset = 2; //GetHeaderOffset(image.Width, image.Height, pixels);
      var head0 = pixels[0, 0][0] & 1;
      var head1 = pixels[0, 0][1] & 1;

      if (head0 == 0 && head1 == 0)
      {
        var length = Read16bitsAsValue(image.Width, offset, pixels);
        Console.WriteLine($"{length}");

        var message = GetXCharString(length, image.Width, offset, pixels);
        Console.WriteLine(message);
      }
      else
        if (head0 == 1 && head1 == 0)
        {
          var width = Read16bitsAsValue(image.Width, offset, pixels);
          var height = Read16bitsAsValue(image.Width, offset + 16, pixels);
          var length = width * height;
          var hiddenImage = GetXBytes(length, image.Width, 16, pixels);
          byte[] rgbArray = ConvertToRGB(hiddenImage);
          Console.WriteLine($"{width} {height}");
          File.WriteAllBytes("hiddenimage.data", rgbArray);
        }

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

  private static byte[] ConvertToRGB(byte[] hiddenImage)
  {
    var result = new byte[hiddenImage.Length * 3];
    var resultIndex = 0;
    for (int i = 0; i < hiddenImage.Length; i++)
    {
      byte value = 0;
      if (hiddenImage[i] == 1)
      {
        value = 255;
      }
      result[resultIndex] = value;
      result[resultIndex + 1] = value;
      result[resultIndex + 2] = value;
      resultIndex += 3;
    }
    return result;
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

  private static byte[] GetXBytes(uint length, uint width, uint offset, IPixelCollection<byte> pixels)
  {
    var result = new byte[length];
    for (int i = 0; i < length; i++)
    {
      var component = GetByteComponentAtIndex((uint)(offset + 18 + i), width, pixels);
      result[i] = (byte)(component & 1);
    }
    return result;
  }
  private static uint Read16bitsAsValue(uint width, uint offset, IPixelCollection<byte> pixels)
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
