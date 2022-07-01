namespace Invariant.Entities
{
    public class DataSet
    {
        /// <summary>
        /// List of image names that represent labels in the learning process.
        /// </summary>
        public List<string> imageClasses;

        public List<Picture> images;

        public Random random;

        public int Count { get { return images.Count; } }

        public DataSet(string pathToTrainingFolder)
        {
            random = new Random(42);

            images = new List<Picture>();
            // Getting the classes
            InitImageClasses(pathToTrainingFolder);

            // Reading the images from path
            foreach (var classFolder in Directory.GetDirectories(pathToTrainingFolder))
            {
                string label = Path.GetFileName(classFolder);
                foreach (var imagePath in Directory.GetFiles(classFolder))
                {
                    images.Add(new Picture(imagePath, label));
                }
            }
        }


        /// <summary>
        /// Adding classes to the Training Data's Class(Label) List
        /// </summary>
        /// <param name="pathToTrainingFolder"></param>
        private void InitImageClasses(string pathToTrainingFolder)
        {
            imageClasses = new List<string>();

            foreach (var a in Directory.GetDirectories(pathToTrainingFolder))
            {
                imageClasses.Add(Path.GetFileNameWithoutExtension(a));
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
            return images[index];
        }
    }
}