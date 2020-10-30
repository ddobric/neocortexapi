# Experiments

## Investigation of sequence learning of SP/TM layers

### Authors

Ghulam Mustafa
ghulam.mustafa@stud.fra-usa.de

Muhammad Mubashir Ali Khan
muhammad.khan2@stud.fra-uas.de

Abdul Samad
abdul.samad@stud.fra-uas.de

Treesa Maria Thomas
treesa.thomas@stud.fra-uas.de

Diab elmehdi
el.diab@stud.fra-uas.de

### Project reference

ML19.20 - 5.4  
Issue 85

### Abstract

#### **Cells Per Column Experiment**

The ability to recognize and predict temporal sequences of sensory inputs is vital for survival in natural environments. Based on the number of known properties of cortical neurons, hierarchical temporal memory (HTM) has been recently proposed as a theoretical framework for sequence learning in the neo cortex. In this paper, we analyze the sequence learning behavior of spatial pooler and temporal memory layer in dependence on learning parameter-Cells per Column. We have demonstrated how changing the number of cells per column improvised the learning process for different input sequences. The results show the proposed model is able to learn a sequence of data by keeping the number of cells beyond a certain value depending upon the complexity of input sequence.

REF to PDF [here](https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/CellsPerColumnExperiment_Paper.pdf)

#### **HTM Sparsity**

It is necessary for survival in natural environment to be able to identify and predict temporal sequence of sensory input. Based on numerous common properties of the cortical neurons, the theoretical framework for sequence learning in the neo cortex recently proposed hierarchical temporal memory (HTM). In this paper, we analyze the sequence learning behavior of spatial pooler and temporal memory layer in dependence on HTM Sparsity. We found the ideal value of HTM Sparsity that will have optimal learning for the given input sequence. We also showed the effect of changing Width and Input Bits on learning such that the value of HTM Sparsity remains constant. We devised a relation between HTM Sparsity and max for optimal learning of the given sequence.

REF to PDF [here](https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/HtmSparsityExperiments_Paper.pdf)

#### **Parameter Change Experiment**

Hierarchical Temporal Memory (HTM) is based on the supposition that the world has a structure and is therefore predictable. The development of HTM for Artificial Neural Networks has led to an advancement in the field of artificial intelligence and leading the computing intelligence to a new age. In this paper, we studied various learning parameters like Width(W), Input Bits(N), Max,Min values and the number of columns, that majorly contribute to optimize the sequence learning behavior of spatial pooler and temporal memory layer. We also performed experiment to obtain stability of Spatial Pooler output by tuning the boost and duty cycles. We evaluated each of these parameters based on the theoretical and practical framework and summarized the results in graphical diagrams.

REF to PDF [here](https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ParameterChangeExperiment_Paper.pdf)

## Performance Spatial Pooler Global vs. Local Inhibition

### Authors

Tran Quang Trung
tranquangtrung.vgu@gmail.com

Nguyen Thanh Quang
thanhquang0912@gmail.com

### Project reference

ML19/20 - 5.7  
Issue 89

### Abstract

Each region in the cortex receives input through
millions of axons from sensory organs and from other cortical
regions. It remains a mystery how cortical neurons learn to
form specific connections from this large number of unlabeled
inputs in order to support further computations. Hierarchical
temporal memory (HTM) provides a theoretical framework for
understanding the computational principles in the neo-cortex.
HTM spatial pooler was created to model how neurons learn
feed forward connections. The spatial pooler method is
converting the arbitrary binary input patterns into sparse
distributed representations (SDRs) using competitive Hebbian
learning’s rules and homeostasis excitability control
mechanisms. In this paper, one of the Spatial Pooler’s key
parameters, which is the “inhibition”, will be described. The
main part is to show the differences between the “local” and
“global” inhibition and how and what kind of affects they
contribute to the process to the Spatial Pooler learning
algorithm.

REF to PDF [here](https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/Performance%20Spatial%20Pooler%20between%20Global%20and%20Local%20Inhibition.pdf)
