# ML 21/22 - Implement Trace SDR Method

#### The goal of this project is to implement the well-formatted tracing of multiple SDRs 

## Project Description 
## Aim:
In Hierarchical Temporal Memory, the underlying Spatial Pooler algorithms generates continuous SDR which are encoded with some input or sequence. The activation of neurons is not always the same for every sequence. This experiment aims to describe the two SDR sequences  taken to investigate the two slightly different sets of encoded inputs by using a new method that will differentiate the SDR values and manifest the dissimilarity in both the traced sequences SDR. The different sets of inputs in both SDR lists have semantic similar inputs. Every bits in SDR are not designated with any value or names, but it has a semantic meaning which are to be learned. The experiment we did here, shows the work of the two SDRs which has the active bits on the similar locations and in some locations there are dissimilar or inactive bits and both SDR sets allot the same semantic attributes because of the active bits in same place. The investigation we proposed here, there is an overlap between both the SDRs as they have somewhere same set of active bits, so we can immediately compare between the two representations which are semantically similar and differentiate the parts or bits which are dissimilar. Within one set of neurons, an SDR at one point in time can associatively link to the next occurring SDR.

## Architecture:
### Part I:

The StringifyVector method, which is already implemented generates or trace out the SDRs. The following line of code is used

> **_Code Line:_**  Helpers.StringifyVector(lyrOut.PredictiveCells.Select(c => c.Index).ToArray())
This method produces the output such as two sets of SDR. The method produces outputs like following one:

---

**Output of StringifyVector method**

     51, 76, 87, 113, 116, 118, 122, 152, 156, 163, 179, 181, 183, 186, 188, 190, 195, 210, 214, 224, 

     51, 76, 113, 116, 118, 156, 163, 179, 181, 182, 183, 186, 188, 190, 195, 197, 210, 214, 224, 243

---
The result of above mentioned method, shows indexes of bits that are active. Now, the output of SDR are similar, then it is challenging to see the difference in both the SDRs. Hence, we constructed a new method called Stringify_TraceSDR. The following new method Stringify_TraceSDR shows how to differ between two SDR sets in a well arranged index.

---
**Block of Code**

```csharp
public static string StringifyTraceSDR(List<int[]> sdrs)
{
     var heads = new List<int>(newint[sdrs.Count]);
     var outputs = new StringBuilder[sdrs.Count]);
}
```

### Part II:

The index of bit that are active are provided into Stringify_TraceSDR method and, now we further examine to create a well formatted SDRs. The implied method stores every bits of active column as a string. 

The next step of our approach was to differentiate between the two SDRs which have similar index bit of active neurons, we create better results by adding gaps or space at places that have not the same index or inactive bits in the SDRs. Following code shows how to fill the space in place of missing index bit which are inactive. 

---

**Block of Code**

```csharp

var numOfSpaces = minActiveColumn.ToString().Length; 
for (var j = 0; j < numOfSpaces; j++)
{
     outputs[i].Append(" ");
}
outputs[i].Append(", ");
```
---

## Result:
The final result of our experiment, the SDR output creates spaces at those places where the representations do not have the same index. 

<pre>
 51, 76, 87, 113, 116, 118, 122, 152, 156, 163, 179, 181,    , 183, 186, 188, 190, 195,    , 210, 214, 224,   ,

 51, 76,   , 113, 116, 118,    ,    , 156, 163, 179, 181, 182, 183, 186, 188, 190, 195, 197, 210, 214, 224, 243
</pre>

## Unit Test Project:
A Unit Test project has been also implemented for our project, tested out the Stringify_TraceSDR method case to make sure major requirements of the module are being validated.

---
**Code**:

```csharp
public void StringifyTest()
{

     var list = new int[] { 51, 76, 87 };

     var list1 = new int[] { 51, 76, 113 };

     var output = Helpers.StringifyTraceSDR(new  List<int[]> { list, list1 });

     var expectedResult = new StringBuilder();
     var sdr1 = new StringBuilder();
     sdr1.Append("51, 76, 87,    , ");

     var sdr2 = new StringBuilder();
     sdr2.Append("51, 76,   , 113, ");

     expectedResult.AppendLine(sdr1.ToString());
     expectedResult.AppendLine(sdr2.ToString());

     Assert.IsTrue(output == expectedResult.ToString());
}
```

---

## Conclusion & Future Work

In Hierarchical Temporal Memory, the underlying algorithm tracks the SDRs. These SDRs are continuously generating encoded bits inside the  mini-columns, and these SDR vectors have a sequence which are mostly identical with each other in the same locations. However, it is quite difficult for a person to differentiate a long string of SDRs, as most of the bit indexes have the same values and locations. Thus, this part of the problem is solved by building a new method applied to the model, which traces out the SDR encodings with similar semantic attributes and, moreover, creates the spacing wherever inactive bits of SDR vectors are observed. However, the proposed model can moreover be modified in the future, such as instead of spacing there can be any symbol inserted, for example a “_” or “$”, which will differentiate between the active and inactive bits.  In addition to that, the active bits have two and three digit numbers. In the future or parallel implementations, it can be changed so that every bit has the same digit number. 


## Documentation

The documentation of this project can be found [here](./ML-21-22-26_Implement_Trace_SDR_Method.pdf)

## Code

The code for this project can be followed [here](../NeoCortexApi/Helpers.cs#L35-L147)

And, the Unit test project is [here](../UnitTestsProject/TraceSDRsTest.cs)
