Spatial Pooler Learning Analysis

I. Train method

1. The method output is CortexLayer instance, inputs are Maxval for the encoder and the input set for training. 

    ```
		public CortexLayer<object, object> Train(double max, List<double> inputValues)
    ```

2. Create new instance of htm
    ```
        // Pre-defined parameters for htm configuration
		    double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
		    int inputBits = 200;
		    int numColumns = 2048;
   
        // htm configuration parameters
    
		    HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,

                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,
            };

        // Create htm memory
            var mem = new Connections(cfg);
    ```

3. Create new instance of Encoder class

    ```       
        // Dictionary defines typical encoder parameters

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
    
        // Create new instance of ScalarEncoder

            EncoderBase encoder = new ScalarEncoder(settings);
    ```

4. Homeostatic Plasticity Controller 
    
    - HPC extends the default Spatial Pooler algorithm.
    - HPC is to set the Spatial Pooler is in the new-born stage at the begining of the training. 
    - In this stage, the boosting is very active, but the SP behaves instable. After this stage, the hpc will control the learning process of the SP.
    - Once the SDR generated for every input gets stable, the HPC will fire event that notifies your program.
         
    ```  
        // the Sp is first set to unstable 
            
            bool isInStableState = false;
     
        // Create new instance of HomeostaticPlasticityController
            
            hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });
    ```

5. Create instance of Spatial Pooler Multithread version

    ```    
        SpatialPooler sp = new SpatialPooler(hpa);

        // Initializes the SP instance 

        sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });
     ```

6. Create instance of NeoCortexLayer 

     ``` 
        // All the algorithms will be performed within this layer

        CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

        // Add encoder as the very first module. This model is connected to the sensory input cells that receives the input
        // The encoder will receive the input and forward the encoded input to the next module.

        cortexLayer.HtmModules.Add("encoder", encoder);

        // The next module in the layer is Spatial Pooler. This module will receive the encoded signal which is the output of encoder module.

        cortexLayer.HtmModules.Add("sp", sp);
     ```

7. Convert input values from list to array

    ```
        double[] inputs = inputValues.ToArray();
    ```

8. SDR inputs and SDR similarities

    ```   
        // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

        // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();
        
        // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0); 
                prevActiveCols.Add(input, new int[0]);
            }
    ```

9. Learning process

    - At the end, the NeocortexLayer model is returned.

    ```
        // Learning process will take 1000 iterations (cycles)

            int maxSPLearningCycles = 1000;
    
        // SP learning cycles

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

                //
                // This trains the layer on input pattern.
                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

                if (isInStableState)
                    break;
            }

            return cortexLayer;
        }
    ```

II. HtmSerializer.Save() method is used for write the trained model to a text file

1. Save method is implemented to save the trained model to a text file using StreamWriter

    - fileName is the name of the file where the model will be saved to.
    - The input object of the Save method is the model need to be saved.
    - New instance of StreamWriter sw will be created to write the parameters of the model to the file (fileName).
    - Htm.Serialize method will be called to serialize the model. 

    ```
        public static void Save(string fileName, object obj)
        {
            Reset();
            using StreamWriter sw = new StreamWriter(fileName);
            Serialize(obj, null, sw);
        }
    ```

2. HtmSerializer.Serialize method analysis

    - Inputs: NeoCortexLayer model, name of parameters (null), stream writer sw.
    - SerializeBegin method is first called.
    - Depend on the type of the objects, corresponding methods will be called.
        + SerializeValue() // If Object is a value.
        + SerializeDictionary() // If Object is a Dictionary
        + SerializeIEnumerable() // If object is a List
        + SerializeKeyValuePair() // If object is a generic type
        + SerializeObject() // If object is a class   
    - When the serialization is finished SerializeEnd method is call. 

    ```
        private static void SerializeBegin(string propName, StreamWriter sw, Type type)
        {
            List<string> list = new List<string> { "Begin" };
            if (!string.IsNullOrEmpty(propName))
            {
                list.Add(propName);
            }

            if (type != null)
            {
                list.Add(type.FullName!.Replace(" ", ""));
            }

            sw.WriteLine(string.Join(' ', list));
        }

    ```

    ```
        public static void Serialize(object obj, string name, StreamWriter sw, Type propertyType = null, List<string> ignoreMembers = null)
        {
            if (obj == null)
            {
                return;
            }

            if (name == "ParentCell")
            {
            }

            Type type = obj.GetType();
            bool flag = propertyType != null && (propertyType.IsInterface || propertyType.IsAbstract || propertyType != type);
            SerializeBegin(name, sw, flag ? type : null);
            int hashCode = obj.GetHashCode();
            if (type.GetInterfaces().FirstOrDefault((Type i) => i.FullName!.Equals(typeof(ISerializable)!.FullName)) != null)
            {
                if (SerializedHashCodes.TryGetValue(obj, out var value))
                {
                    Serialize(value, "ReplaceId", sw);
                }
                else
                {
                    Serialize(Id, "Id", sw);
                    SerializedHashCodes.Add(obj, Id++);
                    (obj as ISerializable).Serialize(obj, name, sw);
                }
            }
            else if (type.IsPrimitive || type == typeof(string))
            {
                SerializeValue(name, obj, sw);
            }
            else if (IsDictionary(type))
            {
                SerializeDictionary(name, obj, sw, ignoreMembers);
            }
            else if (IsList(type))
            {
                SerializeIEnumerable(name, obj, sw, ignoreMembers);
            }
            else if (type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                SerializeKeyValuePair(name, obj, sw);
            }
            else if (type.IsClass)
            {
                if (SerializedHashCodes.TryGetValue(obj, out var value2))
                {
                    Serialize(value2, "ReplaceId", sw);
                }
                else
                {
                    Serialize(Id, "Id", sw);
                    SerializedHashCodes.Add(obj, Id++);
                    SerializeObject(obj, name, sw, ignoreMembers);
                }
            }
    ```

    ```
        private static void SerializeEnd(string propName, StreamWriter sw, Type type)
        {
            sw.WriteLine();
            List<string> list = new List<string> { "End" };
            if (!string.IsNullOrEmpty(propName))
            {
                list.Add(propName);
            }

            if (type != null)
            {
                list.Add(type.FullName!.Replace(" ", ""));
            }

            sw.WriteLine(string.Join(' ', list));
        }
    ```

III. HtmSerializer.Load() method is used for deserialize the trained model from the text file

    - fileName is the file where the model is deserialized.
    - create new instance of StreamReader sr.
    - Deserialize<T>() method is then called.

    ```
        public static T Load<T>(string fileName)
        {
            Reset();
            using StreamReader sr = new StreamReader(fileName);
            return Deserialize<T>(sr);
        }

    ```

    - typeFromHandle is the type of <T>.
    - list contains the all the public methods of the current type. 
    - If the type is value then DeserializeValue<T>() method is called.
    - If the type is Dictionary then DeserializeDictionary<T>() method is called.
    - If the type is List then DeserializeIEnumerable<T>() method is called.
    - If the type is generic type then DeserializeKeyValuePair<T>() method is called.
    - If the type is class then DeserializeObject<T>() is called. 

    ```
        public static T Deserialize<T>(StreamReader sr, string propName = null)
        {
            T val = default(T);
            Type typeFromHandle = typeof(T);
            if (typeFromHandle.GetInterfaces().FirstOrDefault((Type i) => i.FullName!.Equals(typeof(ISerializable)!.FullName)) != null)
            {
                List<MethodInfo> list = typeFromHandle.GetMethods().ToList();
                if (typeFromHandle.BaseType != null)
                {
                    list.AddRange(typeFromHandle.BaseType!.GetMethods());
                }

                MethodInfo methodInfo = list.FirstOrDefault((MethodInfo m) => m.Name == "Deserialize" && m.IsStatic && m.GetParameters().Length == 2);
                if (methodInfo == null)
                {
                    throw new NotImplementedException("Deserialize method is not implemented in the target type " + typeFromHandle.Name);
                }

                return (T)methodInfo.MakeGenericMethod(typeFromHandle).Invoke(null, new object[2] { sr, propName });
            }

            return IsValueType(typeFromHandle) ? DeserializeValue<T>(sr, propName) : (IsDictionary(typeFromHandle) ? DeserializeDictionary<T>(sr, propName) : (IsList(typeFromHandle) ? DeserializeIEnumerable<T>(sr, propName) : ((!typeFromHandle.IsGenericType || !(typeFromHandle.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))) ? DeserializeObject<T>(sr, propName) : DeserializeKeyValuePair<T>(sr, propName))));
        }

    ```
