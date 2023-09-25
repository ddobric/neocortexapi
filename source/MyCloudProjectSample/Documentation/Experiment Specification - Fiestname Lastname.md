# ML22/23-11 Improve UnitTests for Temporal Memory Algorithm - Azure Cloud Implementation
## Introduction
Temporal memory algorithms have gained popularity as a promising approach for modeling temporal sequences in machine learning. This project aims to improve the unit test for the given temporal memory algorithm, which is based on the principles of the cortical column and the neocortex. The algorithm uses a sparse distributed representation of data and incorporates temporal context to predict future values in a sequence. We implemented improvements to the existing unit test, including the addition of more test cases with varying complexity and the implementation of cross-validation techniques for better evaluation of the algorithm's performance. We also optimized the implementation of the algorithm for improved efficiency and scalability. Moreover, running this project on Azure Cloud, we explored and got to use very useful features of Azure Cloud services.
## Recap (Software Engineering Project)
If you need to obtain a copy of our project on your own system, use these links in order to carry out development and testing. Look at the notes on how to deploy the project and experiment with it on a live system. These are the relevant links:

- Project Documentation: [Documentation](https://github.com/Mostainahmed/variable-i/blob/master/source/MySEProject/Documentation/Improve%20UnitTests%20for%20Temporal%20Memory%20Algorithm.pdf) 

- Unit Test Cases: [here](https://github.com/Mostainahmed/variable-i/blob/master/source/UnitTestsProject/TemporalMemoryTest2.cs)

## What is this experiment about

 
## Information about our Azure accounts and their components

|  |  |  |
| --- | --- | --- |
| Resource Group | ```CCProjectR``` | --- |
| Container Registry | ```variablei``` | --- |
| Container Registry server | ```variablei.azurecr.io``` | --- |
| Container Instance | ```adecloudprojectcontainer``` | --- |
| Storage account | ```ccprojectsd``` | --- |
| Queue storage | ```variableiqueue``` | Queue which containes trigger message |
| Blob container | ```variablei-result-files``` | Container used to store results|
| Table storage | ```variableitable``` | Container used to store learning accuracy logs |

## How to run the experiment
