using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public interface IInference
    {
        /**
     * Returns the input record sequence number associated with 
     * the state of a {@link Layer} which this {@code Inference}
     * represents.
     * 
     * @return
     */
         int getRecordNum();
        /**
         * Returns the {@link ComputeCycle}
         * @return
         */
         ComputeCycle getComputeCycle();
        /**
         * Returns a custom Object during sequence processing where one or more 
         * {@link Func1}(s) were added to a {@link Layer} in between algorithmic
         * components.
         *  
         * @return  the custom object set during processing
         */
         Object getCustomObject();
        /**
         * Returns the {@link Map} used as input into a given {@link CLAClassifier}
         * if it exists.
         * 
         * @return
         */
         Dictionary<String, object /*NamedTuple*/> getClassifierInput();
        /**
         * Returns a tuple containing key/value pairings of input field
         * names to the {@link CLAClassifier} used in conjunction with it.
         * 
         * @return
         */
         object /*NamedTuple*/ getClassifiers();
        /**
         * Returns the object used as input into a given Layer
         * which is associated with this computation result.
         * @return
         */
         Object getLayerInput();

        /**
         * Returns the <em>Sparse Distributed Representation</em> vector
         * which is the result of all algorithms in a series of algorithms
         * passed up the hierarchy.
         * 
         * @return
         */
         int[] getSDR();
        /**
         * Returns the initial encoding produced by an {@link Encoder} or one
         * of its subtypes.
         * 
         * @return
         */
         int[] getEncoding();
        /**
         * Returns the most recent {@link Classification}
         * 
         * @param fieldName
         * @return
         */
         Classification<Object> getClassification(String fieldName);
        /**
         * Returns the most recent anomaly calculation.
         * @return
         */
         double getAnomalyScore();
        /**
         * Returns the column activation from a {@link SpatialPooler}
         * @return
         */
         int[] getFeedForwardActiveColumns();
        /**
         * Returns the column activations in sparse form
         * @return
         */
         int[] getFeedForwardSparseActives();
        /**
         * Returns the column activation from a {@link TemporalMemory}
         * @return
         */
         Set<Cell> getActiveCells();
        /**
         * Returns the predicted output from the last inference cycle.
         * @return
         */
         Set<Cell> getPreviousPredictiveCells();
        /**
         * Returns the currently predicted columns.
         * @return
         */
         Set<Cell> getPredictiveCells();
    }
}
