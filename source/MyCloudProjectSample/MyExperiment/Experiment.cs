using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCloudProject.Common;
using MyExperiment.SEProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Random rnd = new Random();
            int rowKeyNumber = rnd.Next(0, 1000);
            string rowKey = "variable-i-" + rowKeyNumber.ToString();

            ExperimentResult res = new ExperimentResult(this.config.GroupId, rowKey);

            res.StartTimeUtc = DateTime.UtcNow;
            res.ExperimentId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            res.RowKey = rowKey;
            res.PartitionKey = "cc-proj-" + rowKey;

            if (inputFile == "runccproject")
            {
                res.TestName = "Temporal Memory Algorithm tests";
                List<TestInfo> testResults = RunTests();

                float totalTests = testResults.Count;
                float passingTests = testResults.Count(info => info.IsPassing);
                float accuracy = (passingTests / totalTests) * 100;

                // Serialize the test results to JSON
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(testResults, Newtonsoft.Json.Formatting.Indented);
                res.Description = json;
                this.logger?.LogInformation($"The file result we got {json}");
                res.TestData = string.IsNullOrEmpty(json) ? null : Encoding.UTF8.GetBytes(json);
                res.Accuracy = accuracy;
            }
            res.EndTimeUtc = DateTime.UtcNow;

            this.logger?.LogInformation("The process successfully completed");
            return Task.FromResult<ExperimentResult>(res);
        }



        /// <inheritdoc/>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {

            //ExperimentResult res = new ExperimentResult("damir", "123");
            //{
            //    //Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),

            //    Accuracy = (float)0.5,
            //};

            //await storageProvider.UploadExperimentResult(res);


            QueueClient queueClient = new QueueClient(this.config.StorageConnectionString, this.config.Queue);

            //
            // Implements the step 3 in the architecture picture.
            while (cancelToken.IsCancellationRequested == false)
            {
                QueueMessage message = await queueClient.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {
                        string msgTxt = Encoding.UTF8.GetString(message.Body.ToArray());

                        this.logger?.LogInformation($"Received the message {msgTxt}");

                        // The message in the step 3 on architecture picture.
                        ExerimentRequestMessage request = JsonSerializer.Deserialize<ExerimentRequestMessage>(msgTxt);

                        // Step 4.
                        //var inputFile = await this.storageProvider.DownloadInputFile(request.InputFile);
                        var inputFile = request.InputFile;

                        // Here is your SE Project code started.(Between steps 4 and 5).
                        ExperimentResult result = await this.Run(inputFile);

                        // Step 4 (oposite direction)
                        //TODO. do serialization of the result.
                        //await storageProvider.UploadResultFile("outputfile.txt", null);

                        // Step 5.
                        this.logger?.LogInformation($"{DateTime.Now} -  UploadExperimentResultFile...");
                        await storageProvider.UploadResultFile($"Test_data_{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}.txt", result.TestData);


                        this.logger?.LogInformation($"{DateTime.Now} -  UploadExperimentResult...");
                        await storageProvider.UploadExperimentResult(result);
                        this.logger?.LogInformation($"{DateTime.Now} -  Experiment Completed Successfully...");

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


        public List<TestInfo> RunTests()
        {
            TemporalMemoryTest2 temporalMemoryTests = new TemporalMemoryTest2();
            List<TestInfo> testResults = new List<TestInfo>();

            // Run each test and accumulate the results
            testResults.Add(temporalMemoryTests.TestNewSegmentGrowthWhenMultipleMatchingSegmentsFound());
            testResults.Add(temporalMemoryTests.TestSynapsePermanenceUpdateWhenMatchingSegmentsFound());
            testResults.Add(temporalMemoryTests.TestColumnDimensions());
            testResults.Add(temporalMemoryTests.TestRecycleLeastRecentlyActiveSegmentToMakeRoomForNewSegment(new int[] { 0, 1, 2, 3 }, new int[] { 4, 5, 6, 7 }, new int[] { 8, 9, 10, 11 }, new int[] { 12 }));
            testResults.Add(temporalMemoryTests.TestNewSegmentAddSynapsesToAllWinnerCells(1, 3));
            testResults.Add(temporalMemoryTests.TestDestroyWeakSynapseOnWrongPrediction(0.017));
            testResults.Add(temporalMemoryTests.TestAddSegmentToCellWithFewestSegments(1, 4));
            testResults.Add(temporalMemoryTests.TestAdaptSegmentToMax(0.9, 1.0));
            testResults.Add(temporalMemoryTests.TestDestroySegmentsWithTooFewSynapsesToBeMatching(0, 1, 2, 2, 0.015, 0.3, 0.009, 0.009, 1));
            testResults.Add(temporalMemoryTests.TestPunishMatchingSegmentsInInactiveColumns(0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.48, 0.48, 0.48, 0.48, 0.48, 0.5, 0.5));
            testResults.Add(temporalMemoryTests.TestHighSparsitySequenceLearningAndRecall());
            testResults.Add(temporalMemoryTests.TestLowSparsitySequenceLearningAndRecall());
            testResults.Add(temporalMemoryTests.TestCreateSynapseInDistalSegment());
            testResults.Add(temporalMemoryTests.TestNewSegmentGrowthWhenNoMatchingSegmentFound());
            testResults.Add(temporalMemoryTests.TestNoOverlapInActiveCells());
            testResults.Add(temporalMemoryTests.TestActiveSegmentGrowSynapsesAccordingToPotentialOverlap(new int[] { 0, 1, 2, 3 }, new int[] { 5 }, new int[] { 0, 1, 2, 3 }, 4));
            testResults.Add(temporalMemoryTests.TestDestroyWeakSynapseOnActiveReinforce(2, 0, 4));
            testResults.Add(temporalMemoryTests.TestAdaptSegment_IncreasePermanence());
            testResults.Add(temporalMemoryTests.TestAdaptSegment_PrevActiveCellsContainPresynapticCell_IncreasePermanence());

            return testResults;
        }
        #region Private Methods


        #endregion
    }
}
