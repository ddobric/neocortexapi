1. Implement Serialize method for class Predictor.
- In order to serialize the Model of MultiSequences Learning, we need to be able to serialize the instance of class Predictor which is the output of the Training method. 
- The Predictor class inherits the interface ISerializable which defines methods for serialization.
```
    public interface ISerializable
    {
        void Serialize(object obj, string name, StreamWriter sw);
        static object Deserialize<T>(StreamReader sr, string name) => throw new NotImplementedException();
    }
``` 
- Inside the Predictor we have three objects that are needed to be serialized. They are connections, layer, and HtmClassifier. The code below shows the objects in the predictor:
```
    public class Predictor : ISerializable
    {
        private Connections connections { get; set; }

        private CortexLayer<object, object> layer { get; set; }

        private HtmClassifier<string, ComputeCycle> classifier { get; set; }
```
- The Predictor.Serialize() method will serialize all the objects in the predictor. It will call the Connections.Serialize(), layer.Serialize(), and classifier.Serialize() methods to serialize the connections, cortex layer, and classifier in the Predictor instance respectively.

```
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        if (obj is Predictor predictor)
        {
            // Serialize the Connections in Predictor instance
            var connections = predictor.connections;
            connections.Serialize(connections, null, sw);

            // Serialize the CortexLayer in Predictor instance               
            var layer = predictor.layer;
            layer.Serialize(layer, null, sw);

            // Serialize the HtmClassifier object in Predictor instance             
            var classifier = predictor.classifier;
            classifier.Serialize(classifier, null, sw);
        }
    }
```
- In order to make the program cleaner and easier to use, Predictor.Save() method is implemented. The save method will take the name of the file where you want to save the Predictor instance to and the Predictor instance as the input arguments. When somebody invokes the Predictor.Save() method and provides a file name. The method will create a stream writer from the file name and call the Predictor.Serialize() method to serialize the Predictor instance. The code below shows how the Predictor.Save() method implemented:

```
    public static void Save(object obj, string fileName)
    {
        if (obj is Predictor predictor)
        {
            HtmSerializer.Reset();
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                predictor.Serialize(obj, null, sw);
                //predictor.Serialize(sw);
            }
        }
    }
```
- Predictor is the output of the multiSequences training method which includes connections, cortexLayer, and the HtmClassifier instances. 
- By using predictor.connections we can get the connections from the predictor. Serialize method which is already implemented within the connections class can be then call to serialize the object of connections class. Serialize method of connections have three input arguments: connections.Serialize( object obj, string name, StreamWriter sw).
```
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        if (obj is Connections connections)
        {
            HtmSerializer ser = new HtmSerializer();

            var ignoreMembers = new List<string>
            {
                nameof(Connections.m_ActiveCells),
                nameof(Connections.winnerCells),
                nameof(Connections.memory),
                nameof(Connections.m_HtmConfig),
                nameof(Connections.m_TieBreaker),
                nameof(Connections.m_BoostedmOverlaps),
                nameof(Connections.m_Overlaps),
                nameof(Connections.m_BoostFactors),
                nameof(Connections.m_ActiveSegments),
                nameof(Connections.m_MatchingSegments),
                nameof(Connections.ActiveCells),
                nameof(Connections.WinnerCells),
                nameof(Connections.m_SegmentForFlatIdx),
                nameof(Connections.Cells),
                nameof(Connections.m_PredictiveCells)

                //nameof(Connections.Cells)
            };
            ser.SerializeBegin(nameof(Connections), sw);

            HtmSerializer.SerializeObject(connections, name, sw, ignoreMembers);
            var cells = connections.GetColumns().SelectMany(c => c.Cells).ToList();
            HtmSerializer.Serialize(cells, "cellsList", sw);

            var ddSynapses = cells.SelectMany(c => c.DistalDendrites).SelectMany(dd => dd.Synapses).ToList();
            var cellSynapses = cells.SelectMany(c => c.ReceptorSynapses).ToList();
            var synapses = ddSynapses.Union(cellSynapses).ToList();

            HtmSerializer.Serialize(synapses, "synapsesList", sw);

            var activeCellIds = connections.ActiveCells.Select(c => c.Index).ToList();
            HtmSerializer.Serialize(activeCellIds, "activeCellIds", sw);

            var winnerCellIds = connections.WinnerCells.Select(c => c.Index).ToList();
            HtmSerializer.Serialize(winnerCellIds, "winnerCellIds", sw);

            var predictiveCellIds = connections.m_PredictiveCells.Select(c => c.Index).ToList();
            HtmSerializer.Serialize(predictiveCellIds, "predictiveCellIds", sw);

            ser.SerializeEnd(nameof(Connections), sw);
        }
    }
```
- Now to serialize the Cortex layer, we have to implement the Serialize method for the Cortexlayer which have three layers inside CortexLayer( encoder, spatialpooler, temporalmemory).The method below shows how to serialize the cortexlayer:
```
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        // Serialize all the HtmModules in the CortexLayer
        if (obj is CortexLayer<object, object> layer)
        {
            HtmSerializer ser = new HtmSerializer();
            foreach (var modulePair in layer.HtmModules)
            {
                ISerializable serializableModule = modulePair.Value as ISerializable;
                string ObjType = serializableModule.GetType().Name;
                if (serializableModule != null)
                {

                    ser.SerializeBegin(ObjType, sw);

                    serializableModule.Serialize(serializableModule, null, sw);

                    ser.SerializeEnd(ObjType, sw);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

```
- Since the ScalarEncoder is inherited the EncoderBase, so that we can use the serialize method which is already implemented for the class EncoderBase to serialize the ScarlarEncoder.
- However, not all the properties of the class EncoderBase are serialized. The method below shows how to serialize the ScalarEncoder instance with in the cortexlayer.
```
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        var excludeMembers = new List<string> 
        { 
            nameof(EncoderBase.Properties),
            nameof(EncoderBase.halfWidth),
            nameof(EncoderBase.rangeInternal),
            nameof(EncoderBase.nInternal),
            nameof(EncoderBase.encLearningEnabled),
            nameof(EncoderBase.flattenedFieldTypeList),
            nameof(EncoderBase.decoderFieldTypes),
            nameof(EncoderBase.topDownValues),
            nameof(EncoderBase.bucketValues),
            nameof(EncoderBase.topDownMapping),
            nameof(EncoderBase.IsForced),
            nameof(EncoderBase.Offset),
        };
        HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers: excludeMembers);
    }
```
- To serialize the Spatial Pooler instance in the CortexLayer, we used the Serialize method, which is implemented within the class Spatial Pooler:
```
public void Serialize(object obj, string name, StreamWriter sw)
{
        HtmSerializer.SerializeObject(obj, name, sw);
}
```
- To serialize the Temporal Memory instance in the CortexLayer, we used the Serialize method, which is implemented within the class Temporal Memory:
```
public void Serialize(object obj, string name, StreamWriter sw)
{
	var ignoreMembers = new List<string>
	{
	    //nameof(TemporalMemory.connections)
	};
	HtmSerializer.SerializeObject(obj, null, sw, ignoreMembers);  
}
```

- After all the layers of the CortexLayer are serialized, we then have to serialize the HtmClassifier.

```
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        //Serialization code below.

        HtmSerializer ser = new HtmSerializer();
        ser.SerializeBegin(nameof(HtmClassifier<TIN, TOUT>), sw);
        ser.SerializeValue(maxRecordedElements, sw);
        ser.SerializeDictionaryValue(m_AllInputs, sw);
        ser.SerializeEnd(nameof(HtmClassifier<TIN, TOUT>), sw);
    }
```
2. Implement De-serialize method for class Predictor. 
- Up to the current state of this project the objects of classes Connections, SpatialPooler, TemporalMemory, HtmClassifier are serialized to different text files. 
- Now we need to retrieve those objects from the text files (deserialize methods). The goal is to implement the Deserialize method for the class Predictor that can retrieves its objects from the text files. Below method is used to deserialize the predictor class

```
    public static object Deserialize<T>(StreamReader sr, string name)
    {
        .......
    }
```
- Firstly, all the values are initialized inside the deserialize method:

```
    HtmSerializer ser = new HtmSerializer();
    // Initialize the Predictor
    Predictor predictor = new Predictor(null, null, null);
    // Initialize the CortexLayer
    CortexLayer<object, object> layer = new CortexLayer<object, object>("L1");

    // Add SP and TM objects to CortexLayer, initialize the values (null) 
    layer.HtmModules.Add("encoder", (ScalarEncoder)null);
    layer.HtmModules.Add("sp", (SpatialPoolerMT)null);
    layer.HtmModules.Add("tm", (TemporalMemory)null);
```
- The connections can be easily de-serialize by calling the method Deserialize of class Connections:

```
// Deserialize Connections object 
    if (data == ser.ReadBegin(nameof(Connections)) && (predictor.connections == null))
    {
        var con = Connections.Deserialize<Connections>(sr, null);
        if (con is Connections connections)
        {
            predictor.connections = connections;
        }

        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);

    }
```
- The Deserialize method which is implemented in the class Connections is defined as below:
	
```
    public static object Deserialize<T>(StreamReader sr, string name)
    {
        var ignoreMembers = new List<string>
        {
            "activeCellIds",
            "winnerCellIds",
            "synapsesList",
            "cellsList",
            "predictiveCellIds"
        };
        var cells = new List<Cell>();
        var conn = HtmSerializer.DeserializeObject<Connections>(sr, name, ignoreMembers, (conn, propName) =>
        {
            if (propName == "cellsList")
            {
                cells = HtmSerializer.Deserialize<List<Cell>>(sr, "cellsList");
            }
            else if (propName == "activeCellIds")
            {
                var activeCellIds = HtmSerializer.Deserialize<List<int>>(sr, "activeCellIds");
                foreach (var cellId in activeCellIds)
                {
                    var cell = cells.FirstOrDefault(c => c.Index == cellId);
                    if (cell != null)
                        conn.ActiveCells.Add(cell);
                }
            }
            else if (propName == "winnerCellIds")
            {
                var winnerCellIds = HtmSerializer.Deserialize<List<int>>(sr, "winnerCellIds");
                foreach (var cellId in winnerCellIds)
                {
                    var cell = cells.FirstOrDefault(c => c.Index == cellId);
                    if (cell != null)
                        conn.WinnerCells.Add(cell);
                }
            }
            else if (propName == "predictiveCellIds")
            {
                var predictiveCellIds = HtmSerializer.Deserialize<List<int>>(sr, "predictiveCellIds");
                foreach (var cellId in predictiveCellIds)
                {
                    var cell = cells.FirstOrDefault(c => c.Index == cellId);
                    if (cell != null)
                        conn.m_PredictiveCells.Add(cell);
                }
            }
            else if (propName == "synapsesList")
            {
                var synapses = HtmSerializer.Deserialize<List<Synapse>>(sr, "synapsesList");
                foreach (var synapse in synapses)
                {
                    synapse.SourceCell = cells.FirstOrDefault(c => c.Index == synapse.InputIndex);
                }

                foreach (var cell in cells)
                {
                    cell.ReceptorSynapses = synapses.Where(s => s.InputIndex == cell.Index).ToList();

                    foreach (var distalDendrite in cell.DistalDendrites)
                    {
                        distalDendrite.Synapses = synapses.Where(s => s.SegmentIndex == distalDendrite.SegmentIndex).ToList();
                    }
                }

                if (conn.Memory != null)
                {

                    var columnIndexes = conn.Memory.GetSparseIndices();

                    var columns = new List<Column>();

                    foreach (var index in columnIndexes)
                    {
                        var col = conn.Memory.GetColumn(index);
                        if (col != null)
                            columns.Add(col);
                    }
                    foreach (var column in columns)
                    {
                        column.Cells = cells.Where(c => c.ParentColumnIndex == column.Index).ToArray();
                    }

                    var distalDendrites = cells.SelectMany(c => c.DistalDendrites).Distinct().OrderBy(dd => dd.SegmentIndex);
                    conn.m_SegmentForFlatIdx = new ConcurrentDictionary<int, DistalDendrite>();
                    foreach (var distalDendrite in distalDendrites)
                    {
                        conn.m_SegmentForFlatIdx.TryAdd(distalDendrite.SegmentIndex, distalDendrite);
                    }
                }
            }
        });

        //var cells = new List<Cell>();
        //for (int i = 0; i < conn.Memory.GetMaxIndex(); i++)
        //{
        //    cells.AddRange(conn.memory.GetColumn(i).Cells);
        //}
        //conn.Cells = cells.ToArray();
        return conn;
    }
```
- The second object of class Predictor is the CortexLayer that means we need to implement the Deserialize method for the class CortexLayer that can deserialize all of the layers with in the CortexLayer.
- The encoder can be deserialize by calling the Deserialize method which is implemented in the class EncoderBase. 

```
		var encoder = ScalarEncoder.Deserialize<ScalarEncoder>(sr_en, null);
```

- The EncoderBase.Deserialize method is defined:

```
    public static object Deserialize<T>(StreamReader sr, string name)
    {
        var excludeMembers = new List<string> { nameof(EncoderBase.Properties) };
        var obj = HtmSerializer.DeserializeObject<T>(sr, name, excludeMembers);

        var en = obj as EncoderBase;

        if (en == null) { 
            return obj; 
        }

        return en;
    }
```

- The SpatialPooler object can be deserialize by calling the Deserialize method which is implemented in the class SpatialPooler.

```
    // Deserialize Spatial Pooler object
    if (data == ser.ReadBegin(nameof(SpatialPoolerMT)) && (layer.HtmModules["sp"] == null))
    {
        var sp = SpatialPooler.Deserialize<SpatialPoolerMT>(sr, null);
        layer.HtmModules["sp"] = (SpatialPoolerMT)sp;

        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
    }
```
- The SpatialPooler.Deserialize method is defined:

```
    public static object Deserialize<T>(StreamReader sr, string propName)
    {
        var obj = HtmSerializer.DeserializeObject<T>(sr, propName);

        var sp = obj as SpatialPooler;
        if (sp == null)
            return obj;
        //sp.m_HomeoPlastAct.SetConnections(sp.connections);
        return sp;
    }
```

- The TemporalMemory object can be retrieved by calling the Deserialize method which is implememted in the class TemporalMemory.

```
    // Deserialize Temporal Memory object
    if (data == ser.ReadBegin(nameof(TemporalMemory)) && (layer.HtmModules["tm"] == null))
    {
        var tm = TemporalMemory.Deserialize<TemporalMemory>(sr, null);
        layer.HtmModules["tm"] = (TemporalMemory)tm;

        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
    }
```

- The TemporalMemory deserialize method is defined as below:

```
    public static object Deserialize<T>(StreamReader sr, string name)
    {
        return HtmSerializer.DeserializeObject<T>(sr, name);
    }
```
- The instance of class HtmClassifier should be deserialize by calling the Deserialize method which is implemented in the class HtmClassifier.

```
    // Deserialize the HtmClassifier object
    if (data == ser.ReadBegin(nameof(HtmClassifier<string, ComputeCycle>)) && (predictor.classifier == null))
    {
        var cls = HtmClassifier<string, ComputeCycle>.Deserialize<HtmClassifier<string, ComputeCycle>>(sr, null);
        predictor.classifier = (HtmClassifier<string, ComputeCycle>)cls;

        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
    }
```
- The HtmClassifier deserialize method is defined as below:

```
    public static object Deserialize<T>(StreamReader sr, string name)
    {

        // Create a new HtmSerializer and HtmClassifier
        HtmSerializer ser = new HtmSerializer();
        HtmClassifier<TIN, TOUT> cls = new HtmClassifier<TIN, TOUT>();

        // Read the input stream line by line
        while (!sr.EndOfStream)
        {
            // Read the current line
            string data = sr.ReadLine();

            // Skip empty lines and the beginning and end of the HtmClassifier
            if (string.IsNullOrEmpty(data))
                continue;

            if (data == ser.ReadBegin(nameof(HtmClassifier<TIN, TOUT>)))
                continue;

            if (data == ser.ReadEnd(nameof(HtmClassifier<TIN, TOUT>)))
                break;

            // If the line contains a key-value pair, deserialize it
            if (data.Contains(HtmSerializer.KeyValueDelimiter))
            {

                var kvp = ser.ReadDictSIarrayList<TIN>(cls.m_AllInputs, data);
                cls.m_AllInputs = kvp;

            }
            // Otherwise, parse the parameters in the line and set them in the HtmClassifier
            else
            {
                // Split the line into its parameters
                string[] str = data.Split(HtmSerializer.ParameterDelimiter);


                // Skip lines with no parameters
                foreach (string value in str)
                {
                    String.IsNullOrWhiteSpace(value);
                    continue;
                }


                // If the first parameter is an integer, set it as the maxRecordedElements property
                if (int.TryParse(str[0], out int maxRecordedElements))
                    cls.maxRecordedElements = maxRecordedElements;
            }
        }

        // Return the deserialized HtmClassifier
        return cls;
    }
```
- The Deserialize method which is implemented in the class CortexLayer will then return the instance of class CortexLayer.

- On the other hand, Predictor.Load() method is also implemented which is used to create a stream reader for the Predictor.Deserialize() method. The input argument for the Load() method is the file name where the predictor was saved.
```
    public static T Load<T>(string fileName)
    {
        HtmSerializer.Reset();
        using StreamReader sr = new StreamReader(fileName);
        return (T)Deserialize<T>(sr, null);
    }
```
3. Multisequence Learning and comparison between normal Predictor and Serialized Predictor.
- For the training, two sequences S1 and S2, containing scaler values are defined and learned and further used to predict the next element from the sequence.
```
    private static void RunMultiSequenceSerializationExperiment()
    {
        Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

        sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
        sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));
``` 
- For learning of sequences, MultiSequenceLearning.Run() method is called from the MultiSequenceLearning class in program.cs, which returns the object of Predictor class.
```
    // Prototype for building the prediction engine.
    MultiSequenceLearning experiment = new MultiSequenceLearning();
    Predictor serializedPredictor;
    var predictor = experiment.Run(sequences, out serializedPredictor, "predictor");
```
- The Run() method also returns the MultiSequenceLearning.RunExperiment() method which finally returns the instance of Predictor.
- For testing, we defined two instances of class Predictor i.e., "predictor" and "serializedPredictor". The "predictor" instance acts as a normal predictor which predicts the next element without being serialized and deserialized. However, "serializedPredictor" is the instance of class Predictor which is the result after serialization and deserialization of Predictor.
```
    public Predictor Run(Dictionary<string, List<double>> sequences, out Predictor serializedPredictor, string fileName)
    {
        .......
        
        return RunExperiment(inputBits, cfg, encoder, sequences, out serializedPredictor, fileName);
    }
    private Predictor RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, Dictionary<string, List<double>> sequences, out Predictor serializedPredictor, string fileName)
    {
        .......
        // The "predictor" is the instance of class Predictor which is result after learning. This "predictor" object later on put in the argument of Save() method for serialization.
        // The "serializedPredictor" is the instance of Predictor class which is the result after serialization and deserialization of Predictor. 
        var predictor = new Predictor(layer1, mem, cls);
        
        //Save() method is callled from Predictor Class, which serialize the instance of Predictor Class.
        Predictor.Save(predictor, fileName);

        //Load() method is callled from Predictor Class, which deserialize the instance of Predictor Class.
        serializedPredictor = Predictor.Load<Predictor>(fileName);
        return predictor;
    }
```
- After learning these two sequences, three sequence lists are defined to check how the prediction works. 
```
    // These list are used to see how the prediction works.
    // Predictor is traversing the list element by element
    // By providing more elements to the prediction, the predictor delivers more precise result.
    var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
    var list2 = new double[] { 2.0, 3.0, 4.0 };
    var list3 = new double[] { 8.0, 1.0, 2.0 };
```
- The Program.PredictNextElement() method predicts the next element by transversing the given list element by element. The PredictNextElement() method is defined as below:
```
    private static void PredictNextElement(Predictor predictor, double[] list)
    {
        Debug.WriteLine("------------------------------");

        foreach (var item in list)
        {
            var res = predictor.Predict(item);

            if (res.Count > 0)
            {
                foreach (var pred in res)
                {
                    Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                }

                var tokens = res.First().PredictedInput.Split('_');
                var tokens2 = res.First().PredictedInput.Split('-');
                Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                Console.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}\n");
            }
            else
                Debug.WriteLine("Nothing predicted :(");
        }

        Debug.WriteLine("------------------------------");
    }
```
- Now, we compare the prediction output from "predictor" and "serializedPredictor" instance and the result must be same to test that serialization and deserialization for instance of Predictor class is correct. 
- As described below in the code, the "predictor" and "serializedPredictor are used as the argument in PredictNextElement() method, then it predict the next element for normal predictor and the serialized predictor respectively. Here, the next element for list2 is predicted and the result from both the normal predictor and the serialized predictor should be same.
```
// The "predictor" is the instance of class Predictor which is result after learning. The PredictNextElement() method will predict the next element.
predictor.Reset();
Console.WriteLine("Prediction next elements with normal predictor \n");

// The "predictor" instance is used as an arugment in the PredictNextElement() method means it is a normal predictor and it predicts the next element without being serialized and deserialized.
PredictNextElement(predictor, list2);

// The "serializedPredictor" is the instance of Predictor class which is the result after serialization and deserialization of Predictor.
serializedPredictor.Reset();
Console.WriteLine("Prediction next elements with serialized predictor \n");

// The "serializedPredictor" instance is used as an arugment in the PredictNextElement() method, then it predict the next element after the predictor instance is being serialized and deserialized.
PredictNextElement(serializedPredictor, list2);
```
- Below shows the predicted output which we got from the normal predictor and the serialized Predictor respectively. As shown below, the predicted outputs for next elements are 5, 4 and 2 which is same for both the normal predictor and the serialized Predictor.
```
Hello NeocortexApi! Experiment MultiSequenceLearning
Prediction next elements with normal predictor

S1_0-1-2-3-4-2-5 - 33,33
S2_10-7-11-8-1-2-9 - 33,33
S1_4-2-5-0-1-2-3 - 1,67
Predicted Sequence: S1, predicted next element 5

S1_-1.0-0-1-2-3-4 - 100
S1_2-5-0-1-2-3-4 - 100
S1_-1.0-0-1-2-3-4-2 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 4

S1_-1.0-0-1-2-3-4-2 - 100
S1_5-0-1-2-3-4-2 - 100
S1_-1.0-0-1-2-3-4 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 2

Prediction next elements with serialized predictor

S1_0-1-2-3-4-2-5 - 33,33
S2_10-7-11-8-1-2-9 - 33,33
S1_4-2-5-0-1-2-3 - 1,67
Predicted Sequence: S1, predicted next element 5

S1_-1.0-0-1-2-3-4 - 100
S1_2-5-0-1-2-3-4 - 100
S1_-1.0-0-1-2-3-4-2 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 4

S1_-1.0-0-1-2-3-4-2 - 100
S1_5-0-1-2-3-4-2 - 100
S1_-1.0-0-1-2-3-4 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 2
```


