# ML 23/24  Approve Prediction of Multisequence Learning 

## Introduction Part

In this project, I have  introduced novel enhancements to the MultisequenceLearning algorithm. These enhancements involve automating the process of dataset retrieval from a specified path using the function HelpMethod.ReadDataset(datasetPath). Additionally, I  possess test data located in a separate file, which also needs to be read for subsequent evaluation of subsequences, formatted similarly to HelpMethod.ReadDataset(testsetPath). The function RunMultiSequenceLearningExperiment(sequences, sequencesTest) is employed to execute the learning experiment, utilizing multiple sequences provided in sequences along with test subsequences from sequencesTest. Following the completion of the learning phase, the accuracy of predicted elements is calculated for evaluation.


Above the flow of implementation of our project.

`Sequence` is the model of how we process and store the dataset. And can be seen below:

 csharp code
public class Sequence
{
        public String name { get; set; }
        public object Name { get; internal set; }
        public int[] data { get; set; }
        public int[] Data { get; internal set; }
        public object Value { get; internal set; }
        public object Key { get; internal set; }
}


eg:
- Dataset

json part
[
  {
    "name": "S1",
    "data": [ 0, 2, 5, 6, 7, 8, 10, 11, 13 ]
  },
  {
    "name": "S2",
    "data": [ 1, 2, 3, 4, 6, 11, 12, 13, 14 ]
  },
  {
    "name": "S3",
    "data": [ 1, 2, 3, 4, 7, 8, 10, 12, 14 ]
  }
]


