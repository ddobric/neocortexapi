using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using GleamTech.VideoUltimate;
using System;

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
        public List<Video> videoEncodedList;
        public List<string> videoName;
        public string setLabel;

        public VideoSet(string videoSetPath, ColorMode colorMode, int frameWidth, int frameHeight)
        {
            videoEncodedList = new();
            videoName = new();
            // Set the label of the video collection as the name of the folder that contains it 
            this.setLabel = Path.GetFileNameWithoutExtension(videoSetPath);

            // Read videos from the video folder path 
            videoEncodedList = ReadVideos(videoSetPath, colorMode, frameWidth, frameHeight);
        }
        /// <summary>
        /// Read all videos within a provided folder's full path, the foleder name will be used as videoset's Label
        /// </summary>
        /// <param name="videoSetPath"> The Path of the folder that contains the videos</param>
        private List<Video> ReadVideos(string videoSetPath, ColorMode colorMode, int frameWidth, int frameHeight)
        {
            List<Video> videoList = new();
            // Iteate through each videos in the videos' folder
            foreach (string file in Directory.GetFiles(videoSetPath))
            {
                string fileName = Path.GetFileName(videoSetPath);
                videoName.Add(fileName);
                Debug.WriteLine($"Video file name: {fileName}");
                videoList.Add(new Video(file, colorMode, frameWidth, frameHeight));
            }
            return videoList;
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
    public class Video
    {
        public string name;
        public List<int[]> frames;

        private readonly ColorMode colorMode;
        private readonly int frameWidth;
        private readonly int frameHeight;
        /// <summary>
        /// Generate a Video object
        /// </summary>
        /// <param name="videoPath">full path to the video</param>
        /// <param name="colorMode">Color mode to encode each frame in the Video, see enum VideoSet.ColorMode</param>
        /// <param name="frameHeight">height in pixels of the video resolution</param>
        /// <param name="frameWidth">width in pixels of the video resolution</param>
        public Video(string videoPath, ColorMode colorMode, int frameWidth, int frameHeight)
        {
            this.colorMode = colorMode;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            this.frames = new();
            this.name = Path.GetFileName(videoPath);
            this.frames = BitmapToBinaryArray(ReadVideo(videoPath));
        }
        /// <summary>
        /// <para>Method to read a video into a list of Bitmap, from video path to a list of Bitmap</para>
        /// The current implementation used Videos Ultimate C# wrapper from GleamTech.
        /// Which will create a trademark when create a video after 30 days trial
        /// </summary>
        /// <param name="videoPath"> full path of the video to be read </param>
        /// <returns>List of Bitmaps</returns>
        private static List<Bitmap> ReadVideo(string videoPath)
        {
            // FURTHER IMPLEMENTATION:
            //  include frame rate read,
            //  read metadata from video 
            List<Bitmap> frames_binaryArray = new();

            // using VideosReader nuget package Videos.Ultimate from GleamTech
            var videoFrameReader = new VideoFrameReader(videoPath);
            int frameIndex = 0;
            while (videoFrameReader.Read())
            {
                Debug.WriteLine("Coded Frame Number: " + videoFrameReader.CurrentFrameNumber);
                Debug.WriteLine("Frame Index: " + frameIndex++);
                Bitmap currentFrame = videoFrameReader.GetFrame();
                frames_binaryArray.Add(currentFrame);
                Debug.WriteLine($"{frames_binaryArray[frameIndex-1]}");
            }
            //
            return frames_binaryArray;
        }

        /// <summary>
        /// Encode Bitmap to an int array by iterating through every pixel of the frame.
        /// </summary>
        /// <param name="image"> Bitmap image object to encode</param>
        /// <returns>returns an int array from Bitmap image object</returns>
        private int[] BitmapToBinaryArray(Bitmap image)
        {
            Bitmap img = ResizeBitmap(image, frameWidth, frameHeight);
            List<int> imageBinary = new();

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
                            imageBinary.Add((pixel.R > 255 / 2) ? 1 : 0);
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
            List<int> binaryList = new();
            string BNR = Convert.ToString(r);
            foreach(char a in BNR){
                binaryList.Add(a);
            }
            return binaryList;
        }
        /// <summary>
        /// Convert a List of Bitmap to an int array, which will be used as input for Spatial Pooler
        /// </summary>
        /// <param name="imageList">List of input Bitmap</param>
        /// <returns></returns>
        private List<int[]> BitmapToBinaryArray(List<Bitmap> imageList)
        {
            List<int[]> binaryArrayList = new();
            foreach(Bitmap bmp in imageList)
            {
                binaryArrayList.Add(BitmapToBinaryArray(bmp));
            }
            return binaryArrayList;
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
            Bitmap result = new(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }
    }

}
