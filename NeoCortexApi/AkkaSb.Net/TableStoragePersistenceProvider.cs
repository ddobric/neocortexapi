using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkkaSb.Net
{
    public class TableStoragePersistenceProvider : IPersistenceProvider
    {
        const string cConnStr = "storageConnectionString";

        private string actorSystemId;

        private CloudTableClient tableClient;

        private ILogger logger;

        public async Task Initialize(string actorSystemId, Dictionary<string, object> settings, ILogger logger)
        {
            this.actorSystemId = actorSystemId;

            this.logger = logger;

            if (!settings.ContainsKey(cConnStr))
                throw new ArgumentException($"'{cConnStr}' argument must be contained in settings.");

            CloudStorageAccount storageAccount = CreateFromConnectionString(settings[cConnStr] as string);

            // Create a table client for interacting with the table service
            tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            string tableName = $"ActorSystem_{actorSystemId}";

            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
               this.logger?.LogTrace("Created Table : {0}", tableName);
            }
            else
            {
                this.logger?.LogTrace("Created Table : {0}", tableName);
            }
        }

        public Task<ActorBase> DeserializeActor(ActorId actorId)
        {
            this.logger?.LogTrace("Deserializing actor: {0}", actorId);

            this.logger?.LogTrace("Deserializing actor: {0}", actorId);
        }


        public Task SerializeActor(ActorBase actorInstance)
        {
            this.logger?.LogTrace("Serializing actor: {0}", actorInstance.Id);

            this.logger?.LogTrace("Serializing actor: {0}", actorInstance.Id);
        }

        #region Private Methods
        public static CloudStorageAccount CreateFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
        #endregion
    }
}
