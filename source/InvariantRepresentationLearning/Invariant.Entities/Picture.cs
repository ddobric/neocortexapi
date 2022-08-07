using SkiaSharp;

namespace Invariant.Entities
{
    public class Picture
    {
        /// <summary>
        /// full path of the image
        /// </summary>
        public string imagePath;

        /// <summary>
        /// label of the image
        /// </summary>
        public string label;

        /// <summary>
        /// image width in pixels
        /// </summary>
        public int imageWidth;

        /// <summary>
        /// imamge height in pixels
        /// </summary>
        public int imageHeight;

        public Picture(string imagePath, string label)
        {
            this.label = label;
            this.imagePath = imagePath;
            imageWidth = SKBitmap.Decode(this.imagePath).Width;
            imageHeight = SKBitmap.Decode(this.imagePath).Height;
        }

        public double[,,] GetPixels()
        {
            int lastXIndex = imageWidth - 1;
            int lastYIndex = imageHeight - 1;
            return GetPixels(new Frame(0, 0, lastXIndex, lastYIndex));
        }

        /// <summary>
        /// Get the pixels information of an image based on a frame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public double[,,] GetPixels(Frame frame)
        {
            using (SKBitmap inputBitmap = SKBitmap.Decode(this.imagePath))
            {

                double[,,] colorData = new double[frame.brX - frame.tlX + 1, frame.brY - frame.tlY + 1, 3];

                //
                //Get image pixels array and stride of image
                byte[] pixels = inputBitmap.Bytes;
                int stride = inputBitmap.Info.RowBytes;

                int bytesPerPixel = inputBitmap.Info.BytesPerPixel;

                for (int y = frame.brY; y >= frame.tlY; y--)
                {
                    int currentLine = y * stride;
                    for (int x = frame.brX * bytesPerPixel; x >= frame.tlX * bytesPerPixel; x = x - bytesPerPixel)
                    {
                        //
                        // The 3D array colors is in RGB order like the colors order of input Skbitmap.                    
                        colorData[x / bytesPerPixel - frame.tlX, y - frame.tlY, 0] = pixels[currentLine + x];
                        colorData[x / bytesPerPixel - frame.tlX, y - frame.tlY, 1] = pixels[currentLine + x + 1];
                        colorData[x / bytesPerPixel - frame.tlX, y - frame.tlY, 2] = pixels[currentLine + x + 2];
                    }
                }
                return colorData;
            };
        }

        /// <summary>
        /// Save an Image in png, BRG format from a 2D matrix of BRG values
        /// </summary>
        /// <param name="colorData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void SaveAsImage(double[,,] colorData, string outputPath)
        {
            SKBitmap outputBitmap = new SKBitmap(colorData.GetLength(0), colorData.GetLength(1));

            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    int r = (int)colorData[x, y, 0];
                    int g = (int)colorData[x, y, 1];
                    int b = (int)colorData[x, y, 2];
                    outputBitmap.SetPixel(x, y, new SKColor((byte)(b), (byte)(g), (byte)(r)));
                }
            }

            //
            // 3. Reading a Picture file in pixels
            using (var image = SKImage.FromBitmap(outputBitmap))
            {
                string encodingFormat = "Png";
                SKEncodedImageFormat frm = (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), encodingFormat);


                using (var data = image.Encode(frm, 80))
                {
                    // save the data to a stream
                    using (var stream = File.OpenWrite($"{outputPath}"))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
        }

        /// <summary>
        /// Saved the Image into a square shaped image
        /// </summary>
        /// <param name="imagePath">outputImagePath</param>
        /// <param name="dimension">pixel length of square side</param>
        public void SaveImageWithSquareDimension(string imagePath, int dimension)
        {
            SaveImageWithDimension(imagePath, dimension, dimension);
        }

        /// <summary>
        /// Saved the Image into a square shaped image
        /// </summary>
        /// <param name="imagePath">outputImagePath</param>
        /// <param name="width">pixel length of width</param>
        /// <param name="height">pixel length of height</param>
        public void SaveImageWithDimension(string imagePath, int width, int height)
        {
            SKBitmap output = new SKBitmap(width, height);
            using (SKBitmap inputBitmap = SKBitmap.Decode(this.imagePath))
            {
                inputBitmap.ScalePixels(output, SKFilterQuality.High);
                using (var image = SKImage.FromBitmap(output))
                {
                    string encodingFormat = "Png";
                    SKEncodedImageFormat frm = (SKEncodedImageFormat)Enum.Parse(typeof(SKEncodedImageFormat), encodingFormat);


                    using (var data = image.Encode(frm, 80))
                    {
                        // save the data to a stream
                        using (var stream = File.OpenWrite($"{imagePath}"))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }
            };
        }


        /// <summary>
        /// Save the Image to a path in png
        /// </summary>
        /// <param name="path"></param>
        public void SaveTo(string path)
        {
            SaveAsImage(this.GetPixels(), path);
        }

        public void SaveTo(string path, Frame frame)
        {
            SaveAsImage(this.GetPixels(frame), path);
        }

        public bool IsRegionEmpty(Frame frame)
        {
            var test = this.GetPixels(frame);
            for (int y = 0; y < test.GetLength(0); y++)
            {
                for (int x = 0; x < test.GetLength(1); x++)
                {
                    if (test[x, y, 0] > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check for the density of the region from this image, as low bit density may be a result of the number/symbol not presented in a frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="pixelDensityThreshold"></param>
        /// <returns></returns>
        public bool IsRegionBelowDensity(Frame frame, double pixelDensityThreshold)
        {
            var test = this.GetPixels(frame);
            int whitePixelsCount = 0;
            for (int y = 0; y < test.GetLength(0); y++)
            {
                for (int x = 0; x < test.GetLength(1); x++)
                {
                    if (test[x, y, 0] > 0)
                    {
                        whitePixelsCount++;
                    }
                }
            }
            double whitePixelDensity = (double)whitePixelsCount / ((double)frame.PixelCount);
            return (whitePixelDensity <= pixelDensityThreshold) ? true : false;
        }
    }
}