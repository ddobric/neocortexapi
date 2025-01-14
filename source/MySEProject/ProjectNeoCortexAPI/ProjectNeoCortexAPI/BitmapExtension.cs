using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Daenet.Binarizer.ExtensionMethod
{

    /// <summary>
    /// Extension for System.Drawing.Bitmap
    /// </summary>
    static class BitmapExtension
    {
        /// <summary>
        /// Get Bytes array that contains color info of Image
        /// </summary>
        /// <param name="bitmapInput">This bitmap object</param>
        /// <returns>1D array of bitmap data</returns>
        public static byte[] GetBytes(this Bitmap bitmapInput)
        {
            BitmapData bitmapData = bitmapInput.LockBits(new Rectangle(0, 0, bitmapInput.Width, bitmapInput.Height), ImageLockMode.ReadOnly, bitmapInput.PixelFormat);
            int byteCount = bitmapData.Stride * bitmapInput.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            bitmapInput.UnlockBits(bitmapData);

            return pixels;
        }

        /// <summary>
        /// Get stride of Image (number of bit per row
        /// </summary>
        /// <param name="bitmapInput">This bitmap object</param>
        /// <returns>Stride of bitmap</returns>
        public static int GetStride(this Bitmap bitmapInput)
        {
            BitmapData bitmapData = bitmapInput.LockBits(new Rectangle(0, 0, bitmapInput.Width, bitmapInput.Height), ImageLockMode.ReadOnly, bitmapInput.PixelFormat);
            bitmapInput.UnlockBits(bitmapData);

            return bitmapData.Stride;
        }
    }
}