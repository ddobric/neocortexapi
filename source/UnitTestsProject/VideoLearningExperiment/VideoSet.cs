using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using GleamTech.VideoUltimate;
using NeoCortexApi.Utility;
namespace UnitTestsProject.VideoLearningExperiments
{
    /// <summary>
    /// <para>VideoSet is created to represent an folder of videos</para>
    /// <para>Each videos will be read to a Video object</para>
    /// <para>For Example:</para>
    /// <para>
    /// <br>A folder ball/ contains 3 file one.mp4, two.mp4 and three.mp4</br>
    /// <br>will create a VideoSet with label "ball", which contains 3 Video objects:</br>
    /// <br>each with name field respectively: one.mp4, two</br>
    /// </para>
    /// </summary>
    public class VideoSet
    {
        public List<Video> videoEncodedList;
        public List<string> videoName;
        public string setLabel;
        public VideoSet(string videoSetPath)
        {
            videoEncodedList = new();
            videoName = new();

            // Set the label of the video collection as the name of the folder that contains it 
            this.setLabel = Path.GetFileNameWithoutExtension(videoSetPath);

            // Read videos from the video folder path 
            videoEncodedList = ReadVideos(videoSetPath);
        }
        /// <summary>
        /// Read all videos within a provided folder's full path, the foleder name will be used as videoset's Label
        /// </summary>
        /// <param name="videoSetPath"> The Path of the folder that contains the videos</param>
        private List<Video> ReadVideos(string videoSetPath)
        {
            List<Video> videoList = new();
            // Iteate through each videos in the videos' folder
            foreach (string file in Directory.GetFiles(videoSetPath))
            {
                string fileName = Path.GetFileName(videoSetPath);
                videoName.Add(fileName);
                Debug.WriteLine($"Video file name: {fileName}");
                videoList.Add(new Video(file));
            }
            return videoList;
        }
    }
    /// <summary>
    /// <para>
    /// <br>Represent a single video, which contains</br>
    /// <br>Name of the video, including suffix format</br>
    /// <br>List of int[], with each int[] is a frame in chronological order</br>
    /// </para>
    /// </summary>
    public class Video
    {
        public string name;
        public List<int[]> frames;
        /// <summary>
        /// Generate a Video object
        /// </summary>
        /// <param name="videoPath">full path to the video</param>
        public Video(string videoPath)
        {
            this.frames = new();
            this.name = videoPath;
            this.frames = ReadVideo(videoPath);
        }

        private static List<int[]> ReadVideo(string videoPath)
        {
            List<int[]> frames_binaryArray = new();

            // using VideosReader nuget package Videos.Ultimate from GleamTech
            var videoFrameReader = new VideoFrameReader(videoPath);
            int frameIndex = 0;
            while (videoFrameReader.Read())
            {
                Debug.WriteLine("Coded Frame Number: " + videoFrameReader.CurrentFrameNumber);
                Debug.WriteLine("Frame Index: " + frameIndex++);
                Bitmap currentFrame = videoFrameReader.GetFrame();
                int[] a = BitmapToBinaryArray(currentFrame, 10, 10);
                Debug.WriteLine(a.ArrToString());
                frames_binaryArray.Add(a);
                Debug.WriteLine($"{frames_binaryArray[frameIndex-1]}");
            }
            //
            return frames_binaryArray;
        }

        // Ultility Bitmap Image functions
        /// <summary>
        /// Bitmap Binary Encoder Binarize an Bitmap type image
        /// and encode it to an int[] of binary value
        /// <para>Example</para> 
        /// <para>int[] inputBitArray = ImageToBin(imageBitmapType, scaledWidth = 10, scaledHeight = 10)</para>
        /// <para>will produce an int[] inputBitArray with length 10 x 10 = 100 {1, 0, 0, 0, 1, 1, 1, ...}</para>
        /// </summary>
        /// <param name="file"> Bitmap image object to encode</param>
        /// <param name="width"> Width of the output image before encoding</param>
        /// <param name="height">Height of the output image before encoding</param>
        /// <returns>returns an int[] of binarized pixels from Bitmap image object</returns>
        private static int[] BitmapToBinaryArray(Bitmap file, int width, int height)
        {
            var image = new Bitmap(file);
            Bitmap img = ResizeBitmap(image, width, height);
            int length = img.Width * img.Height;
            int[] imageBinary = new int[length];

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    if (pixel.R < 100)
                    {
                        imageBinary[j + i * img.Height] = 1;
                    }
                    else
                    {
                        imageBinary[j + i * img.Height] = 0;
                    }
                }
            }
            return imageBinary;
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
