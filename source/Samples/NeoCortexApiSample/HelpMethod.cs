using System;
using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using MultiSequenceLearning;
using Newtonsoft.Json;


namespace MultiSequenceLearning
{

    public class HelpMethod
    {
        public HelpMethod();

        {

        }


        /// <summary>
        /// HTM Config for creating Connections
        /// </summary>
        /// <param name="inputBits">input bits</param>
        /// <param name="numColumns">number of columns</param>
        /// <returns>Object of HTMConfig</returns>
        ///


        public static HtmConfig FetchHTMConfig(int inputBits, int numColumns)
        {
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1,

                //NumInputs = 88 

            };

            return cfg;
        }
    

    /// <summary>
    /// Takes in user input and return encoded SDR for prediction

    /// <param name="userInput"></param>
    /// <returns></returns>
    public static int[] EncodeSingleInput(string userInput)
    {
        int[] sdr = new int[0];



        return sdr;
    }


//// Get the encoder with settings
 
        /// <param name="inputBits">input bits</param>
        /// <returns>Object of EncoderBase</returns>
        public static EncoderBase GetEncoder(int inputBits)
{
    double max = 20;

    Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

    EncoderBase encoder = new ScalarEncoder(settings);

    return encoder;
}



    /// </summary>
    /// <param name="path">full path of the file</param>
    /// <returns>Object of list of Sequence</returns>
    /// Reads dataset from the file
    public static List<Sequence> ReadDataset(string path)
{
    Console.WriteLine("Reading Sequence...");
    String lines = File.ReadAllText(path);
    var sequence = JsonConvert.DeserializeObject(lines);
        List<Sequence> sequence = System.Text.Json.JsonSerializer.Deserialize<List<Sequence>>(lines);

    return sequence;
}


    public static List<Sequence> CreateDataset()
    {
        int numberOfSequence = 30;
        int size = 12;
        int startVal = 0;
        int endVal = 15;
       
        return sequence;
    }