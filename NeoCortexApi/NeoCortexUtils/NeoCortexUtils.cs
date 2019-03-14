using NeoCortexApi.Utility;
using System;
using System.Drawing.Imaging;

namespace NeoCortex
{
    public class NeoCortexUtils
    {

        /// <summary>
        /// Draws the bitmap from array of active columns.
        /// </summary>
        /// <param name="twoDimArray">Array of active columns.</param>
        /// <param name="width">Output width.</param>
        /// <param name="height">Output height.</param>
        /// <param name="filePath">The bitmap filename.</param>
        public static void DrawBitmap(int[,] twoDimArray, int width, int height, String filePath)
        {
            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);

            if (w > width || h > height)
                throw new ArgumentException("Requested width/height must be greather than width/height inside of array.");

            var scale = width / w;

            if (scale * w < width)
                scale++;

            DrawBitmap(twoDimArray, scale, filePath);

        }

        /// <summary>
        /// Draws the bitmap from array of active columns.
        /// </summary>
        /// <param name="twoDimArray">Array of active columns.</param>
        /// <param name="scale">Scale of bitmap. If array of active columns is 10x10 and scale is 5 then output bitmap will be 50x50.</param>
        /// <param name="filePath">The bitmap filename.</param>
        public static void DrawBitmap(int[,] twoDimArray, int scale, String filePath)
        {
            int w = twoDimArray.GetLength(0);
            int h = twoDimArray.GetLength(1);

            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(w*scale, h*scale);
            int k = 0;
            for (int Xcount = 0; Xcount < w; Xcount++)
            {
                for (int Ycount = 0; Ycount < h; Ycount++)
                {
                    for (int padX = 0; padX < scale; padX++)
                    {
                        for (int padY = 0; padY < scale; padY++)
                        {
                            if (twoDimArray[Xcount, Ycount] == 1)
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Yellow); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, System.Drawing.Color.Yellow); // HERE IS YOUR LOGIC
                                k++;
                            }
                            else
                            {
                                //myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                                myBitmap.SetPixel(Xcount * scale + padX, Ycount * scale + padY, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                                k++;
                            }
                        }
                    }

                }
            }
            myBitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
