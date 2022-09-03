using SkiaSharp;

namespace Invariant.Entities
{
    public class Image
    {
        /// <summary>
        /// full path of the image
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// label of the image
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// image width in pixels
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// imamge height in pixels
        /// </summary>
        public int ImageHeight { get; set; }
        public Image(string imagePath, string label)
        {
            this.Label = label;
            this.ImagePath = imagePath;
            ImageWidth = SKBitmap.Decode(this.ImagePath).Width;
            ImageHeight = SKBitmap.Decode(this.ImagePath).Height;
        }

        /// <summary>
        /// Get pixels information of the whole image
        /// </summary>
        /// <returns></returns>
        public double[,,] GetPixels()
        {
            int lastXIndex = ImageWidth - 1;
            int lastYIndex = ImageHeight - 1;
            return GetPixels(new Frame(0, 0, lastXIndex, lastYIndex));
        }

        /// <summary>
        /// Get the pixels data of an image in the area of a frame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public double[,,] GetPixels(Frame frame)
        {
            using (SKBitmap inputBitmap = SKBitmap.Decode(this.ImagePath))
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
        public static void SaveTo(double[,,] colorData, string outputPath)
        {
            SKBitmap outputBitmap = new SKBitmap(colorData.GetLength(0), colorData.GetLength(1));

            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    int red = (int)colorData[x, y, 0];
                    int green = (int)colorData[x, y, 1];
                    int blue = (int)colorData[x, y, 2];
                    outputBitmap.SetPixel(x, y, new SKColor((byte)(blue), (byte)(green), (byte)(red)));
                }
            }

            //
            // 3. Writing a Picture file in pixels
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
        public void SaveTo_Scaled(string imagePath, int dimension)
        {
            SaveTo_Scaled(imagePath, dimension, dimension);
        }

        /// <summary>
        /// Saved the Image into a square shaped image
        /// </summary>
        /// <param name="imagePath">outputImagePath</param>
        /// <param name="width">pixel length of width</param>
        /// <param name="height">pixel length of height</param>
        public void SaveTo_Scaled(string imagePath, int width, int height)
        {
            SKBitmap output = new SKBitmap(width, height);
            using (SKBitmap inputBitmap = SKBitmap.Decode(this.ImagePath))
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
        /// Save this Image to a path into a png
        /// </summary>
        /// <param name="imagePath"></param>
        public void SaveTo(string imagePath)
        {
            SaveTo(this.GetPixels(), imagePath);
        }

        public void SaveTo(string path, Frame frame, bool binarized = true, double binarizeThreshold = 255/2)
        {
            if (binarized)
            {
                SaveTo(Binarize(this.GetPixels(frame), binarizeThreshold), path);
            }
            else
            {
                SaveTo(this.GetPixels(frame), path);
            }
        }

        public static bool AreSamePixels(double[,,] pixels1, double[,,] pixels2, bool binarized = true, double binarizedThreshold = 255/2)
        {
            if (binarized)
            {
                pixels1 = Binarize(pixels1, binarizedThreshold);
                pixels2 = Binarize(pixels2, binarizedThreshold);
            }
            for (int y = 0; y < pixels1.GetLength(0); y++)
            {
                for (int x = 0; x < pixels2.GetLength(1); x++)
                {
                    if (pixels1[x, y, 0] != pixels2[x, y, 0])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static double[,,] Binarize(double[,,] image, double binarizedThreshold = 255/2)
        {
            double[,,] res = new double[image.GetLength(1), image.GetLength(0), 3];
            for (int y = 0; y < image.GetLength(0); y++)
            {
                for (int x = 0; x < image.GetLength(1); x++)
                {
                    if (image[x, y, 0] > binarizedThreshold)
                    {
                        res[x, y, 0] = 255;
                        res[x, y, 1] = 255;
                        res[x, y, 2] = 255;
                    }
                }
            }
            return res;
        }

        public static double[,,] ApplyPixels(double[,,] pixelFromImage, double[,,] outputPixels, Frame selectedFrame)
        {
            double[,,] resultOutputPixels = outputPixels;
            for(int i = selectedFrame.tlX; i <= selectedFrame.brX; i += 1)
            {
                for(int j = selectedFrame.tlY; j <= selectedFrame.brY; j += 1)
                {
                    outputPixels[i, j, 0] = pixelFromImage[i - selectedFrame.tlX, j - selectedFrame.tlY, 0];
                    outputPixels[i, j, 1] = pixelFromImage[i - selectedFrame.tlX, j - selectedFrame.tlY, 1];
                    outputPixels[i, j, 2] = pixelFromImage[i - selectedFrame.tlX, j - selectedFrame.tlY, 2];
                }
            }
            return resultOutputPixels;
        }
        /// <summary>
        /// Check for the density of the region from this image, as low bit density may be a result of the number/symbol not presented in a frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="binarizeThreshold"></param>
        /// <returns></returns>
        public bool IsRegionBelowDensity(Frame frame, double binarizeThreshold)
        {
            double whitePixelDensity = FrameDensity(frame, binarizeThreshold);
            return (whitePixelDensity <= binarizeThreshold) ? true : false;
        }
        public bool IsRegionBelowDensity(double pixelDensityThreshold)
        {
            Frame frame = new Frame(0, 0, this.ImageWidth - 1, this.ImageHeight - 1);
            double whitePixelDensity = FrameDensity(frame, pixelDensityThreshold);
            return (whitePixelDensity <= pixelDensityThreshold) ? true : false;
        }
        public bool IsRegionInDensityRange(Frame frame, double lowerDensity, double UpperDensity)
        {
            if (IsRegionAboveDensity(frame,lowerDensity) && IsRegionBelowDensity(frame,UpperDensity))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsRegionAboveDensity(Frame frame, double pixelDensityThreshold)
        {
            double whitePixelDensity = FrameDensity(frame, pixelDensityThreshold);
            return (whitePixelDensity >= pixelDensityThreshold) ? true : false;
        }
        public bool IsRegionAboveDensity(double pixelDensityThreshold)
        {
            Frame frame = new Frame(0, 0, this.ImageWidth - 1, this.ImageHeight - 1);
            double whitePixelDensity = FrameDensity(frame, pixelDensityThreshold);
            return (whitePixelDensity >= pixelDensityThreshold) ? true : false;
        }

        public bool IsRegionOverBinarizedThreshold(Frame frame, double threshold)
        {
            var test = this.GetPixels(frame);
            for (int y = 0; y < test.GetLength(0); y++)
            {
                for (int x = 0; x < test.GetLength(1); x++)
                {
                    if (test[x, y, 0] > threshold)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Image's frame Density as binarized white pixels(1) over all pixels(0 and 1) of an image
        /// </summary>
        /// <param name="frame">region to check</param>
        /// <param name="pixelDensiTyThreshold">binarized Threshold</param>
        /// <returns>pixel density</returns>
        public double FrameDensity(Frame frame, double pixelDensiTyThreshold)
        {
            var test = this.GetPixels(frame);
            int whitePixelsCount = 0;
            for (int y = 0; y < test.GetLength(0); y++)
            {
                for (int x = 0; x < test.GetLength(1); x++)
                {
                    if (test[x, y, 0] > pixelDensiTyThreshold)
                    {
                        whitePixelsCount++;
                    }
                }
            }
            double whitePixelDensity = (double)whitePixelsCount / ((double)frame.PixelCount) *100;
            return whitePixelDensity;
        }
        /// <summary>
        /// Image Density as binarized white pixels(1) over all pixels(0 and 1) of an image
        /// </summary>
        /// <param name="pixelDensiTyThreshold">binarized Threshold</param>
        /// <returns>pixel density</returns>
        public double Density(double pixelDensityThreshold)
        {
            Frame frame = new Frame(0, 0, ImageWidth - 1, ImageHeight - 1);
            return FrameDensity(frame, pixelDensityThreshold);
        }
    }
}