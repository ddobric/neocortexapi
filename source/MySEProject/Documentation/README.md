# Project Title: ML22/23-15 Approve Prediction of Multisequence Learning.

#### Group Name: Team_MSL

Group Members - 

1.Poonam Paraskar
2.Pratik Desai
3.Ankita Talande

### Link to Project: [Team_MSL_Project_Link](https://github.com/pparaska/neocortexapi_Team_MSL/tree/Team_MSL)

**Summary of Project:**
=======================================

Implemented a new method RunPredictionMultiSequenceExperiment, that improves the existing RunMultiSequenceLearningExperiment. 
The new method should automatically read learning sequences from a file and learn them. After learning is completed,
the sample should read testing subsequences from another file and calculate the prediction accuracy.
Project's aim - To calculate the accuracy of matched sequences and write the accuracy into CSV file.

In Program.cs file team has implemented new methods as mentioned below:
1. _RunPredictionMultiSequenceExperiment()_ - This method automatically reads the training sequences from a file and learning them. 
											https://github.com/pparaska/neocortexapi_Team_MSL/blob/Team_MSL/source/Samples/NeoCortexApiSample/Program.cs
2. _GetInputFromExcelFile()_ - To avoid user intervention, we are taking input samples from external Excel file, before this method team has also tried to implement 
[GetInputFromTextFile(),](https://github.com/ddobric/neocortexapi/commit/584195c394160724a467aa48c2c42d7a1feddcd6) 
[GetInputFromCSVFile()](https://github.com/pparaska/neocortexapi_Team_MSL/commit/589e1feedcc7ea0db28973ee0ddb11585483c744)
	Link for modified – [Program.cs](https://github.com/pparaska/neocortexapi_Team_MSL/blob/Team_MSL/source/Samples/NeoCortexApiSample/Program.cs), [MultisequenceLearning.cs](https://github.com/pparaska/neocortexapi_Team_MSL/blob/Team_MSL/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs)
							 
3. _GetSubSequencesInputFromExcelFile()_ - This method has implemented to avoid user intervention for subSequnces/test data inputs.
4. In Program.cs file for _PredictNextElement()_, method team has implemented below changes:
	1. In the method argument instead of passing double[], we have changed it to List<double> elements and calling this _PredictNextElement()_ method inside of for loop of _RunPredictionMultiSequenceExperiment()_, method where we can pass multiple test sequences/subsequences, and for each subSequnces/test sequence, _PredictNextElement()_ method will execute.
	2. In _PredictNextElement()_, method accuracy calculation logic is added. Accuracy is getting calculated by increasing the matches (which is counter), this counter is getting incremented by comparing the next appearing value with the last predicted value. These matches are getting divided by total number of predictions. 
	3. We are exporting this accuracy result in external csv file _Final Accuracy.csv_ for each run and result is getting appended at new line.
	
	```csharp
			if (nextItem == double.Parse(tokens2.Last())){
               {
                countOfMatches++;
               }
            }
            else
            {
				Debug.WriteLine("Nothing predicted :(");
            }
                totalPredictions++;
            }
            double accuracy = (double)countOfMatches / totalPredictions * 100;
            Debug.WriteLine($"Final Accuracy: {accuracy}%");
            Debug.WriteLine(string.Format("The test data list: ({0}).", string.Join(", ", list)));
            string filePath = Path.Combine(Environment.CurrentDirectory, "Final Accuracy.csv");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Final Accuracy is ,{accuracy}%");
            }
	```
5. In [MultisequenceLearning.cs](https://github.com/pparaska/neocortexapi_Team_MSL/blob/Team_MSL/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) encoder setting is updated to the range of min-max value 0-99, for the same input validation has been introduced in [Program.cs](https://github.com/pparaska/neocortexapi_Team_MSL/blob/Team_MSL/source/Samples/NeoCortexApiSample/Program.cs),  file.

```csharp
			if (reader.GetDouble(i) >= MinVal && reader.GetDouble(i) <= MaxVal)
                            {
                                TestSubSequences.Add(reader.GetDouble(i));
                            }
```

