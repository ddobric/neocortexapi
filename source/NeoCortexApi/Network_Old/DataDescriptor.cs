// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.DataMappers;

namespace NeoCortexApi
{
    /// <summary>
    /// Holds all required data, which is passed to all pipeline
    /// component.
    /// </summary>
    public interface IDataDescriptor
    {
        /// <summary>
        /// Index of the label. If there are more than one label (multiclass classifier), this is the index of the first one.
        /// </summary>
        int LabelIndex { get; set; }

        //feature description: feature type, missing-value, 
        Column[] Features { get; set; }
    }

    /// <summary>
    /// Implements Meta Data information about Training or Testing data. 
    /// The class also defined which problem type LearningAPI can solve.
    /// </summary>
    public class DataDescriptor : IDataDescriptor
    {
        public DataDescriptor()
        {
            //initial value of LabelIndex should be -1.
            //bay default DataDescriptor doesnt contain proper LabelIndex
            //in case LabelIndex is -1, this means no Label data contains which point to unsupervized learning.
            LabelIndex = -1;
        }

        /// <summary>
        /// Index of the Column in RealData pointing to the Labeled column.
        /// </summary>
        public int LabelIndex { get; set; }


        /// <summary>
        ///array of feature which play role in training 
        /// </summary>
        public Column[] Features { get; set; }



        //Used statistics across data transformations
        public double[] Min { get; set; }
        public double[] Max { get; set; }
        public double[] Mean { get; set; }
        public double[] StDev { get; set; }



    }
}
