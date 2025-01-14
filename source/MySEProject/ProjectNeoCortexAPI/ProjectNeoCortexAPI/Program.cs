using Daenet.Binarizer.Entities;

namespace ProjectNeoCortexAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to our project ML 24/25-01");
            Console.WriteLine("Investigate Image Reconstruction by using Classifiers");
            Console.WriteLine("Created by:");
            Console.WriteLine("     Anoushka Piplai[1566664]");
            Console.WriteLine("     Avradip Mazumdar[1566651]");
            Console.WriteLine("     Raka Sarkar[1567153]");
            Console.WriteLine("     Somava Ganguly[1566916]\n\n");

            Run();

        }
        public static void Run()
        {
            Console.WriteLine("Create the image binary with custom width and height and save it to a text file.");

            //Taking the Input Image and adding the Image Binarizer Parameters
            var config = new BinarizerParams
            {
                InputImagePath = "CommonFiles\\IMG.jpeg",
                OutputImagePath = ".\\BinarizedImage.txt",
                ImageWidth = 40,
                ImageHeight = 20
            };

            var img = new CustomImageBinarizer(config);
            img.Run();
        }
    }
}