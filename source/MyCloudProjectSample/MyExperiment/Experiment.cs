using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using MyExperiment.Models;
using MyExperiment.Utilities;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace MyExperiment
{
    /// <summary>
    /// This class implements the ML experiment that will run in the cloud. This is refactored code from my SE project.
    /// </summary>
    public class Experiment : IExperiment
    {
        private IStorageProvider storageProvider;

        private ILogger logger;

        private MyConfig config;

        private string expectedProjectName;
        /// <summary>
        /// construct the class
        /// </summary>
        /// <param name="configSection"></param>
        /// <param name="storageProvider"></param>
        /// <param name="expectedPrjName"></param>
        /// <param name="log"></param>
        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, string expectedPrjName, ILogger log)
        {
            this.storageProvider = storageProvider;
            this.logger = log;
            this.expectedProjectName = expectedPrjName;
            config = new MyConfig();
            configSection.Bind(config);
        }

        /// <summary>
        /// Run Software Engineering project method
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <returns>experiment result object</returns>
        public Task<ExperimentResult> Run(string inputFile)
        {
            var inputData =
                Newtonsoft.Json.JsonConvert.DeserializeObject<InputModel>(FileUtilities.ReadFile(inputFile));

            this.logger?.LogInformation("Starting of software engineering code");
            ExperimentResult res = new ExperimentResult(this.config.GroupId, Guid.NewGuid().ToString());
            res.OutputFiles = new string[2];
            res.StartTimeUtc = DateTime.UtcNow;
            this.logger?.LogInformation("Running software engineering code");
            res.OutputFiles[0] = RunSoftwareEngineeringCode(inputData);
            res.EndTimeUtc = DateTime.UtcNow;
            this.logger?.LogInformation("Finished execution of software engineering code");

            return Task.FromResult(res);
        }



        /// <inheritdoc/>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {
            ExperimentResult res = new ExperimentResult("damir", "123")
            {
                //Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                
                Accuracy = (float)0.5,
            };

            await storageProvider.UploadExperimentResult(res);


            QueueClient queueClient = new QueueClient(this.config.StorageConnectionString, this.config.Queue);

            
            while (cancelToken.IsCancellationRequested == false)
            {
                QueueMessage message = await queueClient.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {

                        string msgTxt = Encoding.UTF8.GetString(message.Body.ToArray());

                        this.logger?.LogInformation($"Received the message {msgTxt}");

                        ExerimentRequestMessage request = JsonSerializer.Deserialize<ExerimentRequestMessage>(msgTxt);

                        var inputFile = await this.storageProvider.DownloadInputFile(request.InputFile);

                        IExperimentResult result = await this.Run(inputFile);

                        //TODO. do serialization of the result.
                        await storageProvider.UploadResultFile("outputfile.txt", null);

                        await storageProvider.UploadExperimentResult(result);

                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                    catch (Exception ex)
                    {
                        this.logger?.LogError(ex, "TODO...");
                    }
                }
                else
                {
                    await Task.Delay(500);
                    logger?.LogTrace("Queue empty...");
                }
            }

            this.logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }

        Task<IExperimentResult> IExperiment.Run(string inputFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method to run software engineering code
        /// </summary>
        /// <param name="inputs">Inputs from azure blob storage</param>
        /// <returns></returns>
        private string RunSoftwareEngineeringCode(InputModel input)
        {
            var temporalMemory = new TemporalMemory();
            var filename = $"output-{Guid.NewGuid()}.txt";
            //var predictionResult = temporalMemory.PunishPredictedColumn();
            //var outputModel = new OutputModel(predictionResult);
            //var outputAsByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(outputModel));
            //FileUtilities.WriteDataInFile(Path.Combine(FileUtilities.GetLocalStorageFilePath(config.LocalPath), filename), predictionResult, input.NextValueParameter);
            return filename;
        }
        #region Private Methods


        #endregion
    }
}
