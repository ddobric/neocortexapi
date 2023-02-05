First iteration: Analyze how to use Serialization, and how to use serialization with Spatial Pooler.

Second iteration: 
	- Analyze ScarlarLearning with Spatial Pooler, and how the serialization is implemented in the example. 
	- Analyze the Training method for ScalarLearning with NeoCortexLayer model, then analyze the HtmSerializer.Save() and HtmSerializer.Load() methods which are used to save and load the trained NeoCortexLayer model.
	- Analyze the Training method for MultiSequenceLearning, specify the inputs and output of the training method, how the training proceeds.
	- Make a comparison between these two approached, identify the differences, consider about whether we can reuse the HtmSerializer.Save() and HtmSerializer.Load() method to serialize and deserialize the trained model for multiSequenceLearning.  
	- First attempt to implement the train method for MultiSequenceLearning, try to use the HtmSerializer.Save() and HtmSerializer.Load() for serialization in multiSequenceLearning.	
	- Questions and conclusion in the current stage:
		+ Is it possible to reuse the HtmSerializer.Save() and HtmSerializer.Load() methods for serialization and deserialization the trained model for MultiSequenceLearning since the input and output of this training is in type of Predictor not NeoCortexLayer?
		+ It would be easier to reuse the HtmSerializer rather than try to create a new class for serializtion since the training model for MultiSequenceLearning is also NeoCortexLayer model. 
		+ Decision: Ask the professor about which approaches should we take before continuing to dig into the solution.  