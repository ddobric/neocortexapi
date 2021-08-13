using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System;

using Emgu.CV;

namespace VideoLibrary
{
    /// <summary>
    /// ColorMode is a defined mode of reading pixel's color of a video in each frame
    /// The goal is to reduce the resolution of the Video used for testing
    /// </summary>
    public enum ColorMode
    {
        BLACKWHITE,
        // Image Binarization by RED channel
        // -- If a pixel has R color value more than 128 it is 0, else is it 1
        // Size int[] = Number of pixel x 1
        BINARIZEDRGB,
        // Taking all the binarized value of each channels in R/G/B 
        // -- If a pixel has R/B/G color value more than 128 it is 0, else is it 1 
        // Size int[] = Number of pixel x 3
        PURE,
        // No binarization, taking all color resolution of R/G/B channel
        // Size int[] = Number of pixel x 24
        // Example
        // [R,G,B] = [42, 24, 44] 
        // -> int[] a = {0, 0, 1, 0, 1, 0, 1, 0,    red channel
        //               0, 0, 0, 1, 1, 0, 0, 0,    green channel
        //               0, 0, 1, 0, 1, 1, 0, 0,}   blue channel
    }
    /// <summary>
    /// <para>VideoSet is created to represent an folder of videos</para>
    /// <para>Each videos will be read to a Video object</para>
    /// <para>For Example:</para>
    /// <para>
    /// <br>A folder ball/ contains 3 file one.mp4, two.mp4 and three.mp4</br>
    /// <br>will create a VideoSet with label "ball", which contains 3 Video objects</br>
    /// <br>respectively: one.mp4, two.mp4</br>
    /// </para>
    /// </summary>
    public class VideoSet
    {
        public List<NVideo> nVideoList { get; set; }

        public List<string> Name { get; set; }

        public string VideoSetLabel { get; set; }

        public VideoSet(string videoSetPath, ColorMode colorMode, int frameWidth, int frameHeight, double frameRate = 0)
        {
            nVideoList = new List<NVideo>();
            Name = new List<string>();
            // Set the label of the video collection as the name of the folder that contains it 
            this.VideoSetLabel = Path.GetFileNameWithoutExtension(videoSetPath);

            // Read videos from the video folder path 
            nVideoList = ReadVideos(videoSetPath, colorMode, frameWidth, frameHeight, frameRate);
        }
        /// <summary>
        /// Read all videos within a provided folder's full path, the foleder name will be used as videoset's Label
        /// </summary>
        /// <param name="videoSetPath"> The Path of the folder that contains the videos</param>
        private List<NVideo> ReadVideos(string videoSetPath, ColorMode colorMode, int frameWidth, int frameHeight, double frameRate)
        {
            List<NVideo> videoList = new List<NVideo>();
            // Iteate through each videos in the videos' folder
            foreach (string file in Directory.GetFiles(videoSetPath.Replace("\"","")))
            {
                string fileName = Path.GetFileName(videoSetPath);
                Name.Add(fileName);
                Debug.WriteLine($"Video file name: {fileName}");
                videoList.Add(new NVideo(file, VideoSetLabel, colorMode, frameWidth, frameHeight, frameRate));
            }
            return videoList;
        }
        /// <summary>
        /// Getting the longest sequence of frames count available in set
        /// </summary>
        /// <returns></returns>
        public int GetLongestFramesCountInSet()
        {
            int count = 0;
            foreach (NVideo nv in nVideoList)
            {
                if (nv.nFrames.Count > count)
                {
                    count = nv.nFrames.Count;
                }
            }
            return count;
        }
        public void CreateConvertedVideos(string videoOutputDirectory)
        {
            foreach(NVideo nv in nVideoList)
            {
                string folderName = $"{videoOutputDirectory}//{nv.label}";
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                nv.BitmapListToVideo($"{folderName}//{nv.name}",true);
            }
        }
    }
    /// <summary>
    /// <para>
    /// <br>Represent a single video, which contains</br>
    /// <br>Name of the video, but not including suffix format</br>
    /// <br>List of int[], with each int[] is a frame in chronological order start - end</br>
    /// <br>The Image can be scaled
    /// </para>
    /// </summary>
    public class NVideo
    {
        public string name;
        public List<NFrame> nFrames;
        public string label;

        private readonly ColorMode colorMode;
        public readonly int frameWidth;
        public readonly int frameHeight;
        public readonly double frameRate;
        /// <summary>
        /// Generate a Video object
        /// </summary>
        /// <param name="videoPath">full path to the video</param>
        /// <param name="colorMode">Color mode to encode each frame in the Video, see enum VideoSet.ColorMode</param>
        /// <param name="frameHeight">height in pixels of the video resolution</param>
        /// <param name="frameWidth">width in pixels of the video resolution</param>
        public NVideo(string videoPath, string label, ColorMode colorMode, int frameWidth, int frameHeight, double frameRate)
        {
            this.colorMode = colorMode;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.frameRate = frameRate;
            this.label = label;

            this.nFrames = new List<NFrame>();
            this.name = Path.GetFileNameWithoutExtension(videoPath);
            //this.frames = BitmapToBinaryArray(ReadVideo_GleamTech(videoPath, frameRate = 0));

            var fromBitmaps = ReadVideo(videoPath, frameRate);
            for (int i = 0; i < fromBitmaps.Count; i++)
            {
                nFrames.Add(new NFrame(fromBitmaps[i], name, label, i, frameWidth, frameHeight, colorMode));
            }
        }
        /// <summary>
        /// <para>Method to read a video into a list of Bitmap, from video path to a list of Bitmap</para>
        /// The current implementation used Videos Ultimate C# wrapper from GleamTech.
        /// Which will create a trademark when create a video after 30 days trial
        /// </summary>
        /// <param name="videoPath"> full path of the video to be read </param>
        /// <returns>List of Bitmaps</returns>
        private static List<Bitmap> ReadVideo(string videoPath, double framerate = 0)
        {
            List<Bitmap> videoBitmapArray = new List<Bitmap>();

            // Create VideoFrameReader object for the video from videoPath
            VideoCapture vd = new(videoPath);

            double step = 1;
            // New step for iterating the video in a lower frameRate when specified
            double framerateDefault = vd.Get(Emgu.CV.CvEnum.CapProp.Fps);
            if (framerate != 0)
            {
                step = framerateDefault / framerate;
            }
            Mat currentFrame = new();
            int count = 0;
            int currentFrameIndex = 0;
            int stepCount = 0;
            while (currentFrame != null)
            {
                currentFrame = vd.QueryFrame();
                if (count == currentFrameIndex)
                {
                    if (currentFrame == null)
                    {
                        break;
                    }
                    stepCount += 1;
                    currentFrameIndex = (int)(stepCount*step);
                }
                count += 1;
            }
            vd.Dispose();
            //
            return videoBitmapArray;
        }

        public int[] GetEncodedFrame(string key)
        {
            foreach (NFrame nf in nFrames)
            {
                if(nf.FrameKey == key)
                {
                    return nf.EncodedBitArray;
                }
            }
            return new int[] { 4, 2, 3 };
        }
        public void BitmapListToVideo(string videoOutputPath,bool isColor)
        {
            VideoWriter videoWriter = new($"{videoOutputPath}.mp4",-1, frameRate, new Size(frameWidth, frameHeight),isColor);
            foreach (NFrame frame in nFrames)
            {
                Bitmap tempBitmap = frame.IntArrayToBitmap();
                videoWriter.Write(tempBitmap.ToMat());
            }
            videoWriter.Dispose();
        }
    }
    public class NFrame {

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
                
                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        Color pixel = img.GetPixel(i, j);
                        switch (colorMode)
                        {
                            // adding different color mode here for different format of output int[]
                            // more info/color resolution resulted in increase of output bit
                            case ColorMode.BLACKWHITE:
                                // image binarization of GRAYSCALE source image
                                // taking red channel as comparatee for binarization 
                                imageBinary.Add((pixel.R > 255 / 2) ? 0 : 1);
                                break;
                            case ColorMode.BINARIZEDRGB:
                                // image binarization of RGB source image
                                // binarize each color from RGB channels in order : red --> green --> blue
                                imageBinary.AddRange(new List<int>() { (pixel.R > 255 / 2) ? 1 : 0, (pixel.G > 255 / 2) ? 1 : 0, (pixel.B > 255 / 2) ? 1 : 0 });
                                break;
                            case ColorMode.PURE:
                                imageBinary.AddRange(ColorChannelToBinList(pixel.R));
                                break;
                        }
                    }
                }
                return imageBinary.ToArray();
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
                List<int> binaryList = new List<int>();
                string BNR = Convert.ToString(r);
                foreach (char a in BNR)
                {
                    binaryList.Add(a);
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
            private static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
            {
                Bitmap result = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(result))
                {
                g.DrawImage(bmp, 0, 0, width, height);
                }
                return result;
            }
        public Bitmap IntArrayToBitmap()
        {
            int[] rgb = { 0, 0, 0 };
            Bitmap output = new(frameWidth,frameHeight);
            for (int h = 0; h < frameHeight; h += 1) {
                for (int w = 0; w < frameWidth; w += 1)
                {
                    switch (colorMode)
                    {
                        case ColorMode.BLACKWHITE:
                            if (EncodedBitArray[h * w + w] == 0)
                            {
                                output.SetPixel(w, h, Color.FromArgb(255,255,255,255));
                            }
                            else
                            {
                                output.SetPixel(w, h, Color.FromArgb(255, 0, 0, 0));
                            }
                            break;
                        case ColorMode.BINARIZEDRGB:
                            for(int i = 0; i < 3; i += 1)
                            {
                                rgb[i] = (EncodedBitArray[(h * w + w)*3+i] == 1) ? 255 : 0;
                            }
                            output.SetPixel(w, h, Color.FromArgb(255, rgb[0], rgb[1], rgb[2]));
                            break;
                        case ColorMode.PURE:
                            for (int i = 0; i < 3; i += 1)
                            {
                                int[] binaryColorArray = SubArray<int>(EncodedBitArray, (h * w + w) * 24 + 8 * i, 8);

                                rgb[i] = Convert.ToInt32(binaryColorArray);
                                
                            }
                            output.SetPixel(w, h, Color.FromArgb(255, rgb[0], rgb[1], rgb[2]));
                            break;
                    }
                }
            }
            

            return output;
        }
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
