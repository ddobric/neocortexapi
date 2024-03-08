using NeocortexApiLLMSample.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeocortexApiLLMSample
{
    internal class LevensteinSimilarity : ITokenSimilarity
    {
        public int CalcSimilarity(string word1, string word2)
        {
            int m = word1.Length;
            int n = word2.Length;

            // Create a 2D matrix to store distances
            int[,] dp = new int[m + 1, n + 1];

            // Initialize the matrix
            for (int i = 0; i <= m; ++i)
                dp[i, 0] = i;

            for (int j = 0; j <= n; ++j)
                dp[0, j] = j;

            // Fill the matrix
            for (int i = 1; i <= m; ++i)
            {
                for (int j = 1; j <= n; ++j)
                {
                    if (word1[i - 1] == word2[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = 1 + Math.Min(Math.Min(dp[i - 1, j], dp[i, j - 1]), dp[i - 1, j - 1]);
                }
            }

            return dp[m, n];
        }
    }
    
}
