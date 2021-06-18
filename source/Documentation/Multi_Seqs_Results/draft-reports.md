TODO: Test with longer sequence(s)
TODO: Clean up the code

# Requirements:

1.	Extend the experiment to learn the single sequence with Reset()
	a.	Take few sequences and learn as it is now, without Reset(). Trace out results.
	b.	Then add Reset and repeat same sequences.
	c.	Compare output.
2.	Play with the exit  condition for the stable TM
  if (maxMatchCnt >= 30)
3.	Try to learn multiple sequences

## Aproach/Criterias

The single sequences tested:
(For single sequence)

The multi-sequences tested:
{ 6.0, 7.0, 9.0, 10.0 }
{ 15.0, 5.0, 3.0, 4.0, 20.0, 3.0, 5.0 }

### Without Reset:
1. Cycle: 3500
2. Accuracy: accuracy = matches/input length*100.0.
3. maxstabilitycycles: The longest number of times it reaches 100% accuracy consecutively
4. instabilitycycles: Number of instabilities
5. minstabilitycycles: shortest muber of cstable cycles.

#### Example1
"****************++++++++"

longest=7
Number of instabilities=0
Shortest=0

#### Example2
"****************++++++++*****++++"

longest=7
Number of instabilities=1
Shortest=5
 
#### Example3
"****************++++++++*****++++**++*++"

longest=7
Number of instabilities=3
Shortest=1

## Elapsed time.
NOTE on point 3 and 4: These are for requirement number 4. maxMatchCnt should be greater than the second longest number of times.
Which means it will be in the stable area of the longest number of times.

With Reset:
Same as above except point 2.
2. Accuracy: accuracy = matches/(input length - 1)*100.0.
Reason: Reset makes the first prediction to be always incorrect.
=> Easier to be counted as 100% due to this limitation.

## Results

### Single sequence w/wo Reset

#### Without Reset 
		Cycle: 3500	Matches=4 of 4	 100%
		Elapsed time: 4 min.
		Maximum number of consecutive 100% correct: 3409
		Last correct cycle: 3500
		Second longest number of consecutive 100% correct: 0
#### With Reset 
		Cycle: 3500	Matches=3 of 4	 100%
		Elapsed time: 3 min.
		Maximum number of consecutive 100% correct: 3493
		Last correct cycle: 3500
		Second longest number of consecutive 100% correct: 0
		
	Conclusion: There are some differences but seems small. Could be within margin of error. They look alike.
	The second longest number is 0. Which means there is no need to set maxMatchCnt to be very big. Should test again with longer sequequences.

### Multiple  sequences (two) w/wo Reset

#### Without Reset

**Sequence 1**

Cycle: 3500	Matches=4 of 4	 100%
Elapsed time: 5 min.
Maximum number of consecutive 100% correct: 3384
Last correct cycle: 3500
Second longest number of consecutive 100% correct: 0

**Sequence 2**
Cycle: 3500	Matches=7 of 7	 100%
Elapsed time: 6 min.
Maximum number of consecutive 100% correct: 3001
Last correct cycle: 3500
Second longest number of consecutive 100% correct: 2
		
#### With Reset 

**Sequence 1**
		
Cycle: 3500	Matches=3 of 4	 100%
Elapsed time: 3 min.
Maximum number of consecutive 100% correct: 3493
Last correct cycle: 3500
Second longest number of consecutive 100% correct: 1

**Sequence 2**

Cycle: 3500	Matches=6 of 7	 100%
Elapsed time: 6 min.
Maximum number of consecutive 100% correct: 2961
Last correct cycle: 3500
Second longest number of consecutive 100% correct: 3
	
## Conclusion
  
Reduced number of consecutive 100% on the second run => takes more time to re-learn?
There are now some instable area (second longest number > 0) but still small => maxMatchCnt does not need to be a big number.
With and without Reset still feel similar.
