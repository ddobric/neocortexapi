using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment.Utilities
{
    public class AzureHelper
    {
        public static Task<string> GetInputFileUrl(string fileName, MyConfig config)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(config.StorageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(config.TrainingContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return Task.FromResult(blobClient.Uri.ToString());
        }
    }
}
