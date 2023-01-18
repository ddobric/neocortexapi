1. Prepare input values for the HTM system by randomly choose from the sequence of integer.
	- List<double> values is created. Since the max value is set to 20, the list values will contain a sequence of integer values from 0 to 19 [0,1,2,...,19]
	- Add method is used to add the new object to the end of the list. Hence, the size of the list is then increased by 1. 
--------------------------------------------------------------------------
	List<double> values = new List<double>();
        int max = 20;
        for (int i = 0; i < max; i++)
        {
            values.Add(i);
        }
--------------------------------------------------------------------------
	- seed is a random number used to calculate a starting value for the pseudo-random number sequence.
	- random is new instance of the Random class, which is initialized with a specified seed value(42).
	- List<double> inputValues is then created, it contains random numbers which are selected from the List values. The size of the List inputValues is half of the size of the List values.
--------------------------------------------------------------------------
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
----------------------------------------------------------------------------
	- Retrieve objects from the List values that do not exist in the list inputValues to create another list testValues. Hence, we have two different random input sequences.
	- These two random input sequences will be in turn used to train the HTM model. 
----------------------------------------------------------------------------  
	List<double> testValues = values.Except(inputValues).ToList();
----------------------------------------------------------------------------


2. Train the model with the first inputValues set.
 










