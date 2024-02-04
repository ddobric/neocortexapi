#  ML 23/24-10 Multi-Sequence Learning with language semantic.


# Introduction

This project aims to implement the application of multi-sequence learning techniques to generate a predicting code based on song or story text that represents the completion engine as used by GPT with prediction accuracy in C#/. NET Core. This project will first explore the existing __RunMultiSequenceLearningExperiment()__, which was implemented in __NeoCortex__ API, a .NET Core library. NeoCortex API is the implementation of Hierarchical Temporal Memory Cortical Learning Algorithm based on Spatial Pooler, Temporal Pooler, various encoders and CorticalNetwork algorithms.

This project implements a new method __RunLanguageSemanticExperiment()__ for learning data from the text file (long text). The data will be divided into two parts. 90% of the data will be used for training and 10% for testing. The model will be trained with the training data first. After learning is completed, the model is tested by testing data from another file to evaluate its prediction accuracy.


# Getting Started


