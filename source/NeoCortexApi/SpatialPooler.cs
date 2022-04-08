// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NeoCortexApi
{
    /// <summary>
    /// Handles the relationships between the columns of a region and the inputs bits. The primary public interface to this function is the 
    /// "compute" method, which takes in an input vector and returns a list of activeColumns columns.<br/>
    /// Example Usage:<br/>
    /// ><br/>
    /// > SpatialPooler sp = SpatialPooler();<br/>
    /// > Connections c = new Connections();<br/>
    /// > sp.Init(c);<br/>
    /// > for line in file:<br/>
    /// >   inputVector = prepared int[] (containing 1's and 0's)<br/>
    /// >   sp.compute(inputVector)<br/>
    /// </summary>
    /// <remarks>
    /// Author David Ray, Damir Dobric
    /// </remarks>
    /// <summary>
    /// Spatial Pooler algorithm. Single-threaded version.
    /// Original version by David Ray, migrated from HTM JAVA. Over time, more and more code has been changed.
    /// </summary>
    public class SpatialPooler : IHtmAlgorithm<int[], int[]>
    {
        /// <summary>
        /// The instance of the <see cref="HomeostaticPlasticityController"/>.
        /// </summary>
        private HomeostaticPlasticityController m_HomeoPlastAct;

        public string Name { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="homeostaticPlasticityActivator">Includes the newborn effect in the SP. This feture was not a part of SP in the original version.</param>
        public SpatialPooler(HomeostaticPlasticityController homeostaticPlasticityActivator = null)
        {
            m_HomeoPlastAct = homeostaticPlasticityActivator;
        }

        private Connections connections;

        /// <summary>
        /// Initializes the Spatial Pooler algorithm.
        /// </summary>
        /// <param name="c">The HTM memory instance.</param>
        /// <param name="distMem"></param>
        public void Init(Connections c, DistributedMemory distMem = null)
        {
            this.connections = c;

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            if (c.HtmConfig.NumActiveColumnsPerInhArea == 0 && (c.HtmConfig.LocalAreaDensity == 0 ||
                c.HtmConfig.LocalAreaDensity > 0.5))
            {
                throw new ArgumentException("Inhibition parameters are invalid");
            }

            if (c.HtmConfig.PotentialRadius == -1)
            {
                c.HtmConfig.PotentialRadius = ArrayUtils.Product(c.HtmConfig.InputDimensions);
            }

            InitMatrices(c, distMem);

            ConnectAndConfigureInputs(c);


            //sw.Stop();
            //Console.WriteLine($"Init time: {(float)sw.ElapsedMilliseconds / (float)1000} s");
        }

        /// <summary>
        /// Initialzes mini-columns, sensory input and other required lists like duty cycles and boost factors. 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="distMem">Optionally used if the paralle version of the SP should be used.</param>
        public virtual void InitMatrices(Connections conn, DistributedMemory distMem)
        {
            if (conn.HtmConfig.Memory == null)
                conn.HtmConfig.Memory = new SparseObjectMatrix<Column>(conn.HtmConfig.ColumnDimensions, dict: null);

            conn.HtmConfig.InputMatrix = new SparseBinaryMatrix(conn.HtmConfig.InputDimensions);

            // Initiate the topologies
            conn.HtmConfig.ColumnTopology = new Topology(conn.HtmConfig.ColumnDimensions);
            conn.HtmConfig.InputTopology = new Topology(conn.HtmConfig.InputDimensions);

            //Calculate numInputs and numColumns
            int numInputs = conn.HtmConfig.InputMatrix.GetMaxIndex() + 1;
            int numColumns = conn.HtmConfig.Memory.GetMaxIndex() + 1;

            if (numColumns <= 0)
            {
                throw new ArgumentException("Invalid number of columns: " + numColumns);
            }
            if (numInputs <= 0)
            {
                throw new ArgumentException("Invalid number of inputs: " + numInputs);
            }

            conn.HtmConfig.NumInputs = numInputs;
            conn.HtmConfig.NumColumns = numColumns;

            //
            // Fill the sparse matrix with column objects
            var numCells = conn.HtmConfig.CellsPerColumn;

            List<KeyPair> colList = new List<KeyPair>();
            for (int i = 0; i < numColumns; i++)
            {
                colList.Add(new KeyPair() { Key = i, Value = new Column(numCells, i, conn.HtmConfig.SynPermConnected, conn.HtmConfig.NumInputs) });
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            conn.HtmConfig.Memory.set(colList);

            sw.Stop();

            //Initialize state meta-management statistics
            conn.HtmConfig.OverlapDutyCycles = new double[numColumns];
            conn.HtmConfig.ActiveDutyCycles = new double[numColumns];
            conn.HtmConfig.MinOverlapDutyCycles = new double[numColumns];
            conn.HtmConfig.MinActiveDutyCycles = new double[numColumns];
            conn.BoostFactors = (new double[numColumns]);

            ArrayUtils.FillArray(conn.BoostFactors, 1);
        }


        /// <summary>
        /// Implements single threaded initialization of SP.
        /// It creates the pool of potentially connected synapses on ProximalDendrite segment.
        /// </summary>
        /// <param name="conn"></param>
        protected virtual void ConnectAndConfigureInputs(Connections conn)
        {
            List<double> avgSynapsesConnected = new List<double>();

            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = conn.HtmConfig.NumColumns;

            Random rnd;

            if (conn.HtmConfig.Random == null)
                rnd = new Random(42);
            else
                rnd = conn.HtmConfig.Random;

            for (int i = 0; i < numColumns; i++)
            {
                // Gets RF
                int[] potential = HtmCompute.MapPotential(conn.HtmConfig, i, rnd);

                Column column = conn.GetColumn(i);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                column.CreatePotentialPool(conn.HtmConfig, potential, -1);

                double[] perm = HtmCompute.InitSynapsePermanences(conn.HtmConfig, potential, rnd /*c.getRandom()*/);

                HtmCompute.UpdatePermanencesForColumn(conn.HtmConfig, perm, column, potential, true);

                avgSynapsesConnected.Add(GetAvgSpanOfConnectedSynapses(conn, i));
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            UpdateInhibitionRadius(conn, avgSynapsesConnected);
        }


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

        // TODO naming convention cause problem with similar method
        /// <summary>
        /// This is the primary public method of the SpatialPooler class. This function takes a input vector and outputs the indices of the active columns.
        /// If 'learn' is set to True, this method also updates the permanences of the columns. 
        /// </summary>
        /// <param name="inputVector">
        /// An array of 0's and 1's that comprises the input to the spatial pooler. The array will be treated as a one dimensional array, therefore the dimensions 
        /// of the array do not have to match the exact dimensions specified in the class constructor. In fact, even a list would suffice. The number of input bits 
        /// in the vector must, however, match the number of bits specified by the call to the constructor. Therefore there must be a '0' or '1' in the array for 
        /// every input bit.
        /// </param>
        /// <param name="activeArray">
        /// An array whose size is equal to the number of columns. Before the function returns this array will be populated with 1's at the indices of the active 
        /// columns, and 0's everywhere else.
        /// </param>
        /// <param name="learn">
        /// A boolean value indicating whether learning should be performed. Learning entails updating the  permanence values of the synapses, and hence modifying 
        /// the 'state' of the model. Setting learning to 'off' freezes the SP and has many uses. For example, you might want to feed in various inputs and examine 
        /// the resulting SDR's.
        /// </param>
        public void compute(int[] inputVector, int[] activeArray, bool learn)
        {
            if (inputVector.Length != this.connections.HtmConfig.NumInputs)
            {
                throw new ArgumentException(
                        "Input array must be same size as the defined number of inputs: From Params: " + this.connections.HtmConfig.NumInputs +
                        ", From Input Vector: " + inputVector.Length);
            }

            UpdateBookeepingVars(this.connections, learn);

            // Gets overlap over every single column.
            var overlaps = CalculateOverlap(this.connections, inputVector);

            this.connections.Overlaps = overlaps;

            double[] boostedOverlaps;

            //
            // Here we boost calculated overlaps. This is related to Homeostatic Plasticity Mechanism.
            // Boosting factors are calculated in the previous cycle.
            if (learn)
            {
                boostedOverlaps = ArrayUtils.Multiply(this.connections.BoostFactors, overlaps);
            }
            else
            {
                boostedOverlaps = ArrayUtils.ToDoubleArray(overlaps);
            }

            this.connections.BoostedOverlaps = boostedOverlaps;

            int[] activeColumns = InhibitColumns(this.connections, boostedOverlaps);

            if (learn)
            {
                AdaptSynapses(this.connections, inputVector, activeColumns);
                UpdateDutyCycles(this.connections, overlaps, activeColumns);
                BumpUpWeakColumns(this.connections);
                UpdateBoostFactors(this.connections);
                if (IsUpdateRound(this.connections))
                {
                    UpdateInhibitionRadius(this.connections);
                    UpdateMinDutyCycles(this.connections);
                }
            }

            ArrayUtils.FillArray(activeArray, 0);
            if (activeColumns.Length > 0)
            {
                ArrayUtils.SetIndexesTo(activeArray, activeColumns, 1);
            }

            // Involve homeostatic plasticity newborn effect, which will disable boosting.
            if (this.m_HomeoPlastAct != null)
                this.m_HomeoPlastAct.Compute(inputVector, activeArray);

            //Debug.WriteLine($"SP-OUT: {Helpers.StringifyVector(activeColumns.OrderBy(c=>c).ToArray())}");
        }

        /// <summary>
        /// Removes the set of columns who have never been active from the set of active columns selected in the inhibition round. Such columns cannot
        /// represent learned pattern and are therefore meaningless if only inference is required. This should not be done when using a random, unlearned SP
        /// since you would end up with no active columns.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="activeColumns">An array containing the indices of the active columns</param>
        /// <returns>a list of columns with a chance of activation</returns>
        public int[] StripUnlearnedColumns(Connections c, int[] activeColumns)
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

            var res = activeColumns.Where(i => c.HtmConfig.ActiveDutyCycles[i] > 0).ToArray();
            return res;
            //return Arrays.stream(activeColumns).filter(i->c.getActiveDutyCycles()[i] > 0).toArray();
        }

        /// <summary>
        /// This value is automatically calculated when the RF is created on init of the SP and in every learning cycle.
        /// It helps to calculate the inhibition density.
        /// The inhibition radius determines the size of a column's local neighborhood. 
        /// A mini-column's overlap mist be highest in its neighborhood in order to become active.
        /// </summary>
        internal int InhibitionRadius { get; set; } = 0;

        /// <summary>
        /// Updates the minimum duty cycles defining normal activity for a column. A column with activity duty cycle below this minimum threshold is boosted.
        /// </summary>
        /// <param name="c"></param>
        public void UpdateMinDutyCycles(Connections c)
        {
            if (c.HtmConfig.GlobalInhibition || this.InhibitionRadius > c.HtmConfig.NumInputs)
            {
                UpdateMinDutyCyclesGlobal(c);
            }
            else
            {
                UpdateMinDutyCyclesLocal(c);
            }
        }

        /// <summary>
        /// Updates the minimum duty cycles for SP that uses global inhibition. 
        /// Sets the minimum duty cycles for the overlap and activation of all columns to be a percent of 
        /// the maximum in the region, specified by MinOverlapDutyCycles and minPctActiveDutyCycle respectively. 
        /// Functionality it is equivalent to <see cref="UpdateMinDutyCyclesLocal(Connections)"/>, 
        /// but this function exploits the globalness of the computation to perform it in a straightforward, and more efficient manner.
        /// </summary>
        /// <param name="c"></param>
        public void UpdateMinDutyCyclesGlobal(Connections c)
        {
            // Sets the minoverlaps to the MinPctOverlapDutyCycles * Maximal Overlap in the cortical column.
            ArrayUtils.InitArray(c.HtmConfig.MinOverlapDutyCycles, (double)(c.HtmConfig.MinPctOverlapDutyCycles * ArrayUtils.Max(c.HtmConfig.OverlapDutyCycles)));

            // Sets the mindutycycles to the MinPctActiveDutyCycles * Maximal Active Duty Cycles in the cortical column.
            ArrayUtils.InitArray(c.HtmConfig.MinActiveDutyCycles, (double)(c.HtmConfig.MinPctActiveDutyCycles * ArrayUtils.Max(c.HtmConfig.ActiveDutyCycles)));
        }

        /// <summary>
        /// Gets a neighborhood of columns.
        /// Simply calls topology.neighborhood or topology.wrappingNeighborhood
        /// A subclass can insert different topology behavior by overriding this method.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> memory encapsulation</param>
        /// <param name="centerColumn">The center of the neighborhood.</param>
        /// <param name="inhibitionRadius">Span of columns included in each neighborhood</param>
        /// <returns>The columns in the neighborhood (1D)</returns>
        public int[] GetColumnNeighborhood(Connections c, int centerColumn, int inhibitionRadius)
        {
            var topology = c.HtmConfig.ColumnTopology.HtmTopology;
            return c.HtmConfig.WrapAround ?
                HtmCompute.GetWrappingNeighborhood(centerColumn, inhibitionRadius, topology) :
                    HtmCompute.GetNeighborhood(centerColumn, inhibitionRadius, topology);
        }

        /// <summary>
        /// Updates the minimum duty cycles. The minimum duty cycles are determined locally. Each column's minimum duty cycles are set to be a percent of the 
        /// maximum duty cycles in the column's neighborhood. Unlike <see cref="UpdateMinDutyCyclesGlobal(Connections)"/>, here the values can be quite different 
        /// for different columns.
        /// </summary>
        /// <param name="c"></param>
        public void UpdateMinDutyCyclesLocal(Connections c)
        {
            int len = c.HtmConfig.NumColumns;

            Parallel.For(0, len, (i) =>
            {
                int[] neighborhood = GetColumnNeighborhood(c, i, this.InhibitionRadius);

                double maxActiveDuty = ArrayUtils.Max(ArrayUtils.ListOfValuesByIndicies(c.HtmConfig.ActiveDutyCycles, neighborhood));
                double maxOverlapDuty = ArrayUtils.Max(ArrayUtils.ListOfValuesByIndicies(c.HtmConfig.OverlapDutyCycles, neighborhood));

                c.HtmConfig.MinActiveDutyCycles[i] = maxActiveDuty * c.HtmConfig.MinPctActiveDutyCycles;

                c.HtmConfig.MinOverlapDutyCycles[i] = maxOverlapDuty * c.HtmConfig.MinPctOverlapDutyCycles;
            });
        }

        /// <summary>
        /// Updates the duty cycles for each column. 
        /// The OVERLAP duty cycle is a moving average of the number of inputs which overlapped with each column.
        /// The ACTIVITY duty cycles is a moving average of the frequency of activation for each column.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> (spatial pooler memory)</param>
        /// <param name="overlaps">
        /// an array containing the overlap score for each column. The overlap score for a column is defined as the number of synapses in a "connected state"
        /// (connected synapses) that are connected to input bits which are turned on.
        /// </param>
        /// <param name="activeColumns">
        /// An array containing the indices of the active columns, the sparse set of columns which survived inhibition
        /// </param>
        public void UpdateDutyCycles(Connections c, int[] overlaps, int[] activeColumns)
        {
            // All columns with overlap are set to 1. Otherwise 0.
            double[] overlapFrequencies = new double[c.HtmConfig.NumColumns];

            // All active columns are set on 1, otherwise 0.
            double[] activeColFrequencies = new double[c.HtmConfig.NumColumns];

            //
            // if (sourceA[i] > 0) then targetB[i] = 1;
            // This ensures that all values in overlapCycles are set to 1, if column has some overlap.
            ArrayUtils.GreaterThanXThanSetToYInB(overlaps, overlapFrequencies, 0, 1);
          
            if (activeColumns.Length > 0)
            {
                // After this step, all rows in activeCycles are set to 1 at the index of active column.
                ArrayUtils.SetIndexesTo(activeColFrequencies, activeColumns, 1);
            }

            int period = c.HtmConfig.DutyCyclePeriod;
            if (period > c.SpIterationNum)
            {
                period = c.SpIterationNum;
            }

            c.HtmConfig.OverlapDutyCycles = CalcEventFrequency(c.HtmConfig.OverlapDutyCycles, overlapFrequencies, period);

            c.HtmConfig.ActiveDutyCycles = CalcEventFrequency(c.HtmConfig.ActiveDutyCycles, activeColFrequencies, period);
        }


        /// <summary>
        /// Calculates the normalized counter value of the frequency of an event. 
        /// Event can be overlap or activation of the column.
        /// Updates a duty cycle estimate with a new value. This is a helper function that is used to update several duty cycle variables in
        /// the Column class, such as: overlapDutyCucle, activeDutyCycle, minPctDutyCycleBeforeInh, minPctDutyCycleAfterInh, etc. returns
        /// the updated duty cycle. Duty cycles are updated according to the following formula: <br/>
        /// 
        ///  
        ///                (period - 1)*dutyCycle + newValue<br/>
        ///  dutyCycle := ----------------------------------<br/>
        ///                        period<br/>
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> (spatial pooler memory)</param>
        /// <param name="dutyCycles">An array containing one or more duty cycle values that need to be updated</param>
        /// <param name="newInput">A new numerical value used to update the duty cycle. Typically 1 or 0</param>
        /// <param name="period">The period of the duty cycle</param>
        /// <remarks>
        /// This looks a bit complicate. But, it is simple normalized counter that counts how many times the column was 
        /// connected to the non-zero input bit (in a case of the overlapp) or how often the column was active (in a case of active).</remarks>
        /// <returns></returns>
        public static double[] CalcEventFrequency(double[] dutyCycles, double[] newInput, double period)
        {
            return ArrayUtils.Divide(ArrayUtils.AddOffset(ArrayUtils.Multiply(dutyCycles, period - 1), newInput), period);
        }

        /// <summary>
        /// Update the inhibition radius. The inhibition radius is a measure of the square (or hypersquare) of columns that each a column is "connected to"
        /// on average. Since columns are not connected to each other directly, we determine this quantity by first figuring out how many *inputs* a column is
        /// connected to, and then multiplying it by the total number of columns that exist for each input. For multiple dimension the aforementioned
        /// calculations are averaged over all dimensions of inputs and columns. This value is meaningless if global inhibition is enabled.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> (spatial pooler memory)</param>
        /// <param name="avgCollected"></param>
        public void UpdateInhibitionRadius(Connections c, List<double> avgCollected = null)
        {
            if (c.HtmConfig.GlobalInhibition)
            {
                this.InhibitionRadius = ArrayUtils.Max(c.HtmConfig.ColumnDimensions);
                return;
            }

            if (avgCollected == null)
            {
                avgCollected = new List<double>();

                for (int i = 0; i < c.HtmConfig.NumColumns; i++)
                {
                    avgCollected.Add(GetAvgSpanOfConnectedSynapses(c, i));
                }
            }

            double avgConnectedSpan = ArrayUtils.Average(avgCollected.ToArray());

            double diameter = avgConnectedSpan * CalcAvgColumnsPerInput(c);
            double radius = (diameter - 1) / 2.0d;
            radius = Math.Max(1, radius);

            this.InhibitionRadius = (int)(radius + 0.5);
        }


        /// <summary>
        /// It calculates ratio numOfCols/numOfInputs for every dimension.This value is used to calculate the inhibition radius.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Average ratio numOfCols/numOfInputs across all dimensions.</returns>
        public virtual double CalcAvgColumnsPerInput(Connections c)
        {
            //int[] colDim = Array.Copy(c.getColumnDimensions(), c.getColumnDimensions().Length);
            int[] colDim = new int[c.HtmConfig.ColumnDimensions.Length];
            Array.Copy(c.HtmConfig.ColumnDimensions, colDim, c.HtmConfig.ColumnDimensions.Length);

            int[] inputDim = new int[c.HtmConfig.InputDimensions.Length];
            Array.Copy(c.HtmConfig.InputDimensions, inputDim, c.HtmConfig.InputDimensions.Length);

            double[] columnsPerInput = ArrayUtils.Divide(
                ArrayUtils.ToDoubleArray(colDim), ArrayUtils.ToDoubleArray(inputDim), 0, 0);

            return ArrayUtils.Average(columnsPerInput);
        }

 
        /// <summary>
        /// It traverses all connected synapses of the column and calculates the span, which synapses
        /// span between all input bits. Then it calculates average of spans accross all dimensions. 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public virtual double GetAvgSpanOfConnectedSynapses(Connections c, int columnIndex)
        {
            return HtmCompute.CalcAvgSpanOfConnectedSynapses(c.GetColumn(columnIndex), c.HtmConfig);
        }


        /// <summary>
        /// The primary method in charge of learning. Adapts the permanence values of the synapses based on the input vector, 
        /// and the chosen columns after inhibition round. Permanence values are increased for synapses connected to input bits
        /// that are turned on, and decreased for synapses connected to inputs bits that are turned off.
        /// </summary>
        /// <param name="conn">the <see cref="Connections"/> (spatial pooler memory)</param>
        /// <param name="inputVector">a integer array that comprises the input to the spatial pooler. There exists an entry in the array for every input bit.</param>
        /// <param name="activeColumns">an array containing the indices of the columns that survived inhibition.</param>
        public virtual void AdaptSynapses(Connections conn, int[] inputVector, int[] activeColumns)
        {
            // Get all indicies of input vector, which are set on '1'.
            var actInputIndexes = ArrayUtils.IndexWhere(inputVector, inpBit => inpBit > 0);

            double[] permChanges = new double[conn.HtmConfig.NumInputs];

            // First we initialize all permChanges to minimum decrement values,
            // which are used in a case of none-connections to input.
            ArrayUtils.InitArray(permChanges, -1 * conn.HtmConfig.SynPermInactiveDec);

            // Then we set SynPermActiveInc to all connected synapses.
            ArrayUtils.SetIndexesTo(permChanges, actInputIndexes.ToArray(), conn.HtmConfig.SynPermActiveInc);

            for (int i = 0; i < activeColumns.Length; i++)
            {
                Column col = conn.GetColumn(activeColumns[i]);

                Pool pool = col.ProximalDendrite.RFPool;

                // Gets permanences of all synapses to all input bits.
                double[] permanences = pool.GetDensePermanences(conn.HtmConfig.NumInputs);

                // The current permanence values in 'permanences' will be raised by values in permChanges.
                ArrayUtils.RaiseValuesBy(permChanges, permanences);

                // Get indexes of input bits that are synaptically connected with this column.
                int[] indexes = pool.GetSparsePotential();

                HtmCompute.UpdatePermanencesForColumn(conn.HtmConfig, permanences, col, indexes, true);
            }

            //Debug.WriteLine("Permance after update in adaptSynapses: " + permChangesStr);
        }


        /// <summary>
        /// This method increases the permanence values of synapses of columns whose 
        /// overlap level is too low. Such columns are identified by having an 
        /// overlap duty cycle (activation frequency) that drops too much below those of their peers. 
        /// The permanence values for such columns are increased. 
        /// </summary>
        /// <param name="c"></param>
        public virtual void BumpUpWeakColumns(Connections c)
        {
            // Get columns with too low overlap.
            var weakColumns = c.HtmConfig.Memory.Get1DIndexes().Where(i => c.HtmConfig.OverlapDutyCycles[i] < c.HtmConfig.MinOverlapDutyCycles[i]).ToArray();

            for (int i = 0; i < weakColumns.Length; i++)
            {
                Column col = c.GetColumn(weakColumns[i]);
        
                Pool pool = col.ProximalDendrite.RFPool;
                double[] perm = pool.GetSparsePermanences();
                ArrayUtils.RaiseValuesBy(c.HtmConfig.SynPermBelowStimulusInc, perm);
                int[] indexes = pool.GetSparsePotential();

                UpdatePermanencesForColumnSparse(c, perm, col, indexes, true);
            }
        }

        /// <summary>
        /// This method ensures that each column has enough connections to input bits to allow it to become active. 
        /// Since a column must have at least 'stimulusThreshold' overlaps in order to be considered during the inhibition phase,
        /// columns without such minimal number of connections, even if all the input bits they are connected to turn on, 
        /// have no chance of obtaining the minimum threshold. For such columns, the permanence values are increased until 
        /// the minimum number of connections are formed.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="perm">the permanence values</param>
        /// <param name="maskPotential"></param>
        public virtual void RaisePermanenceToThreshold(HtmConfig htmConfig, double[] perm, int[] maskPotential)
        {
            HtmCompute.BoostProximalSegment(htmConfig, perm, maskPotential);
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

        /// <summary>
        /// This method ensures that each column has enough connections to input bits to allow it to become active. 
        /// Since a column must have at least 'stimulusThreshold' overlaps in order to be considered during the inhibition phase, 
        /// columns without such minimal number of connections, even if all the input bits they are connected to turn on, 
        /// have no chance of obtaining the minimum threshold. For such columns, the permanence values are increased until 
        /// the minimum number of connections are formed.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> memory</param>
        /// <param name="perm">permanence values</param>
        /// <remarks>Note: This method services the "sparse" versions of corresponding methods</remarks>
        //public virtual void RaisePermanenceToThresholdSparse(Connections c, double[] perm)
        //{
        //    HtmCompute.RaisePermanenceToThresholdSparse(c.HtmConfig, perm);
        //}

        /// <summary>
        /// This method updates the permanence matrix with a column's new permanence values. The column is identified by its index,
        /// which reflects the row in the matrix, and the permanence is given in 'sparse' form, (i.e. an array whose members are 
        /// associated with specific indexes). It is in charge of implementing 'clipping' - ensuring that the permanence values are
        /// always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out all permanence values below 'synPermTrimThreshold'.
        /// Every method wishing to modify the permanence matrix should do so through this method.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> which is the memory model.</param>
        /// <param name="perm">
        /// An array of permanence values for a column. The array is "sparse", i.e. it contains an entry for each input bit, even if 
        /// the permanence value is 0.
        /// </param>
        /// <param name="column">The column in the permanence, potential and connectivity matrices</param>
        /// <param name="maskPotential">Indexes of potential connections to input neurons.</param>
        /// <param name="raisePerm">a boolean value indicating whether the permanence values</param>
        public void UpdatePermanencesForColumnSparse(Connections c, double[] perm, Column column, int[] maskPotential, bool raisePerm)
        {
            column.UpdatePermanencesForColumnSparse(c.HtmConfig, perm, maskPotential, raisePerm);
        }

        /// <summary>
        /// If the <see cref="HtmConfig.LocalAreaDensity"/> is specified, then this value is used as density.
        /// d = min(MaxInibitionDensity=0.5, NumActiveColumnsPerInhArea/[(2*InhibitionRadius + 1)**ColumnDimensions.Length]
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private double CalcInhibitionDensity(Connections conn)
        {
            double density = conn.HtmConfig.LocalAreaDensity;
            double inhibitionArea;

            //
            // If density is not specified then inhibition radius must be specified.
            // In that case the density is calculated from inhibition radius.
            if (density <= 0)
            {
                // inhibition area can be higher than num of all columns, if 
                // radius is near to number of columns of a dimension with highest number of columns.
                // In that case we limit it to number of all columns.
                inhibitionArea = Math.Pow(2 * this.InhibitionRadius + 1, conn.HtmConfig.ColumnDimensions.Length);

                inhibitionArea = Math.Min(conn.HtmConfig.NumColumns, inhibitionArea);

                // TODO; The ihibition Area is here calculated in the column dimension. However it should be calculated in th einput dimension.
                //inhibitionArea = Math.Pow(2 * this.InhibitionRadius + 1, conn.HtmConfig.InputDimensions.Length);
                //inhibitionArea = Math.Min(conn.HtmConfig.NumInputs, inhibitionArea);

                density = conn.HtmConfig.NumActiveColumnsPerInhArea / inhibitionArea;

                density = Math.Min(density, conn.HtmConfig.MaxInibitionDensity);
            }

            return density;
        }

        /// <summary>
        /// Performs the inhibition algorithm. This method calculates the density of the inhibition and executes either clobal or local inhibition
        /// algorithm.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> The HTM instance.</param>
        /// <param name="initialOverlaps">
        /// an array containing the overlap score for each column. The overlap score for a column is defined as the number of synapses
        /// in a "connected state" (connected synapses) that are connected to input bits which are turned on.
        /// </param>
        /// <returns></returns>
        public virtual int[] InhibitColumns(Connections c, double[] initialOverlaps)
        {
            double[] overlaps = new List<double>(initialOverlaps).ToArray();

            double density = CalcInhibitionDensity(c);

            if (c.HtmConfig.GlobalInhibition || this.InhibitionRadius > ArrayUtils.Max(c.HtmConfig.ColumnDimensions))
            {
                return InhibitColumnsGlobal(c, overlaps, density);
            }
            else
            {
                return InhibitColumnsLocal(c, overlaps, density);
            }
            //return inhibitColumnsLocalNewApproach(c, overlaps);
        }


        /// <summary>
        ///  Perform global inhibition. It selects top active columns with the highest overlap score in the entire region. 
        ///  The number of selected active columns is defined by the 'density' argument.
        ///  At most half of the columns in a local neighborhood are allowed tobe active.
        /// <param name="c">Connections (memory)</param>
        /// <param name="overlaps">An array containing the overlap score for each  column.</param>
        /// <param name="density">Defines the number of columns that will survive the inhibition.</param>
        /// <returns>We return all columns, of synapses in a "connected state" (connected synapses) that have overlap greather than stimulusThreshold.</returns>
        public virtual int[] InhibitColumnsGlobal(Connections c, double[] overlaps, double density)
        {
            int numCols = c.HtmConfig.NumColumns;
            int numActive = (int)(density * numCols);

            Dictionary<int, double> indices = new Dictionary<int, double>();
            for (int i = 0; i < overlaps.Length; i++)
            {
                indices.Add(i, overlaps[i]);
            }

            var sortedWinnerIndices = indices.OrderBy(k => k.Value).ToArray();

            // Enforce the stimulus threshold. This is a minimum number of synapses that must be ON in order for a columns to turn ON. 
            // The purpose of this is to prevent noise input from activating columns. Specified as a percent of a fully grown synapse.
            double stimulusThreshold = c.HtmConfig.StimulusThreshold;

            // Calculate difference between num of columns and num of active. Num of active is less than 
            // num of columns, because of specified density.
            int start = sortedWinnerIndices.Length - numActive;

            //
            // Here we peek columns with highest overlap
            while (start < sortedWinnerIndices.Length)
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

        /// <summary>
        /// Implements the local inhibition algorithm. 
        /// This method calculates the necessary values needed to actually perform inhibition and then delegates
        /// the task of picking the active columns to helper functions.
        /// </summary>
        /// <param name="mem">the <see cref="Connections"/> matrix</param>
        /// <param name="overlaps">
        /// an array containing the overlap score for each  column. The overlap score for a column is defined as the number of synapses
        /// in a "connected state" (connected synapses) that are connected to input bits which are turned on.
        /// </param>
        /// <param name="density">
        /// The fraction of columns to survive inhibition. This value is only an intended target. Since the surviving columns are picked
        /// in a local fashion, the exact fraction of surviving columns is likely to vary.
        /// </param>
        /// <returns>indices of the winning columns</returns>
        public virtual int[] InhibitColumnsLocalOriginal(Connections mem, double[] overlaps, double density)
        {
            double winnerDelta = ArrayUtils.Max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }

            double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            List<int> winners = new List<int>();

            int inhibitionRadius = this.InhibitionRadius;

            for (int column = 0; column < overlaps.Length; column++)
            {
                if (overlaps[column] >= mem.HtmConfig.StimulusThreshold)
                {
                    int[] neighborhood = GetColumnNeighborhood(mem, column, inhibitionRadius);
                    // Take overlapps of neighbors
                    double[] neighborhoodOverlaps = ArrayUtils.ListOfValuesByIndicies(tieBrokenOverlaps, neighborhood);

                    // Filter neighbors with overlaps larger than column overlap
                    long numHigherOverlap = neighborhoodOverlaps.Count(d => d > overlaps[column]);

                    // density will reduce radius
                    // numActive is the number of columns that participate in the inhibition.
                    int numActive = (int)(0.5 + density * neighborhood.Length);

                    //
                    // numActive is the number of maximal active columns in the neighborhood.
                    // numHigherOverlap is the number of columns in the neighborhood that have higher overlap than the referencing column.
                    // Column is added as a winner one if the number of higher overlapped columns
                    // is less than number of active columns defined by density and radius.
                    if (numHigherOverlap < numActive)
                    {
                        winners.Add(column);
                        tieBrokenOverlaps[column] += winnerDelta;
                    }
                }
            }

            //return winners.OrderBy(w=>w).Take((int)mem.HtmConfig.NumActiveColumnsPerInhArea).ToArray();
            return winners.ToArray();
        }


        public virtual int[] InhibitColumnsLocalNewApproach(Connections c, double[] overlaps)
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
            var activeCols = ArrayUtils.IndexWhere(overlaps, (el) => el > c.HtmConfig.StimulusThreshold);
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
                int[] neighborhood = GetColumnNeighborhood(c, colNum, maxInhibitionRadius);
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
        public virtual int[] InhibitColumnsLocalNewApproach11(Connections c, double[] overlaps, double density)
        {
            List<int> winners = new List<int>();
            // NEW INHIBITION ALGORITHM HERE.
            // for c in columns
            //     minLocalActivity = kthScore(neighbors(c), numActiveColumnsPerInhArea)
            //         if overlap(c) > stimulusThreshold and
            //             overlap(c) ≥ minLocalActivity then
            //             activeColumns(t).append(c)

            double winnerDelta = ArrayUtils.Max(overlaps) / 1000.0d;
            if (winnerDelta == 0)
            {
                winnerDelta = 0.001;
            }

            double[] tieBrokenOverlaps = new List<double>(overlaps).ToArray();

            Parallel.ForEach(overlaps, (val, b, index) =>
            {
                // int column = i;
                if ((int)index >= c.HtmConfig.StimulusThreshold)
                {
                    // GETS INDEXES IN THE ARRAY FOR THE NEIGHBOURS WITHIN THE INHIBITION RADIUS.
                    List<int> neighborhood = GetColumnNeighborhood(c, (int)index, this.InhibitionRadius).ToList();

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
        public virtual int[] InhibitColumnsLocalNew(Connections c, double[] overlaps, double density)
        {
            // WHY IS THIS DONE??
            double winnerDelta = ArrayUtils.Max(overlaps) / 1000.0d;
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
                if (overlaps[column] >= c.HtmConfig.StimulusThreshold)
                {
                    // GETS INDEXES IN THE ARRAY FOR THE NEIGHBOURS WITHIN THE INHIBITION RADIUS.
                    int[] neighborhood = GetColumnNeighborhood(c, column, inhibitionRadius);

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


        /// <summary>
        /// Update the boost factors for all columns. The boost factors are used to increase the overlap of inactive columns to improve
        /// their chances of becoming active. and hence encourage participation of more columns in the learning process. 
        /// This is known as Homeostatc Plasticity Mechanism.
        /// This is a line defined as: 
        /// y = mx + b 
        /// boost = (1-maxBoost)/minDuty * activeDutyCycle + maxBoost. 
        /// Intuitively this means that columns that have been active enough have a boost factor of 1, meaning their overlap is not boosted.
        /// Columns whose active duty cycle drops too much below that of their neighbors are boosted depending on how infrequently they 
        /// have been active. The more infrequent, the more they are boosted. The exact boost factor is linearly interpolated between the points
        /// (dutyCycle:0, boost:maxFiringBoost) and (dutyCycle:minDuty, boost:1.0).
        /// 
        ///         boostFactor
        ///             ^
        /// maxBoost _  |
        ///             |\
        ///             | \
        ///       1  _  |  \ _ _ _ _ _ _ _
        ///             |
        ///             +--------------------> activeDutyCycle
        ///                |
        ///         minActiveDutyCycle
        /// </summary>
        /// <param name="c"></param>
        public void UpdateBoostFactors(Connections c)
        {
            double[] activeDutyCycles = c.HtmConfig.ActiveDutyCycles;
            double[] minActiveDutyCycles = c.HtmConfig.MinActiveDutyCycles;

            double[] boostFactors;

            //
            // Boost factors are NOT recalculated if minimum active duty cycles are all set on 0.
            // MinActiveDutyCycles is set in UpdateMinDutyCycles and also controlled by HomeostaticPlasticityController.
            if (minActiveDutyCycles.Count(ma => ma > 0) == 0)
            {
                boostFactors = c.BoostFactors;
            }
            else
            {
                double[] oneMinusMaxBoostFact = new double[c.HtmConfig.NumColumns];
                ArrayUtils.InitArray(oneMinusMaxBoostFact, 1 - c.HtmConfig.MaxBoost);
                boostFactors = ArrayUtils.Divide(oneMinusMaxBoostFact, minActiveDutyCycles, 0, 0);
                boostFactors = ArrayUtils.Multiply(boostFactors, activeDutyCycles, 0, 0);
                boostFactors = ArrayUtils.AddAmount(boostFactors, c.HtmConfig.MaxBoost);
            }

            // Filtered indexes are indexes of columns whose activeDutyCycles is larger than calculated minActiveDutyCycles of the column.
            List<int> idxOfActiveColumns = new List<int>();

            for (int i = 0; i < activeDutyCycles.Length; i++)
            {
                if (activeDutyCycles[i] >= minActiveDutyCycles[i])
                {
                    idxOfActiveColumns.Add(i);
                }
            }

            // Already very active columns will have boost factor 1.0. That mean their synapses on the proximal segment 
            // will not be stimulated.
            ArrayUtils.SetIndexesTo(boostFactors, idxOfActiveColumns.ToArray(), 1.0d);

            c.BoostFactors = boostFactors;
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
            int[] overlaps = new int[c.HtmConfig.NumColumns];

            //
            // Calculates the overlapp for each mini-column.
            for (int col = 0; col < c.HtmConfig.NumColumns; col++)
            {
                overlaps[col] = c.GetColumn(col).CalcMiniColumnOverlap(inputVector, c.HtmConfig.StimulusThreshold);
            }

            return overlaps;
        }

        /// <summary>
        /// Return the overlap to connected counts ratio for a given column
        /// </summary>
        /// <param name="c"></param>
        /// <param name="overlaps"></param>
        /// <returns></returns>
        public double[] CalculateOverlapPct(Connections c, int[] overlaps)
        {
            int[] columnsCounts = new int[overlaps.Length];

            for (int i = 0; i < c.HtmConfig.NumColumns; i++)
            {
                columnsCounts[i] = c.GetColumn(i).ConnectedInputCounterMatrix.GetTrueCounts()[0];
            }

            return ArrayUtils.Divide(overlaps, columnsCounts);
        }


        /// <summary>
        /// Returns true if enough rounds have passed to warrant updates of duty cycles
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> memory encapsulation</param>
        /// <returns></returns>
        public bool IsUpdateRound(Connections c)
        {
            return c.SpIterationNum % c.HtmConfig.UpdatePeriod == 0;
        }

        /// <summary>
        /// Updates counter instance variables each cycle.
        /// </summary>
        /// <param name="c">the <see cref="Connections"/> memory encapsulation<s</param>
        /// <param name="learn">
        /// a boolean value indicating whether learning should be performed. Learning entails updating the  permanence values 
        /// of the synapses, and hence modifying the 'state' of the model. setting learning to 'off' might be useful for indicating 
        /// separate training vs. testing sets.
        /// </param>
        public void UpdateBookeepingVars(Connections c, bool learn)
        {
            c.SpIterationNum += 1;
            if (learn)
                c.SpIterationLearnNum += 1;
        }

        public double GetRobustness(double k, int[] oriOut, int[] realOut)
        {
            double result = 0;
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
                double outDiff = (double)count / oriOut.Length;
                result = (double)(k / outDiff);
            }
            else
            {
                result = 1;
            }
            return result;
        }

        public bool Equals(SpatialPooler obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            else if (Name != obj.Name)
                return false;

            SpatialPooler other = (SpatialPooler)obj;

            if (m_HomeoPlastAct == null)
            {
                if (other.m_HomeoPlastAct != null)
                    return false;
            }
            else if (!m_HomeoPlastAct.Equals(other.m_HomeoPlastAct))
                return false;

            if (connections == null)
            {
                if (other.connections != null)
                    return false;
            }
            else if (!connections.Equals(other.connections))
                return false;

            return true;

        }


        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(SpatialPooler), writer);

            ser.SerializeValue(this.Name, writer);

            if (this.m_HomeoPlastAct != null)
            {
                this.m_HomeoPlastAct.Serialize(writer);
            }

            if (this.connections != null)
            {
                this.connections.Serialize(writer);
            }

            ser.SerializeEnd(nameof(SpatialPooler), writer);
        }

        public static SpatialPooler Deserialize(StreamReader sr)
        {
            SpatialPooler sp = new SpatialPooler();

            HtmSerializer2 ser = new HtmSerializer2();

            while (sr.Peek() >= 0)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(SpatialPooler)))
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(HomeostaticPlasticityController)))
                {
                    sp.m_HomeoPlastAct = HomeostaticPlasticityController.Deserialize(sr);
                }
                else if (data == ser.ReadBegin(nameof(Connections)))
                {
                    sp.connections = Connections.Deserialize(sr);
                }
                else if (data == ser.ReadEnd(nameof(SpatialPooler)))
                {
                    break;
                }
                else
                {
                    string[] str = data.Split(HtmSerializer2.ParameterDelimiter);
                    for (int i = 0; i < str.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                {
                                    //sp.MaxInibitionDensity = ser.ReadDoubleValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    sp.Name = ser.ReadStringValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }

            return sp;
        }
    }
}

