namespace Invariant.Entities
{
    public class DataSet
    {
        /// <summary>
        /// List of image names that represent labels in the learning process.
        /// </summary>
        public List<string> ImageClasses;

        public List<Picture> Images;

        public Random random;

        public int Count { get { return Images.Count; } }

        public DataSet(List<Picture> originDataSetPics)
        {
            random = new Random(42);
            ImageClasses = new List<string>();
            Images = new List<Picture>();
            this.Images = originDataSetPics;
            foreach(Picture picture in originDataSetPics)
            {
                if (!this.ImageClasses.Contains(picture.label))
                {
                    this.ImageClasses.Add(picture.label);
                }
            }
        }

        public DataSet(string pathToTrainingFolder)
        {
            random = new Random(42);
            ImageClasses = new List<string>();
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
        /// Adding classes to the Training Data's Class(Label) List
        /// </summary>
        /// <param name="pathToTrainingFolder"></param>
        private void InitImageClasses(string pathToTrainingFolder)
        {

            foreach (var a in Directory.GetDirectories(pathToTrainingFolder))
            {
                ImageClasses.Add(Path.GetFileNameWithoutExtension(a));
            }
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

            for(int kIndex = 0; kIndex< k; kIndex += 1)
            {
                List<Picture> tempList = new List<Picture>(this.Images);
                int startIndex = tempList.Count/k * kIndex;
                int count = tempList.Count/k;
                testingDataSet.Add(new DataSet(tempList.GetRange(startIndex, count)));

                tempList.RemoveRange(startIndex,count);
                trainingDataSet.Add(new DataSet(tempList));
            }

            return (trainingDataSet, testingDataSet);
        }
    }
}