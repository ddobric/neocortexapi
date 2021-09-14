using Emgu.CV;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
namespace VideoLibrary
{
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

            var fromBitmaps = ReadVideo(videoPath, frameRate);
            for (int i = 0; i < fromBitmaps.Count; i++)
            {
                NFrame tempFrame = new NFrame(fromBitmaps[i], name, label, i, frameWidth, frameHeight, colorMode);
                nFrames.Add(tempFrame);
            }
        }
        /// <summary>
        /// <para>Method to read a video into a list of Bitmap, from video path to a list of Bitmap</para>
        /// The current implementation uses OpenCV wrapper emgu
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
                    currentFrameIndex = (int)(stepCount * step);
                    videoBitmapArray.Add(currentFrame.ToBitmap());
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
                if (nf.FrameKey == key)
                {
                    return nf.EncodedBitArray;
                }
            }
            return new int[] { 4, 2, 3 };
        }
        public static void NFrameListToVideo(List<NFrame> bitmapList, string videoOutputPath, int frameRate, Size dimension, bool isColor)
        {
            using (VideoWriter videoWriter = new($"{videoOutputPath}.mp4", -1, (int)frameRate, dimension, isColor))
            {
                foreach (NFrame frame in bitmapList)
                {
                    Bitmap tempBitmap = frame.IntArrayToBitmap(frame.EncodedBitArray);
                    videoWriter.Write(tempBitmap.ToMat());
                }
            }
        }
    }
}
