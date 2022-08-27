using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Invariant.Entities
{
    /// <summary>
    /// A collection of Images, with methods for ease of image manipulation
    /// </summary>
    public class DataSet:IList
    {
        /// <summary>
        /// List of all Image Classes/Labels
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// List of all Images
        /// </summary>
        public List<Picture> Images { get; set; }

        public Random random;

        /// <summary>
        /// Number of Images in the Dataset
        /// </summary>
        public int Count { get { return Images.Count; } }

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public object? this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Create another dataset based on a list of Images
        /// </summary>
        /// <param name="originDataSetPics"></param>
        public DataSet(List<Picture> imageList)
        {
            random = new Random(42);
            Labels = new List<string>();

            Images = new List<Picture>();
            this.Images = imageList;

            this.RecalculateImagesClasses();
        }

        /// <summary>
        /// Create a Dataset from a path to the training folder
        /// </summary>
        /// <param name="pathToTrainingFolder">path to the training folder</param>
        public DataSet(string pathToTrainingFolder)
        {
            random = new Random(42);
            Labels = new List<string>();
            Images = new List<Picture>();
            // Getting the classes
            InitImageClasses(pathToTrainingFolder);

            // Reading the images from path
            foreach (var classFolder in Directory.GetDirectories(pathToTrainingFolder))
            {
                string label = Path.GetFileName(classFolder);
                foreach (var imagePath in Directory.GetFiles(classFolder))
                {
                    Images.Add(new Picture(imagePath, label));
                }
            }
        }

        /// <summary>
        /// return a List of Images which was cut out from this dataset by percent
        /// </summary>
        /// <param name="outputTestData"></param>
        /// <param name="perCentSample">percentage from 0 to 100</param>
        /// <returns></returns>
        public DataSet GetTestData(double perCentSample)
        {
            List<Picture> takenImages = new List<Picture>();

            foreach (var imageClass in Labels)
            {
                var imageOfSameClass = new List<Picture>(Images.Where(p => (p.label == imageClass)));
                int stopIndex = (int) ((double)(imageOfSameClass.Count) * perCentSample / 100);
                for (int i = 0; i < stopIndex; i += 1)
                {
                    takenImages.Add(imageOfSameClass[i]);
                    Images.Remove(imageOfSameClass[i]);
                }
            }
            RecalculateImagesClasses();
            DataSet result = new DataSet(takenImages);
            return result;
        }

        /// <summary>
        /// Adding classes to the Training Data's Class(Label) List
        /// </summary>
        /// <param name="pathToTrainingFolder"></param>
        private void InitImageClasses(string pathToTrainingFolder)
        {

            foreach (var a in Directory.GetDirectories(pathToTrainingFolder))
            {
                Labels.Add(Path.GetFileNameWithoutExtension(a));
            }
        }

        /// <summary>
        /// Calculate ImageClasses from Images list 's label
        /// </summary>
        private void RecalculateImagesClasses (){ 
            List<string> imgClasses = new List<string>();
            foreach(var image in Images)
            {
                if (!imgClasses.Contains(image.label))
                {
                    imgClasses.Add(image.label);
                }
            }
            this.Labels = imgClasses;
        }


        /// <summary>
        /// Pick a random element in the set, with a specified seed
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Picture PickRandom(int seed = 42)
        {
            int index = random.Next(this.Count);
            return Images[index];
        }

        /// <summary>
        /// Provides shuffling of the list, based on Fisher-Yates Algorithm
        /// </summary>
        public void Shuffle()
        {
            Random rng = new Random(DateTime.Now.Hour);
            int n = this.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = Images[k];
                Images[k] = Images[n];
                Images[n] = value;
            }
        }


        /// <summary>
        /// Split the current dataset using K-Fold cross validation. 
        /// </summary>
        /// <param name="k"></param>
        /// <returns>first output: List of the Training DataSet, Second output: List of the Testing Set</returns>
        public (List<DataSet>, List<DataSet>) KFoldDataSetSplit(int k)
        {
            List<DataSet> trainingDataSet = new List<DataSet>();
            List<DataSet> testingDataSet = new List<DataSet>();

            this.Shuffle();

            for (int kIndex = 0; kIndex < k; kIndex += 1)
            {
                List<Picture> tempList = new List<Picture>(this.Images);
                int startIndex = tempList.Count / k * kIndex;
                int count = tempList.Count / k;
                testingDataSet.Add(new DataSet(tempList.GetRange(startIndex, count)));

                tempList.RemoveRange(startIndex, count);
                trainingDataSet.Add(new DataSet(tempList));
            }

            return (trainingDataSet, testingDataSet);
        }

        /// <summary>
        /// Original KFold take random datasample from the dataset. 
        /// Sampling around hundreds of sample resulted in random number of training sample for each label
        /// with the current Experiment setup of around hundreds sample, the old sampling is prone to cause uneven data distribution for samples of different labels.
        /// the smaller the number of training sample, the more the testing samples of that class there are. This cause low sample learning of some label when practice with a small number of samples.
        /// Thus splitting the data more evenly for all labels etablish a fairer validation for our Training model.
        /// At the time, the images set is filtered for each label, then added one by one accross all kFold block. The blocks are then used to construct the training and testing data
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public (List<DataSet>, List<DataSet>) KFoldDataSetSplitEvenly(int k)
        {
            List<DataSet> trainingDataSet = new List<DataSet>();
            List<DataSet> testingDataSet = new List<DataSet>();
            List<List<Picture>> kFoldPicSets = new List<List<Picture>>();
            for (int kIndex = 0; kIndex < k; kIndex += 1)
            {
                kFoldPicSets.Add(new List<Picture>());
            }
            foreach (var imageClass in Labels)
            {
                var imageOfSameClass = new List<Picture>(Images.Where(p => (p.label == imageClass)));
                for(int i = 0;i < imageOfSameClass.Count;i+=1)
                {
                    kFoldPicSets[i % k].Add(imageOfSameClass[i]);
                }
            }

            foreach (var kFoldPicSet in kFoldPicSets)
            {
                
                testingDataSet.Add(new DataSet(kFoldPicSet));

                List<List<Picture>> kFoldPicSetsTemp = new List<List<Picture>>(kFoldPicSets);
                kFoldPicSetsTemp.Remove(kFoldPicSet);
                trainingDataSet.Add(new DataSet(kFoldPicSetsTemp.SelectMany(x => x).ToList()));
            }

            return (trainingDataSet, testingDataSet);
        }

        /// <summary>
        /// output the Dataset in the specified path
        /// </summary>
        /// <param name="path">path where the dataset should be drawn</param>
        public void VisualizeSet(string path)
        {
            foreach (var imageClass in Labels)
            {
                string imageClassPath = Path.Combine(path, $"{imageClass}");
                if (!Directory.Exists(imageClassPath))
                {
                    Directory.CreateDirectory(imageClassPath);
                }
            }
            foreach (var image in Images)
            {
                string imagePath = Path.Combine(path, image.label, Path.GetFileName(image.imagePath));
                image.SaveTo(imagePath);
            }
        }

        public static DataSet CreateTestSet(DataSet sourceSet_32x32, int samples, int width, int height, string fileName)
        {
            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }

            List<Picture> testImages = new List<Picture>();

            int[] checkIndicies = new int[10];
            Random random = new Random(42);
            foreach (var image in sourceSet_32x32.Images)
            {
                double[,,] outputPixels = new double[width, height, 3];
                double[,,] pixelFromImage = image.GetPixels();
                if(!Directory.Exists(Path.Combine(fileName, image.label.ToString())))
                {
                    Directory.CreateDirectory(Path.Combine(fileName, image.label.ToString()));
                }
                string testImagePath = Path.Combine(fileName, image.label.ToString(), $"{checkIndicies[Int64.Parse(image.label)]}");
                checkIndicies[Int64.Parse(image.label)] += 1;
                List<Frame> listFrames = Frame.GetConvFramesbyPixel(width, height, sourceSet_32x32.Images[0].imageWidth, sourceSet_32x32.Images[0].imageHeight);

                
                var selectedFrame = listFrames[random.Next() % listFrames.Count];

                outputPixels = Picture.ApplyPixels(pixelFromImage, outputPixels, selectedFrame);

                Picture.SaveAsImage(outputPixels, $"{testImagePath}.png");

                Picture testImage = new Picture($"{testImagePath}.png", image.label);
                testImages.Add(testImage);
            }
            DataSet outputSet = new DataSet(testImages);
            return outputSet;
        }

        #region Implement List Iteration
        public int Add(object? pic)
        {
            if (pic != null)
            {
                Images.Add((Picture)pic);
                return this.Count;
            }
            return -1;
        }

        public void Clear()
        {
            Images.Clear();
        }

        public bool Contains(object? pic)
        {
            if(pic is Picture)
            {
                return Images.Contains(pic);
            }
            return false;
        }

        public int IndexOf(object? pic)
        {
            if (pic is Picture)
            {
                return Images.IndexOf((Picture)pic);
            }
            return -1;
        }

        public void Insert(int index, object? pic)
        {
            if (pic is Picture)
            {
                Images.Insert(index,(Picture)pic);
            }
        }

        public void Remove(object? pic)
        {
            if (pic is Picture)
            {
                Images.Remove((Picture)pic);
            }
        }

        public void RemoveAt(int index)
        {
            Images.RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            Images.CopyTo((Picture[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return new DataSetEnumerator(Images);
        }

        private class DataSetEnumerator : IEnumerator
        {
            private List<Picture> Images;
            int position = -1;
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            public DataSetEnumerator(List<Picture> images)
            {
                this.Images = images;
            }

            public object Current {
                get
                {
                    try
                    {
                        return Images[position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public bool MoveNext()
            {
                position++;
                return (position < Images.Count);
            }

            public void Reset()
            {
                position = -1;
            }
        }
        #endregion
    }
}