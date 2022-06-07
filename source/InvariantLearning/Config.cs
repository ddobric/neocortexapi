using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvariantLearning
{

    /// <summary>
    /// Experiment Folder Configuration
    /// </summary>
    public class ExperimentConfig
    {
        /// <summary>
        /// Path to the the folder which containthe training image
        /// </summary>
        public string? PathToTrainingFolder;

        /// <summary>
        /// Run Parameter of the Experiment
        /// </summary>
        public RunConfig? runParams;
    }

    /// <summary>
    /// Experiment running configuration
    /// </summary>
    public class RunConfig
    {
        /// <summary>
        /// Hierarchical Temporal Memory Configuration
        /// </summary>
        public HtmConfig? htmConfig;

        /// <summary>
        /// Iteration through whole dataset
        /// </summary>
        public int epochs;
    }
}
