using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task UploadExperimentResult(ExperimentResult result)
        {
            Random rnd = new Random();
            int rowKeyNumber = rnd.Next(0, 1000);
            string rowKey = "variable-i-" + rowKeyNumber.ToString();
            string partitionKey = "cc-proj-" + rowKey;
            
            var testResult = new ExperimentResult(partitionKey, rowKey)
            {
                
                ExperimentId = result.ExperimentId,
                Name = result.Name,
                Description = result.Description,
                StartTimeUtc = result.StartTimeUtc,
                EndTimeUtc = result.EndTimeUtc,
                TestData = result.TestData,
                TestName = result.TestName
            };
            Console.WriteLine($"Upload ExperimentResult to table: {this.config.ResultTable}");
            var client = new TableClient(this.config.StorageConnectionString, this.config.ResultTable);

            await client.CreateIfNotExistsAsync();
            try
            {
                await client.AddEntityAsync<ExperimentResult>(testResult);
                //await client.UpsertEntityAsync<ExperimentResult>(minimalResult);
                Console.WriteLine("Uploaded to Table Storage completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to upload to Table Storage: {ex.ToString()}");
            }

        }

        public async Task<byte[]> UploadResultFile(string fileName, byte[] data)
        {
            var experimentLabel = fileName;

            BlobServiceClient blobServiceClient = new BlobServiceClient(this.config.StorageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(this.config.ResultContainer);

            // Write encoded data to text file
            byte[] testData = data;

            // Generate a unique blob name (you can customize this logic)
            string blobName = experimentLabel;

            // Upload the text data to the blob container
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (MemoryStream memoryStream = new MemoryStream(testData))
            {
                await blobClient.UploadAsync(memoryStream);
            }
        
        }   

    }


}
