#define REPAIR_STABILITY
// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using NeoCortexApi.DistributedComputeLib;
using System.Collections.Concurrent;
using System.Threading;
using NeoCortexApi.DistributedCompute;

/**
 * Handles the relationships between the columns of a region 
 * and the inputs bits. The primary public interface to this function is the 
 * "compute" method, which takes in an input vector and returns a list of 
 * activeColumns columns.
 * Example Usage:
 * >
 * > SpatialPooler sp = SpatialPooler();
 * > Connections c = new Connections();
 * > sp.init(c);
 * > for line in file:
 * >   inputVector = prepared int[] (containing 1's and 0's)
 * >   sp.compute(inputVector)
 * 
 * @author David Ray, Damir Dobric
 *
 */

namespace NeoCortexApi
{
    public class SpatialPooler : IHtmAlgorithm<int[], int[]>
    {

        public double MaxInibitionDensity { get; set; } = 0.5;
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /** Default Serial Version  */
        private static readonly long serialVersionUID = 1L;

        /**
         * Constructs a new {@code SpatialPooler}
         */
        public SpatialPooler() { }

        private Connections connections;

        /**
         * Initializes the specified {@link Connections} object which contains
         * the memory and structural anatomy this spatial pooler uses to implement
         * its algorithms.
         * 
         * @param c     a {@link Connections} object
         */
        public void init(Connections c, DistributedMemory distMem = null)
        {
            this.connections = c;

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            if (c.NumActiveColumnsPerInhArea == 0 && (c.LocalAreaDensity == 0 ||
                c.LocalAreaDensity > 0.5))
            {
                throw new ArgumentException("Inhibition parameters are invalid");
            }

            c.doSpatialPoolerPostInit();

            InitMatrices(c, distMem);

            ConnectAndConfigureInputs(c);

            //sw.Stop();
            //Console.WriteLine($"Init time: {(float)sw.ElapsedMilliseconds / (float)1000} s");
        }

        /**
         * Called to initialize the structural anatomy with configured values and prepare
         * the anatomical entities for activation.
         * 
         * @param c
         */
        public virtual void InitMatrices(Connections c, DistributedMemory distMem)
        {
            SparseObjectMatrix<Column> memory = (SparseObjectMatrix<Column>)c.getMemory();

            c.setMemory(memory == null ? memory = new SparseObjectMatrix<Column>(c.getColumnDimensions(), dict: null) : memory);

            c.setInputMatrix(new SparseBinaryMatrix(c.getInputDimensions()));

            // Initiate the topologies
            c.setColumnTopology(new Topology(c.getColumnDimensions()));
            c.setInputTopology(new Topology(c.getInputDimensions()));

            //Calculate numInputs and numColumns
            int numInputs = c.getInputMatrix().getMaxIndex() + 1;
            int numColumns = c.getMemory().getMaxIndex() + 1;
            if (numColumns <= 0)
            {
                throw new ArgumentException("Invalid number of columns: " + numColumns);
            }
            if (numInputs <= 0)
            {
                throw new ArgumentException("Invalid number of inputs: " + numInputs);
            }
            c.NumInputs = numInputs;
            c.setNumColumns(numColumns);

            //
            // Fill the sparse matrix with column objects
            var numCells = c.getCellsPerColumn();

            List<KeyPair> colList = new List<KeyPair>();
            for (int i = 0; i < numColumns; i++)
            {
                colList.Add(new KeyPair() { Key = i, Value = new Column(numCells, i, c.getSynPermConnected(), c.NumInputs) });
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            memory.set(colList);

            sw.Stop();
            //c.setPotentialPools(new SparseObjectMatrix<Pool>(c.getMemory().getDimensions(), dict: distMem == null ? null : distMem.PoolDictionary));

            //Debug.WriteLine($" Upload time: {sw.ElapsedMilliseconds}");

            //c.setConnectedMatrix(new SparseBinaryMatrix(new int[] { numColumns, numInputs }));
            //  this IS removed. Every colun maintains its own matrix.

            //Initialize state meta-management statistics
            c.setOverlapDutyCycles(new double[numColumns]);
            c.setActiveDutyCycles(new double[numColumns]);
            c.setMinOverlapDutyCycles(new double[numColumns]);
            c.setMinActiveDutyCycles(new double[numColumns]);
            c.BoostFactors = (new double[numColumns]);
            ArrayUtils.fillArray(c.BoostFactors, 1);
        }


        /// <summary>
        /// Implements single threaded (originally based on JAVA implementation) initialization of SP.
        /// It creates columns, initializes the pool of potentially connected synapses on ProximalDendrites and
        /// set initial permanences for every column.
        /// </summary>
        /// <param name="c"></param>
        protected virtual void ConnectAndConfigureInputs(Connections c)
        {
            List<double> avgSynapsesConnected = new List<double>();

            //List<KeyPair> colList = new List<KeyPair>();

            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = c.NumColumns;

            Random rnd = new Random(42);

            for (int i = 0; i < numColumns; i++)
            {
                // Gets RF
                int[] potential = HtmCompute.MapPotential(c.HtmConfig, i, rnd /*c.getRandom()*/);

                Column column = c.getColumn(i);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                column.CreatePotentialPool(c.HtmConfig, potential, -1);
                //connectColumnToInputRF(c.HtmConfig, potential, column);

                //c.getPotentialPools().set(i, potPool);

                // colList.Add(new KeyPair() { Key = i, Value = column });

                double[] perm = HtmCompute.InitSynapsePermanences(c.HtmConfig, potential, rnd /*c.getRandom()*/);

                HtmCompute.UpdatePermanencesForColumn(c.HtmConfig, perm, column, potential, true);

                avgSynapsesConnected.Add(GetAvgSpanOfConnectedSynapses(c, i));
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c, avgSynapsesConnected);
        }

        /*
                /// <summary>
                /// Implements muticore initialization of pooler.
                /// </summary>
                /// <param name="c"></param>
                public void connectAndConfigureInputsMultiThreadedStrategy(Connections c)
                {
                    List<KeyPair> colList = new List<KeyPair>();
                    ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

                    int numColumns = c.NumColumns;

                    // Parallel implementation of initialization
                    ParallelOptions opts = new ParallelOptions();
                    //int synapseCounter = 0;

                    Parallel.For(0, numColumns, opts, (indx) =>
                    {
                        int i = (int)indx;
                        var data = new ProcessingData();

                        // Gets RF
                        data.Potential = mapPotential(c, i, c.isWrapAround());
                        data.Column = c.getColumn(i);

                        // This line initializes all synases in the potential pool of synapses.
                        // It creates the pool on proximal dendrite segment of the column.
                        // After initialization permancences are set to zero.
                        connectColumnToInputRF(c, data.Potential, data.Column);

                        //Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

                        //colList.Add(new KeyPair() { Key = i, Value = column });

                        data.Perm = initPermanence(c, data.Potential, data.Column);

                        updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                        if (!colList2.TryAdd(i, new KeyPair() { Key = i, Value = data }))
                        {

                        }
                    });

                    //c.setProximalSynapseCount(synapseCounter);

                    foreach (var item in colList2.Values)
                    //for (int i = 0; i < numColumns; i++)
                    {
                        int i = (int)item.Key;

                        ProcessingData data = (ProcessingData)item.Value;
                        //ProcessingData data = new ProcessingData();

                        // Debug.WriteLine(i);
                        //data.Potential = mapPotential(c, i, c.isWrapAround());

                        //var st = string.Join(",", data.Potential);
                        //Debug.WriteLine($"{i} - [{st}]");

                        //var counts = c.getConnectedCounts();

                        //for (int h = 0; h < counts.getDimensions()[0]; h++)
                        //{
                        //    // Gets the synapse mapping between column-i with input vector.
                        //    int[] slice = (int[])counts.getSlice(h);
                        //    Debug.Write($"{slice.Count(y => y == 1)} - ");
                        //}
                        //Debug.WriteLine(" --- ");
                        // Console.WriteLine($"{i} - [{String.Join(",", ((ProcessingData)item.Value).Potential)}]");

                        // This line initializes all synases in the potential pool of synapses.
                        // It creates the pool on proximal dendrite segment of the column.
                        // After initialization permancences are set to zero.
                        //var potPool = data.Column.createPotentialPool(c, data.Potential);
                        //connectColumnToInputRF(c, data.Potential, data.Column);

                        //data.Perm = initPermanence(c.getSynPermConnected(), c.getSynPermMax(),
                        //      c.getRandom(), c.getSynPermTrimThreshold(), c, data.Potential, data.Column, c.getInitConnectedPct());

                        //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                        colList.Add(new KeyPair() { Key = i, Value = data.Column });
                    }

                    SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.getMemory();

                    if (mem.IsRemotelyDistributed)
                    {
                        // Pool is created and attached to the local instance of Column.
                        // Here we need to update the pool on remote Column instance.
                        mem.set(colList);
                    }

                    // The inhibition radius determines the size of a column's local
                    // neighborhood.  A cortical column must overcome the overlap score of
                    // columns in its neighborhood in order to become active. This radius is
                    // updated every learning round. It grows and shrinks with the average
                    // number of connected synapses per column.
                    updateInhibitionRadius(c);
                }
        */

        /// <summary>
        /// Performs SpatialPooler compute algorithm.
        /// </summary>
        /// <param name="input">Input vector</param>
        /// <param name="learn">Learn or Predict.</param>
        /// <returns>Indicies of active columns.</returns>
        public int[] Compute(int[] input, bool learn)
        {
            int[] activeColumnsArr = new int[this.connections.HtmConfig.NumColumns];
            this.compute(input, activeColumnsArr, learn);
            return ArrayUtils.IndexWhere(activeColumnsArr, (el) => el == 1);
        }


        /// <summary>
        /// Performs SPatialPooler compute algorithm.
        /// </summary>
        /// <param name="input">Input vector</param>
        /// <param name="learn">Learn or Predict.</param>
        /// <param name="returnActiveColIndiciesOnly">If set on TRUE indicies of active columns are returned (sparse array of active columns).
        /// If FALSE, dense aray of all coulmns is returned.</param>
        /// <returns></returns>
        public int[] Compute(int[] input, bool learn, bool returnActiveColIndiciesOnly = true)
        {
            int[] activeColumnsArr = new int[this.connections.HtmConfig.NumColumns];

            this.compute(input, activeColumnsArr, learn);

            if (returnActiveColIndiciesOnly)
                return ArrayUtils.IndexWhere(activeColumnsArr, (el) => el == 1);
            else
                return activeColumnsArr;
        }

        bool inRepair = false;

        private int[] prevActCols = new int[0];

        private int[] stableActCols = new int[0];

        private double[] prevOverlaps = new double[0];

        double prevSimilarity = 0.0;

        /**
         * This is the primary public method of the SpatialPooler class. This
         * function takes a input vector and outputs the indices of the active columns.
         * If 'learn' is set to True, this method also updates the permanences of the
         * columns. 
         * @param inputVector       An array of 0's and 1's that comprises the input to
         *                          the spatial pooler. The array will be treated as a one
         *                          dimensional array, therefore the dimensions of the array
         *                          do not have to match the exact dimensions specified in the
         *                          class constructor. In fact, even a list would suffice.
         *                          The number of input bits in the vector must, however,
         *                          match the number of bits specified by the call to the
         *                          constructor. Therefore there must be a '0' or '1' in the
         *                          array for every input bit.
         * @param activeArray       An array whose size is equal to the number of columns.
         *                          Before the function returns this array will be populated
         *                          with 1's at the indices of the active columns, and 0's
         *                          everywhere else.
         * @param learn             A boolean value indicating whether learning should be
         *                          performed. Learning entails updating the  permanence
         *                          values of the synapses, and hence modifying the 'state'
         *                          of the model. Setting learning to 'off' freezes the SP
         *                          and has many uses. For example, you might want to feed in
         *                          various inputs and examine the resulting SDR's.
         */
        public void compute(int[] inputVector, int[] activeArray, bool learn)
        {
            if (inputVector.Length != this.connections.NumInputs)
            {
                throw new ArgumentException(
                        "Input array must be same size as the defined number of inputs: From Params: " + this.connections.NumInputs +
                        ", From Input Vector: " + inputVector.Length);
            }

            updateBookeepingVars(this.connections, learn);

            // Gets overlap over every single column.
            var overlaps = CalculateOverlap(this.connections, inputVector);

            //var overlapsStr = Helpers.StringifyVector(overlaps);
            //Debug.WriteLine("overlap: " + overlapsStr);

            //totalOverlap = overlapActive * weightActive + overlapPredictedActive * weightPredictedActive

            this.connections.Overlaps = overlaps;

            double[] boostedOverlaps;

            //
            // We perform boosting here and right after that, we will recalculate bossted factors for next cycle.
            if (learn)
            {
                //Debug.WriteLine("Boosted Factor: " + c.BoostFactors);
                boostedOverlaps = ArrayUtils.Multiply(this.connections.BoostFactors, overlaps);
            }
            else
            {
                boostedOverlaps = ArrayUtils.toDoubleArray(overlaps);
            }

            //Debug.WriteLine("BO: " + Helpers.StringifyVector(boostedOverlaps));

            this.connections.BoostedOverlaps = boostedOverlaps;

            int[] activeColumns = inhibitColumns(this.connections, boostedOverlaps);

            //var indexes = ArrayUtils.IndexWhere(this.connections.BoostFactors.OrderBy(i => i).ToArray(), x => x > 1.0);
            //Debug.WriteLine($"Boost factors: {indexes.Length} -" + Helpers.StringifyVector(indexes));

#if REPAIR_STABILITY
            // REPAIR STABILITY FEATURE
            var similarity = MathHelpers.CalcArraySimilarity(prevActCols, activeColumns);
            if (prevSimilarity == 100.0 && similarity < 70.0)
            {
                Debug.WriteLine(" O: " + Helpers.StringifyVector<double>(prevOverlaps.OrderBy(x => x).ToArray(), (indx, val) => $"{indx}-{val}"));
                Debug.WriteLine(" O: " + Helpers.StringifyVector<double>(boostedOverlaps.OrderBy(x => x).ToArray(), (indx, val) => $"{indx}-{val}"));
                Debug.WriteLine("prevActCols: " + Helpers.StringifyVector(prevActCols.OrderBy(x => x).ToArray()));
                Debug.WriteLine("    ActCols: " + Helpers.StringifyVector(activeColumns.OrderBy(x => x).ToArray()));

                stableActCols = prevActCols;

                inRepair = true;
            }

            // REPAIR STABILITY FEATURE
            if (similarity >= 95.0 && inRepair)
            {
                inRepair = false;
                Debug.WriteLine("Entered stable state again!");
            }

            prevOverlaps = boostedOverlaps;
            prevActCols = activeColumns;
            prevSimilarity = similarity;
#endif

            if (learn)
            {
                AdaptSynapses(this.connections, inputVector, activeColumns);
                updateDutyCycles(this.connections, overlaps, activeColumns);
                BumpUpWeakColumns(this.connections);
                UpdateBoostFactors(this.connections);
                if (isUpdateRound(this.connections))
                {
                    updateInhibitionRadius(this.connections);
                    updateMinDutyCycles(this.connections);
                }
            }

            // REPAIR STABILITY FEATURE
#if REPAIR_STABILITY
            if (inRepair)
            {
                Debug.WriteLine("Stable columns output..");
                activeColumns = stableActCols;
            }
#endif
            ArrayUtils.fillArray(activeArray, 0);
            if (activeColumns.Length > 0)
            {
                ArrayUtils.setIndexesTo(activeArray, activeColumns, 1);
            }

            //Debug.WriteLine($"SP-OUT: {Helpers.StringifyVector(activeColumns.OrderBy(c=>c).ToArray())}");
        }

        /**
             * Removes the set of columns who have never been active from the set of
             * active columns selected in the inhibition round. Such columns cannot
             * represent learned pattern and are therefore meaningless if only inference
             * is required. This should not be done when using a random, unlearned SP
             * since you would end up with no active columns.
             *  
             * @param activeColumns An array containing the indices of the active columns
             * @return  a list of columns with a chance of activation
             */
        public int[] stripUnlearnedColumns(Connections c, int[] activeColumns)
        {
            //TIntHashSet active = new TIntHashSet(activeColumns);
            //TIntHashSet aboveZero = new TIntHashSet();
            //int numCols = c.getNumColumns;
            //double[] colDutyCycles = c.getActiveDutyCycles();
            //for (int i = 0; i < numCols; i++)
            //{
            //    if (colDutyCycles[i] <= 0)
            //    {
            //        aboveZero.add(i);
            //    }
            //}
            //active.removeAll(aboveZero);
            //TIntArrayList l = new TIntArrayList(active);
            //l.sort();

            //return Arrays.stream(activeColumns).filter(i->c.getActiveDutyCycles()[i] > 0).toArray();



            ////TINTHashSet 
            //HashSet<int> active = new HashSet<int>(activeColumns);
            //HashSet<int> aboveZero = new HashSet<int>();

            //int numCols = c.getNumColumns;
            //double[] colDutyCycles = c.getActiveDutyCycles();
            //for (int i = 0; i < numCols; i++)
            //{
            //    if (colDutyCycles[i] <= 0)
            //    {
            //        aboveZero.Add(i);
            //    }
            //}

            //foreach (var inactiveColumn in aboveZero)
            //{
            //    active.Remove(inactiveColumn);
            //}
            ////active.Remove(aboveZero);
            ////List<int> l = new List<int>(active);
            ////l.sort();

            var res = activeColumns.Where(i => c.getActiveDutyCycles()[i] > 0).ToArray();
            return res;
            //return Arrays.stream(activeColumns).filter(i->c.getActiveDutyCycles()[i] > 0).toArray();
        }

        /**
         * Updates the minimum duty cycles defining normal activity for a column. A
         * column with activity duty cycle below this minimum threshold is boosted.
         *  
         * @param c
         */
        public void updateMinDutyCycles(Connections c)
        {
            if (c.GlobalInhibition || c.InhibitionRadius > c.NumInputs)
            {
                updateMinDutyCyclesGlobal(c);
            }
            else
            {
                updateMinDutyCyclesLocal(c);
            }
        }

        /**
         * Updates the minimum duty cycles in a global fashion. Sets the minimum duty
         * cycles for the overlap and activation of all columns to be a percent of the
         * maximum in the region, specified by {@link Connections#getMinOverlapDutyCycles()} and
         * minPctActiveDutyCycle respectively. Functionality it is equivalent to
         * {@link #updateMinDutyCyclesLocal(Connections)}, but this function exploits the globalness of the
         * computation to perform it in a straightforward, and more efficient manner.
         * 
         * @param c
         */
        public void updateMinDutyCyclesGlobal(Connections c)
        {
            ArrayUtils.fillArray(c.getMinOverlapDutyCycles(),
                   (double)(c.getMinPctOverlapDutyCycles() * ArrayUtils.max(c.getOverlapDutyCycles())));

            ArrayUtils.fillArray(c.getMinActiveDutyCycles(),
                    (double)(c.getMinPctActiveDutyCycles() * ArrayUtils.max(c.getActiveDutyCycles())));
        }

        /**
     * Gets a neighborhood of columns.
     * 
     * Simply calls topology.neighborhood or topology.wrappingNeighborhood
     * 
     * A subclass can insert different topology behavior by overriding this method.
     * 
     * @param c                     the {@link Connections} memory encapsulation
     * @param centerColumn          The center of the neighborhood.
     * @param inhibitionRadius      Span of columns included in each neighborhood
     * @return                      The columns in the neighborhood (1D)
     */
        public int[] getColumnNeighborhood(Connections c, int centerColumn, int inhibitionRadius)
        {
            var topology = c.getColumnTopology().HtmTopology;
            return c.isWrapAround() ?
                HtmCompute.GetWrappingNeighborhood(centerColumn, inhibitionRadius, topology) :
                    HtmCompute.GetNeighborhood(centerColumn, inhibitionRadius, topology);
        }

        /**
         * Updates the minimum duty cycles. The minimum duty cycles are determined
         * locally. Each column's minimum duty cycles are set to be a percent of the
         * maximum duty cycles in the column's neighborhood. Unlike
         * {@link #updateMinDutyCyclesGlobal(Connections)}, here the values can be 
         * quite different for different columns.
         * 
         * @param c
         */
        public void updateMinDutyCyclesLocal(Connections c)
        {
            int len = c.NumColumns;
            int inhibitionRadius = c.InhibitionRadius;
            double[] activeDutyCycles = c.getActiveDutyCycles();
            double minPctActiveDutyCycles = c.getMinPctActiveDutyCycles();
            double[] overlapDutyCycles = c.getOverlapDutyCycles();
            double minPctOverlapDutyCycles = c.getMinPctOverlapDutyCycles();

            //Console.WriteLine($"{inhibitionRadius: inhibitionRadius}");

            Parallel.For(0, len, (i) =>
            {
                int[] neighborhood = getColumnNeighborhood(c, i, inhibitionRadius);

                double maxActiveDuty = ArrayUtils.max(ArrayUtils.ListOfValuesByIndicies(activeDutyCycles, neighborhood));
                double maxOverlapDuty = ArrayUtils.max(ArrayUtils.ListOfValuesByIndicies(overlapDutyCycles, neighborhood));

                // Used for debugging of thread-safe capability.
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();

                //sb.Append("[");
                //for (int k = 0; k < neighborhood.Length; k++)
                //{
                //    sb.Append(neighborhood[k]);
                //    sb.Append(" - ");
                //    var x = overlapDutyCycles[k].ToString("N8");
                //    sb.Append(x);
                //    sb.Append(" | ");
                //}
                //sb.Append("]");

                //Console.WriteLine($"{i} - maxOverl: {maxOverlapDuty}\t - {sb.ToString()}");

                c.getMinActiveDutyCycles()[i] = maxActiveDuty * minPctActiveDutyCycles;

                c.getMinOverlapDutyCycles()[i] = maxOverlapDuty * minPctOverlapDutyCycles;
            });
        }

        /**
         * Updates the duty cycles for each column. The OVERLAP duty cycle is a moving
         * average of the number of inputs which overlapped with each column. The
         * ACTIVITY duty cycles is a moving average of the frequency of activation for
         * each column.
         * 
         * @param c                 the {@link Connections} (spatial pooler memory)
         * @param overlaps          an array containing the overlap score for each column.
         *                          The overlap score for a column is defined as the number
         *                          of synapses in a "connected state" (connected synapses)
         *                          that are connected to input bits which are turned on.
         * @param activeColumns     An array containing the indices of the active columns,
         *                          the sparse set of columns which survived inhibition
         */
        public void updateDutyCycles(Connections c, int[] overlaps, int[] activeColumns)
        {
            // All columns with overlap are set to 1. Otherwise 0.
            double[] overlapArray = new double[c.NumColumns];

            // All active columns are set on 1, otherwise 0.
            double[] activeArray = new double[c.NumColumns];

            //
            // if (sourceA[i] > 0) then targetB[i] = 1;
            // This ensures that all values in overlapArray are set to 1, if column has some overlap.
            ArrayUtils.greaterThanXThanSetToYInB(overlaps, overlapArray, 0, 1);
            if (activeColumns.Length > 0)
            {
                // After this step, all rows in activeArray are set to 1 at the index of active column.
                ArrayUtils.SetIndexesTo(activeArray, activeColumns, 1);
            }

            int period = c.getDutyCyclePeriod();
            if (period > c.getIterationNum())
            {              
                period = c.getIterationNum();
            }

            c.setOverlapDutyCycles(updateDutyCyclesHelper(c, c.getOverlapDutyCycles(), overlapArray, period));

            c.setActiveDutyCycles(updateDutyCyclesHelper(c, c.getActiveDutyCycles(), activeArray, period));

            //var strActiveArray = Helpers.StringifyVector(activeArray);
            //Debug.WriteLine("Active Array:" + strActiveArray);
            //var strOverlapArray = Helpers.StringifyVector(overlapArray);
            //Debug.WriteLine("Overlap Array:" + strOverlapArray);
        }

        /**
         * Updates a duty cycle estimate with a new value. This is a helper
         * function that is used to update several duty cycle variables in
         * the Column class, such as: overlapDutyCucle, activeDutyCycle,
         * minPctDutyCycleBeforeInh, minPctDutyCycleAfterInh, etc. returns
         * the updated duty cycle. Duty cycles are updated according to the following
         * formula:
         * 
         *  
         *                (period - 1)*dutyCycle + newValue
         *  dutyCycle := ----------------------------------
         *                        period
         *
         * @param c             the {@link Connections} (spatial pooler memory)
         * @param dutyCycles    An array containing one or more duty cycle values that need
         *                      to be updated
         * @param newInput      A new numerical value used to update the duty cycle. Typically 1 or 0
         * @param period        The period of the duty cycle
         * @return
         */
        public double[] updateDutyCyclesHelper(Connections c, double[] dutyCycles, double[] newInput, double period)
        {
            return ArrayUtils.divide(ArrayUtils.AddOffset(ArrayUtils.multiply(dutyCycles, period - 1), newInput), period);
        }

        /**
         * Update the inhibition radius. The inhibition radius is a measure of the
         * square (or hypersquare) of columns that each a column is "connected to"
         * on average. Since columns are not connected to each other directly, we
         * determine this quantity by first figuring out how many *inputs* a column is
         * connected to, and then multiplying it by the total number of columns that
         * exist for each input. For multiple dimension the aforementioned
         * calculations are averaged over all dimensions of inputs and columns. This
         * value is meaningless if global inhibition is enabled.
         * 
         * @param c     the {@link Connections} (spatial pooler memory)
         */
        public void updateInhibitionRadius(Connections c, List<double> avgCollected = null)
        {
            if (c.GlobalInhibition)
            {
                c.InhibitionRadius = ArrayUtils.max(c.getColumnDimensions());
                return;
            }

            if (avgCollected == null)
            {
                avgCollected = new List<double>();
                int len = c.NumColumns;
                for (int i = 0; i < len; i++)
                {
                    avgCollected.Add(GetAvgSpanOfConnectedSynapses(c, i));
                }
            }

            double avgConnectedSpan = ArrayUtils.average(avgCollected.ToArray());

            double diameter = avgConnectedSpan * calcAvgColumnsPerInput(c);
            double radius = (diameter - 1) / 2.0d;
            radius = Math.Max(1, radius);

            c.InhibitionRadius = (int)(radius + 0.5);
        }


        /// <summary>
        /// It calculates ratio numOfCols/numOfInputs for every dimension.This value is used to calculate the inhibition radius.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Average ratio numOfCols/numOfInputs across all dimensions.</returns>
        public virtual double calcAvgColumnsPerInput(Connections c)
        {
            //int[] colDim = Array.Copy(c.getColumnDimensions(), c.getColumnDimensions().Length);
            int[] colDim = new int[c.getColumnDimensions().Length];
            Array.Copy(c.getColumnDimensions(), colDim, c.getColumnDimensions().Length);

            int[] inputDim = new int[c.getInputDimensions().Length];
            Array.Copy(c.getInputDimensions(), inputDim, c.getInputDimensions().Length);

            double[] columnsPerInput = ArrayUtils.divide(
                ArrayUtils.toDoubleArray(colDim), ArrayUtils.toDoubleArray(inputDim), 0, 0);

            return ArrayUtils.average(columnsPerInput);
        }

        /**
       * The range of connectedSynapses per column, averaged for each dimension.
       * This value is used to calculate the inhibition radius. This variation of
       * the function supports arbitrary column dimensions.
       *  
       * @param c             the {@link Connections} (spatial pooler memory)
       * @param columnIndex   the current column for which to avg.
       * @return
       */

        /// <summary>
        /// It traverses all connected synapses of the column and calculates the span, which synapses
        /// spans between all input bits. Then it calculates average of spans accross all dimensions. 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public virtual double GetAvgSpanOfConnectedSynapses(Connections c, int columnIndex)
        {
            //var dims = c.getInputDimensions();

            //var dimensionMultiplies = AbstractFlatMatrix<Column>.InitDimensionMultiples(dims);

            return HtmCompute.CalcAvgSpanOfConnectedSynapses(c.getColumn(columnIndex), c.HtmConfig);
        }

        ///// <summary>
        ///// It traverses all connected synapses of the column and calculates the span, which synapses
        ///// spans between all input bits. Then it calculates average of spans accross all dimensions. 
        ///// </summary>
        ///// <param name="c"></param>
        ///// <param name="columnIndex"></param>
        ///// <returns></returns>
        //internal static double CalcAvgSpanOfConnectedSynapses(Column column, int[] inpDims, int[] dimensionMultiplies, bool isColumnMajor)
        //{
        //    // Gets synapses connected to input bits.(from pool of the column)
        //    int[] connected = column.ProximalDendrite.getConnectedSynapsesSparse();

        //    if (connected == null || connected.Length == 0) return 0;

        //    int[] maxCoord = new int[inpDims.Length];
        //    int[] minCoord = new int[inpDims.Length];
        //    ArrayUtils.fillArray(maxCoord, -1);
        //    ArrayUtils.fillArray(minCoord, ArrayUtils.max(inpDims));

        //    //
        //    // It takes all connected synapses
        //    for (int i = 0; i < connected.Length; i++)
        //    {
        //        maxCoord = ArrayUtils.maxBetween(maxCoord, AbstractFlatMatrix.ComputeCoordinates(inpDims.Length,
        //           dimensionMultiplies , isColumnMajor, connected[i]));

        //        minCoord = ArrayUtils.minBetween(minCoord, AbstractFlatMatrix.ComputeCoordinates(inpDims.Length,
        //           dimensionMultiplies, isColumnMajor, connected[i]));

        //    }

        //    return ArrayUtils.average(ArrayUtils.add(ArrayUtils.subtract(maxCoord, minCoord), 1));
        //}

        /**
         * The primary method in charge of learning. Adapts the permanence values of
         * the synapses based on the input vector, and the chosen columns after
         * inhibition round. Permanence values are increased for synapses connected to
         * input bits that are turned on, and decreased for synapses connected to
         * inputs bits that are turned off.
         * 
         * @param c                 the {@link Connections} (spatial pooler memory)
         * @param inputVector       a integer array that comprises the input to
         *                          the spatial pooler. There exists an entry in the array
         *                          for every input bit.
         * @param activeColumns     an array containing the indices of the columns that
         *                          survived inhibition.
         */
        public virtual void AdaptSynapses(Connections c, int[] inputVector, int[] activeColumns)
        {

            // Get all indicies of input vector, which are set on '1'.
            var inputIndices = ArrayUtils.IndexWhere(inputVector, inpBit => inpBit > 0);

            double[] permChanges = new double[c.NumInputs];

            // First we initialize all permChanges to minimum decrement values,
            // which are used in a case of none-connections to input.
            ArrayUtils.fillArray(permChanges, -1 * c.getSynPermInactiveDec());

            // Then we update all connected permChanges to increment values for connected values.
            // Permanences are set in conencted input bits to default incremental value.
            ArrayUtils.SetIndexesTo(permChanges, inputIndices.ToArray(), c.getSynPermActiveInc());

            for (int i = 0; i < activeColumns.Length; i++)
            {
                //Pool pool = c.getPotentialPools().get(activeColumns[i]);
                Pool pool = c.getColumn(activeColumns[i]).ProximalDendrite.RFPool;
                double[] perm = pool.getDensePermanences(c.NumInputs);
                int[] indexes = pool.getSparsePotential();
                ArrayUtils.raiseValuesBy(permChanges, perm);
                Column col = c.getColumn(activeColumns[i]);
                HtmCompute.UpdatePermanencesForColumn(c.HtmConfig, perm, col, indexes, true);
            }

            //Debug.WriteLine("Permance after update in adaptSynapses: " + permChangesStr);
        }


        /// <summary>
        /// This method increases the permanence values of synapses of columns whose 
        /// activity level has been too low. Such columns are identified by having an 
        /// overlap duty cycle that drops too much below those of their peers. The 
        /// permanence values for such columns are increased. 
        /// </summary>
        /// <param name="c"></param>
        public virtual void BumpUpWeakColumns(Connections c)
        {
            if(c.IsBumpUpWeakColumnsDisabled)
                return;

            var weakColumns = c.getMemory().get1DIndexes().Where(i => c.getOverlapDutyCycles()[i] < c.getMinOverlapDutyCycles()[i]).ToArray();
            //var weakColumnsStr = Helpers.StringifyVector(weakColumns);
            //Debug.WriteLine("weak Columns:" + weakColumnsStr);
            for (int i = 0; i < weakColumns.Length; i++)
            {
                Column col = c.getColumn(weakColumns[i]);
                //Pool pool = c.getPotentialPools().get(weakColumns[i]);
                Pool pool = col.ProximalDendrite.RFPool;
                double[] perm = pool.getSparsePermanences();
                ArrayUtils.raiseValuesBy(c.getSynPermBelowStimulusInc(), perm);
                int[] indexes = pool.getSparsePotential();

                updatePermanencesForColumnSparse(c, perm, col, indexes, true);
                //var permStr = Helpers.StringifyVector(perm);
                //Debug.WriteLine("pearm after bump up weak column:" + permStr);
            }
        }

        /**
         * This method ensures that each column has enough connections to input bits
         * to allow it to become active. Since a column must have at least
         * 'stimulusThreshold' overlaps in order to be considered during the
         * inhibition phase, columns without such minimal number of connections, even
         * if all the input bits they are connected to turn on, have no chance of
         * obtaining the minimum threshold. For such columns, the permanence values
         * are increased until the minimum number of connections are formed.
         * 
         * @param c                 the {@link Connections} memory
         * @param perm              the permanence values
         * @param maskPotential         
         */
        public virtual void RaisePermanenceToThreshold(HtmConfig htmConfig, double[] perm, int[] maskPotential)
        {
            HtmCompute.RaisePermanenceToThreshold(htmConfig, perm, maskPotential);
            //if (maskPotential.Length < c.StimulusThreshold)
            //{
            //    throw new ArgumentException("This is likely due to a " +
            //        "value of stimulusThreshold that is too large relative " +
            //        "to the input size. [len(mask) < self._stimulusThreshold]");
            //}

            //ArrayUtils.Clip(perm, c.getSynPermMin(), c.getSynPermMax());
            //while (true)
            //{
            //    // Gets number of synapses with permanence value grather than 'PermConnected'.
            //    int numConnected = ArrayUtils.ValueGreaterThanCountAtIndex(c.getSynPermConnected(), perm, maskPotential);
            //    if (numConnected >= c.StimulusThreshold)
            //        return;

            //    // If number of note connected synapses, then permanences of all synapses will be incremented (raised).
            //    ArrayUtils.raiseValuesBy(c.getSynPermBelowStimulusInc(), perm, maskPotential);
            //}
        }

        /**
         * This method ensures that each column has enough connections to input bits
         * to allow it to become active. Since a column must have at least
         * 'stimulusThreshold' overlaps in order to be considered during the
         * inhibition phase, columns without such minimal number of connections, even
         * if all the input bits they are connected to turn on, have no chance of
         * obtaining the minimum threshold. For such columns, the permanence values
         * are increased until the minimum number of connections are formed.
         * 
         * Note: This method services the "sparse" versions of corresponding methods
         * 
         * @param c         The {@link Connections} memory
         * @param perm      permanence values
         */
        public virtual void RaisePermanenceToThresholdSparse(Connections c, double[] perm)
        {
            HtmCompute.RaisePermanenceToThresholdSparse(c.HtmConfig, perm);
        }



        /**
         * This method updates the permanence matrix with a column's new permanence
         * values. The column is identified by its index, which reflects the row in
         * the matrix, and the permanence is given in 'sparse' form, (i.e. an array
         * whose members are associated with specific indexes). It is in
         * charge of implementing 'clipping' - ensuring that the permanence values are
         * always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out
         * all permanence values below 'synPermTrimThreshold'. Every method wishing
         * to modify the permanence matrix should do so through this method.
         * 
         * @param c                 the {@link Connections} which is the memory model.
         * @param perm              An array of permanence values for a column. The array is
         *                          "sparse", i.e. it contains an entry for each input bit, even
         *                          if the permanence value is 0.
         * @param column            The column in the permanence, potential and connectivity matrices
         * @param raisePerm         a boolean value indicating whether the permanence values
         */
        public void updatePermanencesForColumnSparse(Connections c, double[] perm, Column column, int[] maskPotential, bool raisePerm)
        {
            column.UpdatePermanencesForColumnSparse(c.HtmConfig, perm, maskPotential, raisePerm);
            //if (raisePerm)
            //{
            //    RaisePermanenceToThresholdSparse(c, perm);
            //}

            //ArrayUtils.LessOrEqualXThanSetToY(perm, c.getSynPermTrimThreshold(), 0);
            //ArrayUtils.Clip(perm, c.getSynPermMin(), c.getSynPermMax());
            //column.setProximalPermanencesSparse(c, perm, maskPotential);
        }

        ///**
        // * Returns a randomly generated permanence value for a synapse that is
        // * initialized in a connected state. The basic idea here is to initialize
        // * permanence values very close to synPermConnected so that a small number of
        // * learning steps could make it disconnected or connected.
        // *
        // * Note: experimentation was done a long time ago on the best way to initialize
        // * permanence values, but the history for this particular scheme has been lost.
        // * 
        // * @return  a randomly generated permanence value
        // */
        //public static double initPermConnected(double synPermMax, double synPermConnected, Random rnd)
        //{
        //    //double p = c.getSynPermConnected() + (c.getSynPermMax() - c.getSynPermConnected()) * c.random.NextDouble();
        //    double p = synPermConnected + (synPermMax - synPermConnected) * rnd.NextDouble();

        //    // Note from Python implementation on conditioning below:
        //    // Ensure we don't have too much unnecessary precision. A full 64 bits of
        //    // precision causes numerical stability issues across platforms and across
        //    // implementations
        //    p = ((int)(p * 100000)) / 100000.0d;
        //    return p;
        //}


        ///// <summary>
        ///// Returns a randomly generated permanence value for a synapses that is to be
        ///// initialized in a non-connected state.</summary>
        ///// <param name="synPermConnected"></param>
        ///// <param name="rnd">Random generator to be used to generate permanence.</param>
        ///// <returns>Permanence value.</returns>
        //public static double initPermNonConnected(double synPermConnected, Random rnd)
        //{
        //    //double p = c.getSynPermConnected() * c.getRandom().NextDouble();
        //    double p = synPermConnected * rnd.NextDouble();

        //    // Note from Python implementation on conditioning below:
        //    // Ensure we don't have too much unnecessary precision. A full 64 bits of
        //    // precision causes numerical stability issues across platforms and across
        //    // implementations
        //    p = ((int)(p * 100000)) / 100000.0d;
        //    return p;
        //}

        ///**
        // * Initializes the permanences of a column. The method
        // * returns a 1-D array the size of the input, where each entry in the
        // * array represents the initial permanence value between the input bit
        // * at the particular index in the array, and the column represented by
        // * the 'index' parameter.
        // * 
        // * @param c                 the {@link Connections} which is the memory model
        // * @param potentialPool     An array specifying the potential pool of the column.
        // *                          Permanence values will only be generated for input bits
        // *                          corresponding to indices for which the mask value is 1.
        // *                          WARNING: potentialPool is sparse, not an array of "1's"
        // * @param index             the index of the column being initialized
        // * @param connectedPct      A value between 0 or 1 specifying the percent of the input
        // *                          bits that might maximally start off in a connected state.
        // *                          0.7 means, maximally 70% of potential might be connected
        // * @return
        // */
        //public static double[] InitSynapsePermanences(HtmConfig htmConfig, int[] potentialPool, Random random)
        //{
        //    //Random random = new Random();
        //    double[] perm = new double[htmConfig.NumInputs];

        //    //foreach (int idx in column.ProximalDendrite.ConnectedInputs)
        //    foreach (int idx in potentialPool)
        //    {
        //        if (random.NextDouble() <= htmConfig.InitialSynapseConnsPct)
        //        {
        //            perm[idx] = initPermConnected(htmConfig.SynPermMax, htmConfig.SynPermMax, random);
        //        }
        //        else
        //        {
        //            htmConfig.SynPermConnected =
        //            perm[idx] = initPermNonConnected(htmConfig.SynPermConnected, random);
        //        }

        //        perm[idx] = perm[idx] < htmConfig.SynPermTrimThreshold ? 0 : perm[idx];

        //    }

        //    return perm;
        //}





        private double calcInhibitionDensity(Connections c)
        {
            double density = c.LocalAreaDensity;
            double inhibitionArea;

            // If density is not specified then inhibition radius must be specified.
            // In that case we calculate density from inhibition radius.
            if (density <= 0)
            {
                // inhibition area can be higher than num of all columns, if 
                // radius is near to number of columns of a dimension with highest number of columns.
                // In that case we limit it to number of all columns.
                inhibitionArea = Math.Pow(2 * c.InhibitionRadius + 1, c.getColumnDimensions().Length);
                inhibitionArea = Math.Min(c.NumColumns, inhibitionArea);

                density = c.NumActiveColumnsPerInhArea / inhibitionArea;

                density = Math.Min(density, MaxInibitionDensity);
            }

            return density;
        }

        /**
         * Performs inhibition. This method calculates the necessary values needed to
         * actually perform inhibition and then delegates the task of picking the
         * active columns to helper functions.
         * 
         * @param c             the {@link Connections} matrix
         * @param overlaps      an array containing the overlap score for each  column.
         *                      The overlap score for a column is defined as the number
         *                      of synapses in a "connected state" (connected synapses)
         *                      that are connected to input bits which are turned on.
         * @return
         */
        public virtual int[] inhibitColumns(Connections c, double[] initialOverlaps)
        {
            double[] overlaps = new List<double>(initialOverlaps).ToArray();

            double density = calcInhibitionDensity(c);
            //Debug.WriteLine("Inhibition step......");
            //Debug.WriteLine("Density: " + density);
            //Add our fixed little bit of random noise to the scores to help break ties.
            //ArrayUtils.d_add(overlaps, c.getTieBreaker());

            if (c.GlobalInhibition || c.InhibitionRadius > ArrayUtils.max(c.getColumnDimensions()))
            {
                return inhibitColumnsGlobal(c, overlaps, density);
            }
            return InhibitColumnsLocal(c, overlaps, density);
            //return inhibitColumnsLocalNewApproach(c, overlaps);
        }


        /// <summary>
        ///  Perform global inhibition. Performing global inhibition entails picking the
        ///  top 'numActive' columns with the highest overlap score in the entire</summary>
        ///  region. At most half of the columns in a local neighborhood are allowed to
        ///  be active.
        /// <param name="c">Connections (memory)</param>
        /// <param name="overlaps">An array containing the overlap score for each  column.</param>
        /// <param name="density"> The fraction of the overlap score for a column is defined as the numbern of columns to survive inhibition.</param>
        /// <returns>We return all columns, whof synapses in a "connected state" (connected synapses)ich have overlap greather than stimulusThreshold.</returns>
        public virtual int[] inhibitColumnsGlobal(Connections c, double[] overlaps, double density)
        {
            int numCols = c.NumColumns;
            int numActive = (int)(density * numCols);

            Dictionary<int, double> indices = new Dictionary<int, double>();
            for (int i = 0; i < overlaps.Length; i++)
            {
                indices.Add(i, overlaps[i]);
            }

            var sortedWinnerIndices = indices.OrderBy(k => k.Value).ToArray();

            // Enforce the stimulus threshold. This is a minimum number of synapses that must be ON in order for a columns to turn ON. 
            // The purpose of this is to prevent noise input from activating columns. Specified as a percent of a fully grown synapse.
            double stimulusThreshold = c.StimulusThreshold;

            // Calculate difference between num of columns and num of active. Num of active is less than 
            // num of columns, because of specified density.
            int start = sortedWinnerIndices.Count() - numActive;

            //
            // Here we peek columns with highest overlap
            while (start < sortedWinnerIndices.Count())
            {
                int i = sortedWinnerIndices[start].Key;
                if (overlaps[i] >= stimulusThreshold) break;
                ++start;
            }

            // We return all columns, which have overlap greather than stimulusThreshold.
            return sortedWinnerIndices.Skip(start).Select(p => (int)p.Key).ToArray();
        }

        public virtual int[] InhibitColumnsLocal(Connections c, double[] overlaps, double density)
        {
            return InhibitColumnsLocalOriginal(c, overlaps, density);
        }

        /**
         * Performs inhibition. This method calculates the necessary values needed to
         * actually perform inhibition and then delegates the task of picking the
         * active columns to helper functions.
         * 
         * @param c         the {@link Connections} matrix
         * @param overlaps  an array containing the overlap score for each  column.
         *                  The overlap score for a column is defined as the number
         *                  of synapses in a "connected state" (connected synapses)
         *                  that are connected to input bits which are turned on.
         * @param density   The fraction of columns to survive inhibition. This
         *                  value is only an intended target. Since the surviving
         *                  columns are picked in a local fashion, the exact fraction
         *                  of surviving columns is likely to vary.
         * @return  indices of the winning columns
         */
        public virtual int[] InhibitColumnsLocalOriginal(Connections c, double[] overlaps, double density)
        {
            double winnerDelta = ArrayUtils.max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }

            double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            List<int> winners = new List<int>();
            int inhibitionRadius = c.InhibitionRadius;
            //int inhibitionRadius = 5;
            //Debug.WriteLine("Inhibition Radius: " + inhibitionRadius);
            for (int column = 0; column < overlaps.Length; column++)
            {
                if (overlaps[column] >= c.StimulusThreshold)
                {
                    int[] neighborhood = getColumnNeighborhood(c, column, inhibitionRadius);
                    // Take overlapps of neighbors
                    double[] neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(tieBrokenOverlaps, neighborhood);

                    // Filter neighbors with overlaps bigger than column overlap
                    long numBigger = neighborhoodOverlaps.Count(d => d > overlaps[column]);
                    // density will reduce radius
                    int numActive = (int)(0.5 + density * neighborhood.Length);
                    if (numBigger < numActive)
                    {
                        winners.Add(column);
                        tieBrokenOverlaps[column] += winnerDelta;
                    }
                }
            }


            return winners.ToArray();
        }


        public virtual int[] inhibitColumnsLocalNewApproach(Connections c, double[] overlaps)
        {
            int preActive = 0;
            for (int i = 0; i < overlaps.Length; i++)
            {
                if (overlaps[i] > 0)
                {
                    preActive++;
                }
            }
            List<int> winners = new List<int>();
            int maxInhibitionRadius = (int)((Math.Sqrt((preActive / (overlaps.Length * 0.02)) + 1) / 2) - 1);
            maxInhibitionRadius = Math.Max(1, maxInhibitionRadius);
            int count = (int)(0.02 * overlaps.Length);
            var activeCols = ArrayUtils.IndexWhere(overlaps, (el) => el > c.StimulusThreshold);
            double max = 0;
            int colNum = 0;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < overlaps.Length; j++)
                {
                    if (max < overlaps[j])
                    {
                        max = overlaps[j];
                        colNum = j;
                    }
                }
                winners.Add(colNum);
                int[] neighborhood = getColumnNeighborhood(c, colNum, maxInhibitionRadius);
                double[] neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(overlaps, neighborhood);
                for (int col = 0; col < neighborhood.Length; col++)
                {
                    double newOverlap = neighborhoodOverlaps[col] - 0.5;
                    int a = neighborhood[col];
                    overlaps[a] = newOverlap;
                }

                overlaps = ArrayUtils.RemoveIndices(overlaps, colNum);
                max = 0;
            }
            return winners.ToArray();
        }
        /*
        public virtual int[] inhibitColumnsLocalNewApproach2(Connections c, double[] overlaps, double density)
        {
            
            double winnerDelta = ArrayUtils.max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }
            

            //double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            List<int> winners = new List<int>();

            int inhibitionRadius = 500;
            int windowNum = inhibitionRadius;
            for (int column = 0; column < overlaps.Length; column++)
            {
                // int column = i;
                if (overlaps[column] >= c.StimulusThreshold)
                {
                    int[] neighborhood = getColumnNeighborhood(c, column, inhibitionRadius);
                    // Take overlapps of neighbors
                    double[] neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(overlaps, neighborhood);

                    if (column < inhibitionRadius)
                    {
                        windowNum = windowNum + 1;
                        for (int i = 0; i < (inhibitionRadius-column); i++)
                        {
                            neighborhoodOverlaps[i] = 0;
                        }
                    }
                    else if (column >= (overlaps.Length - inhibitionRadius))
                    {
                        windowNum = overlaps.Length + inhibitionRadius - column;
                        for (int i = (neighborhoodOverlaps.Length-1); i > (overlaps.Length-column+inhibitionRadius-1); i--)
                        {
                            neighborhoodOverlaps[i] = 0;
                        }
                    }
                    else
                    {
                        windowNum = inhibitionRadius * 2 + 1;
                    }
                    // Filter neighbors with overlaps bigger than column overlap

                    long numBigger = neighborhoodOverlaps.Count(d => d > overlaps[column]);
                    // density will reduce radius
                    int numActive = (int)(0.5 + density * windowNum);
                    if (numBigger < numActive)
                    {
                        winners.Add(column);
                        //tieBrokenOverlaps[column] += winnerDelta;
                    }
                }
            }

            return winners.ToArray();
        }
        public virtual int[] inhibitColumnsLocalNewApproach3(Connections c, double[] overlaps, double density)
        {
            
            double winnerDelta = ArrayUtils.max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }
            

            //double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            List<int> winners = new List<int>();
            double[] overlapArr = new List<double>(overlaps).ToArray();
            int inhibitionRadius = c.InhibitionRadius;
            Debug.WriteLine("Radius: " + inhibitionRadius);
            int count = 0;
            int neighborhoodSize = inhibitionRadius * 2 + 1;
            for (int column = 0; column < (neighborhoodSize-1); column++)
            {
                double[] arr = new double[neighborhoodSize];
                arr[column] = overlapArr[column];
            }

            return winners.ToArray();
        }
        */
        public virtual int[] inhibitColumnsLocalNewApproach11(Connections c, double[] overlaps, double density)
        {
            List<int> winners = new List<int>();
            // NEW INHIBITION ALGORITHM HERE.
            // for c in columns
            //     minLocalActivity = kthScore(neighbors(c), numActiveColumnsPerInhArea)
            //         if overlap(c) > stimulusThreshold and
            //             overlap(c) ≥ minLocalActivity then
            //             activeColumns(t).append(c)

            double winnerDelta = ArrayUtils.max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }

            double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();


            int inhibitionRadius = c.InhibitionRadius;

            Parallel.ForEach(overlaps, (val, b, index) =>
            {
                // int column = i;
                if ((int)index >= c.StimulusThreshold)
                {
                    // GETS INDEXES IN THE ARRAY FOR THE NEIGHBOURS WITHIN THE INHIBITION RADIUS.
                    List<int> neighborhood = getColumnNeighborhood(c, (int)index, inhibitionRadius).ToList();

                    // GETS THE NEIGHBOURS WITHIN THE INHI
                    // Take overlapps of neighbors
                    List<double> neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(tieBrokenOverlaps, neighborhood.ToArray()).ToList();

                    // Filter neighbors with overlaps bigger than column overlap
                    long numBigger = neighborhoodOverlaps.Count(d => d > val);

                    // density will reduce radius
                    int numActive = (int)(0.5 + density * neighborhood.Count);
                    if (numBigger < numActive)
                    {
                        winners.Add((int)index);
                        tieBrokenOverlaps[(int)index] += winnerDelta;
                    }
                }
            });


            return winners.ToArray();
        }

        // lateral inhibition algorithm new approch /////////////////////
        public virtual int[] inhibitColumnsLocalNew(Connections c, double[] overlaps, double density)
        {
            // WHY IS THIS DONE??
            double winnerDelta = ArrayUtils.max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }

            double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            List<int> winners = new List<int>();

            // FIXED
            int inhibitionRadius = 2; //c.InhibitionRadius;
            double[] alpha = new double[] { -0.025, -0.075, 1, -0.075, -0.025 };

            for (int column = 0; column < overlaps.Length; column++)
            {
                // int column = i;
                if (overlaps[column] >= c.StimulusThreshold)
                {
                    // GETS INDEXES IN THE ARRAY FOR THE NEIGHBOURS WITHIN THE INHIBITION RADIUS.
                    int[] neighborhood = getColumnNeighborhood(c, column, inhibitionRadius);

                    // GETS THE NEIGHBOURS WITHIN THE INHI
                    // Take overlapps of neighbors
                    double[] neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(tieBrokenOverlaps, neighborhood);

                    // Filter neighbors with overlaps bigger than column overlap
                    double sum = 0;
                    for (int i = 0; i < neighborhoodOverlaps.Length; i++)
                    {
                        sum += alpha[i] * neighborhoodOverlaps[i];
                    }
                    //(-0.025 * neighborhoodOverlaps[0]) + (-0.075 * neighborhoodOverlaps[1]) + neighborhoodOverlaps[2] + (-0.075 * neighborhoodOverlaps[3] + (-0.025 * neighborhoodOverlaps[4]));

                    // density will reduce radius
                    int threshold = (int)(0.5 + density * neighborhood.Length) / overlaps.Length;
                    if (sum > threshold)
                    {
                        winners.Add(column);
                        tieBrokenOverlaps[column] += winnerDelta;
                    }
                }
            }

            return winners.ToArray();
        }
        ///////////////////////////////////////////////

        /**
         * Update the boost factors for all columns. The boost factors are used to
         * increase the overlap of inactive columns to improve their chances of
         * becoming active. and hence encourage participation of more columns in the
         * learning process. This is a line defined as: 
         * y = mx + b 
         * boost = (1-maxBoost)/minDuty * activeDutyCycle + maxBoost. 
         * Intuitively this means
         * that columns that have been active enough have a boost factor of 1, meaning
         * their overlap is not boosted. Columns whose active duty cycle drops too much
         * below that of their neighbors are boosted depending on how infrequently they
         * have been active. The more infrequent, the more they are boosted. The exact
         * boost factor is linearly interpolated between the points (dutyCycle:0,
         * boost:maxFiringBoost) and (dutyCycle:minDuty, boost:1.0).
         * 
         *         boostFactor
         *             ^
         * maxBoost _  |
         *             |\
         *             | \
         *       1  _  |  \ _ _ _ _ _ _ _
         *             |
         *             +--------------------> activeDutyCycle
         *                |
         *         minActiveDutyCycle
         */
        public void UpdateBoostFactors(Connections c)
        {
            double[] activeDutyCycles = c.getActiveDutyCycles();
            //var strActiveDutyCycles = Helpers.StringifyVector(activeDutyCycles);
            //Debug.WriteLine("Active Dutycycles:" + strActiveDutyCycles);
            double[] minActiveDutyCycles = c.getMinActiveDutyCycles();
            //var strMinActiveDutyCycles = Helpers.StringifyVector(activeDutyCycles);
            //Debug.WriteLine("Min active dudycycles:" + strMinActiveDutyCycles);
            List<int> mask = new List<int>();
            //Indexes of values > 0
            for (int i = 0; i < minActiveDutyCycles.Length; i++)
            {
                if (minActiveDutyCycles[i] > 0)
                    mask.Add(i);
            }

            double[] boostInterim;


            //
            // Boost factors are NOT recalculated if minimum active duty cycles are all set on 0.
            if (mask.Count < 1)
            {
                boostInterim = c.BoostFactors;
            }
            else
            {
                double[] oneMinusMaxBoostFact = new double[c.NumColumns];
                ArrayUtils.fillArray(oneMinusMaxBoostFact, 1 - c.getMaxBoost());
                boostInterim = ArrayUtils.divide(oneMinusMaxBoostFact, minActiveDutyCycles, 0, 0);
                boostInterim = ArrayUtils.multiply(boostInterim, activeDutyCycles, 0, 0);
                boostInterim = ArrayUtils.d_add(boostInterim, c.getMaxBoost());
            }

            List<int> filteredIndexes = new List<int>();

            for (int i = 0; i < activeDutyCycles.Length; i++)
            {
                if (activeDutyCycles[i] > minActiveDutyCycles[i])
                {
                    filteredIndexes.Add(i);
                }
            }

            ArrayUtils.SetIndexesTo(boostInterim, filteredIndexes.ToArray(), 1.0d);

            var bostIndexes = ArrayUtils.IndexWhere(boostInterim, x => x > 1.0);

            //if (bostIndexes.Length > 0)
            //    Debug.WriteLine("**New boost factors:" + Helpers.StringifyVector(bostIndexes.OrderBy(i => i).ToArray()));

            c.BoostFactors = boostInterim;
        }



        /// <summary>
        /// This function determines each column's overlap with the current input
        /// vector.The overlap of a column is the number of synapses for that column)
        /// to input bits which are turned on.Overlap values that are lower than
        /// the 'stimulusThreshold' are ignored.The implementation takes advantage of
        /// the SpraseBinaryMatrix class to perform this calculation efficiently.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        public virtual int[] CalculateOverlap(Connections c, int[] inputVector)
        {
            int[] overlaps = new int[c.NumColumns];
            for (int col = 0; col < c.NumColumns; col++)
            {
                overlaps[col] = c.getColumn(col).GetColumnOverlapp(inputVector, c.StimulusThreshold);
            }
            //c.getConnectedCounts().rightVecSumAtNZ(inputVector, overlaps, c.StimulusThreshold);
            //string st = string.Join(",", overlaps);
            //Debug.WriteLine($"Overlap: {st}");
            return overlaps;
        }


        /**
         * Return the overlap to connected counts ratio for a given column
         * @param c
         * @param overlaps
         * @return
         */
        public double[] CalculateOverlapPct(Connections c, int[] overlaps)
        {
            int[] columnsCounts = new int[overlaps.Length];

            for (int i = 0; i < c.NumColumns; i++)
            {
                columnsCounts[i] = c.getColumn(i).ConnectedInputCounterMatrix.getTrueCounts()[0];
            }

            return ArrayUtils.divide(overlaps, columnsCounts);
        }

        //    /**
        //     * Similar to _getNeighbors1D and _getNeighbors2D (Not included in this implementation), 
        //     * this function Returns a list of indices corresponding to the neighbors of a given column. 
        //     * Since the permanence values are stored in such a way that information about topology
        //     * is lost. This method allows for reconstructing the topology of the inputs,
        //     * which are flattened to one array. Given a column's index, its neighbors are
        //     * defined as those columns that are 'radius' indices away from it in each
        //     * dimension. The method returns a list of the flat indices of these columns.
        //     * 
        //     * @param c                     matrix configured to this {@code SpatialPooler}'s dimensions
        //     *                              for transformation work.
        //     * @param columnIndex           The index identifying a column in the permanence, potential
        //     *                              and connectivity matrices.
        //     * @param topology              A {@link SparseMatrix} with dimensionality info.
        //     * @param inhibitionRadius      Indicates how far away from a given column are other
        //     *                              columns to be considered its neighbors. In the previous 2x3
        //     *                              example, each column with coordinates:
        //     *                              [2+/-radius, 3+/-radius] is considered a neighbor.
        //     * @param wrapAround            A boolean value indicating whether to consider columns at
        //     *                              the border of a dimensions to be adjacent to columns at the
        //     *                              other end of the dimension. For example, if the columns are
        //     *                              laid out in one dimension, columns 1 and 10 will be
        //     *                              considered adjacent if wrapAround is set to true:
        //     *                              [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
        //     *               
        //     * @return              a list of the flat indices of these columns
        //     */
        //    public TIntArrayList getNeighborsND(Connections c, int columnIndex, SparseMatrix<?> topology, int inhibitionRadius, boolean wrapAround) {
        //        final int[] dimensions = topology.getDimensions();
        //        int[] columnCoords = topology.computeCoordinates(columnIndex);
        //        List<int[]> dimensionCoords = new ArrayList<>();
        //
        //        for(int i = 0;i < dimensions.length;i++) {
        //            int[] range = ArrayUtils.range(columnCoords[i] - inhibitionRadius, columnCoords[i] + inhibitionRadius + 1);
        //            int[] curRange = new int[range.length];
        //
        //            if(wrapAround) {
        //                for(int j = 0;j < curRange.length;j++) {
        //                    curRange[j] = (int)ArrayUtils.positiveRemainder(range[j], dimensions[i]);
        //                }
        //            }else{
        //                final int idx = i;
        //                curRange = ArrayUtils.retainLogicalAnd(range, 
        //                    new Condition[] { ArrayUtils.GREATER_OR_EQUAL_0,
        //                        new Condition.Adapter<Integer>() {
        //                            @Override public boolean eval(int n) { return n < dimensions[idx]; }
        //                        }
        //                    }
        //                );
        //            }
        //            dimensionCoords.add(ArrayUtils.unique(curRange));
        //        }
        //
        //        List<int[]> neighborList = ArrayUtils.dimensionsToCoordinateList(dimensionCoords);
        //        TIntArrayList neighbors = new TIntArrayList(neighborList.size());
        //        int size = neighborList.size();
        //        for(int i = 0;i < size;i++) {
        //            int flatIndex = topology.computeIndex(neighborList.get(i), false);
        //            if(flatIndex == columnIndex) continue;
        //            neighbors.add(flatIndex);
        //        }
        //        return neighbors;
        //    }

        /**
         * Returns true if enough rounds have passed to warrant updates of
         * duty cycles
         * 
         * @param c the {@link Connections} memory encapsulation
         * @return
         */
        public bool isUpdateRound(Connections c)
        {
            return c.getIterationNum() % c.getUpdatePeriod() == 0;
        }

        /**
         * Updates counter instance variables each cycle.
         *  
         * @param c         the {@link Connections} memory encapsulation
         * @param learn     a boolean value indicating whether learning should be
         *                  performed. Learning entails updating the  permanence
         *                  values of the synapses, and hence modifying the 'state'
         *                  of the model. setting learning to 'off' might be useful
         *                  for indicating separate training vs. testing sets.
         */
        public void updateBookeepingVars(Connections c, bool learn)
        {
            c.spIterationNum += 1;
            if (learn)
                c.spIterationLearnNum += 1;
        }





        public Double getRobustness(Double k, int[] oriOut, int[] realOut)
        {
            Double result = 0;
            int count = 0;
            if (oriOut.Length < realOut.Length)
            {
                for (int i = 0; i < oriOut.Length; i++)
                {
                    if (oriOut[i] != realOut[i])
                    {
                        count++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < realOut.Length; i++)
                {
                    if (oriOut[i] != realOut[i])
                    {
                        count++;
                    }
                }
            }
            Debug.WriteLine("Count: " + count);
            if (count != 0)
            {
                Double outDiff = (Double)count / oriOut.Length;
                result = (Double)(k / outDiff);
            }
            else
            {
                result = 1;
            }
            return result;
        }


    }
}

