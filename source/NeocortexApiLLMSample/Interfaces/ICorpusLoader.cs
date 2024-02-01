using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeocortexApiLLMSample.Interfaces
{
    public interface ICorpusLoader
    {
        /// <summary>
        /// Loads the corpus data from the given bookmark.
        /// </summary>
        /// <param name="bookmark">Specifies ehere to start loading.</param>
        /// <returns>Return the bookmark, that might be used at the next call.</returns>
        Task<string> Load(string bookmark);

    }
}
