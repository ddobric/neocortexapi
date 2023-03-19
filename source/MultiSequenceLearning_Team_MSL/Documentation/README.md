Team MSL: 
1. Ankita Talande
2. Poonam Dashrath Paraskar
3. Pratik Desai

Topic: ML22/23-15 Approve Prediction of Multisequence Learning

To-Do: Analyzing the existing project and method.

Understanding the sequences and prediction using - https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/tree/master/Source/MySEProjectSample/MyProjectSample

We have finalized to use input type as Double, because in project ScalarEncoder is used which supports double as input type.

Each member will work on - 
1. Understanding the exsiting methods present in SequenceLearning.cs
// Removing specification added for LSTM with the help of professor's comment on the issue as the project is not related to LSTM directly.
2. All team members will read about HTM/TemporalMemeory and how does sequence learning automatically works using these technologies.
3. We have started understanding the HTM Algorithm

HTM Algorith:

The HTM algorithm is based on the well understood principles and core building blocks of the Thousand Brains Theory. In particular, it focuses on three main properties: sequence learning, continual learning and sparse distributed representations.

Even if substantially different in terms of algorithmic approach and learning rule, at a higher level, it may be possible to associate Hierarchical Temporal Memories to Recurrent Neural Networks. In fact, HTMs are particularly suited for sequence learning modeling as the latest RNNs incarnation such as LSTMs or GRUs.
--- HTM algorithm supports by design several properties every learning algorithm should possess:

1. Sequence learning
2. High-order predictions
3. Multiple simultaneous predictions
4. Continual Learning
5. Online learning
6. Noise robustness and fault tolerance
7. No hyperparameter tuning

https://www.numenta.com/blog/2019/10/24/machine-learning-guide-to-htm/


https://demotoshow.github.io/

========================================Project Progress ============================================================
All team Members are able to fork neocortexapi, build and run the multisequence learning application on local system.

Ankita - 
1. Forked "neocortexapi multisequence Learning" code from Github. Run the program code in visual studio. 

Poonam -
1. Forked "NeocoterxAPI" open source project.
2. Resolved build errors while trying to run the Multisequecne Learning application - by installing .Net 5.0
3. Added Nuget Package plugin while running the application. Existing application is up and running now in local system.

Pratik - 
1. Forked "NeocoterxAPI" open source project.
2. Installed compatible version of .Net 5.0 to run the application
3. Resolved the issue of Nugget package by giving the source as local path.
======================================================================================================================

Testing Phase:
Please find performance testing report in attached link: https://docs.google.com/spreadsheets/d/1DefOwD5Xcg0SZ9lGAKfDWmm9pbE3M-xHERitR8tOLTg/edit#gid=0

======================================================================================================================
