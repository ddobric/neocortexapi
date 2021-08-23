## HtmClassifier
The HtmClassifier is a *neocortexapi* module that is used to predict of the next element in the process of learning of the sequences.
The classifier provides two methods:

~~~csharp
Learn(string key, int[] sdr)
~~~

and

~~~csharp
public List<ClassifierResult> GetPredictedInputValues(Cell[] predictiveCells, short howMany)
~~~

The method receives the key string, that represents the sequence and memorizes the SDR for the given key. This value can be anything. It should solely describe the input.
Assume, the following sequence is learned: 
~~~
1-2-3-4-5-3-5
~~~
In every cycle, the experiment might create the key that represents the sequence in that cycle. For example, the key might look like:

Cycle 1: '1-2-3-4-5-3-5' , 
Cycle 2: '2-3-4-5-3-5-1', 
Cycle 3: '3-4-5-3-5-1-2', 
etc..

During the learning process, the input in every cycle is the cell-SDR of cells produced by the Temporal Memory (TM) algorithm. The same Spatial Pooler (SP) output (column SDR) for some element (i.e.: ‘3’) will be represented by the same column-SDR if the SP is in a stable state. 
However, the TM does not generate the same set of active cells for the same element (i.e.’3’). The TM is building the context when learning the sequence.
That means, the element ‘3’ followed by the element ‘2’ produces a different set of active cells than the element ‘3’ followed by the element ‘5’.

But, the cells activated for bot elements ‘3’ belong always to the same set of mini-columns as activated by the SP. 
This is why we choose the key for the classifier in the form shown above. However, developers are free to build a key some other way. For example, if your code is using learning of video frames and you want to classify the video, but not the frame, then the key can be the name of the video with some index (i.e. myvideo-156767.avi). In this case, the classifier learns for all video frames the same key. However, the key must be a unique value for all learned elements. This is why the 156767 is appended.

When predicting (GetPredcitedInputValues), the classifier is traversing through hash values of all memorized SDRs and tries to match the best ones, that matches with the highest number of bits in the SDR. Finally, the classifier returns the array of best-matching inputs.
The argument *howMany* defines the wanted number of top predictions that should be considered in the predicted list from the HTM Classifier.
The following example shows the trace for a sequence and here the index 0,3,12 have a similarity of 100. The classifier implementation provides top three possible outcomes. 
~~~
2-3
2-4
2-6
~~~

If you use the SequenceExperiment.cs, following output will be created:

~~~
Match. Actual value: 2-6-2-6-2-5-2-3-2-3-2-5-2-6-2-3-2-5 - Predicted value: 2-6-2-6-2-5-2-3-2-3-2-5-2-6-2-3-2-5\
Item length: 40	 Items: 18\
Predictive cells: 40 	 7111, 7604, 7862, 8022, 8068, 8332, 8452, 8503, 8552, 8604, 8654, 8688, 8852, 9016, 9112, 9262, 9533, 9693, 9904, 10337, 10418, 1.        0474, 10591, 10761, 10825, 10881, 11128, 11392, 11446, 11591, 11719, 11872, 11881, 11902, 12030, 12237, 12956, 13032, 13555, 13637, \
>indx:0	inp/len: 6-2-3-2-5-2-6-2-6-2-5-2-3-2-3-2-5-2/40	similarity 100%	 7111, 7604, 7862, 8022, 8068, 8332, 8452, 8503, 8552, 8604, 8654, 8688, 8852,            9016, 9112, 9262, 9533, 9693, 9904, 10337, 10418, 10474, 10591, 10761, 10825, 10881, 11128, 11392, 11446, 11591, 11719, 11872, 11881, 11902, 12030, 12237,          12956, 13032, 13555, 13637, 
>indx:3	inp/len: 2-5-2-6-2-6-2-5-2-3-2-3-2-5-2-6-2-3/904 ,Same Bits = 18	, similarity 100% 	 9300, 9301, 9302, 9303, 9304, 9305, 9306, 9307,          9308, 9309, 9310, 9311, 9312, 9313, 9314, 9315, 9316, 9317, 9318, 9319, 9320, 9321, 9322, 9323, 9324, 9400, 9401, 9402, 9403, 9404, 9405, 9406, 9407, 9408,        9409, 9410, 9411, 9412, 9413, 9414, 9415, 9416, 9417, 9418, 9419, 9420, 9421, 9422, 9423, 9424, 9525, 9526, 9527, 9528, 9529, 9530, 9531, 9532, 9533, 9534,        9535, 9536, 9537, 9538 
>indx:12	inp/len: 3-2-3-2-5-2-6-2-3-2-5-2-6-2-6-2-5-2/40	similarity 100%	 7111, 7604, 7862, 8022, 8068, 8332, 8452, 8503, 8552, 8604, 8654, 8688, 8852,            9016, 9112, 9262, 9533, 9693, 9904, 10337, 10418, 10474, 10591, 10761, 10825, 10881, 11128, 11392, 11446, 11591, 11719, 11872, 11881, 11902, 12030, 12237,          12956, 13032, 13555, 13637, 
Current Input: 5
The predictions with similarity greater than 50% are\
Predicted Input: 6-2-3-2-5-2-6-2-6-2-5-2-3-2-3-2-5-2,	Similarity Percentage: 100, 	Number of Same Bits: 40\
Predicted Input: 6-2-6-2-5-2-3-2-3-2-5-2-6-2-3-2-5-2,	Similarity Percentage: 100, 	Number of Same Bits: 40\
Predicted Input: 3-2-3-2-5-2-6-2-3-2-5-2-6-2-6-2-5-2,	Similarity Percentage: 100, 	Number of Same Bits: 40\
~~~
Once the classifier has learnt the sequence (code omitted), you can write the inferring (prediction) code. The following example illustrates this. The method *InputSequence* requires the user to input a few sequence elements.
~~~csharp
      private static List<double> InputSequence( List<double> inputValues)
       {
            Console.WriteLine("HTM Classifier is ready");
            Console.WriteLine("Please enter a sequence to be learnt");
            string userValue = Console.ReadLine();
            var numbers = userValue.Split(',');
            double sequence;
            foreach (var number in numbers)
            {
                if (double.TryParse(number, out sequence))
                {
                    inputValues.Add(sequence);
                }
         }

            return inputValues;
        }
~~~     
The following method is implementing prediction.
~~~csharp
       private static void Predict(int input, bool learn, CortexLayer<object, object> layer1, HtmClassifier<string, ComputeCycle> cls)
        {
            var result = layer1.Compute(input, false) as ComputeCycle;
            var predresult = cls.GetPredictedInputValues(result.PredictiveCells.ToArray(), 3);
            Console.WriteLine("\n The predictions are:");
            foreach (var ans in predresult)
            {
                Console.WriteLine($"Predicted Input: {string.Join(", ", ans.PredictedInput)}," +
                                  $"\tSimilarity Percentage: {string.Join(", ", ans.Similarity)}, " +
                                  $"\tNumber of Same Bits: {string.Join(", ", ans.NumOfSameBits)}");
            }
        }
~~~

Now the implemented HTM classifier method returns all possibilities as shown in the following trace:
~~~
Please enter a number that has been learnt
    2
~~~
After the user enters the number that has been learnt before by the classifier, the possible predicted outputs will be shown.
 ~~~
     Active segments: 80, Matching segments: 120
     Item length: 80	 Items: 9
     Predictive cells: 80 	 30516, 30543, 30704, 30773, 30865, 30896, 30937, 31009, 31025, 31145, 31191, 31245, 31264, 31279, 31327, 31357, 31404, 31450,          31499, 31621, 31644, 31815, 31940, 32023, 32134, 32180, 32201, 32237, 32254, 32277, 32322, 32347, 32356, 32418, 32459, 32485, 32512, 32630, 32724, 32767,          33384, 33441, 33450, 33486, 33513, 33573, 33599, 33622, 33640, 33660, 33687, 33708, 33774, 33786, 33805, 33871, 33886, 34012, 34039, 34051, 34089, 34111,          34160, 34185, 34266, 34316, 34413, 34428, 34453, 34478, 34517, 34530, 34561, 34612, 34630, 34665, 34697, 34739, 34756, 34813, 
     Predicted Input: 5-6-1-2-3-4-3-2-4,	Similarity Percentage: 50, 	Number of Same Bits: 40
     Predicted Input: 4-3-2-4-5-6-1-2-3,	Similarity Percentage: 50, 	Number of Same Bits: 40
~~~





