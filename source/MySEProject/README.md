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
- Unit Test Project [TimeSeriesUnitTest](https://github.com/rabiul64/neocortexapi/tree/master/source/MySEProject/UnitTestProject)

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

For learning time-series and predicting it we are using Multi-sequence Learning using HTM Classifier [example] (https://github.com/rabiul64/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) which is been forked form [NeoCortexApi](https://github.com/ddobric/neocortexapi)


The HTM Classifier consists of Spatial Pooler and Temporal Memory which takes in encoded data to learn the sequence.

I. Create Multiple Sequences of the time-series

II. Encode the datetime as segment

III. Learn using HTM Classifier

IV. Predict the label 
                        
3.Encoding and Learning
-------------

I.After reformatting the datetime, we found a processed CSV file where every dates have 24 segments (https://github.com/rabiul64/neocortexapi/tree/master/source/MySEProject/TimeSeriesSequence/DataSet/2021_Green_Processed.csv). 
Then we grouped by the date and segment for making a sequence. We have considered every date as a sequence which have 24 segments. In the example below, we can see 01/01/2021 is sequence with 24 segments. as seen below:

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
After Processing the data and creating the sequences, we have found 216 sequences where every date is a squence and every date has 24 segments. As working with 216 sequences
are very time consuming, so we consider 50 sequences with 50 cycles for learning and predicting the user input. After learning the sequences we have tracked the accuracy in 
a log file. The Output Accuracy Logs for [maxCycles 50](https://github.com/rabiul64/neocortexapi/blob/master/source/MySEProject/TimeSeriesSequence/TrainingLogs/TaxiPassangerPredictionExperiment637835742529422044.txt)
We are showing here 1 sequence accuracy with 50 cycles:

******Sequence Starting******
cycle : 0 	 Accuracy :4.166666666666666
 	 
cycle : 1 	 Accuracy :45.83333333333333
	 
Cycle: 2 	 Saturated Accuracy : 45.83333333333333 	 Number of times repeated 1

cycle : 3 	 Accuracy :58.333333333333336
 	 
cycle : 4 	 Accuracy :54.166666666666664
 	 
cycle : 5 	 Accuracy :62.5
 	 
cycle : 6 	 Accuracy :70.83333333333334
 	 
Cycle: 7 	 Saturated Accuracy : 70.83333333333334 	 Number of times repeated 1

cycle : 8 	 Accuracy :79.16666666666666
 	 
cycle : 9 	 Accuracy :75
 	 
cycle : 10 	 Accuracy :70.83333333333334
 	 
cycle : 11 	 Accuracy :75
 	 
cycle : 12 	 Accuracy :83.33333333333334
 	 
Cycle: 13 	 Saturated Accuracy : 83.33333333333334 	 Number of times repeated 1

Cycle: 14 	 Saturated Accuracy : 83.33333333333334 	 Number of times repeated 2

cycle : 15 	 Accuracy :75
 	 
cycle : 16 	 Accuracy :87.5
 	 
Cycle: 17 	 Saturated Accuracy : 87.5 	 Number of times repeated 1

Cycle: 18 	 Saturated Accuracy : 87.5 	 Number of times repeated 2

cycle : 19 	 Accuracy :91.66666666666666
 	 
Cycle: 20 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 1

Cycle: 21 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 2

Cycle: 22 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 3

cycle : 23 	 Accuracy :87.5
 	 
cycle : 24 	 Accuracy :91.66666666666666 
	 
Cycle: 25 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 1

Cycle: 26 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 2

Cycle: 27 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 3

Cycle: 28 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 4

Cycle: 29 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 5

Cycle: 30 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 6

Cycle: 31 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 7

Cycle: 32 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 8

Cycle: 33 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 9

cycle : 34 	 Accuracy :95.83333333333334 
	 
cycle : 35 	 Accuracy :91.66666666666666 
	 
Cycle: 36 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 1

cycle : 37 	 Accuracy :95.83333333333334 
	 
Cycle: 38 	 Saturated Accuracy : 95.83333333333334 	 Number of times repeated 1

Cycle: 39 	 Saturated Accuracy : 95.83333333333334 	 Number of times repeated 2

Cycle: 40 	 Saturated Accuracy : 95.83333333333334 	 Number of times repeated 3

Cycle: 41 	 Saturated Accuracy : 95.83333333333334 	 Number of times repeated 4

Cycle: 42 	 Saturated Accuracy : 95.83333333333334 	 Number of times repeated 5

cycle : 43 	 Accuracy :91.66666666666666
 	 
Cycle: 44 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 1

Cycle: 45 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 2

Cycle: 46 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 3

Cycle: 47 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 4

Cycle: 48 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 5

Cycle: 49 	 Saturated Accuracy : 91.66666666666666 	 Number of times repeated 6

*****Sequence Ending*****

- User Input Result : 
After giving the user inputs as dd/MM/yyyy hh:mm format, user can see the predicated results. 
Results provide the best 3 results with similarity.

![3](https://user-images.githubusercontent.com/31253296/160933384-e26d30c4-1111-40ee-a378-a88adbbdc19d.PNG)

Based on the above result, we can see that user has input following dates from table:

|   User Date  | Predicted Passengers |    Similarity   | Actual Passenger |
| :----------: | :----------------:   | :-------------: | :------------:   |
| 05-01-2022 04:00 | 20,30,16 | 57.14%, 57.14%, 57.14% | 23,30,13 |
| 26-02-2022 01:00 | 2,65,145 | 50%,50%,50% | 70,190,6 |
| 21-01-2022 03:00 | 2,7,65   | 75%,60%,33.33% | 5,12,75 |
| 21-02-2022 06:00 | 29,112,128 | 50%,50%,50% | 32,120,150 |


5.Discussion
-------------

In this paper, time series and sequences are thoroughly studied from two aspects, sequence learning, and prediction making.
A detailed review summarizing existing approaches is provided. We observe that time series have been studied for decades and
various models/learning methods are available but adapting the existing methods to high-dimensional and noisy scenarios remains
a challenge. The study on sequences/point processes is relatively new and improvements have been made in recent years. 
We used HTM sequence memory, a newly created neural network model, to solve real-time sequence learning issues with time-varying input 
streams in this research. The sequence memory concept is based on cortical pyramidal neurons’ computational principles. On real-world datasets,
we discussed model performance. The model meets a set of requirements for online sequence learning from input streams with constantly changing
statistics, an issue that the cortex must deal with in natural settings. These characteristics determine an algorithm’s overall
flexibility and its ability to be employed automatically. Even though HTM is still in its infancy compared to other classic neural network
models, it satisfies these features and shows promise on real-time sequence learning issues. In the experiments, HTM gives better performance
for single element sequences but for sequences that are constituted of multiple elements HTM gives a comparable performance which provides 
an insight that still there is scope for improvement in that direction. We have used HTM configuration for this experiment which proves there
is no need for hyper-parameter tuning for different kinds of data which in this case is not true. Time taken for HTM training is less, 
as HTM takes the whole sequence set for training which is not time and resource-consuming

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
