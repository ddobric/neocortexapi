## HtmClassifier
The HtmClassifier is a neocortexapi module that is used for prediction of the next element by presenting the current sequence state. The classifier provides two methods:

The first one is used for learning of the SDR assotiated with the given key. For example, the SDR=00001110001110010011000 might represent some label 'A'. In that case the label 'A' is used as a key and the SDR is the array that will be associated to that key. That means, the HtmClassifier is rather an association algorithm than a classical learning algorithm. To associated an SDR int-array with the key, following method is used:

~~~csharp
void Learn(string key, int[] sdr)
~~~

The second important method in this context is the prediction method. After the learning process is completed, the prediction code is typicall implemented to predict the label (key) from the given element. For example, imaging there is a learned sequence ABCDE. The prediction code should return B for a given A, then D for a given C and so on. 

~~~csharp
public List<ClassifierResult> GetPredictedInputValues(Cell[] predictiveCells, short howMany)
~~~
This method returns the list of guesses (predictions) for the given element. The returned prediction is a list that contains the predicted value the similarity in percent and the number of same (overlapped) bits.

~~~csharp
    /// <summary>
    /// Defines the predicting input.
    /// </summary>
    public class ClassifierResult<TIN>
    {
        /// <summary>
        /// The predicted input value.
        /// </summary>
        public TIN PredictedInput { get; set; }

        /// <summary>
        /// Number of identical non-zero bits in the SDR.
        /// </summary>
        public int NumOfSameBits { get; set; }

        /// <summary>
        /// The similarity between the SDR of  predicted cell set with the SDR of the input.
        /// </summary>
        public double Similarity { get; set; }
    }
~~~

### The learning process
The *Learn*-method receives as a first argument the key string, that represents the “name” or “identifier” of the learning element that should be associated with the learning SDR. The *HtmClassifier* will memorize that association. The key can be any value. It should solely describe the input.
For example, assume, the following sequence S1 is learned:

~~~
S1: 1.0 2.0 3.0 4.0 5.0 6.0 7.0 
~~~

In every cycle, the experiment will create the key that represents the element in context of the sequence in the current cycle. For example, the key might look like:

Cycle 1: 'S1_-1.0-1-2-3-4-5' , 
Cycle 2: 'S1_-1.0-1-2-3-4-5-6', 
Cycle 3: 'S1_1-2-3-4-5-6-7', 
etc..

This means, the sequence starts from beginning, which is indicated by ‘-1’. This value can be anything. As next, the element 1.0 appears, then 2.0 etc (S1_-1.0-1-2-3-4-5). In the next cycle, S1_-1.0-1-2-3-4-5-6, element 6 will appear, because the code creates the key up to 6 elements. Then the following key will appear S1_1-2-3-4-5-6-7. We build keys in our examples as a sequence, so we know when for example number 7 appears, that it has appeared after number six. This is important because the same number 7 can inside of the same sequence appears after other numbers too. We build the key a convenient way to be able to know the position of the element. But, you can use any other way to build the key, that might be more suitable for your scenario. For example, if your code is using learning of video frames and you want to classify the video, but not the frame, then the key can be the name of the video with some index (i.e. myvideo-156767.avi). In this case, the classifier learns for all video frames the same key. However, the key must be a unique value for all learned elements. This is why the 156767 is appended.Another useful example is learning of multiple sequences. This is very similar to learning of videos. To build the key, it is important to know that tmClassifier requires the unique key for every element. F your prediction code must predict the sequence instead of element (like the video as a sequence of frames), then the key should contain the sequence. For example “SEQUENCE1_Element1-Element2-..” or “VIDEO1_Framne1_Frame2…”.

During the learning process, the input in every cycle is the cell-SDR of cells produced by the Temporal Memory (TM) algorithm. The same Spatial Pooler (SP) output (column SDR) for some element (i.e.: ‘3’) will be represented by the same column-SDR if the SP is in a stable state. However, the TM does not generate the same set of active cells for the same element (i.e.’3’). The TM is building the context when learning the sequence.
That means, the element ‘3’ followed by the element ‘2’ produces a different set of active cells than the element ‘3’ followed by the element ‘5’. However, cells activated for both elements ‘3’ belong always to the same set of mini-columns as activated by the SP. 

If you use the MultisequenceLearning.cs or similar experiments that can be found in this repository, following output will be created.

~~~
-------------- 5 ---------------

Active segments: 20, Matching segments: 20
Col  SDR: 271, 274, 281, 282, 288, 292, 296, 305, 327, 328, 342, 344, 356, 358, 363, 364, 388, 395, 396, 400, 
Cell SDR: 6798, 6869, 7038, 7069, 7222, 7305, 7412, 7629, 8179, 8219, 8560, 8618, 8908, 8950, 9082, 9117, 9710, 9876, 9911, 10008, 
Match. Actual value: S1_6-7-1-2-3-4-5 - Predicted value: S1_6-7-1-2-3-4-5.
Item length: 20	 Items: 9
Predictive cells: 20 	 7062, 8178, 8204, 8520, 8552, 8615, 8678, 8972, 9259, 9775, 9887, 9903, 10019, 10049, 10098, 10133, 10213, 10390, 10582, 11200, 
<indx:000	inp/len: S1_-1.0-1-2-3-4-5/20, Same Bits = 000	, Similarity 000.00 %	 6778, 6869, 7038, 7069, 7082, 7217, 7324, 7417, 7626, 7685, 8179, 8219, 8436, 8618, 8902, 8950, 9082, 9121, 9710, 9911, 
>indx:001	inp/len: S1_-1.0-1-2-3-4-5-6/20, Same Bits = 010	, Similarity 050.00 % 	 7038, 7069, 8179, 8219, 8520, 8560, 8618, 8678, 8950, 9259, 9775, 9876, 9911, 10008, 10049, 10098, 10133, 10213, 10390, 10582, 
<indx:002	inp/len: S1_1-2-3-4-5-6-7/20, Same Bits = 000	, Similarity 000.00 %	 8184, 8572, 8617, 8686, 8966, 9265, 9471, 9777, 10006, 10099, 10125, 10208, 10590, 10716, 10737, 11216, 11390, 11546, 12354, 12433, 
<indx:003	inp/len: S1_2-3-4-5-6-7-1/20, Same Bits = 000	, Similarity 000.00 %	 1104, 1397, 2327, 2950, 3414, 3858, 4044, 4117, 4177, 4217, 4506, 4803, 5375, 5411, 5464, 5671, 5721, 5879, 6225, 6349, 
<indx:004	inp/len: S1_3-4-5-6-7-1-2/20, Same Bits = 000	, Similarity 000.00 %	 3863, 4123, 4182, 4203, 4516, 4816, 5226, 5399, 5417, 5472, 5600, 5656, 5702, 5786, 5877, 6040, 6227, 6338, 7093, 7321, 
<indx:005	inp/len: S1_4-5-6-7-1-2-3/20, Same Bits = 000	, Similarity 000.00 %	 4204, 5357, 5399, 5466, 5592, 5600, 5656, 5702, 5781, 5887, 6034, 6227, 6784, 7034, 7093, 7321, 7420, 7478, 7542, 8449, 
<indx:006	inp/len: S1_5-6-7-1-2-3-4/20, Same Bits = 000	, Similarity 000.00 %	 5714, 5779, 6039, 6228, 6610, 6778, 7030, 7065, 7082, 7217, 7324, 7417, 7528, 7626, 7685, 7905, 8190, 8436, 8902, 9121, 
<indx:007	inp/len: S1_6-7-1-2-3-4-5/20, Same Bits = 000	, Similarity 000.00 %	 6798, 6869, 7038, 7069, 7222, 7305, 7412, 7629, 8179, 8219, 8560, 8618, 8908, 8950, 9082, 9117, 9710, 9876, 9911, 10008, 
>indx:008	inp/len: S1_7-1-2-3-4-5-6/20, Same Bits = 020	, Similarity 100.00 %	 7062, 8178, 8204, 8520, 8552, 8615, 8678, 8972, 9259, 9775, 9887, 9903, 10019, 10049, 10098, 10133, 10213, 10390, 10582, 11200, 
Current Input: 5 	| Predicted Input: S1_7-1-2-3-4-5-6 - 1
Current Input: 5 	| Predicted Input: S1_-1.0-1-2-3-4-5-6 - 50
Current Input: 5 	| Predicted Input: S1_-1.0-1-2-3-4-5 - 0
Current Input: 5 	| Predicted Input: S1_1-2-3-4-5-6-7 - 0
-------------- 6 ---------------

Active segments: 20, Matching segments: 20
Col  SDR: 282, 327, 328, 340, 342, 344, 347, 358, 370, 391, 395, 396, 400, 401, 403, 405, 408, 415, 423, 448, 
Cell SDR: 7062, 8178, 8204, 8520, 8552, 8615, 8678, 8972, 9259, 9775, 9887, 9903, 10019, 10049, 10098, 10133, 10213, 10390, 10582, 11200, 
Match. Actual value: S1_7-1-2-3-4-5-6 - Predicted value: S1_7-1-2-3-4-5-6.
Item length: 20	 Items: 9
Predictive cells: 20 	 8184, 8572, 8617, 8686, 8966, 9265, 9471, 9777, 10006, 10099, 10125, 10208, 10590, 10716, 10737, 11216, 11390, 11546, 12354, 12433, 
<indx:000	inp/len: S1_-1.0-1-2-3-4-5/20, Same Bits = 000	, Similarity 000.00 %	 6778, 6869, 7038, 7069, 7082, 7217, 7324, 7417, 7626, 7685, 8179, 8219, 8436, 8618, 8902, 8950, 9082, 9121, 9710, 9911, 
<indx:001	inp/len: S1_-1.0-1-2-3-4-5-6/20, Same Bits = 000	, Similarity 000.00 %	 7038, 7069, 8179, 8219, 8520, 8560, 8618, 8678, 8950, 9259, 9775, 9876, 9911, 10008, 10049, 10098, 10133, 10213, 10390, 10582, 
>indx:002	inp/len: S1_1-2-3-4-5-6-7/20, Same Bits = 020	, Similarity 100.00 %	 8184, 8572, 8617, 8686, 8966, 9265, 9471, 9777, 10006, 10099, 10125, 10208, 10590, 10716, 10737, 11216, 11390, 11546, 12354, 12433, 
<indx:003	inp/len: S1_2-3-4-5-6-7-1/20, Same Bits = 000	, Similarity 000.00 %	 1104, 1397, 2327, 2950, 3414, 3858, 4044, 4117, 4177, 4217, 4506, 4803, 5375, 5411, 5464, 5671, 5721, 5879, 6225, 6349, 
<indx:004	inp/len: S1_3-4-5-6-7-1-2/20, Same Bits = 000	, Similarity 000.00 %	 3863, 4123, 4182, 4203, 4516, 4816, 5226, 5399, 5417, 5472, 5600, 5656, 5702, 5786, 5877, 6040, 6227, 6338, 7093, 7321, 
<indx:005	inp/len: S1_4-5-6-7-1-2-3/20, Same Bits = 000	, Similarity 000.00 %	 4204, 5357, 5399, 5466, 5592, 5600, 5656, 5702, 5781, 5887, 6034, 6227, 6784, 7034, 7093, 7321, 7420, 7478, 7542, 8449, 
<indx:006	inp/len: S1_5-6-7-1-2-3-4/20, Same Bits = 000	, Similarity 000.00 %	 5714, 5779, 6039, 6228, 6610, 6778, 7030, 7065, 7082, 7217, 7324, 7417, 7528, 7626, 7685, 7905, 8190, 8436, 8902, 9121, 
<indx:007	inp/len: S1_6-7-1-2-3-4-5/20, Same Bits = 000	, Similarity 000.00 %	 6798, 6869, 7038, 7069, 7222, 7305, 7412, 7629, 8179, 8219, 8560, 8618, 8908, 8950, 9082, 9117, 9710, 9876, 9911, 10008, 
<indx:008	inp/len: S1_7-1-2-3-4-5-6/20, Same Bits = 000	, Similarity 000.00 %	 7062, 8178, 8204, 8520, 8552, 8615, 8678, 8972, 9259, 9775, 9887, 9903, 10019, 10049, 10098, 10133, 10213, 10390, 10582, 11200, 
Current Input: 6 	| Predicted Input: S1_1-2-3-4-5-6-7 - 1
Current Input: 6 	| Predicted Input: S1_-1.0-1-2-3-4-5 - 0
Current Input: 6 	| Predicted Input: S1_-1.0-1-2-3-4-5-6 - 0
Current Input: 6 	| Predicted Input: S1_2-3-4-5-6-7-1 - 0
-------------- 7 ---------------
~~~

When predicting (GetPredcitedInputValues), the classifier is traversing through hash values of all memorized SDRs (lines 93-101) and tries to match the best ones, that matches with the highest number of bits in the SDR. Finally, the classifier returns the array of best-matching inputs. The argument *howMany* defines the wanted number of top predictions that should be considered in the predicted list from the HTM Classifier (see lines 81-84 and 102-105).

Once the classifier has learnt the sequence (code omitted), you can write the inferring (prediction) code. The following example illustrates this. The method *InputSequence* requires the user to input a few sequence elements. The following code-snippet shows an exaple that implements the prediction.

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
Please enter a number that has been learned
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





