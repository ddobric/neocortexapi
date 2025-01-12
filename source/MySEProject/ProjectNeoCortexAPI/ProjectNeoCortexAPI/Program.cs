namespace ProjectNeoCortexAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Welcome to our project ML 24/25-01");
                Console.WriteLine("Investigate Image Reconstruction by using Classifiers");
                Console.WriteLine("Created by:");
                Console.WriteLine("     Anoushka Piplai[1566664]");
                Console.WriteLine("     Avradip Mazumdar[1566651]");
                Console.WriteLine("     Raka Sarkar[1567153]");
                Console.WriteLine("     Somava Ganguly[1566916]\n\n");


                // Specifying the folder path containing images to load for the experiment
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Project_Pictures");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
