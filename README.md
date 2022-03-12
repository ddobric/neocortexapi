# Project Title
ML21/22 24.  Investigate Label Prediction from the time-series sequence (taxi)

## Team Name
CodeWarriors

## Getting Started
se-cloud-2021-2022
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See the notes down on how to deploy the project and experiment on a live system.

## Project Directory Guide
* **../ML21/22 24.  Investigate Label Prediction from the time-series sequence (taxi)/** 
```
This directory has our Final Project codes 
https://github.com/rabiul64/neocortexapi/tree/master/MyProject/Source/TimeSeriesSequence

```
* **../Documents/** 

```
This directory contains all documents regard this project. Including experiment design, schedule, and the project document.(Remarks- Work in progress)

```
* **../Presentation/** 
```
This directory contains Everything regards our presentation. (Remarks- Work in progress)

```
# Project Plan
The commitment of each person on program can be tracked by following table

1. Read the passenger data from sample data set and a csv file is almost prepared.
==> We discovered every example data set has a lot of columns,. However, we believe the lpep_pickup_datetime and passenger_count columns will be important to consider. Then, from that data set, we'll count the number of passengers for each 1 hour and record them to a csv file. There will be a part every 1 hours, as mentioned in the project description. As a result, the column name in the changed csv files will be

For example:
	We have data in sample data set like

	lpep_pickup_datetime passenger_count
	01-01-2021 00:12 5
	01-01-2021 00:18 3
	01-01-2021 00:28 7

	We will make csv file something like this

	Datetime, segment, number of passenger
	01-01-2021 01:00 00 2000 --- all the passengers will be accumulated from 01-01-2021 00:00 to 01-01-2021 01:00
	01-01-2021 02:00 01 2000
	01-01-2021 03:00 02 3000
	....
	01-01-2021 05:00 05 4000

	We are considering every 60 mins one segment of a given date

2. Read the data from modified csv and encode the data
==> We will use 4 encoders to train data

    DayEncoder
    MonthEncoder
    DayofWeekEncoder -- considering this for weekdays or weekend
    SegmentEncoder (we will pass the time here and convert it into be a segment based on 60 mins)

	Please note: The year is not taken into account during encoding because it is a static one. For example, suppose the data in the dataset is from the year 2021, and the user wants to know the forecasted value for the year 2022.

3. HTM classifier will learn data at the infant stage of the algorithm for train data.

4. The user can input any date, time, or date segment, and htmClassifier will return a predicted result. GetPredictedInputValues(encoded time-segment); is a function that returns the predicted input values for an encoded time-segment. In my opinion, a date time like 01-01-2022 00:18 should be considered.

# Contribution
The commitment of each person on program can be tracked by following table

| Name | Commitment on master branch | Remarks |
| :---------------: | :-------------: | :---------: |
| Md Rizwanul Islam | https://github.com/rabiul64/neocortexapi/commits?author=BdCode-Worm |  |
| Md Rabiul Islam | https://github.com/rabiul64/neocortexapi/commits?author=rabiul64 |  |
| Md Jubayar Hosan | https://github.com/rabiul64/neocortexapi/commits?author=jubayar |  |

## Acknowledgments

* NeoCortexApi
* FUAS-SE-Cloud-2021-2022 colleagues