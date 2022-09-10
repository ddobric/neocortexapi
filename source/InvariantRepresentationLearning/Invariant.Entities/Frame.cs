namespace Invariant.Entities
{
    public class Frame
    {
        /// <summary>
        /// top left x coordinate
        /// </summary>
        public int tlX;

        /// <summary>
        /// top left y coordinate
        /// </summary>
        public int tlY;

        /// <summary>
        /// bottom right x coordinate
        /// </summary>
        public int brX;

        /// <summary>
        /// bottom left y coordinate
        /// </summary>
        public int brY;

        public int PixelCount { get 
            { 
                int count = (brX-tlX+1)*(brY-tlY+1);
                return count;
            } 
        }

        /// <summary>
        /// pixel-scale tool variable for defining a frame
        /// </summary>
        /// <param name="v1">top left X coord</param>
        /// <param name="v2">top left Y coord</param>
        /// <param name="lastXIndex">bottom right X coord</param>
        /// <param name="lastYIndex">bottom right Y coord</param>
        public Frame(int v1, int v2, int lastXIndex, int lastYIndex)
        {
            tlX = v1;
            tlY = v2;
            brX = lastXIndex;
            brY = lastYIndex;
        }

        /// <summary>
        /// <br>get division index from the given input params</br>
        /// <br>E.g:</br>
        /// <br>input head = 0; tail = 6, No = 3</br>
        /// <br>output list = {0, 3, 6}</br>
        /// </summary>
        /// <param name="firstIndex">first index of output series</param>
        /// <param name="lastIndex">last index of output series</param>
        /// <param name="No"></param>
        /// <returns></returns>
        public static List<int> GetIndexes(
            int firstIndex, 
            int lastIndex, 
            int No)
        {
            #region error input handling
            if (lastIndex <= firstIndex)
            {
                throw new Exception("head index cannot be larger than tail index");
            }
            else if (No < 2)
            {
                throw new Exception("number of frame index cannot be smaller than 2");
            }
            else if (firstIndex < 0 | lastIndex < 0 | No < 0)
            {
                throw new Exception("None of the input params can be negative");
            }
            #endregion

            List<int> result = new List<int>();

            result.Add(firstIndex);
            if (No > 2)
            {
                for (int i = 1; i <= No - 2; i++)
                {
                    double nextIndex = (double)(lastIndex - firstIndex) / (No - 1) * i + firstIndex;
                    result.Add((int)nextIndex);
                }
            }
            result.Add(lastIndex);
            return result;
        }

        /// <summary>
        /// Get multiple convolutional frames from original image
        /// </summary>
        /// <param name="imgWidth">width of original image</param>
        /// <param name="imgHeight">height of original image</param>
        /// <param name="frameWidth">width of the convolutional frame</param>
        /// <param name="frameHeight">height of the convolutional frame</param>
        /// <param name="NoX">number of convolutional frame in 1 width-length row</param>
        /// <param name="NoY">number of convolutional frame in 1 height-length column</param>
        /// <returns></returns>
        public static List<Frame> GetConvFrames(int imgWidth, int imgHeight, int frameWidth, int frameHeight, int NoX, int NoY)
        {
            List<Frame> result = new List<Frame>();

            var xIndicies = GetIndexes(0, imgWidth - frameWidth, NoX);
            var yIndicies = GetIndexes(0, imgHeight - frameHeight, NoY);
            for (int i = 0; i < xIndicies.Count; i++)
            {
                for (int j = 0; j < yIndicies.Count; j++)
                {
                    result.Add(
                        new Frame(
                            xIndicies[i], 
                            yIndicies[j], 
                            xIndicies[i] + frameWidth - 1, 
                            yIndicies[j] + frameHeight - 1
                            )
                        );
                }
            }
            return result;
        }

        /// <summary>
        /// Get a list of Frames which are positioned in a grid spaced by number of pixels from an image
        /// </summary>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        /// <param name="NoPixels"></param>
        /// <returns></returns>
        public static List<Frame> GetConvFramesbyPixel(int imgWidth, int imgHeight, int frameWidth, int frameHeight, int NoPixels = 1)
        {
            List<Frame> result = new List<Frame>();

            var xIndicies = GetDivisionIndexByPixel(0, imgWidth - frameWidth);
            var yIndicies = GetDivisionIndexByPixel(0, imgHeight - frameHeight);
            for (int i = 0; i < xIndicies.Count; i+=NoPixels)
            {
                if (i > xIndicies.Count)
                {
                    continue;
                }
                for (int j = 0; j < yIndicies.Count; j+=NoPixels)
                {
                    if (j > yIndicies.Count)
                    {
                        continue;
                    }
                    result.Add(new Frame(xIndicies[i], yIndicies[j], xIndicies[i] + frameWidth - 1, yIndicies[j] + frameHeight - 1));
                }
            }
            return result;
        }

        /// <summary>
        /// Get the division indicies from start indicies to end indicies, in which the next index is shifted from the previous by 1 pixels
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tails"></param>
        /// <returns></returns>
        private static List<int> GetDivisionIndexByPixel(int head, int tails)
        {
            List<int> result = new List<int>();
            for (int i = head; i <= tails; i += 1)
            {
                result.Add(i);
            }
            return result;
        }
    }
}