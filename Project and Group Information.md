Team: Prediction Warriors
Members:
	- Mandar Patkar
	- Amar Singh

Project Name: ML21/22 - 24 - Investigate Label Prediction from the time-series sequence (Power Consumption)

Description:

In many industrial scenarios, there is a time-series sequence of values. For example, the following sequence shows the number of taxi pick-ups at a specific time.
```
01-01-2021 00:00:00,19276
02-01-2021 00:00:00,24435
03-01-2021 00:00:00,19852
04-01-2021 00:00:00,29341
05-01-2021 00:00:00,30299
06-01-2021 01:00:00,31589
07-01-2021 02:00:00,31897
08-01-2021 03:00:00,32207
09-01-2021 04:00:00,27455
```

Your task is to implement the sequence learning and prediction code (see [MutlisequenceLearning example](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) ) that learns the sequence.
The sequence in HTM is learned (simplified) by using the following statement:

```
htmClassifier.Learn(label, [t, day of week])
```

Your first task is to implement the code that learns sequence. Think about the right approach. For example, the label can be the number of passengers. The second argument of the Learn function (t, day of week) is the encoded time-series value. You can encode the date and time as a combined encoding of two scalar values:
000date0000time00
In the left part of the code you can encode the date and in the second part-time. You can also reduce the time precision to 30 or 60 minutes. The following example demonstrates 30 Min. segmentation. Assuming that every hour consists of two segments:
```
Date        time          date   segment
----------------------------------------
08-01-2021 00:20:00 => 08-01-2021 00
08-01-2021 00:45:00 => 08-01-2021 01
08-01-2021 01:15:00 => 08-01-2021 02
...
08-01-2021 23:59:00 => 08-01-2021 47
```

You should think about how to do that and propose one or more solutions.
The second task is to use the learned model for prediction. The user enters the time-segment (08-01-2021 01) and the classifier is used to predict the number of passengers.
```
var predictedValues = cls.GetPredictedInputValues(encoded time-segment);
```
You should analyse the result and calculate the prediction acurray.

For more information about sequence learning please see: [Editing neocortexapi/htm-classifier.md at master · ddobric/neocortexapi](https://github.com/ddobric/neocortexapi/edit/master/source/Documentation/htm-classifier.md) . Please also provide or commit improvements to this document.
For more information please see also following thesis: [UniversityOfAppliedSciencesFrankfurt/thesis-LSTM0-vs-HTM-Yash-Vyas: Comparison of performance LSTM vs HTM](https://github.com/UniversityOfAppliedSciencesFrankfurt/thesis-LSTM0-vs-HTM-Yash-Vyas) . (Only students who chose this project will get access to the thesis).

The taxi passenger count is just an example. One dataset example can be used by max. Two students working together in the group. You can find another interesting example.

The full data sets (examples):
1.	TLC Trip Record Data - TLC (nyc.gov): [Dataset](https://www1.nyc.gov/site/tlc/about/tlc-trip-record-data.page)

2.	Power Consumption dataset in UnitTests used by PowerConsumptionExperiment: [Dataset](https://github.com/numenta/nupic/blob/master/src/nupic/datafiles/extra/hotgym/rec-center-hourly.csv)
