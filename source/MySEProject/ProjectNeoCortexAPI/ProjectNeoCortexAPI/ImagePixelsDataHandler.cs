using Daenet.Binarizer.ExtensionMethod;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Daenet.Binarizer
{
    /// <summary>
    /// Get Set pixels data
    /// </summary>
    class ImagePixelsDataHandler
    {
        /// <summary>
        /// Get Pixels data from System.Drawing.Bitmap object.
        /// </summary>
        /// <param name="bitmapInput">Input Bitmap</param>
        /// <returns>3D array of color data</returns>
        public double[,,] GetPixelsColors(Bitmap bitmapInput)
        {
            double[,,] colorData = new double[bitmapInput.Width, bitmapInput.Height, 3];

            //
            //Check bits depth of Image
            if (Bitmap.GetPixelFormatSize(bitmapInput.PixelFormat) / 8 < 3)
                bitmapInput = new Bitmap(bitmapInput, bitmapInput.Width, bitmapInput.Height);

            //
            //Get image pixels array and stride of image
            byte[] pixels = bitmapInput.GetBytes();
            int stride = bitmapInput.GetStride();

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmapInput.PixelFormat) / 8;
            int heightInPixels = bitmapInput.Height;
            int widthInBytes = bitmapInput.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    //
                    // Bimap object saves the color bits in BGRA order in the byte array, where as the 3D array is in RGB order
                    colorData[x / bytesPerPixel, y, 2] = pixels[currentLine + x];
                    colorData[x / bytesPerPixel, y, 1] = pixels[currentLine + x + 1];
                    colorData[x / bytesPerPixel, y, 0] = pixels[currentLine + x + 2];
                }
            }

            return colorData;
        }

        /// <summary>
        /// Get Pixels data from SkiaSharp.SKBitmap object.
        /// In this method, the input colors order of SKBitmap object needs to be converted to RGBA before passing to the method
        /// </summary>
        /// <param name="bitmapInput">Input SKBitmap</param>
        /// <returns>3D array of color data</returns>
        public double[,,] GetPixelsColors(SKBitmap bitmapInput)
        {
            double[,,] colorData = new double[bitmapInput.Width, bitmapInput.Height, 3];

            //
            //Get image pixels array and stride of image
            byte[] pixels = bitmapInput.Bytes;
            int stride = bitmapInput.Info.RowBytes;

            int bytesPerPixel = bitmapInput.Info.BytesPerPixel;
            int heightInPixels = bitmapInput.Height;
            int widthInBytes = bitmapInput.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    //
                    // The 3D array colors is in RGB order like the colors order of input Skbitmap.                    
                    colorData[x / bytesPerPixel, y, 0] = pixels[currentLine + x];
                    colorData[x / bytesPerPixel, y, 1] = pixels[currentLine + x + 1];
                    colorData[x / bytesPerPixel, y, 2] = pixels[currentLine + x + 2];
                }
            }

            return colorData;
        }

        /// <summary>
        /// Set Pixel data to System.Drawing.Bitmap object
        /// </summary>
        /// <param name="data">3D array of image data</param>
        /// <returns>System.Drawing.Bitmap object</returns>
        public Bitmap SetPixelsColors(double[,,] data)
        {
            Bitmap bitmapOutput = new Bitmap(data.GetLength(0), data.GetLength(1), PixelFormat.Format24bppRgb);
            BitmapData bitmapData = bitmapOutput.LockBits(new Rectangle(0, 0, bitmapOutput.Width, bitmapOutput.Height), ImageLockMode.ReadWrite, bitmapOutput.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmapOutput.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * bitmapOutput.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    //
                    // Bimap object saves the color bits in BGRA order in the byte array, where as the 3D array is in RGB order.
                    // Calculate new pixel value
                    pixels[currentLine + x] = (byte)data[x / bytesPerPixel, y, 2];
                    pixels[currentLine + x + 1] = (byte)data[x / bytesPerPixel, y, 1];
                    pixels[currentLine + x + 2] = (byte)data[x / bytesPerPixel, y, 0];
                }
            }

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmapOutput.UnlockBits(bitmapData);
            return bitmapOutput;
        }

    }
}