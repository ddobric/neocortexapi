using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;


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
 * @author David Ray
 *
 */

namespace NeoCortexApi
{
    public class SpatialPooler : IHtmAlgorithm
    {

        public double MaxInibitionDensity { get; set; } = 0.5;

        /** Default Serial Version  */
        private static readonly long serialVersionUID = 1L;

        /**
         * Constructs a new {@code SpatialPooler}
         */
        public SpatialPooler() { }

        /**
         * Initializes the specified {@link Connections} object which contains
         * the memory and structural anatomy this spatial pooler uses to implement
         * its algorithms.
         * 
         * @param c     a {@link Connections} object
         */
        public void init(Connections c, IDictionary<int, Column> dictionary = null)
        {
            if (c.NumActiveColumnsPerInhArea == 0 && (c.LocalAreaDensity == 0 ||
                c.LocalAreaDensity > 0.5))
            {
                throw new ArgumentException("Inhibition parameters are invalid");
            }

            c.doSpatialPoolerPostInit();
            initMatrices(c, dictionary);
            connectAndConfigureInputs(c);
        }

        /**
         * Called to initialize the structural anatomy with configured values and prepare
         * the anatomical entities for activation.
         * 
         * @param c
         */
        public void initMatrices(Connections c, IDictionary<int, Column> dictionary = null)
        {
            SparseObjectMatrix<Column> memory = c.getMemory();

            c.setMemory(memory == null ? memory = new SparseObjectMatrix<Column>(c.getColumnDimensions(), dict: dictionary) : memory);

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

            // PERF
            for (int i = 0; i < numColumns; i++)
            {
                memory.set(i, new Column(numCells, i));
            }

            c.setPotentialPools(new SparseObjectMatrix<Pool>(c.getMemory().getDimensions()));

            c.setConnectedMatrix(new SparseBinaryMatrix(new int[] { numColumns, numInputs }));

            //Initialize state meta-management statistics
            c.setOverlapDutyCycles(new double[numColumns]);
            c.setActiveDutyCycles(new double[numColumns]);
            c.setMinOverlapDutyCycles(new double[numColumns]);
            c.setMinActiveDutyCycles(new double[numColumns]);
            c.BoostFactors = (new double[numColumns]);
            ArrayUtils.fillArray(c.BoostFactors, 1);
        }

        /**
         * Step two of pooler initialization kept separate from initialization
         * of static members so that they may be set at a different point in 
         * the initialization (as sometimes needed by tests).
         * 
         * This step prepares the proximal dendritic synapse pools with their 
         * initial permanence values and connected inputs.
         * 
         * @param c     the {@link Connections} memory
         */
        public void connectAndConfigureInputs(Connections c)
        {
            // Initialize the set of permanence values for each column. Ensure that
            // each column is connected to enough input bits to allow it to be
            // activated.
            int numColumns = c.getNumColumns();
            for (int i = 0; i < numColumns; i++)
            {
                // Gets RF
                int[] potential = mapPotential(c, i, c.isWrapAround());
                Column column = c.getColumn(i);

                // This line initializes all synases in the potential pool of synapces.
                // After initialization permancences are set to zero.
                var potPool = column.createPotentialPool(c, potential);

                c.getPotentialPools().set(i, potPool);

                double[] perm = initPermanence(c, potential, i, c.getInitConnectedPct());

                updatePermanencesForColumn(c, perm, column, potential, true);
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c);
        }

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
        public void compute(Connections c, int[] inputVector, int[] activeArray, bool learn)
        {
            if (inputVector.Length != c.NumInputs)
            {
                throw new ArgumentException(
                        "Input array must be same size as the defined number of inputs: From Params: " + c.NumInputs +
                        ", From Input Vector: " + inputVector.Length);
            }

            updateBookeepingVars(c, learn);

            // Gets overlap over every single column.
            var overlaps = calculateOverlap(c, inputVector);

            //var overlapsStr = Helpers.StringifyVector(overlaps);
            //Debug.WriteLine("overlap: " + overlapsStr);

            //overlapActive = calculateOverlap(activeInput)
            //overlapPredictedActive = calculateOverlap(predictedActiveInput)
            //totalOverlap = overlapActive * weightActive + overlapPredictedActive * weightPredictedActive

            c.Overlaps = overlaps;

            double[] boostedOverlaps;

            //
            // We perform boosting here and right after that, we will recalculate bossted factors for next cycle.
            if (learn)
            {
                //Debug.WriteLine("Boosted Factor: " + c.BoostFactors);
                boostedOverlaps = ArrayUtils.multiply(c.BoostFactors, overlaps);
                //var boostedoverlapsStr = Helpers.StringifyVector(boostedOverlaps);
                //Debug.WriteLine("boosted overlap: " + boostedoverlapsStr);
            }
            else
            {
                boostedOverlaps = ArrayUtils.toDoubleArray(overlaps);
            }

            c.BoostedOverlaps = boostedOverlaps;

            int[] activeColumns = inhibitColumns(c, boostedOverlaps);

            if (learn)
            {
                adaptSynapses(c, inputVector, activeColumns);
                updateDutyCycles(c, overlaps, activeColumns);
                bumpUpWeakColumns(c);
                updateBoostFactors(c);
                if (isUpdateRound(c))
                {
                    updateInhibitionRadius(c);
                    updateMinDutyCycles(c);
                }
            }

            ArrayUtils.fillArray(activeArray, 0);
            if (activeColumns.Length > 0)
            {
                ArrayUtils.setIndexesTo(activeArray, activeColumns, 1);
            }
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
            //int numCols = c.getNumColumns();
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

            //int numCols = c.getNumColumns();
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
            return c.isWrapAround() ?
                c.getColumnTopology().wrappingNeighborhood(centerColumn, inhibitionRadius) :
                    c.getColumnTopology().GetNeighborhood(centerColumn, inhibitionRadius);
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
            int len = c.getNumColumns();
            int inhibitionRadius = c.InhibitionRadius;
            double[] activeDutyCycles = c.getActiveDutyCycles();
            double minPctActiveDutyCycles = c.getMinPctActiveDutyCycles();
            double[] overlapDutyCycles = c.getOverlapDutyCycles();
            double minPctOverlapDutyCycles = c.getMinPctOverlapDutyCycles();

            Parallel.For(0, len, (i) =>
            {
                int[] neighborhood = getColumnNeighborhood(c, i, inhibitionRadius);

                double maxActiveDuty = ArrayUtils.max(
                    ArrayUtils.ListOfValuesByIndicies(activeDutyCycles, neighborhood));
                double maxOverlapDuty = ArrayUtils.max(
                    ArrayUtils.ListOfValuesByIndicies(overlapDutyCycles, neighborhood));

                c.getMinActiveDutyCycles()[i] = maxActiveDuty * minPctActiveDutyCycles;

                c.getMinOverlapDutyCycles()[i] = maxOverlapDuty * minPctOverlapDutyCycles;
            });

            //// Parallelize for speed up
            //IntStream.range(0, len).forEach(i-> {
            //    int[] neighborhood = getColumnNeighborhood(c, i, inhibitionRadius);

            //    double maxActiveDuty = ArrayUtils.max(
            //        ArrayUtils.sub(activeDutyCycles, neighborhood));
            //    double maxOverlapDuty = ArrayUtils.max(
            //        ArrayUtils.sub(overlapDutyCycles, neighborhood));

            //    c.getMinActiveDutyCycles()[i] = maxActiveDuty * minPctActiveDutyCycles;

            //    c.getMinOverlapDutyCycles()[i] = maxOverlapDuty * minPctOverlapDutyCycles;
            //});
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
            double[] overlapArray = new double[c.getNumColumns()];

            // All active columns are set on 1, otherwise 0.
            double[] activeArray = new double[c.getNumColumns()];

            //
            // if (sourceA[i] > 0) then targetB[i] = 1;
            // This ensures that all values in overlapArray are set to 1, if column has some overlap.
            ArrayUtils.greaterThanXThanSetToYInB(overlaps, overlapArray, 0, 1);
            if (activeColumns.Length > 0)
            {
                // After this step, all rows in activeArray are set to 1 at the index of active column.
                ArrayUtils.setIndexesTo(activeArray, activeColumns, 1);
            }

            int period = c.getDutyCyclePeriod();
            if (period > c.getIterationNum())
            {
                period = c.getIterationNum();
            }
            //Debug.WriteLine("period is: " + period);
            c.setOverlapDutyCycles(updateDutyCyclesHelper(c, c.getOverlapDutyCycles(), overlapArray, period));

            c.setActiveDutyCycles(
                    updateDutyCyclesHelper(c, c.getActiveDutyCycles(), activeArray, period));
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
            return ArrayUtils.divide(ArrayUtils.d_add(ArrayUtils.multiply(dutyCycles, period - 1), newInput), period);
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
        public void updateInhibitionRadius(Connections c)
        {
            if (c.GlobalInhibition)
            {
                c.InhibitionRadius = ArrayUtils.max(c.getColumnDimensions());
                return;
            }

            List<double> avgCollected = new List<double>();
            int len = c.getNumColumns();
            for (int i = 0; i < len; i++)
            {
                avgCollected.Add(getAvgSpanOfConnectedSynapsesForColumn(c, i));
            }
            double avgConnectedSpan = ArrayUtils.average(avgCollected.ToArray());

            double diameter = avgConnectedSpan * avgColumnsPerInput(c);
            double radius = (diameter - 1) / 2.0d;
            radius = Math.Max(1, radius);

            c.InhibitionRadius = (int)(radius + 0.5);
        }

        /**
         * The average number of columns per input, taking into account the topology
         * of the inputs and columns. This value is used to calculate the inhibition
         * radius. This function supports an arbitrary number of dimensions. If the
         * number of column dimensions does not match the number of input dimensions,
         * we treat the missing, or phantom dimensions as 'ones'.
         *  
         * @param c     the {@link Connections} (spatial pooler memory)
         * @return
         */
        public virtual double avgColumnsPerInput(Connections c)
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
        public virtual double getAvgSpanOfConnectedSynapsesForColumn(Connections c, int columnIndex)
        {
            int[] dimensions = c.getInputDimensions();

            // Gets synapses connected to input bits.(from pool of the column)
            int[] connected = c.getColumn(columnIndex).getProximalDendrite().getConnectedSynapsesSparse(c);

            if (connected == null || connected.Length == 0) return 0;

            int[] maxCoord = new int[c.getInputDimensions().Length];
            int[] minCoord = new int[c.getInputDimensions().Length];
            ArrayUtils.fillArray(maxCoord, -1);
            ArrayUtils.fillArray(minCoord, ArrayUtils.max(dimensions));
            ISparseMatrix<int> inputMatrix = c.getInputMatrix();

            //
            // It takes all connected synapses
            // 
            for (int i = 0; i < connected.Length; i++)
            {
                maxCoord = ArrayUtils.maxBetween(maxCoord, inputMatrix.computeCoordinates(connected[i]));
                minCoord = ArrayUtils.minBetween(minCoord, inputMatrix.computeCoordinates(connected[i]));
            }
            return ArrayUtils.average(ArrayUtils.add(ArrayUtils.subtract(maxCoord, minCoord), 1));
        }

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
        public void adaptSynapses(Connections c, int[] inputVector, int[] activeColumns)
        {
            //int[] inputIndices = ArrayUtils.where(inputVector, ArrayUtils.INT_GREATER_THAN_0);

            // Get all indicies of input vector, which are set on '1'.
            var inputIndices = ArrayUtils.IndexWhere(inputVector, inpBit => inpBit > 0);

            double[] permChanges = new double[c.NumInputs];

            // First we initialize all permChanges to minimum decrement values,
            // which are used in a case of none-connections to input.
            ArrayUtils.fillArray(permChanges, -1 * c.getSynPermInactiveDec());
            //var permChangesStr = Helpers.StringifyVector(permChanges);
            //Debug.WriteLine("Initial Permance: " + permChangesStr);

            // Then we update all connected permChanges to increment values for connected values.
            // Permanences are set in conencted input bits to default incremental value.
            ArrayUtils.setIndexesTo(permChanges, inputIndices.ToArray(), c.getSynPermActiveInc());
            //permChangesStr = Helpers.StringifyVector(permChanges);
            //Debug.WriteLine("Initial Permance: " + permChangesStr);
            for (int i = 0; i < activeColumns.Length; i++)
            {
                Pool pool = c.getPotentialPools().get(activeColumns[i]);
                double[] perm = pool.getDensePermanences(c);
                int[] indexes = pool.getSparsePotential();
                ArrayUtils.raiseValuesBy(permChanges, perm);
                Column col = c.getColumn(activeColumns[i]);
                updatePermanencesForColumn(c, perm, col, indexes, true);
            }
            //Debug.WriteLine("Permance after update in adaptSynapses: " + permChangesStr);
        }

        /**
         * This method increases the permanence values of synapses of columns whose
         * activity level has been too low. Such columns are identified by having an
         * overlap duty cycle that drops too much below those of their peers. The
         * permanence values for such columns are increased.
         *  
         * @param c
         */
        public void bumpUpWeakColumns(Connections c)
        {
            //    int[] weakColumns = ArrayUtils.where(c.getMemory().get1DIndexes(), new Condition.Adapter<Integer>() {
            //        @Override public boolean eval(int i)
            //    {
            //        return c.getOverlapDutyCycles()[i] < c.getMinOverlapDutyCycles()[i];
            //    }
            //});

            var weakColumns = c.getMemory().get1DIndexes().Where(i => c.getOverlapDutyCycles()[i] < c.getMinOverlapDutyCycles()[i]).ToArray();
            //var weakColumnsStr = Helpers.StringifyVector(weakColumns);
            //Debug.WriteLine("weak Columns:" + weakColumnsStr);
            for (int i = 0; i < weakColumns.Length; i++)
            {
                Pool pool = c.getPotentialPools().get(weakColumns[i]);
                double[] perm = pool.getSparsePermanences();
                ArrayUtils.raiseValuesBy(c.getSynPermBelowStimulusInc(), perm);
                int[] indexes = pool.getSparsePotential();
                Column col = c.getColumn(weakColumns[i]);
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
        public virtual void raisePermanenceToThreshold(Connections c, double[] perm, int[] maskPotential)
        {
            if (maskPotential.Length < c.StimulusThreshold)
            {
                throw new ArgumentException("This is likely due to a " +
                    "value of stimulusThreshold that is too large relative " +
                    "to the input size. [len(mask) < self._stimulusThreshold]");
            }

            ArrayUtils.clip(perm, c.getSynPermMin(), c.getSynPermMax());
            while (true)
            {
                // Gets number of synapses with permanence value grather than 'PermConnected'.
                int numConnected = ArrayUtils.valueGreaterCountAtIndex(c.getSynPermConnected(), perm, maskPotential);
                if (numConnected >= c.StimulusThreshold)
                    return;

                // If number of note connected synapses, then permanences of all synapses will be incremented (raised).
                ArrayUtils.raiseValuesBy(c.getSynPermBelowStimulusInc(), perm, maskPotential);
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
         * Note: This method services the "sparse" versions of corresponding methods
         * 
         * @param c         The {@link Connections} memory
         * @param perm      permanence values
         */
        public void raisePermanenceToThresholdSparse(Connections c, double[] perm)
        {
            ArrayUtils.clip(perm, c.getSynPermMin(), c.getSynPermMax());
            while (true)
            {
                int numConnected = ArrayUtils.valueGreaterCount(c.getSynPermConnected(), perm);
                if (numConnected >= c.StimulusThreshold) return;
                ArrayUtils.raiseValuesBy(c.getSynPermBelowStimulusInc(), perm);
            }
        }

        /**
         * This method updates the permanence matrix with a column's new permanence
         * values. The column is identified by its index, which reflects the row in
         * the matrix, and the permanence is given in 'sparse' form, i.e. an array
         * whose members are associated with specific indexes. It is in
         * charge of implementing 'clipping' - ensuring that the permanence values are
         * always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out
         * all permanence values below 'synPermTrimThreshold'. It also maintains
         * the consistency between 'permanences' (the matrix storing the
         * permanence values), 'connectedSynapses', (the matrix storing the bits
         * each column is connected to), and 'connectedCounts' (an array storing
         * the number of input bits each column is connected to). Every method wishing
         * to modify the permanence matrix should do so through this method.
         * 
         * @param c                 the {@link Connections} which is the memory model.
         * @param perm              An array of permanence values for a column. The array is
         *                          "dense", i.e. it contains an entry for each input bit, even
         *                          if the permanence value is 0.
         * @param column            The column in the permanence, potential and connectivity matrices
         * @param maskPotential     The indexes of inputs in the specified {@link Column}'s pool.
         * @param raisePerm         a boolean value indicating whether the permanence values
         */
        public void updatePermanencesForColumn(Connections c, double[] perm, Column column, int[] maskPotential, bool raisePerm)
        {
            if (raisePerm)
            {
                // During every learning cycle, this method ensures that every column 
                // has enough connections ('SynPermConnected') to iput space.
                raisePermanenceToThreshold(c, perm, maskPotential);
            }

            // Here we set all permanences to 0 
            ArrayUtils.lessThanOrEqualXThanSetToY(perm, c.getSynPermTrimThreshold(), 0);
            ArrayUtils.clip(perm, c.getSynPermMin(), c.getSynPermMax());
            column.setProximalPermanences(c, perm);
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
            if (raisePerm)
            {
                raisePermanenceToThresholdSparse(c, perm);
            }

            ArrayUtils.lessThanOrEqualXThanSetToY(perm, c.getSynPermTrimThreshold(), 0);
            ArrayUtils.clip(perm, c.getSynPermMin(), c.getSynPermMax());
            column.setProximalPermanencesSparse(c, perm, maskPotential);
        }

        /**
         * Returns a randomly generated permanence value for a synapse that is
         * initialized in a connected state. The basic idea here is to initialize
         * permanence values very close to synPermConnected so that a small number of
         * learning steps could make it disconnected or connected.
         *
         * Note: experimentation was done a long time ago on the best way to initialize
         * permanence values, but the history for this particular scheme has been lost.
         * 
         * @return  a randomly generated permanence value
         */
        public static double initPermConnected(Connections c)
        {
            double p = c.getSynPermConnected() + (c.getSynPermMax() - c.getSynPermConnected()) * c.random.NextDouble();

            // Note from Python implementation on conditioning below:
            // Ensure we don't have too much unnecessary precision. A full 64 bits of
            // precision causes numerical stability issues across platforms and across
            // implementations
            p = ((int)(p * 100000)) / 100000.0d;
            return p;
        }

        /**
         * Returns a randomly generated permanence value for a synapses that is to be
         * initialized in a non-connected state.
         * 
         * @return  a randomly generated permanence value
         */
        public static double initPermNonConnected(Connections c)
        {
            double p = c.getSynPermConnected() * c.getRandom().NextDouble();

            // Note from Python implementation on conditioning below:
            // Ensure we don't have too much unnecessary precision. A full 64 bits of
            // precision causes numerical stability issues across platforms and across
            // implementations
            p = ((int)(p * 100000)) / 100000.0d;
            return p;
        }

        /**
         * Initializes the permanences of a column. The method
         * returns a 1-D array the size of the input, where each entry in the
         * array represents the initial permanence value between the input bit
         * at the particular index in the array, and the column represented by
         * the 'index' parameter.
         * 
         * @param c                 the {@link Connections} which is the memory model
         * @param potentialPool     An array specifying the potential pool of the column.
         *                          Permanence values will only be generated for input bits
         *                          corresponding to indices for which the mask value is 1.
         *                          WARNING: potentialPool is sparse, not an array of "1's"
         * @param index             the index of the column being initialized
         * @param connectedPct      A value between 0 or 1 specifying the percent of the input
         *                          bits that might maximally start off in a connected state.
         *                          0.7 means, maximally 70% of potential might be connected
         * @return
         */
        public double[] initPermanence(Connections c, int[] potentialPool, int colIndx, double connectedPct)
        {
            double[] perm = new double[c.NumInputs];
            foreach (int idx in potentialPool)
            {
                if (c.random.NextDouble() <= connectedPct)
                {
                    perm[idx] = initPermConnected(c);
                }
                else
                {
                    perm[idx] = initPermNonConnected(c);
                }

                perm[idx] = perm[idx] < c.getSynPermTrimThreshold() ? 0 : perm[idx];

            }
            c.getColumn(colIndx).setProximalPermanences(c, perm);
            return perm;
        }

        /**
         * Uniform Column Mapping 
         * Maps a column to its respective input index, keeping to the topology of
         * the region. It takes the index of the column as an argument and determines
         * what is the index of the flattened input vector that is to be the center of
         * the column's potential pool. It distributes the columns over the inputs
         * uniformly. The return value is an integer representing the index of the
         * input bit. Examples of the expected output of this method:
         * * If the topology is one dimensional, and the column index is 0, this
         *   method will return the input index 0. If the column index is 1, and there
         *   are 3 columns over 7 inputs, this method will return the input index 3.
         * * If the topology is two dimensional, with column dimensions [3, 5] and
         *   input dimensions [7, 11], and the column index is 3, the method
         *   returns input index 8. 
         *   
         * @param columnIndex   The index identifying a column in the permanence, potential
         *                      and connectivity matrices.
         * @return              Flat index of mapped column.
         */
        public int mapColumn(Connections c, int columnIndex)
        {
            int[] columnCoords = c.getMemory().computeCoordinates(columnIndex);
            double[] colCoords = ArrayUtils.toDoubleArray(columnCoords);

            double[] columnRatios = ArrayUtils.divide(
                colCoords, ArrayUtils.toDoubleArray(c.getColumnDimensions()), 0, 0);

            double[] inputCoords = ArrayUtils.multiply(
                ArrayUtils.toDoubleArray(c.getInputDimensions()), columnRatios, 0, 0);

            var colSpanOverInputs = ArrayUtils.divide(
                        ArrayUtils.toDoubleArray(c.getInputDimensions()),
                        ArrayUtils.toDoubleArray(c.getColumnDimensions()), 0, 0);

            inputCoords = ArrayUtils.d_add(inputCoords, ArrayUtils.multiply(colSpanOverInputs, 0.5));

            // Makes sure that inputCoords are in range [0, inpDims]
            int[] inputCoordInts = ArrayUtils.clip(ArrayUtils.toIntArray(inputCoords), c.getInputDimensions(), -1);

            return c.getInputMatrix().computeIndex(inputCoordInts);
        }

        /**
         * Maps a column to its input bits. This method encapsulates the topology of
         * the region. It takes the index of the column as an argument and determines
         * what are the indices of the input vector that are located within the
         * column's potential pool. The return value is a list containing the indices
         * of the input bits. The current implementation of the base class only
         * supports a 1 dimensional topology of columns with a 1 dimensional topology
         * of inputs. To extend this class to support 2-D topology you will need to
         * override this method. Examples of the expected output of this method:
         * * If the potentialRadius is greater than or equal to the entire input
         *   space, (global visibility), then this method returns an array filled with
         *   all the indices
         * * If the topology is one dimensional, and the potentialRadius is 5, this
         *   method will return an array containing 5 consecutive values centered on
         *   the index of the column (wrapping around if necessary).
         * * If the topology is two dimensional (not implemented), and the
         *   potentialRadius is 5, the method should return an array containing 25
         *   '1's, where the exact indices are to be determined by the mapping from
         *   1-D index to 2-D position.
         * 
         * @param c             {@link Connections} the main memory model
         * @param columnIndex   The index identifying a column in the permanence, potential
         *                      and connectivity matrices.
         * @param wrapAround    A boolean value indicating that boundaries should be
         *                      ignored.
         * @return
         */
        public int[] mapPotential(Connections c, int columnIndex, bool wrapAround)
        {
            int centerInput = mapColumn(c, columnIndex);

            // Here we have Receptive Field (RF)
            int[] columnInputs = getInputNeighborhood(c, centerInput, c.getPotentialRadius());

            //Debug.WriteLine($"{Helpers.StringifyVector(columnInputs)}");

            // Select a subset of the receptive field to serve as the the potential pool.
            int numPotential = (int)(columnInputs.Length * c.getPotentialPct() + 0.5);
            int[] retVal = new int[numPotential];
            return ArrayUtils.sample(columnInputs, retVal, c.getRandom());
        }


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
                inhibitionArea = Math.Min(c.getNumColumns(), inhibitionArea);

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
            return inhibitColumnsLocal(c, overlaps, density);
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
            int numCols = c.getNumColumns();
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

        public virtual int[] inhibitColumnsLocal(Connections c, double[] overlaps, double density)
        {
            return inhibitColumnsLocalOriginal(c, overlaps, density);
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
        public virtual int[] inhibitColumnsLocalOriginal(Connections c, double[] overlaps, double density)
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
        public void updateBoostFactors(Connections c)
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


            //        int[] mask = ArrayUtils.where(minActiveDutyCycles, ArrayUtils.GREATER_THAN_0);

            double[] boostInterim;

            //
            // Boost factors are NOT recalculated if minimum active duty cycles are all set on 0.
            if (mask.Count < 1)
            {
                boostInterim = c.BoostFactors;
            }
            else
            {
                double[] oneMinusMaxBoostFact = new double[c.getNumColumns()];
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

            ArrayUtils.setIndexesTo(boostInterim, filteredIndexes.ToArray(), 1.0d);
            //var boostInterimStr = Helpers.StringifyVector(boostInterim);


            //    ArrayUtils.setIndexesTo(boostInterim, ArrayUtils.where(activeDutyCycles, new Condition.Adapter<Object>() {
            //        int i = 0;
            //    @Override public boolean eval(double d) { return d > minActiveDutyCycles[i++]; }
            //}), 1.0d);
            //Debug.WriteLine("new boost factor:" + boostInterimStr);
            c.BoostFactors = boostInterim;
        }

        /**
         * This function determines each column's overlap with the current input
         * vector. The overlap of a column is the number of synapses for that column
         * that are connected (permanence value is greater than '_synPermConnected')
         * to input bits which are turned on. Overlap values that are lower than
         * the 'stimulusThreshold' are ignored. The implementation takes advantage of
         * the SpraseBinaryMatrix class to perform this calculation efficiently.
         *  
         * @param c             the {@link Connections} memory encapsulation
         * @param inputVector   an input array of 0's and 1's that comprises the input to
         *                      the spatial pooler.
         * @return
         */
        public int[] calculateOverlap(Connections c, int[] inputVector)
        {
            int[] overlaps = new int[c.getNumColumns()];
            c.getConnectedCounts().rightVecSumAtNZ(inputVector, overlaps, c.StimulusThreshold);
            return overlaps;
        }

        /**
         * Return the overlap to connected counts ratio for a given column
         * @param c
         * @param overlaps
         * @return
         */
        public double[] calculateOverlapPct(Connections c, int[] overlaps)
        {
            return ArrayUtils.divide(overlaps, c.getConnectedCounts().getTrueCounts());
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



        /**
         * Gets a neighborhood of inputs.
         * 
         * Simply calls topology.wrappingNeighborhood or topology.neighborhood.
         * 
         * A subclass can insert different topology behavior by overriding this method.
         * 
         * @param c                     the {@link Connections} memory encapsulation
         * @param centerInput           The center of the neighborhood.
         * @param potentialRadius       Span of the input field included in each neighborhood
         * @return                      The input's in the neighborhood. (1D)
         */
        public int[] getInputNeighborhood(Connections c, int centerInput, int potentialRadius)
        {
            return c.isWrapAround() ?
                c.getInputTopology().wrappingNeighborhood(centerInput, potentialRadius) :
                    c.getInputTopology().GetNeighborhood(centerInput, potentialRadius);
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

