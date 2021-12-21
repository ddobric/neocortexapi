using System;
using System.Collections.Generic;
using System.Drawing;

namespace VideoLibrary
{
    /// <summary>
    /// ColorMode is a defined mode of reading pixel's color of a video in each frame
    /// The goal is to reduce the resolution of the Video used for testing
    /// </summary>
    public enum ColorMode
    {
        BLACKWHITE = 1,
        // Image Binarization by RED channel
        // -- If a pixel has R color value more than 128 it is 0, else is it 1
        // Size int[] = Number of pixel x 1
        BINARIZEDRGB = 3,
        // Taking all the binarized value of each channels in R/G/B 
        // -- If a pixel has R/B/G color value more than 128 it is 0, else is it 1 
        // Size int[] = Number of pixel x 3
        PURE = 24,
        // No binarization, taking all color resolution of R/G/B channel
        // Size int[] = Number of pixel x 24
        // Example
        // [R,G,B] = [42, 24, 44] 
        // -> int[] a = {0, 0, 1, 0, 1, 0, 1, 0,    red channel
        //               0, 0, 0, 1, 1, 0, 0, 0,    green channel
        //               0, 0, 1, 0, 1, 1, 0, 0,}   blue channel
    }
    public class NFrame
    {

        public string FrameKey { get; }
        public int[] EncodedBitArray { get; set; }

        public readonly int index;
        public readonly string label;
        public readonly string videoName;
        public readonly ColorMode colorMode;
        public readonly int frameWidth;
        public readonly int frameHeight;
        public NFrame(Bitmap bmp, string videoName, string label, int index, int frameWidth, int frameHeight, ColorMode colorMode)
        {
            this.index = index;
            this.label = label;
            this.colorMode = colorMode;
            this.frameHeight = frameHeight;
            this.frameWidth = frameWidth;
            this.videoName = videoName;
            FrameKey = $"{label}_{videoName}_{index}";
            EncodedBitArray = BitmapToBinaryArray(bmp);
        }
        /// <summary>
        /// Encode Bitmap to an int array by iterating through every pixel of the frame.
        /// </summary>
        /// <param name="image"> Bitmap image object to encode</param>
        /// <returns>returns an int array from Bitmap image object</returns>
        private int[] BitmapToBinaryArray(Bitmap image)
        {
            Bitmap img = ResizeBitmap(image, frameWidth, frameHeight);
            List<int> imageBinary = new List<int>();

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Color pixel = img.GetPixel(x, y);
                    switch (colorMode)
                    {
                        // adding different color mode here for different format of output int[]
                        // more info/color resolution resulted in increase of output bit
                        case ColorMode.BLACKWHITE:
                            // image binarization of GRAYSCALE source image
                            // taking red channel as comparatee for binarization 
                            double luminance = (3 * pixel.R + pixel.B + 4 * pixel.G) >> 3;
                            imageBinary.Add((luminance > 255 / 2) ? 0 : 1);
                            break;
                        case ColorMode.BINARIZEDRGB:
                            // image binarization of RGB source image
                            // binarize each color from RGB channels in order : red --> green --> blue
                            imageBinary.AddRange(new List<int>() { (pixel.R > 255 / 2) ? 1 : 0, (pixel.G > 255 / 2) ? 1 : 0, (pixel.B > 255 / 2) ? 1 : 0 });
                            break;
                        case ColorMode.PURE:
                            imageBinary.AddRange(ColorChannelToBinList(pixel.R));
                            imageBinary.AddRange(ColorChannelToBinList(pixel.G));
                            imageBinary.AddRange(ColorChannelToBinList(pixel.B));
                            break;
                    }
                }
            }
            return imageBinary.ToArray();
        }

        public void SaveFrame(string outputFile)
        {
            Bitmap temp = IntArrayToBitmap(EncodedBitArray);
            temp.Save(outputFile);
        }

        /// <summary>
        /// Convert a Color byte value to a int list of 8 bit
        ///     FURTHER DEVELOPMENT:
        ///         adding gray code implement (adjacent color tone have near bit representation)
        ///         scaling color resolution e.g. 8 bit --> 5bit
        /// </summary>
        /// <param name="r">color byte to convert</param>
        /// <returns></returns>
        public static List<int> ColorChannelToBinList(byte r)
        {
            List<int> binaryList = new();
            string BNR = Convert.ToString(r, 2).PadLeft(8, '0');
            foreach (char a in BNR)
            {
                //Debug.WriteLine(Char.GetNumericValue(a));
                binaryList.Add((int)Char.GetNumericValue(a));
            }
            return binaryList;
        }

        /// <summary>
        /// Resize a Bitmap object to desired width and height
        /// </summary>
        /// <param name="bmp">Bitmap Object to be resized</param>
        /// <param name="width">Output width</param>
        /// <param name="height">Output height</param>
        /// <returns></returns>
        public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }
        public Bitmap IntArrayToBitmap(int[] EncodedBitArray)
        {
            int[] rgb = { 0, 0, 0 };
            Bitmap output = new(frameWidth, frameHeight);
            for (int y = 0; y < frameHeight; y += 1)
            {
                for (int x = 0; x < frameWidth; x += 1)
                {
                    switch (colorMode)
                    {
                        case ColorMode.BLACKWHITE:
                            if (EncodedBitArray[y * frameWidth + x] == 0)
                            {
                                output.SetPixel(x, y, Color.FromArgb(255, 255, 255, 254));
                            }
                            else
                            {
                                output.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                            }
                            break;
                        case ColorMode.BINARIZEDRGB:
                            for (int i = 0; i < 3; i += 1)
                            {
                                rgb[i] = (EncodedBitArray[(y * frameWidth + x) * 3 + i] == 1) ? 255 : 0;
                            }
                            output.SetPixel(x, y, Color.FromArgb(255, rgb[0], rgb[1], rgb[2]));
                            break;
                        case ColorMode.PURE:
                            for (int i = 0; i < 3; i += 1)
                            {
                                int[] binaryColorArray = SubArray<int>(EncodedBitArray, (y * frameWidth + x) * 24 + 8 * i, 8);
                                rgb[i] = BinaryToDecimal(binaryColorArray);

                            }
                            output.SetPixel(x, y, Color.FromArgb(255, rgb[0], rgb[1], rgb[2]));
                            break;
                    }
                }
            }


            return output;
        }
        public static int BinaryToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int power = 0;
            for (int i = binaryArray.Length - 1; i >= 0; i -= 1)
            {
                decimalValue += binaryArray[i] * (int)Math.Pow(2, power);
                power += 1;
            }
            return decimalValue;
        }
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
