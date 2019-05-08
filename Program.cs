using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/*
    svg2ico    

        Creates .ico files from SVG source files

    Keith Fletcher
    May 2019

    This file is Unlicensed.
    See the foot of the file, or refer to <http://unlicense.org>
*/

namespace HisRoyalRedness.com
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ParseCommandLine(args))
                CreateIco();
            else
                ShowUsage();
        }

        static void CreateIco()
        {
            try
            {
                var svg = SvgDocument.Open(InputPath);
                var file = new IconFile();
                file.AddImage(svg, 16);
                file.AddImage(svg, 32);
                file.AddImage(svg, 48);
                file.AddImage(svg, 64);
                file.Save(OutputPath);
                Console.WriteLine($"Converted {Path.GetFileName(InputPath)} to {Path.GetFileName(OutputPath)}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        #region Command line
        static void ShowUsage()
        {
            var assemLoc = typeof(Program).Assembly.Location;
            Console.WriteLine($"Usage:   {Path.GetFileName(assemLoc)}  <input_svg>  <output_ico>");
            Console.WriteLine($"   Ver {FileVersionInfo.GetVersionInfo(assemLoc).FileVersion}");
            Console.WriteLine();
            Console.WriteLine("  where:");
            Console.WriteLine("      <input_svg>    File path to the input SVG file");
            Console.WriteLine("      <output_ico>   File path to the icon file to be generated.");
            Console.WriteLine();
        }

        static bool ParseCommandLine(string[] args)
        {
            if (args.Length != 2)
                return false;

            try
            {
                var inFile = Path.GetFullPath(args[0]);
                if (!File.Exists(inFile))
                {
                    Console.WriteLine($"ERROR: File not found. {args[0]}");
                    return false;
                }
                InputPath = inFile;
                OutputPath = Path.GetFullPath(args[1]);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return false;
            }
            return false;
        }
        #endregion Command line

        static string InputPath { get; set; }
        static string OutputPath { get; set; }
    }

    #region IconFile
    public class IconFile
    {
        const uint HEADER_SIZE = 6;
        const uint ENTRY_SIZE = 16;

        public void AddImage(IconImage image)
        {
            _images.Add(image);
        }

        public void AddImage(SvgDocument svg, int width)
            => AddImage(new IconImage(width, GetPngFromSvg(svg, width)));

        static byte[] GetPngFromSvg(SvgDocument svg, int width)
        {
            using (var png = svg.Draw(width, width))
            {
                using (var stream = new MemoryStream())
                {
                    png.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }

        public void Save(string fileName)
        {
            if (_images.Count == 0)
                return;

            using (var writer = new BinaryWriter(File.Create(fileName)))
            {
                var offset = HEADER_SIZE + ((uint)_images.Count * ENTRY_SIZE);
                WriteHeader(writer);
                foreach (var img in _images)
                {
                    WriteImage(writer, img, offset);
                    offset += (uint)img.Data.Length;
                }
                foreach (var img in _images)
                    writer.Write(img.Data, 0, img.Data.Length);
            }
        }

        void WriteHeader(BinaryWriter writer)
        {
            writer.Write((UInt16)0);                    // Reserved. Always 0
            writer.Write((UInt16)1);                    // Image type: 1 - icon (.ico), 2 = cursor (.cur)
            writer.Write((UInt16)_images.Count);        // Number of images
        }

        void WriteImage(BinaryWriter writer, IconImage image, UInt32 offset)
        {
            writer.Write((byte)image.Width);            // Width in pixels
            writer.Write((byte)image.Height);           // Height in pixels
            writer.Write((byte)0);                      // Number of colours in palette (0 for no palette)
            writer.Write((byte)0);                      // Reserved. Always 0
            writer.Write((UInt16)1);                    // Number of colour planes
            writer.Write((UInt16)image.BitsPerPixel);   // Bits per pixel
            writer.Write((UInt32)image.Data.Length);    // Size of image data
            writer.Write((UInt32)offset);               // Start of image data
        }

        List<IconImage> _images = new List<IconImage>();
    }
    #endregion IconFile

    #region IconImage
    public struct IconImage
    {
        public IconImage(int width, byte[] data)
        {
            Width = width;
            Height = width;
            BitsPerPixel = 4;
            Data = data;
        }

        public IconImage(int width, int bitsPerPixel, byte[] data)
        {
            Width = width;
            Height = width;
            BitsPerPixel = bitsPerPixel;
            Data = data;
        }

        public IconImage(int width, int height, int bitsPerPixel, byte[] data)
        {
            Width = width;
            Height = height;
            BitsPerPixel = bitsPerPixel;
            Data = data;
        }

        public int Width { get; }
        public int Height { get; }
        public int BitsPerPixel { get; }
        public byte[] Data { get; }
    }
    #endregion IconImage
}

/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
*/
