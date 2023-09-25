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
| Container Instance | ```variable-i-project-container``` | --- |
| Storage account | ```ccprojectsd``` | --- |
| Queue storage | ```variableiqueue``` | Queue which containes trigger message |
| Blob container | ```variablei-result-files``` | Container used to store results|
| Table storage | ```variableitable``` | Container used to store learning accuracy logs |

The experiment Docker image can be pulled from the Azure Container Registry using the instructions below.
~~~
docker login variablei.azurecr.io -u variablei -p ALNnME150vKMZcGdmRnMlvVQaWwssz1YUHognRLKFT+ACRCNaFL/
~~~
~~~
docker pull variablei.azurecr.io/variablei-cc-project:latest
~~~

## How to run the experiment
## Step1 : Message input from azure portal
at a message to queues inside Azure storage account.
p.s Encode the message body in Base64

**How to add message :** 

Azure portal > Home > ccprojectsd | Queues > variableiqueue> Add message
![Screenshot (108)](https://github.com/Mostainahmed/variable-i/assets/74201172/aa4f44c5-7e9f-4214-8d38-13cf168d5fe0)

**Messages added to queue :**

![Screenshot (104)](https://github.com/Mostainahmed/variable-i/assets/74201172/4a13b7c4-1415-4a10-a7fc-782f8cbef151)

### Queue Message that will trigger the experiment:
~~~json
{
  "ExperimentId": "1",
  "InputFile": "runccproject",
  "Description": "Cloud Computing Implementation",
  "ProjectName": "ML22/23-11. Implement UnitTests on Temporal Memory Algorithm",
  "GroupName": "variabl-i",
  "Students": [ "Syed Mostain Ahmed", "Farjana Akter", "Shamsir Doha" ]
}
~~~
Go to "variable-i-container," "Containers," and "logs" to make sure the experiment is being run from a container instance.

![Screenshot (105)](https://github.com/Mostainahmed/variable-i/assets/74201172/e81b0eca-b1a6-45cc-83e7-acb5d1bb19a3)

when the experiment  is successful bellow message(Experiment complete successfully) will be shown. Experiment successfully

![Screenshot (109)](https://github.com/Mostainahmed/variable-i/assets/74201172/65bd578b-0c17-4946-b83d-8bb194a0f81d)

## Step2: Describe the Experiment Result Output Container

after the experiments are completed, the result file is stored in Azure storage blob containers 

![output](https://github.com/Mostainahmed/variable-i/assets/74201172/c2c56f47-2d7b-44e1-b3e0-d0be3e1023bf)

the result data are also subsequently uploaded into a database table named "variable-i-table"

![ssssssssssssssssssssssss](https://github.com/Mostainahmed/variable-i/assets/74201172/a644dfae-af81-4e9b-ad37-287eb631755b)

