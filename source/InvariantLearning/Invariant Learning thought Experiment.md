## Invariant Learning thought Experiment

These Experiment test human visual cognition, to raise clue on how recognition of invariant represenation occurs and how can we model it.

In this experiment, the final model to be made will be running on a computer.  
so the source of input for the model will be of images, pixelated. A test on human can be done to measures and invetigates the region of visual perception (RVP) where one can still see object.  
TEST:  
1. stare straight, take an object and move it further out of ones'RVP, mark where it is in space.  
2. take another unknown object and put it in that position, then slightly move it into the stare region.  
NOTICE: it is possible that the object hold can be bias by neuronal potential triggered from haptic. Because of this, object of the same shape but different color can be used. Or the person who put the second object is another one. What should be ensure is the person who is staring doesn't know about the object.  

TAKE:  
Peripheral vision is weak in humans, especially at distinguishing detail, color, and shape. This is because the density of receptor and ganglion cells in the retina is greater at the center and lowest at the edges, and, moreover, the representation in the visual cortex is much smaller than that of the fovea. (wiki - Strasburger, Hans; Rentschler, Ingo; Jüttner, Martin (2011). "Peripheral vision and pattern recognition: A review". )

**So to accurately imitate vision, Do we need to incooperate this regions' differences to the model?**

Because the lack of receptive cells in the outer region, it is suggested that most of the object representation. If learned long enough will have its SDRs located in the neural net behind the foeva region. Because they are activated more.  
When given a thought it is rather easy to understand when related to learning an object through vision. Do we stare into it or just stare somewhere else and put the object in peripheral vision to learn it?  

What to think of:
The training object image for the model doesn't need to be of invariant representations, one can have a tight frame-fit image of the object as training data. This is because the cells anatomy belongs to the input-eye, so we can model the object image in this way, it is not an effect from the model.  
There can be still problem with rotational object, that requires learning different rotational variances: e.g Thatcher effect, face inversion effect.  

Further notes:  
We know that brain learns things contextually, so seeing some object in another rotational direction really does triggered the neuron differently.  I think we all had a time feeling not right seeing things upside down. For example imagine an upside down Eiffel tower, it is harder for the brain to infer it to your feeling of vision than it in normal position.  
This means that invariant representation object recognition in human also benefit from knowledge of geometry transformation (rotation) and contextual knowledge. And in terms of HTM-CLA, it means more prior knowledge/inputs/pre-trained SPs and TMs.  
This states the limitation of creating a model with only Invariant object representations and SPs compared to the scope of the full cognitive system.