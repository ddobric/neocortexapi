internal class TrainingData
{
    public List<string> classes;

    public List<InvImage> images;

    public TrainingData(string pathToTrainingFolder)
    {
        images = new List<InvImage>();
        // Getting the classes
        ClassesInit(pathToTrainingFolder);

        // Reading the images from path
        foreach (var classFolder in Directory.GetDirectories(pathToTrainingFolder))
        {
            string label = Path.GetFileName(classFolder);
            foreach (var imagePath in Directory.GetFiles(classFolder))
            {
                images.Add(new InvImage(imagePath, label));
            }
        }
    }

    public int Count { get { return images.Count; } }

    /// <summary>
    /// adding classes to the Training Data's Class(Label) List
    /// </summary>
    /// <param name="pathToTrainingFolder"></param>
    private void ClassesInit(string pathToTrainingFolder)
    {
        classes = new List<string>();
        foreach (var a in Directory.GetDirectories(pathToTrainingFolder))
        {
            classes.Add(Path.GetFileNameWithoutExtension(a));
        }
    }

    /// <summary>
    /// Pick a random element in the set, with a specified seed
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    internal InvImage PickRandom(int seed = 42)
    {
        var random = new Random(seed);
        int index = random.Next(this.Count);
        return images[index];
    }
}