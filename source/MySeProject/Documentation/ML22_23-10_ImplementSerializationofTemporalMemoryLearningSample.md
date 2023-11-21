1. Implement Serialize method for class Predictor.
- In order to serialize the Model of MultiSequences Learning, we need to be able to serialize the instance of class Predictor which is the output of the Training method. For the training, two sequences containing scaler values are defined and further used to predict the next element from the sequence .
```
    private static void RunMultiSequenceSerializationExperiment()
    {
        Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

        sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
        sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));
``` 
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
- The Predictor.Serialize() method will serialize all the objects in the predictor. It will call the Connections.Serialize(), CortexLayer.Serialize(), and HtmClassifier.Serialize() methods to serialize the connections, layer, and classifier in the Predictor instance respectively.

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
- By using predictor.connections we can get the connections from the predictor. Serialize method which is already implemented within the connections class can be then call to serialize the object of connections class.Serialize method of connections have three input arguments: connections.Serialize( object obj, string name, StreamWriter sw).
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
- The encoder used in this project is the ScalarEncoder, we can yield the encoder from the cortexLayer:
```
	var en = layer.HtmModules["encoder"];
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
        //HtmSerializer ser = new HtmSerializer();
        //ser.SerializeBegin(obj.GetType().Name, sw);
        HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers: excludeMembers);
        //ser.SerializeEnd(obj.GetType().Name, sw);
    }
```
