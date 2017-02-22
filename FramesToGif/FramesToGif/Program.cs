using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace FramesToGif
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: FramesToGif.exe {directoryWithFrames} {outputFile}");
                return;
            }

            var directory = new DirectoryInfo(args[0]);
            var files = directory.EnumerateFiles("*.jpg");
            var gifEncoder = new GifBitmapEncoder();

            foreach (var fileInfo in files.OrderBy(file => file.Name))
            {
                Console.WriteLine(fileInfo.Name);

                using (var sourceImage = new Bitmap(fileInfo.FullName))
                {
                    using (var negativeBitmap = Transform(sourceImage))
                    {
                        var outputImageSize = negativeBitmap.Width + 2*negativeBitmap.Height;

                        using (var outputImage = CreateBlackBitmap(outputImageSize))
                        {
                            FillBitmapWithImages(outputImage, negativeBitmap);

                            var bmp = outputImage.GetHbitmap();
                            var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                bmp,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            gifEncoder.Frames.Add(BitmapFrame.Create(src));
                        }
                    }
                    
                }
            }

            using (var fs = new FileStream(args[1], FileMode.Create))
            {
                gifEncoder.Save(fs);
            }
        }

        private static Bitmap CreateBlackBitmap(int outputImageSize)
        {
            var bitmap = new Bitmap(outputImageSize, outputImageSize);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, outputImageSize, outputImageSize));
            }

            return bitmap;
        }

        private static void FillBitmapWithImages(Image outputImage, Image negativeBitmap)
        {
            using (var graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(negativeBitmap, new Point(negativeBitmap.Height, 20));

                negativeBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                graphics.DrawImage(negativeBitmap, new Point(negativeBitmap.Height, negativeBitmap.Height + negativeBitmap.Width - 20));

                negativeBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                graphics.DrawImage(negativeBitmap, new Point(20, negativeBitmap.Width));

                negativeBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                graphics.DrawImage(negativeBitmap, new Point(negativeBitmap.Height + negativeBitmap.Width - 20, negativeBitmap.Width));
            }
        }

        private static Bitmap Transform(Image source)
        {
            var newBitmap = new Bitmap(source.Width, source.Height);

            using (var graphics = Graphics.FromImage(newBitmap))
            {
                var colorMatrix = new ColorMatrix(new[]
                {
                    new float[] {-1, 0, 0, 0, 0},
                    new float[] {0, -1, 0, 0, 0},
                    new float[] {0, 0, -1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {1, 1, 1, 0, 1}
                });

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                            0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);

                return newBitmap;
            }
        }
    }
}
