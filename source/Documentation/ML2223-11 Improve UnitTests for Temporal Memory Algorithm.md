# Improve UnitTests for Temporal Memory Algorithm
## Introduction
We describe the approach to evaluate the performance of given Temporal Memory algorithm. The project's objective was to test the algorithm's ability to learn and predict spatio-temporal patterns. To achieve this, we performed a series of experiments using unit tests, which allowed us to systematically evaluate the algorithm's behavior under different conditions. All the Unit Tests which have been build on existing algorithm can be found [_here_](https://github.com/Mostainahmed/variable-i/blob/f0e1d25aa6bead46ca2cc20be7fa2dcec9131b5a/source/UnitTestsProject/TemporalMemoryTest2.cs)

## Brief Sumary
Here the summary of the improvement of UNIT TESTS on TemporalMemory Algorithm done so far in this project:

1. Creation, removal, and update of synapses in distal segments
2. Growth of new dendrite segments when no or multiple matching segments are found
3. Activation of cells in columns and detection/handling of duplicate active columns
4. Learning and recalling patterns of sequences with different sparsity rates
5. Initialization of TemporalMemory with custom parameters (number of cells per column, number of column dimensions)
6. Adaptation of segments and increase of permanence of active synapses
7. Limitation of the number of active cells per column
8. Retrieval of winner cells from TemporalMemory Compute
9. Least used cell selection
10. Using different parameters for existing unit tests to re-enforce the testing.
11. Fix one issue with a method GetLeastUsedCell in TemporalMemory and ran UnitTest on it.
12. Fix some typo on Documentation of TemporalMemoryTest and TemporalMemory file

## Architecture
Here is the general architecture of this project,

![diagram](https://user-images.githubusercontent.com/62109347/228382136-78503f04-137b-42f3-b39d-7de57d69714c.jpg)

## Run the project

In order to run the project, [repository](https://github.com/Mostainahmed/variable-i.git) can be cloned in the local system. After that, go to the source folder 

![1](https://user-images.githubusercontent.com/62109347/228389657-60034ea0-920d-4af3-a018-f923e8b010fd.png)

Open neocortexapi.sln with visual studio.

![2](https://user-images.githubusercontent.com/62109347/228389720-3dfbf325-19ff-45bf-a656-33f50c8db1a3.png)

Then in solution explorer go to unit test project and open TemporalMemoryTest2.cs

![3](https://user-images.githubusercontent.com/62109347/228389950-6173897f-c85d-44d1-b0bc-b0baeb57065c.png)

Now click on the Test tab on the toolbar and click on the Test Explorer in order to open the Test Explorer section

![4](https://user-images.githubusercontent.com/62109347/228390622-0f73cbed-c028-4593-8e90-5fbef6bea924.png)

Now a tab like this should be seen in the visual studio

![5](https://user-images.githubusercontent.com/62109347/228391222-4792f9a4-16a0-4cb6-bf97-2aafc4816dc8.png)

Then in Test Explorer click on the marked "Section 1" which will open a filter and make sure that the marked "Section 2" is checked.

![6](https://user-images.githubusercontent.com/62109347/228391675-6b7f408f-d0b2-49e1-9e37-160a3a912a2a.png)

Now scroll down in the Test Explorer and find the _TemporalMemoryTest2_ file and expand it until a series view

![7](https://user-images.githubusercontent.com/62109347/228391983-b25c353d-b1fa-417b-bdd5-f9fafeeb27e4.png)

Right click on the TemporalMemoryTest2 file and click run as shown in below figure "Section 1". Now the test cases will start to execute and the final result can be seen in marked "Section 2". The result section consists of total run time, total number of passed test cases, number of skipped test cases and number of failed test cases.

![8](https://user-images.githubusercontent.com/62109347/228392574-10b2e7cd-e328-412b-91bc-8306911fa7fb.png)

In addition, all the successfull Unit Tests are in below at a glance,

![Screenshot (92)](https://user-images.githubusercontent.com/62109347/228382345-b024e782-129e-44ef-80ca-14aef99b086a.png)
