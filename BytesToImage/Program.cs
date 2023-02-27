// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Formats.Asn1.AsnWriter;

Console.WriteLine("Hello, World!");


var fileName = @"C:\Users\JuhanaR\Desktop\verrokki.png";
var binaryFile = @"C:\Users\JuhanaR\Desktop\binary.png";
var fileName2 = @"C:\Users\Rajala\Desktop\reference1.png";

FileToImage(fileName, binaryFile);
//ImageToFile(fileName1, fileName2);

static void FileToImage(string inputFilePath, string outputFile)
{
    FileInfo inputFile = new FileInfo(inputFilePath);
    if (!inputFile.Exists) { return; }

    Bitmap bitmap = new Bitmap(1024, 1024, PixelFormat.Format32bppArgb);

    using (FileStream inputStream = File.OpenRead(inputFilePath))
    {
        byte[] buffer = new byte[1024];
        int c;

        int row = 0;
        while ((c = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            int column = 0;
            int index = 0;
            foreach (byte _byte in buffer)
            {
                bool[] bits = ByteToBits(_byte);

                foreach (bool bit in bits)
                {
                    Color prevColor = bitmap.GetPixel(column, row);
                    Color newColor = EncodeBitToColor(bit, index, prevColor);
                    bitmap.SetPixel(column, row, newColor);
                    index++;

                    if (index == 32)
                    {
                        index = 0;
                        column++;
                    }
                }
            }
            row++;
        }
    }

    bitmap.Save(outputFile, ImageFormat.Bmp);
}

//static void ImageToFile(string inputFile, string outputFile)
//{
//    Bitmap bitmap = new Bitmap(inputFile);
//    using (FileStream outputStream = File.OpenWrite(outputFile))
//    {
//        bool?[] bits = new bool?[8];
//        int pixelCount = 0;
//        for (int i = 0; i < bitmap.Width; i++)
//        {
//            for (int j = 0; j < bitmap.Height; j++)
//            {
//                Color pixel = bitmap.GetPixel(i, j);
//                bool? bit = ColorToBit(pixel);

//                if (bit != null)
//                {
//                    bits[pixelCount] = bit;
//                }

//                if (pixelCount == 8)
//                {
//                    byte _byte = BitsToByte(bits);
//                    outputStream.WriteByte(_byte);
//                    bits = new bool?[8];
//                    pixelCount = 0;
//                }

//                pixelCount++;
//            }
//        }

//        if (pixelCount != 0)
//        {
//            byte _byte = BitsToByte(bits);
//            outputStream.WriteByte(_byte);
//            bits = new bool?[8];
//        }
//    }
//}

static byte ConvertBoolArrayToByte(bool[] source)
{
    byte result = 0;
    // This assumes the array never contains more than 8 elements!
    int index = 8 - source.Length;

    // Loop through the array
    foreach (bool b in source)
    {
        // if the element is 'true' set the bit at that position
        if (b)
        {
            result |= (byte)(1 << (7 - index));
        }

        index++;
    }

    return result;
}

static bool[] ByteToBits(byte b)
{
    // prepare the return result
    bool[] result = new bool[8];

    // check each bit in the byte. if 1 set to true, if 0 set to false
    for (int i = 0; i < 8; i++)
    {
        result[i] = (b & (1 << i)) != 0;
    }

    // reverse the array
    Array.Reverse(result);

    return result;
}

static byte BitsToByte(bool[] bits)
{
    byte val = 0;
    foreach (bool b in bits)
    {
        val <<= 1;
        if (b) val |= 1;
    }
    return val;
}

static Color EncodeBitToColor(bool bit, int index, Color color)
{
    byte colorByte = default;
    int modIndex = index;

    if (0 <= index && index <= 7)
    {
        colorByte = color.A;
    }
    else if (8 <= index && index <= 15)
    {
        colorByte = color.R;
        modIndex = index - 8;
    }
    else if (16 <= index && index <= 23)
    {
        colorByte = color.G;
        modIndex = index - 16;
    }
    else if (24 <= index && index <= 31)
    {
        colorByte = color.B;
        modIndex = index - 24;
    }

    bool[] alphaBits = ByteToBits(colorByte);
    alphaBits[modIndex] = bit;

    byte channelByte = BitsToByte(alphaBits);

    int int32Byte = Convert.ToInt32(channelByte);
    if (0 <= index && index <= 7)
    {
        color = Color.FromArgb(int32Byte, color.R, color.G, color.B);
    }
    else if (8 <= index && index <= 15)
    {
        color = Color.FromArgb(color.A, int32Byte, color.G, color.B);
    }
    else if (16 <= index && index <= 23)
    {
        color = Color.FromArgb(color.A, color.R, int32Byte, color.B);
    }
    else if (24 <= index && index <= 31)
    {
        color = Color.FromArgb(color.A, color.R, color.G, int32Byte);
    }

    return color;
}

static bool? DecodeBitFromColor(Color color)
{
    return color == Color.White ? true : color == Color.Red ? false : null;
}

static byte[] GetBytes(string bitString)
{
    return Enumerable.Range(0, bitString.Length / 8).Select(pos => Convert.ToByte(bitString.Substring(pos * 8, 8), 2)).ToArray();
}