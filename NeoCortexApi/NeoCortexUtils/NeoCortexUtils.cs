using NeoCortexApi.Utility;
using System;
using System.Drawing.Imaging;

namespace NeoCortex
{
    public class NeoCortexUtils
    {

        public static void DrawBitmap(int[,] twoDimenArray, int width, int height, String filePath)
        {
            int w = twoDimenArray.GetLength(0);
            int h = twoDimenArray.GetLength(1);

            if (w > width || h > height)
                throw new ArgumentException("Requested width/height must be greather than width/height inside of array.");

            var scale = width / w;

            if (scale * w < width)
                scale++;

            DrawBitmap(twoDimenArray, scale, filePath);

        }
            

        public static void DrawBitmap(int[,] twoDimenArray, int scale, String filePath)
        {
            int w = twoDimenArray.GetLength(0);
            int h = twoDimenArray.GetLength(1);

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
                            if (twoDimenArray[Xcount, Ycount] == 1)
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
