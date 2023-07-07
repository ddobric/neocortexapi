using System.Collections.Generic;
using System.IO;
using NeoCortexApi.Entities;
using Newtonsoft.Json;
using BlobContainerClient = Azure.Storage.Blobs.BlobContainerClient;

namespace NeoCortexApiSample
{
    public class FetchConfig
    {
        /// <summary>
        /// Initializes object with the specified parameters.
        /// </summary>
        /// <param name="ConnectionString"></param>
        public static HtmConfig ConfigurationHtm(string ConnectionString)
        {
            var container = new BlobContainerClient(ConnectionString, "knn-htm-config");
            var blob = container.GetBlobClient("htm-config.json");
            using (var stream = blob.OpenReadAsync().Result)
            using (var sr = new StreamReader(stream))
            using (var jsonStream = new JsonTextReader(sr))
            {
                return JsonSerializer.CreateDefault().Deserialize<HtmConfig>(jsonStream);
            }
        }
        
        /// <summary>
        /// Initializes object with the specified parameters.
        /// </summary>
        /// <param name="ConnectionString"></param>
        public static Dictionary<string, object> Settings(string ConnectionString)
        {
            var container = new BlobContainerClient(ConnectionString, "knn-htm-config");
            var blob = container.GetBlobClient("settings.json");
            using (var stream = blob.OpenReadAsync().Result)
            using (var sr = new StreamReader(stream))
            using (var jsonStream = new JsonTextReader(sr))
            {
                return JsonSerializer.CreateDefault().Deserialize<Dictionary<string, object>>(jsonStream);
            }
        }
    }
}