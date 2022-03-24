# Project Title : Investigate Label Prediction from the time-series sequence
------------------------------------------------------------------------------

# Introduction : 
This project is an investigation of label prediction from the time series sequence and develops the code for multisequence learning and prediction in 
the [neocortexapi](https://github.com/ddobric/neocortexapi) repository. The project's findings also demonstrated that label prediction from a time series
data set utilizing multisequence learning is the best option for dealing with time prediction. 

# Getting Started : 
For development and testing reasons, follow these procedures to get a copy of the project up and running on your own system. Look at the notes on
how to deploy the project and experiment it out on a live system. Here is the necessary links:
- Project Solution File [NeoCortexApi](https://github.com/rabiul64/neocortexapi/blob/master/source/NeoCortexApi.sln)
- Final Project Codes [TimeSeriesSequence](https://github.com/rabiul64/neocortexapi/tree/master/source/MySEProject/TimeSeriesSequence)
- Project Documentation [Documents](https://github.com/rabiul64/neocortexapi/tree/master/source/MySEProject/Documentation)

**Project Description**
========================

1.Objective
-------------

Label prediction from time-series sequence has been widely applied to the finance industry in applications such as stock market price
and commodity price forecasting. There is a time-series sequence of values in many industrial scenarios, for example, a sequence that
shows the number of taxi pick-ups at a specific time. A popular problem is how to label time series data to measure the forecast accuracy 
of machine learning models and, as a result, ultimate investment returns. Existing time series labeling approaches mostly label data by 
comparing current data to data from a short period in the future. This paper represents an investigation of label prediction from the time 
series sequence and implements the code for multisequence learning and prediction to solve the above mentionrd problem. The results of the paper 
also proved that the label prediction from the time series data set using multisequence learning is the best choice for dealing with the prediction
of a specific time.

In the following experiment we introduce different types of sequence into the HTM 
    Experiment - Taxi Passenger Count Prediction.

2.Approach
-------------

For this approach, we choose to work with a CSV file containing (Green Taxi Trip Records) sample data from January-July 2021 [dataset of Taxi-TLC Trip Record Data] (https://www1.nyc.gov/site/tlc/about/tlc-trip-record-data.page) 
Then, from that data set, the number of passengers has counted for each 1 hour and record them to a csv file. There will be a part every 1 hours, as mentioned in the project description. 
As a result, the column name in the changed csv files become.

For example: We have data in sample data set like

lpep_pickup_datetime passenger_count
01-01-2021 00:12 5
01-01-2021 00:18 3
01-01-2021 00:28 7

We are considering every 60 mins one segment of a given date

```csharp
 public static List<Slot> GetSlots()
        {
            List<Slot> timeSlots = new List<Slot>
            {
               new Slot { Segment="00", StartTime=new TimeSpan(0,0,0), EndTime= new TimeSpan(0,59,59) },
               new Slot { Segment="01", StartTime=new TimeSpan(1,0,0), EndTime= new TimeSpan(1,59,59) },
               new Slot { Segment="02", StartTime=new TimeSpan(2,0,0), EndTime= new TimeSpan(2,59,59) },
               new Slot { Segment="03", StartTime=new TimeSpan(3,0,0), EndTime= new TimeSpan(3,59,59) },
               .....
			   .....
               new Slot { Segment="20", StartTime=new TimeSpan(20,0,0), EndTime= new TimeSpan(20,59,59) },
               new Slot { Segment="21", StartTime=new TimeSpan(21,0,0), EndTime= new TimeSpan(21,59,59) },
               new Slot { Segment="22", StartTime=new TimeSpan(22,0,0), EndTime= new TimeSpan(22,59,59) },
               new Slot { Segment="23", StartTime=new TimeSpan(23,0,0), EndTime= new TimeSpan(23,59,59) },
            };

            return timeSlots;
        }
```
Passing the lpep_pickup_datetime to get slots and accumated the passanger on that slot
```csharp
 private static Slot GetSlot(string pickupTime, List<Slot> timeSlots)
        {
            var time = TimeSpan.Parse(pickupTime);
            Slot slots = timeSlots.FirstOrDefault(x => x.EndTime >= time && x.StartTime <= time);

            return slots;
        }

 var accumulatedPassangerData = processedTaxiDatas.GroupBy(c => new
            {
                c.Date,
                c.Segment
            }).Select(
                        g => new
                        {
                            Date = g.First().Date,
                            TimeSpan = g.First().TimeSpan,
                            Segment = g.First().Segment,
                            Passsanger_Count = g.Sum(s => s.Passanger_count),
                        }).AsEnumerable()
                          .Cast<dynamic>();
```

We have made csv file something like this

Datetime, segment, number of passenger
01-01-2021 01:00 00 2000 --- all the passengers will be accumulated from 01-01-2021 00:00 to 01-01-2021 01:00
01-01-2021 02:00 01 2000
01-01-2021 03:00 02 3000
....
01-01-2021 05:00 05 4000
For learning time-series and predicting it we are using Multi-sequence Learning using HTM Classifier [example] (https://github.com/i-am-mandar/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) which is been forked form [NeoCortexApi](https://github.com/ddobric/neocortexapi)


The HTM Classifier consists of Spatial Pooler and Temporal Memory which takes in encoded data to learn the sequence.

I. Create Multiple Sequences of the time-series

II. Encode the datetime as segment

III. Learn using HTM Classifier

IV. Predict the label 
                        
3.Encoding and Learning
-------------

I.After reformatting the datetime, several sequences of the segments were formed, as seen below:

Sequence 1:
```
01/01/2021 00,145
01/01/2021 01,235
01/01/2021 02,300
01/01/2021 03,457
:      :      :
01/01/2021 21,490
01/01/2021 22,123
01/01/2021 23,246
```

Sequence 2:
```
01/02/2021 00,212
01/02/2021 01,164
01/02/2021 02,470
01/02/2021 03,420
:      :      :
01/02/2021 21,235
01/02/2021 22,475
01/02/2021 23,454
```

and so on


II. Encode the segment

Then read data from the modified CSV file and encode it. For encoding, these four encoders dayEncoder, monthEncoder, segmentEncoder, and dayofWeek 
have been used to train data. Please note that because the year is static, it is not taken into consideration during encoding. 
The encoder used is [Scalar Encoder] (https://github.com/rabiul64/neocortexapi/blob/master/source/NeoCortexApi/Encoders/ScalarEncoder.cs)
The following is the setup that was used:


```csharp
public static ScalarEncoder FetchDayEncoder()
        {
            ScalarEncoder dayEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 35},
                { "MinVal", (double)1}, // Min value = (0).
                { "MaxVal", (double)32}, // Max value = (32).
                { "Periodic", true},
                { "Name", "Date"},
                { "ClipInput", true},
           });

            return dayEncoder;
}
```

```csharp
 public static ScalarEncoder FetchMonthEncoder()
        {
            ScalarEncoder monthEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 15},
                { "MinVal", (double)1}, // Min value = (0).
                { "MaxVal", (double)13}, // Max value = (12).
                { "Periodic", true}, // Since Monday would repeat again.
                { "Name", "Month"},
                { "ClipInput", true},
            });
            return monthEncoder;
}
```

```csharp
 public static ScalarEncoder FetchSegmentEncoder()
        {
            ScalarEncoder segmentEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 27},
                { "MinVal", (double)0}, // Min value = (0).
                { "MaxVal", (double)24}, // Max value = (23).
                { "Periodic", true}, // Since Segment would repeat again.
                { "Name", "Segment"},
                { "ClipInput", true},
            });
            return segmentEncoder;
}
```

```csharp
public static ScalarEncoder FetchWeekDayEncoder()
        {
            ScalarEncoder weekOfDayEncoder = new ScalarEncoder(new Dictionary<string, object>()
            {
                { "W", 3},
                { "N", 11},
                { "MinVal", (double)0}, // Min value = (0).
                { "MaxVal", (double)7}, // Max value = (7).
                { "Periodic", true}, // Since Monday would repeat again.
                { "Name", "WeekDay"},
                { "ClipInput", true},
            });
            return weekOfDayEncoder;
}
```

III. Learn using HTM Classifier

The following HTM Config was used:

```csharp
public static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
{
    HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
    {
			// Temporal Memory parameters
            this.ColumnDimensions = columnDims;
            this.InputDimensions = inputDims;

            this.CellsPerColumn = 32;
            this.ActivationThreshold = 10;
            this.LearningRadius = 10;
            this.MinThreshold = 9;
            this.MaxNewSynapseCount = 20;
            this.MaxSynapsesPerSegment = 225;
            this.MaxSegmentsPerCell = 225;
            this.InitialPermanence = 0.21;
            this.ConnectedPermanence = 0.5;
            this.PermanenceIncrement = 0.10;
            this.PermanenceDecrement = 0.10;
            this.PredictedSegmentDecrement = 0.1;

            // Spatial Pooler parameters

            this.PotentialRadius = 15;
            this.PotentialPct = 0.75;
            this.GlobalInhibition = true;
            //this.InhibitionRadius = 15;
            this.LocalAreaDensity = -1.0;
            this.NumActiveColumnsPerInhArea = 0.02 * 2048;
            this.StimulusThreshold = 5.0;
            this.SynPermInactiveDec = 0.008;
            this.SynPermActiveInc = 0.05;
            this.SynPermConnected = 0.1;
            this.SynPermBelowStimulusInc = 0.01;
            this.SynPermTrimThreshold = 0.05;
            this.MinPctOverlapDutyCycles = 0.001;
            this.MinPctActiveDutyCycles = 0.001;
            this.DutyCyclePeriod = 1000;
            this.MaxBoost = 10.0;
            this.WrapAround = true;
            this.Random = new ThreadSafeRandom(42);
    };

    return cfg;
}
```

Algorithm for Mutli-sequence Learning
```
01. Get HTM Config and initialize memory of Connections 
02. Initialize HTM Classifier and Cortex Layer
03. Initialize HomeostaticPlasticityController
04. Initialize memory for Spatial Pooler and Temporal Memory
05. Add Spatial Pooler memory to Cortex Layer
	05.01 Compute the SDR of all encoded segment for Multi-sequences using Spatial Pooler
	05.02 Continue for maximum number of cycles
06. Add Temporal Memory to Cortex Layer
    06.01 Compute the SDR as Compute Cycle and get Active Cells
	06.02 Learn the Label with Active Cells
	06.03 Get the input predicted values and update the last predicted value depending upon the similarity
	06.04 Reset the Temporal Memory
	06.05 Continue all above steps for sequences of Multi-sequences for maximum cycles
07. Get the trained Cortex Layer and HTM Classifier
```

IV. Predict the label 

The trained Cortex Layer can now be used to compute the Compute Cycle and the HTM Classifier will give the predicted input values as shown below:

```csharp
public List<ClassifierResult<string>> Predict(int[] input)
{
    var lyrOut = this.Layer.Compute(input, false) as ComputeCycle;

    List<ClassifierResult<string>> predictedInputValues = this.Classifier.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);

    return predictedInputValues;
}
```


4.Results :
-------------
More than twenty experiments are done and here is the sample output accuracy logs and the result based on user input.

- Accuracy Logs : Output Accuracy Logs for [maxCycles 50](https://github.com/rabiul64/neocortexapi/commit/d53a7060952a0c6211dd9a809eea7d93fa29e010) 

[TaxiPassangerPredictionExperiment]

******Sequence Starting******

cycle : 0 	 Accuracy :0
 	
cycle : 1 	 Accuracy :16.666666666666664

cycle : 2 	 Accuracy :20.833333333333336 
	 
Cycle: 3 	 Saturated Accuracy : 20.833333333333336 	 Number of times repeated 1

cycle : 4 	 Accuracy :16.666666666666664
  
cycle : 5 	 Accuracy :33.33333333333333
 
cycle : 6 	 Accuracy :20.833333333333336 
 
cycle : 7 	 Accuracy :37.5 
	 
cycle : 8 	 Accuracy :33.33333333333333 
 
cycle : 9 	 Accuracy :54.166666666666664
	 
cycle : 10 	 Accuracy :62.5 
	 
Cycle: 11 	 Saturated Accuracy : 62.5 	 Number of times repeated 1

cycle : 12 	 Accuracy :50

cycle : 13 	 Accuracy :70.83333333333334
 	 
cycle : 14 	 Accuracy :54.166666666666664
 	 
cycle : 15 	 Accuracy :58.333333333333336
	 
cycle : 16 	 Accuracy :70.83333333333334
 	 
cycle : 17 	 Accuracy :79.16666666666666
	 
cycle : 18 	 Accuracy :66.66666666666666
	 
cycle : 19 	 Accuracy :79.16666666666666
	 
cycle : 20 	 Accuracy :75
	 	 
Cycle: 21 	 Saturated Accuracy : 75 	 Number of times repeated 1

cycle : 22 	 Accuracy :66.66666666666666 	
	
cycle : 23 	 Accuracy :75 	 

cycle : 24 	 Accuracy :66.66666666666666
	 
cycle : 25 	 Accuracy :70.83333333333334
	 	 
cycle : 26 	 Accuracy :75
 	 
cycle : 27 	 Accuracy :70.83333333333334 
	 
cycle : 28 	 Accuracy :66.66666666666666 
	 
cycle : 29 	 Accuracy :62.5 
	 
cycle : 30 	 Accuracy :75
 	 
cycle : 31 	 Accuracy :58.333333333333336 
	 
cycle : 32 	 Accuracy :66.66666666666666 
	 
cycle : 33 	 Accuracy :70.83333333333334 
	 
cycle : 34 	 Accuracy :79.16666666666666 
	 
cycle : 35 	 Accuracy :70.83333333333334 
	 
cycle : 36 	 Accuracy :79.16666666666666 
	 
cycle : 37 	 Accuracy :75
 	 
Cycle: 38 	 Saturated Accuracy : 75 	 Number of times repeated 1
	
cycle : 39 	 Accuracy :70.83333333333334
	 
cycle : 40 	 Accuracy :66.66666666666666 
	 
cycle : 41 	 Accuracy :75 
	 
cycle : 42 	 Accuracy :70.83333333333334 
	 
cycle : 43 	 Accuracy :75 
	 
Cycle: 44 	 Saturated Accuracy : 75 	 Number of times repeated 1

cycle : 45 	 Accuracy :70.83333333333334 
	 
Cycle: 46 	 Saturated Accuracy : 70.83333333333334 	 Number of times repeated 1

cycle : 47 	 Accuracy :75 
		 
cycle : 48 	 Accuracy :70.83333333333334 
	 
cycle : 49 	 Accuracy :79.16666666666666

*****Sequence Ending*****

-----------------------------------------------------------------------------

- User Input Result : segment A date-time of 01-01-2022 00:18 is considered.

![UserInputResult](https://user-images.githubusercontent.com/31253296/159751185-27397564-0a57-4809-abf6-28b8f6a40b64.PNG)

5.Discussion
-------------

Yet to be done.

## Similar Studies/Research used as References
[1] Continuous online sequence learning with an unsupervised neural network model.
Author: Yuwei Cui, Subutai Ahmad, Jeff Hawkins| Numenta Inc.

[2] On the performance of HTM predicions of Medical Streams in real time.
Author: Noha O. El-Ganainy, Ilangkp Balasingham, Per Steinar Halvorsen, Leiv Arne Rosseland.

[3] Sequence memory for prediction, inference and behaviour
Author: Jeff Hawkins, Dileep George, Jamie Niemasik | Numenta Inc.

[4] An integrated hierarchical temporal memory network for real-time continuous multi interval 
prediction of data streams
Author: Jianhua Diao, Hyunsyug Kang.

[5] Stock Price Prediction Based on Morphological Similarity Clustering and Hierarchical Temporal 
Memory
Author: XINGQI WANG, KAI YANG, TAILIAN LIU

Similar Thesis used as References:
[6] Real-time Traffic Flow Prediction using Augmented Reality
Author: Minxuan Zhang
