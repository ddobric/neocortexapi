
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudProject.Common
{
    public interface IExperimentResult
    {
        string ExperimentId { get; set; }

        string Name { get; set; }

        string Description { get; set; }
        
        DateTime? StartTimeUtc { get; set; }

        DateTime? EndTimeUtc { get; set; }
    }

}
