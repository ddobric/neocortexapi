# Improve Scalar Encoder
HTM System require input in the form of Sparse Distributed Representations(SDR). It is called sparse because, most of the bits are off and only a few bits are on/active. Each bit carry a distinct semantic meaning. So if two input have many overlap bits then it can be consider that those two inputs are semantically similar.

In the project, I have implemented an Experimental Class, which is referencing the [NeocortexApi](https://github.com/ddobric/neocortexapi). Encode method of ScalarEncoder class is invoked and analysed.

### There are a few important aspects that need to be considered when encoding data: [Resource By Numenta](https://arxiv.org/pdf/1602.05925.pdf)
1. Semantically similar data should result in SDRs with overlapping active bits.
2. The same input should always produce the same SDR as output.
3. The output should have the same dimensionality (total number of bits) for all inputs.
4. The output should have similar sparsity for all inputs and have enough one-bits to handle noise and subsampling.

### Some Abbreviations & Meanings
- W : Number of active bits to have in each representation
- N : Total number of bits to represent the input data
- bucket : A space to hold an input encoded data
- MinVal : Minimum value that the SDR can represent without clipping
- MaxVal : Maximum value that the SDR can represent without clipping
- periodic : Data is repeated after a certain time.
- clipInput: If true, values less than the minimum has SDR similar to minimum value and values more than the maximum has SDR similar to maximum value.
- Total buckets that a SDR can represent = (N-W+1)

### Finding 1
In our first experiment, the details are : 
1. Minimum value that the SDR can represent is 0, rest will be clipped out. 
2. Maximum value that the SDR can represent is 9, rest will be clipped out.
3. W = 3
4. N = 10

The total number of buckets that our configuration can represent is = (10 - 3 + 1 )= 8. Since we have to represent data from 0 to 9, which is 10 so the number of available bucket is less than the required bucket (10). This causes an overlap in SDR of input space. In our case, 2 & 3 input overlap occurs as shown in the image.
![Overlap in SDR of input 2 and 3](./Images/E1.png)

### Finding 2
In our second experiment, the details are : 
1. MinVal = 0
2. MaxVal = 9
3. W = 3
4. N = 11

The total number of buckets that our configuration can represent is = (11 - 3 + 1 )= 9. Since we have to represent data from 0 to 9, which is 10 so the number of available bucket (9) is less than the required bucket (10). This causes an overlap in SDR of input space. In our case, 4 & 5 input overlap occurs as shown in the image.
![Overlap in SDR of input 4 and 5](./Images/E2.png)


### Finding 3
In our third experiment, the details are : 
1. MinVal = 0
2. MaxVal = 9
3. W = 3
4. N = 12

The total number of buckets that our configuration can represent is = (12 - 3 + 1 )= 10. Since we have to represent data from 0 to 9, which is 10 so the number of available bucket (10) is equal to the number of the required bucket (10). This causes an discrete SDR representation of different inputs.
![Error free SDR representation](./Images/E3.png)


### Finding 4
While providing diffferent values of N, two adjacent values are identical because of low number of bucket size between min and max value. Even so, SDR output was seen without exception.

### Finding 5
When we do not provide the value of N, and rely on Resolution to produce SDR, System.OverFlowException was seen. As seen in the picture below, the value of N when relying on Resolution is in negative.

![OverFlow Exception due to Resolution](./Images/E4-resolution-overflowexception.png)

![Value of N during OverFlow Exception](./Images/E4-resolution-overflowexception-valueofN.png)

### Finding 6
Instead of using N and Resolution, this time I proceed with the experimentation by using different values of Radius. As viewed on the image below, at certain value of Radius (which is random values), Index out of range exception occured. Even so, we can view the sdr at different input space.
![Index out of range exception - Radius change](./Images/E5-Radius-IndexOutofRangeException.png)

### Finding 7
As a comment to above Finding 5, I worked with various values of Resolutions (With out setting N or Radius). System.OverFlowException was occured because the default Radius configuration was negative(-1). This resulted in negative value of N. Since the output array was initialized using the value : 

```
output = int[N]
```

This resulted in OverFlowException.

Setting the default value of Radius to 0 inside the Initialize method of EncoderBase.cs file, N was no longer negative and OverFlow exception was solved.

![IndexOutOfRangeError](./Images/Parameter-Default-Value-Set.png)


After solving the issue of OverFlow, System.IndexOutOfRangeException was viewed in some input space with different value of Resolution, to solve this issue, implementation is checked and improved on the forked [Forked NeocortexApi from Damir Dobric](https://github.com/bisalgt/neocortexapi)

### Finding 8
This experiment is related to finding 7. We solved the problem of Overflow Exception on Finding 7. By experimenting with various inputs, we came to find Index out of range exception for some inputs as shown on the image. The error was clearly the result of minbin and maxbin, which denotes the starting position and ending position of the active bits. 

![IndexOutOfRangeError](./Images/IndexOutofRangeException.png)

As show in the image above, the maxbin value was 5 whereas our array could only hold data for position indexed up to 4. This resulted in the exception.


After experimenting with different values and using breakpoints, I found out there might be issue in the N value or centerbin value. I believed that increasing the value of N would decrease the chances of indexoutofrange exception as there would be more positions in output array. I went with experimenting with the logic of centerbin and that seems okay. So I then looked for the value of N(total number of input bits). After various experimentations, I changed the value of N in line 157 of ScalarEncoder.cs to N = (int)Math.Round(nFloat). This solved the problem for our given input.(w=3, min=13, max=29, resolution=1-10). 

![IndexOutOfRangeError](./Images/IndexOutofRangeSolvedPartially.png)

But still the problems seems to be unsolved. With certain inputs, I could see some exceptions with (w=3, min=0, max=9, resolution=1-10)

![IndexOutOfRangeError](./Images/IndexOutofRangeStillExists.png)


So for the next debugging part, I then consulted available documents and experimented with different values. Changing the value of N in line 157 of ScalarEncoder.cs to N = (int)Math.Ceiling(nFloat) solved the issue of Indexoutofrange exception.

![IndexOutOfRangeError](./Images/IndexOutofRangeSolvedCompletely.png)

Using Math.Ceiling increased the value of N even if there is some decimal points. This resulted in increase in positions in output so that the calculated maxbin was always less than or equal to N. Hence, Math.Ceiling solved the issue of IndexOutOfRange Exception caused during experimentation with various value of Resolution.


### Finding 9

In finding 6, IndexOutofRange Exception was observed.
After changing the way how N was calculated, as described and implemented on finding 7, the IndexOutofRange exception was solved.

### Finding 10

In findings 1, 2 and 3, for different input, similar output was observed. For appropriately handling this issue, ArgumentException check had been implemented on the InitEncoder method of ScalarEncoder.cs class. Using the handlers made it interactive for the user and gives some infos about the expected result.
This had been implemented on the forked repository[Adding Parameter Checks](https://github.com/bisalgt/neocortexapi/commit/4f813ec1a960288b9e4e88c75194a92e43aaa650)

For effective handling the parameters when there are values in case of multiple mutually exclusive parameters (N, Radius, Resolution), Resolution is set to 0. This makes it easy to implement the if else block inside InitEncoder.

The Scalar Encoder is made interactive by giving the User option to enter data on the console. Based on the input string by the User, the value of N, Radius or Resolution could be updated. If users enter yes in the console, the value would be updated and distinct encoding would be generated. If user enters no, the value for either N or Radius or Resolution is not updated. Argument exception is thrown in such case.

![Interactive Terminal ](./Images/Interactive-Terminal.png)


#### Examples of Argument Exceptions

![Argument Exception for incorrect Total Bits](./Images/ArgumentExceptionForTotalBits.png)


![Argument Exception for incorrect Resolution](./Images/ArgumentExceptionForResolution.png)


![Argument Exception for incorrect Radius](./Images/ArgumentExceptionForRadius.png)

### Finding 11

The ScalarEncoder has been modified so that it can take input from both the dictionary settings and command line arguments. To work with the command line arguments, arguments passed on the Main function of our program can be directly passed to the ScalarEncoder. Because of function Overloading, ScalarEncoder(string[] args) will be called on our program instead of ScalarEncoder(Dictionary <...>).
1. The command line arguments can be set on visual studio by going to the Properties of our current solution and adding arguments.
2. Command line arguments can also be passed directly from the console and the program can be run as shown below:

```
dotnet run --project source/Samples/NeoCortexApiSample/NeoCortexApiSample.csproj --n 14 --w 3 --minval 1 --maxval 8 --periodic false --clipinput true
```
Proper exception (System.FormatException) is handled if the supplied arguments is not convertible to desired type.

Image is attached to show the usecase:

![ScalarEncoder called from Command Line](./Images/ScalarEncoderFromCLI.png)


### Implementation of UnitTests

Proper Unitest had been implemented that compares improved version of ScalarEncoder and unimproved version of scalar encoder with the similar parameter. UnitTest can be found on : [Unit Test for Scalar Encoder Improved](https://github.com/bisalgt/neocortexapi/blob/master/source/UnitTestsProject/EncoderTests/ScalarEncoderImprovedTests.cs)


### Link to the Presentation Video
https://youtu.be/bwHDblQAuP4