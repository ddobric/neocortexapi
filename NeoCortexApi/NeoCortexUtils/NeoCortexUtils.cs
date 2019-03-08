using NeoCortexApi.Utility;
using System;
using System.Drawing.Imaging;

namespace NeoCortex
{
    public class NeoCortexUtils
    {

        public static void DrawBitmap(int[] sourceArray, int width, int height, String filePath)
        {
            int[,] twoDimenArray = ArrayUtils.Make2DArray<int>(sourceArray, width, height);
            twoDimenArray = ArrayUtils.Transpose(twoDimenArray);
            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(width, height);
            int k = 0;
            for (int Xcount = 0; Xcount < myBitmap.Width; Xcount++)
            {
                for (int Ycount = 0; Ycount < myBitmap.Height; Ycount++)
                {
                    if (twoDimenArray[Xcount, Ycount] == 1)
                    {
                        myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Red); // HERE IS YOUR LOGIC
                        k++;
                    }
                    else
                    {
                        myBitmap.SetPixel(Xcount, Ycount, System.Drawing.Color.Black); // HERE IS YOUR LOGIC
                        k++;
                    }

                }
            }
            myBitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
