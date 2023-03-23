1. Implement Serialize method for class Predictor.
- In order to serialize the Model of MultiSequences Learning, we need to be able to serialize the instance of class Predictor which is the output of the Training method.
- Inside the Predictor we have other three objects that are needed to be serialized. They are connections, layer, and HtmClassifier.
- The objects inside the Predictor should be labeled as ISerializable so that we can use the ISerializable interface to serialize those objects.
- The method which is used to serialize the connection can be called as below:

```
		predictor.connections.Serialize(predictor.connections, null, sw_con);
```

+ Predictor is the output of the multiSequences training method which includes connections, cortexLayer, and the HtmClassifier instances.
+ By using predictor.connections we can get the connections from the predictor.
+ Serialize method which is already implemented within the connections class can be then call to serialize the object of connections class.
+ Serialize method of connections have three input arguments: connections.Serialize( object obj, string name, StreamWriter sw).

```
	public void Serialize(object obj, string name, StreamWriter sw)
        {
            if (obj is Connections connections)
            {

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
            }
        }

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

- From here we have to implement the Serialize method for the Cortexlayer which have three layers inside CortexLayer( encoder, spatialpooler, temporalmemory).
- The encoder used in this project is the ScalarEncoder, we can yield the encoder from the cortexLayer:

```
		var en = layer.HtmModules["encoder"];
```

- Since the ScalarEncoder is inherited the EncoderBase, so that we can use the serialize method which is already implemented for the class EncoderBase to serialize the ScarlarEncoder.
- However, not all the properties of the class EncoderBase are serialized. The method below shows how to serialize the ScalarEncoder instance with in the cortexlayer.

```
	public void Serialize(object obj, string name, StreamWriter sw)
        {
            if (obj is EncoderBase encoder)
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
                // Added properties
                nameof(EncoderBase.IsForced),
                nameof(EncoderBase.IsDelta),
                nameof(EncoderBase.Offset),
                nameof(EncoderBase.Width),
	};

                HtmSerializer.SerializeObject(encoder, name, sw, ignoreMembers: excludeMembers);
            }    
        }
```

- To serialize the SpatialPooler instance in the CortexLayer, we need first to get its value from the layer. The coder below shows how to do that.

```
		var sp = layer.HtmModules["sp"];
```

- The Serialize method is then used to serialize it, which is implemented within the class SpatialPooler:

```
	public void Serialize(object obj, string name, StreamWriter sw)
        {
            HtmSerializer.SerializeObject(obj, name, sw);
        }
```

- Same as other two objects, we have to get the TemporalMemory instance from the CortexLayer:

```
		var tm = (TemporalMemory)layer.HtmModules["tm"];
```

- The serialize method which is implemented in the class TemporalMemory is then called to serialize it:

```
	public void Serialize(object obj, string name, StreamWriter sw)
        {
            var ignoreMembers = new List<string>
            {
                //nameof(TemporalMemory.connections)
            };
            HtmSerializer.SerializeObject(obj, null, sw, ignoreMembers);  
        }

        public static object Deserialize<T>(StreamReader sr, string name)
        {
            return HtmSerializer.DeserializeObject<T>(sr, name);
        }
```

- After all the layers of the CortexLayer are serialized, we then have to serialize the HtmClassifier. (This is the other group's topic)

2. Implement De-serialize method for class Predictor. 
- Up to the current state of this project the objects of classes Connections, Encoder, SpatialPooler, TemporalMemory, HtmClassifier are serialized to different text files. 
- Now we need to retrieve those objects from the text files (deserialize methods). The goal is to implement the Deserialize method for the class Predictor that can retrieves its objects from the text files.
- Firstly, the connections can be easily de-serialize by calling the method Deserialize of class Connections:

```
		var con = Connections.Deserialize<Connections>(sr_con, null);
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
 
- The second object of class Predictor is the CortexLayer that means we need to implement the Deserialize method for the class CortexLayer that can serialize all of the layers with in the CortexLayer.
- The encoder can be deserialize by calling the Deserialize method which is implemented in the class EncoderBase. 

```
		var encoder = ScalarEncoder.Deserialize<ScalarEncoder>(sr_en, null);
```

- The EncoderBase.Deserialize method is defined:

```
	public static object Deserialize<T>(StreamReader sr, string name)
        {
            var excludeMembers = new List<string> { nameof(EncoderBase.Properties) };
            return HtmSerializer.DeserializeObject<T>(sr, name, excludeMembers);
        }
```

- The SpatialPooler object can be deserialize by calling the Deserialize method which is implemented in the class SpatialPooler.

```
		var sp = SpatialPooler.Deserialize<SpatialPooler>(sr_sp, null);
```

- The SpatialPooler.Serialize method is defined:

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
		var tm = TemporalMemory.Deserialize<TemporalMemoryMT>(sr_tm, null);
```

- The TemporalMemory.Deserialize method is defined as below:

```
	public static object Deserialize<T>(StreamReader sr, string name)
        {
            return HtmSerializer.DeserializeObject<T>(sr, name);
        }
```

- After we have all the layers of the CortexLayer, we need to reconstruct the CortexLayer
- First the CortexLayer could be initialized:

```
		CortexLayer<object, object> layer = new CortexLayer<object, object>("L");
```

- Then we can in turn add the layers to the CortexLayer as below:

```
	layer.HtmModules.Add("encoder", (ScalarEncoder)encoder);
        layer.HtmModules.Add("sp", (SpatialPooler)sp);
        layer.HtmModules.Add("tm", (TemporalMemory)tm);
```

- The Deserialize method which is implemented in the class CortexLayer will then return the instance of class CortexLayer which contains the three object that have been retrieved from the text files.
- The instance of class HtmClassifier should be deserialize by calling the Deserialize method which is implemented in the class HtmClassifier. However, it is the other group's topic so we will update it later.
- After, we retrieved all the objects of class Predictor, we can reconstruct it. The code below is then called to do so:

```
		Predictor predictor = new Predictor((CortexLayer<object, object>)layer, (Connections)con, (HtmClassifier) cls);
            	return predictor;
```

- The predictor is then returned, so that we could use it for the next training.