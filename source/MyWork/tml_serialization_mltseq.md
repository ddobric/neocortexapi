1. Prepare input values for the HTM system by randomly choose from the sequence of integer.

	- List<double> values is created. Since the max value is set to 20, the list values will contain a sequence of integer values from 0 to 19 [0,1,2,...,19]
	- Add method is used to add the new object to the end of the list. Hence, the size of the list is then increased by 1. 
    - seed is a random number used to calculate a starting value for the pseudo-random number sequence.
	- random is new instance of the Random class, which is initialized with a specified seed value(42).
	- List<double> inputValues is then created, it contains random numbers which are selected from the List values. The size of the List inputValues is half of the size of the List values.
    - Retrieve objects from the List values that do not exist in the list inputValues to create another list testValues. Hence, we have two different random input sequences.
	- These two random input sequences will be in turn used to train the HTM model. 

```
	List<double> values = new List<double>();
        int max = 20;
        for (int i = 0; i < max; i++)
        {
            values.Add(i);
        }
```

```
	int seed = 42;
        var random = new Random(seed);

        List<double> inputValues = new List<double>();
        for (int i = 0; i < max * 0.5; i++) // The size of the list inputValues is half of the list values( max * 0.5). 
        {
            var index = random.Next(0, max);
            while (inputValues.Contains(values[index])) // When an object of the list values is already there in the list inputValues, it'll look for another value. 
            {
                index = random.Next(0, max);
            }

            inputValues.Add(values[index]); // When it's a new object, it'll be added to the list inputValues.
        }
```

``` 
	List<double> testValues = values.Except(inputValues).ToList();
```


2. Train the model with the first inputValues set.

    - Set a name for the file where we are gonna save the first trained model( Model1.txt).
    - Set a name for file used to save SpatialPooler results of the first trained model( Model1trace.tx).
    - Create a new instance of class CortexLayer for the first model (model1).
    - Call TryLoad method of the class HtmSerializer. This method will check out if the filename already exists, and defines default values for CortexLayer model. Otherwise, it will load the existing file with the Load method.
    - In case the file doest not exist, it will train the model. 
    - New instance of SpatialPatternLearning is then created( experiment).
    - The Train method is called to train the model for SpatialpatternLearning. The input parameter max( Max_Val) is defined for the encoder, and inputValues is the list of integer values that is used for the first training.
    - The SpatialPooler results will be stored in "sp".
    - After the first traing, the trained model will be save in the file model1Name using StreamWritter.
    - sp1 will then be assigned the SpatialPooler results from the last training.
    - the SpatialPooler persistence value of every column then stored in the file model1Trace.

 ```
        public static bool TryLoad<T>(string fileName, out T obj)
        {
            if (!File.Exists(fileName))
            {
                obj = default(T);
                return false;
            }

            obj = Load<T>(fileName);
            return true;
        }
 ```

 ```
         public static void Save(string fileName, object obj)
        {
            Reset();
            using StreamWriter sw = new StreamWriter(fileName);
            Serialize(obj, null, sw);
        }
```

```
        var model1Name = "Model1.txt";
        var model1Trace = "Model1trace.txt";
        CortexLayer<object, object> model1;
        if (HtmSerializer.TryLoad(model1Name, out model1) == false)
        {
            var experiment = new SpatialPatternLearning();
            model1 = experiment.Train(max, inputValues);

            // persist the state of the model.
            HtmSerializer.Save(model1Name, model1);
        }
        var sp1 = (SpatialPooler)model1.HtmModules["sp"];

        // Trace the persistence value of every column.
        sp1.TraceColumnPermenances(model1Trace);
```

3. Train the model with the second inputValues set.

    - Set the name of the file to save the second trained model( Model2.txt).
    - Set the name of the file to save the SpatialPooler results( Model2trace.txt).
    - Create new instance of class CortexLayer( model2).
    - Try to load the file model2Name to see if it does exist. If the file is not found or overwrite mode is on (true), load the first trained model to the second model(Deserialization).
    - Train the second model with the new input sequences( testValues). The other input parameters for the train method are the maxCycle (1000), and the SpatialPooler results from the last training("sp").
    - Save the second trained model in the file model2Name( Serialization).
    - sp2 will then be assigned the SpatialPooler results from the second training.
    - the SpatialPooler persistence value of every column then stored in the file model2Trace.

```
        public static CortexLayer<object, object> Train(this CortexLayer<object, object> model, List<double> inputs, int maxCycle, string spName)
```

```
        // Recreate the model from the persisted state and train it with the second set.
        var model2Name = "Model2.txt";
        var model2Trace = "Model2trace.txt";
        CortexLayer<object, object> model2;
        if (HtmSerializer.TryLoad(model2Name, out model2) == false || overwrite)
        {
            model2 = HtmSerializer.Load<CortexLayer<object, object>>(model1Name);
            model2.Train(testValues, 1000, "sp");

            HtmSerializer.Save(model2Name, model2);
        }
        var sp2 = (SpatialPooler)model2.HtmModules["sp"];

        // Trace the persistence value of every column.
        sp2.TraceColumnPermenances(model2Trace);
```






