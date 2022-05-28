## Test Spatial Pooler(SP) Consistency  
Project Invariant Learning

### 1. Context:
Current approach in Invariant representation learning project tried to see if multiple Spatial Pooler can learn patterns in the same way.  
This means for example we can have multiple Spatial Pooler learning the same pattern, and expect the generated SDRs to appear the same.
The application for the test will be dividing an image into multiple frame projections (pixels value in a rectangualar area inside the image) and let multiple SP learns them.

### 2. Result:
The SP behaves differently even with same random seed given.  
[This test](https://github.com/ddobric/neocortexapi/blob/SPConsistency/source/SPTestForInvLearning/SpatialConsistency.cs) use 2 SP to learn an input until they reach stable state.  

During learning, the SDRs were recorded under OutputFolder/  
![outputFolder](assets/correlation.png)

The SDRs lists are also compared via correlation and save under correlation.txt.  
Visualizing one correlation.txt in Excel gives the following result:  
![result](assets/result.png)  

[Another test](https://github.com/ddobric/neocortexapi/blob/SPConsistency/source/SPTestForInvLearning/RandomTest.cs) was conducted on ThreadSafeRandom. This concluded that Random object with the same seed cannot create same patterns.

### 3. Conclusion:
Although multiple SP approach for all layer cannot be used, we can still think about using only 1 SP in the first layer. Then the next layer may used its output to process something.

### 4. Discussion:
Although there are a lot of research and direction for dealing with invariant repreentation learning. I don't think that Invariant learning is that complicated, and also not as simple.  

Thought experiment 1:  
Do we learn things invariantly?  
- imagine having yourself in a pitch black room, there is only 1 screen and you are watching it, it is everything you can see/perceive.  
- now there is a symbol on the screen and it is moving around.  

If we learn things invariantly, your eyes wouldn't have diverted to the symbol. The eye is great, but we must also be aware that it can move, so it does not only deliver input from lightrays, it is also a feedback mechanism to allign the input via rolling and changing focal.
Why does the eye move if we can learn things invariantly?  
I think that the brain don't learn things invariantly. When learning, the eye use focus and fixed view on object to learn it completely. When inferring, the eye moves to allign the object to fit the trained infos that it got from ealier. These infos are only neccessary for the brain to distinguish the object from the others in the training environment. This leads to the fact that the we can learn "features" from an object, like an edge/small part/e.g. which leads to the thought that we learned things invariantly.  
Thought experiment 2:  
Importance of focus:
if I focus on a point 2 cm away from any text, I will not be able to read it.  

**Preferred training model**
Problem: how to design a tracking system, which we use to allign the object into a familiar learned oobject?  
