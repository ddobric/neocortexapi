using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment
{
    public class AzureStorageProvider : IStorageProvider
    {
        private MyConfig config;

        public AzureStorageProvider(IConfigurationSection configSection)
        {
            config = new MyConfig();
            configSection.Bind(config);
        }

        public async Task<string> DownloadInputFile(string fileName)
        {
            BlobContainerClient container = new BlobContainerClient("read from config", "sample-container TODO. Read from config");
            await container.CreateIfNotExistsAsync();

            // Get a reference to a blob named "sample-file"
            BlobClient blob = container.GetBlobClient(fileName);

            //throw if not exists:
            //blob.ExistsAsync

            // return "../myinputfilexy.csv"
            throw new NotImplementedException();
        }

        public async Task UploadExperimentResult(IExperimentResult result)
        {
            var experimentLabel = result.Name;

            BlobServiceClient blobServiceClient = new BlobServiceClient(this.config.StorageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("adaptsegmentsunittests-teamas");

            // Write encoded data to text file
            string textData = result.Description;

            // Generate a unique blob name (you can customize this logic)
            string blobName = $"Test_data_{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}.txt";

            // Upload the text data to the blob container
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(textData)))
            {
                await blobClient.UploadAsync(memoryStream);
            }
        }

        public async Task<byte[]> UploadResultFile(string fileName, byte[] data)
        {


            throw new NotImplementedException();
        }

    }


}
