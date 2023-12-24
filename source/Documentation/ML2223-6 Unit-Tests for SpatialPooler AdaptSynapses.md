# Project Title : ML 22/23-6 Implement Unit-Tests for the SpatialPooler AdaptSynapses Method
--------------------------------------------------------------------------------------------

# Introduction : 

This project involves implementing unit tests for the SpatialPooler AdaptSynapses method, which is a critical component for performance imporvement of the Spatial Pooler algorithm used in machine learning. Implementing unit tests for this method is necessary to ensure that it functions correctly and meets the desired specifications and development of the code can be found in the [neocortexapi](https://github.com/ddobric/neocortexapi) repository. This project aims to implement unit tests for the for the SpatialPooler AdaptSynapses method. The task is targeting to achieve 100% code coverage for the AdaptSynapses method, ensuring complete testing of the method's functionality. The project's findings also demonstrated accordingly. 


# Getting Started : 

For development and testing reasons, follow these procedures to get a copy of the project up and running on your own system. Look at the notes on
how to deploy the project and experiment it out on a live system. Here is the necessary links:

- Project Solution File [NeoCortexApi](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Team_UnitTestBD/Source/MyProject/UnitTestProject/NeoCortexApi.All.sln)

- Project Documentation [Documentation](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/Team_UnitTestBD/Source/MyProject/Documentation)

- Final Project [UnitTestProject](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/Team_UnitTestBD/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs)

- Unit Test Cases [here](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L2768-L3274)



**Project Description**
========================

Implementing unit tests in software development is significant because it helps ensure that individual components of the software function correctly and reliably. However, unit testing is often considered to be an arduous task. This study employs the implementation of unit tests for the AdaptSynapses method in the Spatial Pooler (SP) algorithm used in the Hierarchical Temporal Memory (HTM) architecture, which is a computational framework inspired by the structure and function of the neocortex. To ensure the reliability and robustness of the method in different situations, we created a diverse range of test case scenarios to test the provided AdaptSynapses method with changing threshold values and Permanence values. During the task accomplishment, we gained valuable insights into how this method connects mini-columns and sensory input. As part of our efforts, we also analyzed many existing unit tests by introducing a range of inputs with varying complexity. Finally, with the visual studio feature, we did cross-validation of the test cases to evaluate the algorithm's performance. We observed all the test cases successfully covers the complete test AdaptSynapses method in SpatialPooler class. Hence all of them resulted in 100% code coverage. The study effectively motivated us to write unit tests that are more comprehensive and of higher quality.


**Project Objectives**
========================

Overall goal of our study is to test the ability of the SpatialPooler class into the AdaptSynapses method.

For this project the following objectives are accomplished:

-	Implement unit tests cases for AdaptSynapses method of SpatialPooler class inside the SpatialPoolerTests class.

-	While doing unit test, SpatialPooler should be initialized using HtmConfig instead of the old initialization.

-	Execute the test cases and identify findings if the cases Passed.

-   Analyze the Code Coverage of the implemented unit tests and achieve as maximum of 100% Code Coverage for the given AdaptSynapses method.


**Implementation**
==================

In this segment, we explain the summarized approaches of the involved methodology in implementing unit tests for the AdaptSynapses method in the SpatialPooler class. The details of the relevant functions and components are guided in the [Project Report](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/Team_UnitTestBD/Source/MyProject/Documentation). As We aimed to test the given algorithm's **AdaptSynapses** ability to learn and make predictions under various conditions by conducting sequence experiments using unit tests. We begin by outlining the experimental framework, followed by several unit tests, analyzing the code coverage tests and the final outcomes we obtained. 

1.Spatial Poller Initialization:
--------------------------------

The Spatial Pooler Initialization is the process of setting up the necessary configurations and parameters for the Spatial Pooler algorithm to start working. It involves defining the input space, number of columns, receptive field size, and the number of active columns that the algorithm should produce for a given input. This initialization process is crucial for the Spatial Pooler to learn and identify patterns in the input data stream. 

There are two ways of Initializing the Spatial Pooler's -
-	Using Parameters, which is obsolete and mostly compatible with Java implementations.
-	Using HtmConfig, which is a configuration object that contains all the required parameters for the algorithm to function effectively.

We have used HtmConfig in all our implemented uni tests. This updated initialization method is recommended over the old one for better performance and reliability of the algorithm.


2.Steps involved in Adapt Synapses:
-----------------------------------

The AdaptSynapses method adjusts the permanence values of synapses between input bits and columns to improve the accuracy and efficiency of the algorithm's learning and prediction capabilities. To understand how the method works, below high level steps are engaged in the process.

-   Initialization: A random value used to initialize the AdaptSynapses.
-   Input processing: The SpatialPooler receives input data and the process through synapses to produce a set of activation values for every neuron within the network.
-   Competition: All the activation values are the compared with a pre-defined threshold value. Higher activation values of the neurons are allowed to pass their  signals to the next layer of the network.
-   Adaptation: Next synapses among active neurons and input data are being updated based on Hebbian learning rule. There are different learning rules. Here synapses adjustment basically allows the network to response to specific inputs over time. Thus adaptation of specific patterns are trained to recognize.

All these steps are repeated while the is a new data input and thus the network continuously learns and adapts to new patterns and variations in the input data.

Here are the steps wise summary to understand the methodology.

Here are some basic steps involved in the process: 
-	Make an array actInputIndexes containing the indices of the active input bits in inputVector.
-	Build a new array permChanges with a size equal to the number of inputs in the HTM configuration. All values of permChanges are set to the negative value of SynPermInactiveDec.
-	Set the values at the indices in actInputIndexes to SynPermActiveInc.
-	Loop over each active column in the model.
-	Get the corresponding Column object using the GetColumn method of the Connections class.
-	Make an array actInputIndexes containing the indices of the active input bits in inputVector.
-	Build a new array permChanges with a size equal to the number of inputs in the HTM configuration. All values of permChanges are set to the negative value of SynPermInactiveDec.
-	Set the values at the indices in actInputIndexes to SynPermActiveInc.
-	Loop over each active column in the model:
-	Get the corresponding Column object using the GetColumn method of the Connections class.


3.Implementing Unit Tests:
--------------------------

While implementing the test cases below aspects of the unit tests were considered as well-designed Unit Tests being fast, repeatable, and highlighting discrepancies with precision. 
-	Setting the environment: First we implement unit test file and import the necessary classes and methods. Then, made instance of the Spatial Pooler class and set the parameters for the test, such as the input vector, active columns, and HTM configuration.
-	Call the AdaptSynapses method: We passed the above instance of the Spatial Pooler class to the AdaptSynapses method along with the required input parameters (i.e., conn, inputVector, and activeColumns).
-	Verification: Here we check the permanence values of the synapses in the active columns of the Spatial Pooler by accessing the Column and Pool objects using the GetColumn and GetPool methods. Then assert necessary changes to the synapse permanences.


One of the unit test TestAdaptSynapsesWithMinThreshold codes is shown in below. All unit-test codes are linked respectively in the result segment.

```csharp
        /// <summary>
        /// Adapt synapses method with mininimum threshold value
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        [TestCategory("Prod")]
        public void TestAdaptSynapsesWithMinThreshold()
        {
            //Initialization with HtmConfig
            var htmConfig = SetupHtmConfigDefaultParameters();
            htmConfig.InputDimensions = new int[] { 8 };
            htmConfig.ColumnDimensions = new int[] { 4 };
            htmConfig.SynPermInactiveDec = 0.01;
            htmConfig.SynPermActiveInc = 0.1;
            htmConfig.WrapAround = false;
            mem = new Connections(htmConfig);
            sp = new SpatialPooler();
            sp.Init(mem);

            //mininimum threshold value
            mem.HtmConfig.SynPermTrimThreshold = 0.01;

            int[][] potentialPools = new int[][] {
            new int[]{ 1, 1, 1, 1, 0, 0, 0, 0 },
            new int[]{ 1, 0, 0, 0, 1, 1, 0, 1 },
            new int[]{ 0, 0, 1, 0, 0, 0, 1, 0 },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, 0 }
            };

            //initialized permanences
            double[][] permanences = new double[][] {
            new double[]{ 0.200, 0.120, 0.090, 0.040, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.150, 0.000, 0.000, 0.000, 0.180, 0.120, 0.000, 0.450 },
            new double[]{ 0.000, 0.000, 0.014, 0.000, 0.000, 0.000, 0.110, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            double[][] truePermanences = new double[][] {
            new double[]{ 0.300, 0.110, 0.080, 0.140, 0.000, 0.000, 0.000, 0.000 },
            new double[]{ 0.250, 0.000, 0.000, 0.000, 0.280, 0.110, 0.000, 0.440 },
            new double[]{ 0.000, 0.000, 0.000, 0.000, 0.000, 0.000, 0.210, 0.000 },
            new double[]{ 0.040, 0.000, 0.000, 0.000, 0.000, 0.000, 0.178, 0.000 }
            };

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                int[] indexes = ArrayUtils.IndexWhere(potentialPools[i], (n) => (n == 1));
                mem.GetColumn(i).SetProximalConnectedSynapsesForTest(mem, indexes);
                mem.GetColumn(i).SetPermanences(mem.HtmConfig, permanences[i]);
            }

            int[] inputVector = new int[] { 1, 0, 0, 1, 1, 0, 1, 0 };
            int[] activeColumns = new int[] { 0, 1, 2 };

            //execute the AdaptSynapses method with parameters 
            sp.AdaptSynapses(mem, inputVector, activeColumns);

            for (int i = 0; i < mem.HtmConfig.NumColumns; i++)
            {
                double[] perms = mem.GetColumn(i).ProximalDendrite.RFPool.GetDensePermanences(mem.HtmConfig.NumInputs);
                for (int j = 0; j < truePermanences[i].Length; j++)
                {
                    Assert.IsTrue(Math.Abs(truePermanences[i][j] - perms[j]) <= 0.01);
                }
            }
        }
```


**Results**
===========

1.Implemented Unit Tests:
-------------------------

We have created 6 unit tests cases for the AdaptSynapses method of SpatialPooler class. Details of the unit tests code are linked to respective tests Methods.

1. [TestAdaptSynapsesWithMinThreshold](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L2768-L2833)
2. [TestAdaptSynapsesWithMaxThreshold](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L2835-L2899)
3. [TestAdaptSynapsesWithSinglePermanences](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L2901-L2955)
4. [TestAdaptSynapsesWithTwoPermanences](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L2957-L3015)
5. [TestAdaptSynapsesWithThreePermanences](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L3017-L3078)
6. [TestAdaptSynapsesWithFourPermanences](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L3080-L3144)
7. [TestAdaptSynapsesWithNoColumns](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L3146-L3209)
8. [TestAdaptSynapsesWithNoColumnsNoInputVector](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/c9399e7a1ae27e3d39e25ff748510ef0bfd11166/Source/MyProject/UnitTestProject/UnitTestsProject/SpatialPoolerTests.cs#L3211-L3274)

We run all the above listed unit tests in visual studio test explorer and all of them are Passed. See the reference screenshot-

![PassedUnitTests](https://user-images.githubusercontent.com/82439851/228396799-f727a8d0-c12c-4beb-bb68-4683b42ea1ab.PNG)


1.Code Coverage Analysis of the Unit Tests:
------------------------------------------

Following table summarized all unit tests code coverage results:


| Test Cases                                   | Covered (%Blocks) 	| Not-Covered (%Blocks)  | Remarks  |
|----------------------------------------------|--------------------|------------------------|----------|
| TestAdaptSynapsesWithMinThreshold	           |       100% 	       |       0%               |          |
| TestAdaptSynapsesWithMaxThreshold            |       100%         |       0%               |          |
| TestAdaptSynapsesWithSinglePermanences       |      	100%         |       0%               |          |
| TestAdaptSynapsesWithTwoPermanences          |       100%         |       0%               |          |
| TestAdaptSynapsesWithThreePermanences        |       100%         |       0%               |          |                
| TestAdaptSynapsesWithFourPermanences         |       100%         |       0%               |          |
| TestAdaptSynapsesWithNoColumns               |       59.26%       |       40.74%           |          |                
| TestAdaptSynapsesWithNoColumnsNoInputVector  |       69.76%       |       30.24            |          |

Code Coverage Results for the method TestAdaptSynapsesWithMinThreshold. Below snapshot is an example

![1](https://user-images.githubusercontent.com/74227867/228898820-ba764425-1f8c-4c98-a225-94ae7b94f89f.jpg)


**Conclusion**
==============

In addition to the findings mentioned above, the study also highlights the importance of unit testing in software development, particularly in the context of machine learning and synaptic learning algorithms. Unit testing not only ensures the reliability and robustness of the code but also helps in identifying and fixing any errors or bugs early in the development cycle. The study also emphasizes the significance of code coverage, as achieving high code coverage helps in identifying any gaps or untested scenarios in the code.

Moreover, the AdaptSynapses method's effectiveness in sparse or noisy input scenarios indicates its potential for application in a wide range of real-world problems, such as natural language processing and image recognition. As such, the study's results can be useful for researchers and practitioners in the field of machine learning and synaptic learning, providing them with valuable insights and information to improve their algorithms' performance and functionality.

To further enhance the algorithm's performance and functionality, future research could focus on implementing integration tests for the algorithm as a whole. Integration tests can help in identifying any issues or errors that arise when different components of the algorithm interact with each other. Additionally, analyzing and covering the remaining test cases can further improve the algorithm's reliability and robustness, ensuring its effectiveness in a wider range of scenarios. Overall, the study's findings have significant implications for software development, machine learning, and synaptic learning research, highlighting the importance of rigorous testing practices and the potential of the AdaptSynapses method for solving real-world problems.



**Similar Studies/Research used as References**
===============================================

[1]. NeoCortexAPI, C# Implementation of HTM : https://github.com/ddobric/neocortexapi

[2]. NeoCortexAPI, SpatialPooler and TemporalMemory documents : https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/SpatialPooler.md#spatial-pooler

[3]. HTM Spatial Pooler, Neocortical Algorithm for Online Sparse Distributed Coding : https://www.frontiersin.org/articles/10.3389/fncom.2017.00111/full
