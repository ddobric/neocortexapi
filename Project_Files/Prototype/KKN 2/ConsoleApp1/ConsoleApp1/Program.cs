using System;
namespace KNN
{
    class KNNProgram
    {
        /*static void Main(string[] args)
        {
                 Console.WriteLine("Begin weighted k-NN classification demo ");
                 Console.WriteLine("Normalized income, education data: ");
                 Console.WriteLine("[id =  0, 0.32, 0.43, class = 0]");
                 Console.WriteLine(" . . . ");
                 Console.WriteLine("[id = 29, 0.71, 0.22, class = 2]");

                 double[][] data = GetData();
                 double[] item = new double[] { 0.62, 0.35 };
                 Console.WriteLine("\nNearest (k=6) to (0.35, 0.38):");
                 Analyze(item, data, 6, 3);  // 3 classes
                 Console.WriteLine("\nEnd weighted k-NN demo ");
                 Console.ReadLine();
             }

            static void Analyze(double[] item, double[][] data, int k, int c)
             {
                 // 1. Compute all distances
                 int N = data.Length;
                 double[] distances = new double[N];
                 for (int i = 0; i < N; ++i)
                     distances[i] = DistFunc(item, data[i]); */

        /// 2. Get ordering
        int[] ordering = new int[N];
                 for (int i = 0; i < N; ++i)
                     ordering[i] = i;
                 double[] distancesCopy = new double[N];
                 Array.Copy(distances, distancesCopy, distances.Length);
                 Array.Sort(distancesCopy, ordering); 

                 // 3. Show info for k-nearest
                 double[] kNearestDists = new double[k];
                 for (int i = 0; i < k; ++i)
                 {
                     int idx = ordering[i];
                     ShowVector(data[idx]);
                     Console.Write("  dist = " +
                       distances[idx].ToString("F4"));
                     Console.WriteLine("  inv dist " +
                       (1.0 / distances[idx]).ToString("F4"));
                     kNearestDists[i] = distances[idx];
                 }

                 // 4. Vote
                 double[] votes = new double[c];  // one per class
                 double[] wts = MakeWeights(k, kNearestDists);
                 Console.WriteLine("\nWeights (inverse technique): ");
                 for (int i = 0; i < wts.Length; ++i)
                     Console.Write(wts[i].ToString("F4") + "  ");
                 Console.WriteLine("\n\nPredicted class: ");
                 for (int i = 0; i < k; ++i)
                 {
                     int idx = ordering[i];
                     int predClass = (int)data[idx][3];
                     votes[predClass] += wts[i] * 1.0;
                 }
                 for (int i = 0; i < c; ++i)
                     Console.WriteLine("[" + i + "]  " +
                     votes[i].ToString("F4"));
             } // Analyze

             static double[] MakeWeights(int k, double[] distances)
             {
                 // Inverse technique
                 double[] result = new double[k];  // one per neighbor
                 double sum = 0.0;
                 for (int i = 0; i < k; ++i)
                 {
                     result[i] = 1.0 / distances[i];
                     sum += result[i];
                 }
                 for (int i = 0; i < k; ++i)
                     result[i] /= sum;
                 return result;
             } */

            static double DistFunc(double[] item, double[] dataPoint)
            {
                double sum = 0.0;
                for (int i = 0; i < 2; ++i)
                {
                    double diff = item[i] - dataPoint[i + 1];
                    sum += diff * diff;
                }
                return Math.Sqrt(sum);
            }

        }
    }
