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
        public List<Image> Images { get; set; }

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
        public DataSet(List<Image> imageList)
        {
            random = new Random(42);
            Labels = new List<string>();

            Images = new List<Image>();
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
            Images = new List<Image>();
            // Getting the classes
            InitImageClasses(pathToTrainingFolder);

            // Reading the images from path
            foreach (var classFolder in Directory.GetDirectories(pathToTrainingFolder))
            {
                string label = Path.GetFileName(classFolder);
                foreach (var imagePath in Directory.GetFiles(classFolder))
                {
                    Images.Add(new Image(imagePath, label));
                }
            }
        }
        public static DataSet ScaleSet(string experimentFolder, int width, int height, DataSet sourceSet, string name)
        {
            string sourceMNIST_32x32 = Path.Combine(experimentFolder, $"{name}_{width}x{height}");
            if (!Directory.Exists(sourceMNIST_32x32)) 
            {
                Directory.CreateDirectory((sourceMNIST_32x32));
            }
            
            foreach (var image in sourceSet.Images)
            {
                string digitLabelFolder = Path.Combine(sourceMNIST_32x32, image.Label);
                if (!Directory.Exists(digitLabelFolder))
                {
                    Directory.CreateDirectory((digitLabelFolder));
                }
                image.SaveTo_Scaled(Path.Combine(digitLabelFolder, Path.GetFileName(image.ImagePath)), width, height);
            }
            DataSet sourceSet_32x32 = new DataSet(sourceMNIST_32x32);
            return sourceSet_32x32;
        }
        /// <summary>
        /// return a List of Images which was cut out from this dataset by percent
        /// </summary>
        /// <param name="outputTestData"></param>
        /// <param name="perCentSample">percentage from 0 to 100</param>
        /// <returns></returns>
        public DataSet GetTestData(double perCentSample)
        {
            List<Image> takenImages = new List<Image>();

            foreach (var imageClass in Labels)
            {
                var imageOfSameClass = new List<Image>(Images.Where(p => (p.Label == imageClass)));
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
                if (!imgClasses.Contains(image.Label))
                {
                    imgClasses.Add(image.Label);
                }
            }
            this.Labels = imgClasses;
        }


        /// <summary>
        /// Pick a random element in the set, with a specified seed
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Image PickRandom(int seed = 42)
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
                List<Image> tempList = new List<Image>(this.Images);
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
            List<List<Image>> kFoldPicSets = new List<List<Image>>();
            for (int kIndex = 0; kIndex < k; kIndex += 1)
            {
                kFoldPicSets.Add(new List<Image>());
            }
            foreach (var imageClass in Labels)
            {
                var imageOfSameClass = new List<Image>(Images.Where(p => (p.Label == imageClass)));
                for(int i = 0;i < imageOfSameClass.Count;i+=1)
                {
                    kFoldPicSets[i % k].Add(imageOfSameClass[i]);
                }
            }

            foreach (var kFoldPicSet in kFoldPicSets)
            {
                
                testingDataSet.Add(new DataSet(kFoldPicSet));

                List<List<Image>> kFoldPicSetsTemp = new List<List<Image>>(kFoldPicSets);
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
                string imagePath = Path.Combine(path, image.Label, Path.GetFileName(image.ImagePath));
                image.SaveTo(imagePath);
            }
        }

        /// <summary>
        /// <br>Check by pixels if an image is already presented in a specified training folder</br>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageSet"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static bool ExistImageInDataSet(Image image, string imageSet, Frame frame)
        {
            foreach (var dir in Directory.GetDirectories(imageSet))
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    Image a = new Image(file, "test");
                    if (Image.AreSamePixels(Image.Binarize(image.GetPixels(frame)), Image.Binarize(a.GetPixels())))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static DataSet CreateTestSet(DataSet sourceSet_32x32, int width, int height, string fileName)
        {
            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }

            List<Image> testImages = new List<Image>();

            int[] checkIndicies = new int[10];
            Random random = new Random(42);
            foreach (var image in sourceSet_32x32.Images)
            {
                double[,,] outputPixels = new double[width, height, 3];
                double[,,] pixelFromImage = image.GetPixels();
                if(!Directory.Exists(Path.Combine(fileName, image.Label.ToString())))
                {
                    Directory.CreateDirectory(Path.Combine(fileName, image.Label.ToString()));
                }
                string testImagePath = Path.Combine(fileName, image.Label.ToString(), $"{checkIndicies[Int64.Parse(image.Label)]}");
                checkIndicies[Int64.Parse(image.Label)] += 1;
                List<Frame> listFrames = Frame.GetConvFramesbyPixel(width, height, sourceSet_32x32.Images[0].ImageWidth, sourceSet_32x32.Images[0].ImageHeight);

                
                var selectedFrame = listFrames[random.Next() % listFrames.Count];

                outputPixels = Image.ApplyPixels(pixelFromImage, outputPixels, selectedFrame);

                Image.SaveTo(outputPixels, $"{testImagePath}.png");

                Image testImage = new Image($"{testImagePath}.png", image.Label);
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
                Images.Add((Image)pic);
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
            if(pic is Image)
            {
                return Images.Contains(pic);
            }
            return false;
        }

        public int IndexOf(object? pic)
        {
            if (pic is Image)
            {
                return Images.IndexOf((Image)pic);
            }
            return -1;
        }

        public void Insert(int index, object? pic)
        {
            if (pic is Image)
            {
                Images.Insert(index,(Image)pic);
            }
        }

        public void Remove(object? pic)
        {
            if (pic is Image)
            {
                Images.Remove((Image)pic);
            }
        }

        public void RemoveAt(int index)
        {
            Images.RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            Images.CopyTo((Image[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return new DataSetEnumerator(Images);
        }

        private class DataSetEnumerator : IEnumerator
        {
            private List<Image> Images;
            int position = -1;
            private IEnumerator getEnumerator()
            {
                return (IEnumerator)this;
            }
            public DataSetEnumerator(List<Image> images)
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