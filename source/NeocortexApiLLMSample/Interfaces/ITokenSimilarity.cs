using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeocortexApiLLMSample.Interfaces
{
    /// <summary>
    /// Defines the used to calculate similarity between tokens.
    /// </summary>
    public interface ITokenSimilarity
    {
        /// <summary>
        /// Calculates the similarity between two words. It does not use embeddings or similar technique.
        /// </summary>
        /// <param name="token1">Any kind of token like syllable.</param>
        /// <param name="token2"></param>
        /// <returns></returns>
        int CalcSimilarity(string token1, string token2);
       
    }
}
