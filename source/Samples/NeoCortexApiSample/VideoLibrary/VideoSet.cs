using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace VideoLibrary
{

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
                string folderName = $"{videoOutputDirectory}"+@"\"+$"{nv.label}";
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                NVideo.NFrameListToVideo(nv.nFrames,$"{folderName}"+@"\"+$"{nv.name}",(int)nv.frameRate,new Size(nv.frameWidth,nv.frameHeight),true);
                if(!Directory.Exists($"{folderName}" + @"\" + $"{nv.name}"))
                {
                    Directory.CreateDirectory($"{folderName}" + @"\" + $"{nv.name}");
                }
                for(int i = 0;i<nv.nFrames.Count;i+=1 )
                {
                    nv.nFrames[i].SaveFrame($"{folderName}" + @"\" + $"{nv.name}" + @"\" + $"{nv.nFrames[i].FrameKey}.png");
                }
            }
        }
        public NFrame GetNFrameFromFrameKey(string currentFrameKey)
        {
            foreach (var nv in nVideoList)
            {
                foreach (var nf in nv.nFrames)
                {
                    if (nf.FrameKey == currentFrameKey)
                    {
                        return nf;
                    }
                }
            }
            return null;
        }
    }

    
}
