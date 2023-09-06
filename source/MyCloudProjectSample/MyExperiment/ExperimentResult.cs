using Azure;
using Azure.Data.Tables;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyExperiment
{

    public class ExperimentResult : ITableEntity, IExperimentResult
    {
        public ExperimentResult(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey= rowKey;
        }

        public string PartitionKey { get; set; }
        
        public string RowKey { get; set; }
       
        public DateTimeOffset? Timestamp { get ; set ; }
        
        public ETag ETag { get; set; }


        public string ExperimentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public long DurationSec { get; set; }

        public string InputFileUrl { get; set; }

        public string[] OutputFiles { get; set; }
        // Your properties related to experiment.

        public float Accuracy { get; set; }


    }
}
