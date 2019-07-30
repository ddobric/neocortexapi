
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkkaSb.Net
{
    public class FileStoragePersistenceProvider : IPersistenceProvider
    {
        private string actorSystemId;

        private string folderName;

        private ILogger logger;


        public async Task InitializeAsync(string actorSystemId, Dictionary<string, object> settings, bool purgeOnStart = false, ILogger logger = null)
        {
            this.actorSystemId = actorSystemId;

            this.logger = logger;

            this.folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"actsys{actorSystemId}");

            if (!Directory.Exists(this.folderName))
            {
                Directory.CreateDirectory(this.folderName);
                this.logger?.LogTrace("Created folder: {0}", folderName);
            }
            else
            {
                if (purgeOnStart == true)
                {
                    await Purge();
                }
                else
                {
                    this.logger?.LogTrace($"Folder already exist. No purge requested. {this.folderName}");
                }
            }
        }

        public async Task<ActorBase> LoadActor(ActorId actorId)
        {
            this.logger?.LogTrace("Loading actor: {0}", actorId);

            var actorInstance = await GetPersistedActorAsync(actorId);

            if (actorInstance == null)
                this.logger?.LogTrace("Actor: {0} was not found in persistence store.", actorId);
            else
                this.logger?.LogTrace("Actor: {0} loaded from persistence store.", actorId);

            return actorInstance;
        }


        public async Task PersistActor(ActorBase actorInstance)
        {
            this.logger?.LogTrace("Persisting actor: {0}", actorInstance.Id);

            var serializedActor = SerializeActor(actorInstance);

            await SaveActorToFile(actorInstance.Id, serializedActor);

            this.logger?.LogTrace("Persisting actor: {0}", actorInstance.Id);
        }


        public async Task Purge()
        {
            this.logger?.LogTrace("Purge started");

            await Task.Run(() =>
            {
                Directory.Delete(this.folderName);
            });

            this.logger?.LogTrace("Purge completed");
        }

        #region Private Methods

        private async Task<ActorBase> GetPersistedActorAsync(string actorId)
        {
            string blobName = getBlobNameFromId(actorId);

            var fName = Path.Combine(this.folderName, getBlobNameFromId(actorId));
            if (File.Exists(fName))
            {
                using (StreamReader sr = new StreamReader(Path.Combine(this.folderName, getBlobNameFromId(actorId))))
                {
                    var json = await sr.ReadToEndAsync();
                    return DeserializeActor<ActorBase>(json);
                }
            }
            else
                return null;
        }

        private async Task SaveActorToFile(string actorId, string serializedActor)
        {
            if (serializedActor == null)
            {
                throw new ArgumentNullException("Entity cannot be null!");
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(this.folderName, getBlobNameFromId(actorId))))
                {
                    await sw.WriteAsync(serializedActor);
                }
            }
            catch (Exception ex)
            {
                {
                    this.logger?.LogWarning(ex.Message, $"Failed to create the blob for actorId {actorId}. Retry started...");
                    Thread.Sleep(1000);
                }
            }
        }

        private static string getBlobNameFromId(string actorId)
        {
            return $"{actorId}.txt";
        }


        internal static string SerializeActor(ActorBase actorInstance)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.TypeNameHandling = TypeNameHandling.All;

            var strObj = JsonConvert.SerializeObject(actorInstance, Formatting.Indented, sett);

            return strObj;
        }

        internal static T DeserializeActor<T>(string serializedActor)
        {
            JsonSerializerSettings sett = new JsonSerializerSettings();

            sett.TypeNameHandling = TypeNameHandling.All;

            var strObj = JsonConvert.DeserializeObject<T>(serializedActor, sett);

            return strObj;
        }


        #endregion
    }
}
