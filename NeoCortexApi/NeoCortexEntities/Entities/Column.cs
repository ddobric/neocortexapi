// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System.Linq;
using System.Diagnostics;
using NeoCortexEntities.NeuroVisualizer;

namespace NeoCortexApi.Entities
{


    /// <summary>
    /// Implementation of the mini-column.
    /// </summary>
    /// <remarks>
    /// Authors of the JAVA implementation:Chetan Surpur, David Ray
    /// </remarks>
    public class Column : IEquatable<Column>, IComparable<Column>
    {
        public AbstractSparseBinaryMatrix connectedInputCounter;

        public AbstractSparseBinaryMatrix ConnectedInputCounterMatrix { get { return connectedInputCounter; } set { connectedInputCounter = value; } }

        public int[] ConnectedInputBits { get => (int[])this.connectedInputCounter.GetSlice(0); }

        /// <summary>
        /// Column index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Dendrites connected to <see cref="SpatialPooler"/> input neural cells.
        /// </summary>
        public ProximalDendrite ProximalDendrite { get; set; }

        /// <summary>
        /// All cells of the column.
        /// </summary>
        public Cell[] Cells { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CellId { get; }

        //private ReadOnlyCollection<Cell> cellList;

        private readonly int hashcode;

        public Column()
        {

        }

        /// <summary>
        /// Creates a new collumn with specified number of cells and a single proximal dendtrite segment.
        /// </summary>
        /// <param name="numCells">Number of cells in the column.</param>
        /// <param name="colIndx">Column index.</param>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public Column(int numCells, int colIndx, double synapsePermConnected, int numInputs)
        {
            this.Index = colIndx;

            this.hashcode = GetHashCode();

            Cells = new Cell[numCells];

            for (int i = 0; i < numCells; i++)
            {
                Cells[i] = new Cell(this.Index, i, this.GetNumCellsPerColumn(), this.CellId, CellActivity.ActiveCell);
            }

            // We keep tracking of this column only
            this.connectedInputCounter = new SparseBinaryMatrix(new int[] { 1, numInputs });

            ProximalDendrite = new ProximalDendrite(colIndx, synapsePermConnected, numInputs);

            this.ConnectedInputCounterMatrix = new SparseBinaryMatrix(new int[] { 1, numInputs });
        }


        /// <summary>
        /// Returns the configured number of cells per column for all <see cref="Column"/> objects within the current {@link TemporalMemory}
        /// </summary>
        /// <returns></returns>
        public int GetNumCellsPerColumn()
        {
            return Cells.Length;
        }

        /// <summary>
        /// Returns the <see cref="Cell"/> with the least number of <see cref="DistalDendrite"/>s.
        /// </summary>
        /// <param name="c">the connections state of the temporal memory</param>
        /// <param name="random"></param>
        /// <returns></returns>
        public Cell GetLeastUsedCell(Connections c, Random random)
        {
            List<Cell> leastUsedCells = new List<Cell>();
            int minNumSegments = Integer.MaxValue;

            foreach (var cell in Cells)
            {
                //DD
                //int numSegments = cell.GetSegments(c).Count;
                int numSegments = cell.DistalDendrites.Count;
                //int numSegments = cell.Segments.Count;

                if (numSegments < minNumSegments)
                {
                    minNumSegments = numSegments;
                    leastUsedCells.Clear();
                }

                if (numSegments == minNumSegments)
                {
                    leastUsedCells.Add(cell);
                }
            }

            int index = random.Next(leastUsedCells.Count);
            leastUsedCells.Sort();
            return leastUsedCells[index];
        }

        /// <summary>
        /// Creates connections between columns and inputs.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="inputVectorIndexes"></param>
        /// <param name="startSynapseIndex"></param>
        /// <returns></returns>
        public Pool CreatePotentialPool(HtmConfig htmConfig, int[] inputVectorIndexes, int startSynapseIndex)
        {
            //var pool = ProximalDendrite.createPool(c, inputVectorIndexes);
            this.ProximalDendrite.Synapses.Clear();

            var pool = new Pool(inputVectorIndexes.Length, htmConfig.NumInputs);

            this.ProximalDendrite.RFPool = pool;

            for (int i = 0; i < inputVectorIndexes.Length; i++)
            {
                //var cnt = c.getProximalSynapseCount();
                //var synapse = createSynapse(c, c.getSynapses(this), null, this.RFPool, synCount, inputIndexes[i]);
                var synapse = this.ProximalDendrite.CreateSynapse(null, startSynapseIndex + i, inputVectorIndexes[i]);
                this.SetPermanence(synapse, htmConfig.SynPermConnected, 0);
                //c.setProximalSynapseCount(cnt + 1);
            }

            //var mem = c.getMemory();

            //mem.set(this.Index, this);

            //c.getPotentialPools().set(this.Index, pool);

            return pool;
        }

        public void SetPermanence(Synapse synapse, double synPermConnected, double perm)
        {
            synapse.Permanence = perm;

            // On proximal dendrite which has no presynaptic cell
            if (synapse.SourceCell == null)
            {
                this.ProximalDendrite.RFPool.UpdatePool(synPermConnected, synapse, perm);
            }
        }

        /// <summary>
        /// Sets the permanences for each <see cref="Synapse"/>. The number of synapses is set by the potentialPct variable which determines the number of input
        /// bits a given column will be "attached" to which is the same number as the number of <see cref="Synapse"/>s
        /// </summary>
        /// <param name="htmConfig">the <see cref="Connections"/> memory</param>
        /// <param name="perms">the floating point degree of connectedness</param>
        public void SetPermanences(HtmConfig htmConfig, double[] perms)
        {
            //var connCounts = c.getConnectedCounts();

            this.ProximalDendrite.RFPool.ResetConnections();

            // Every column contians a single row at index 0.
            this.ConnectedInputCounterMatrix.ClearStatistics(0 /*this.Index*/);

            foreach (Synapse s in this.ProximalDendrite.Synapses)
            {
                this.SetPermanence(s, htmConfig.SynPermConnected, perms[s.InputIndex]);

                if (perms[s.InputIndex] >= htmConfig.SynPermConnected)
                {
                    this.ConnectedInputCounterMatrix.set(1, 0 /*this.Index*/, s.InputIndex);
                }
            }
        }

        /**
         * Sets the permanences on the {@link ProximalDendrite} {@link Synapse}s
         * 
         * @param c				the {@link Connections} memory object
         * @param permanences	floating point degree of connectedness
         */
        //public void setProximalPermanences(Connections c, double[] permanences)
        //{
        //    ProximalDendrite.setPermanences(c, permanences);
        //}

        // TODO better parameters documentation
        /**
         * 
         * 
         * @param c				the {@link Connections} memory object
         * @param permanences	
         */
        /// <summary>
        /// Sets the permanences on the <see cref="ProximalDendrite"/> <see cref="Synapse"/>s
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="permanences">floating point degree of connectedness</param>
        /// <param name="inputVectorIndexes"></param>
        public void setProximalPermanencesSparse(HtmConfig htmConfig, double[] permanences, int[] inputVectorIndexes)
        {
            this.ProximalDendrite.SetPermanences(this.ConnectedInputCounterMatrix, htmConfig, permanences, inputVectorIndexes);
        }

        // TODO better parameters documentation
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
        /// <summary>
        /// This method updates the permanence matrix with a column's new permanence values. The column is identified by its index, which reflects the row in
        /// the matrix, and the permanence is given in 'sparse' form, (i.e. an array whose members are associated with specific indexes). It is in charge of 
        /// implementing 'clipping' - ensuring that the permanence values are always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out all 
        /// permanence values below 'synPermTrimThreshold'. Every method wishing to modify the permanence matrix should do so through this method.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="perm">An array of permanence values for a column. The array is "sparse", i.e. it contains an entry for each input bit, even if the permanence value is 0.</param>
        /// <param name="maskPotential"></param>
        /// <param name="raisePerm">a boolean value indicating whether the permanence values</param>
        public void UpdatePermanencesForColumnSparse(HtmConfig htmConfig, double[] perm, int[] maskPotential, bool raisePerm)
        {
            if (raisePerm)
            {
                HtmCompute.RaisePermanenceToThresholdSparse(htmConfig, perm);
            }

            ArrayUtils.LessOrEqualXThanSetToY(perm, htmConfig.SynPermTrimThreshold, 0);
            ArrayUtils.Clip(perm, htmConfig.SynPermMin, htmConfig.SynPermMax);
            setProximalPermanencesSparse(htmConfig, perm, maskPotential);
        }


        /// <summary>
        /// Calculates the overlapp of the column.
        /// </summary>
        /// <param name="inputVector"></param>
        /// <param name="stimulusThreshold"></param>
        /// <returns></returns>
        public int GetColumnOverlapp(int[] inputVector, double stimulusThreshold)
        {
            int result = 0;

            // Gets the synapse mapping between column-i with input vector.
            int[] slice = (int[])this.connectedInputCounter.GetSlice(0);

            // Go through all connections (synapses) between column and input vector.
            for (int inpBitIndx = 0; inpBitIndx < slice.Length; inpBitIndx++)
            {
                // Result (overlapp) is 1 if 
                result += (inputVector[inpBitIndx] * slice[inpBitIndx]);
                //TODO: check if this is needed!
                if (inpBitIndx == slice.Length - 1)
                {
                    // If the overlap (num of connected synapses to TRUE input) is less than stimulusThreshold then we set result on 0.
                    // If the overlap (num of connected synapses to TRUE input) is greather than stimulusThreshold then result remains as calculated.
                    // This ensures that only overlaps are calculated, which are over the stimulusThreshold. All less than stimulusThreshold are set on 0.
                    result -= result < stimulusThreshold ? result : 0;
                }
            }

            //string strInp = StringifyVector(ArrayUtils.IndexWhere(inputVector, i => i == 1));

            //int numConnectedSynapses = slice.Count(sl => sl == 1);

            //Debug.WriteLine($"Overlap: {result} - numConnectedSynapseso: {numConnectedSynapses} - strInp: {strInp}");


            return result;
        }

        /// <summary>
        /// Creates the string representation of the given vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static string StringifyVector(int[] vector)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var vectorBit in vector)
            {
                sb.Append(vectorBit);
                sb.Append(", ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Delegates the call to set synapse connected indexes to this <see cref="ProximalDendrite"/>
        /// </summary>
        /// <param name="c"></param>
        /// <param name="inputVectorIndexes"></param>
        public void SetProximalConnectedSynapsesForTest(Connections c, int[] inputVectorIndexes)
        {
            //var synapseIndex = c.getProximalSynapseCount();
            //c.setProximalSynapseCount(synapseIndex + inputVectorIndexes.Length);
            this.ProximalDendrite.RFPool = CreatePotentialPool(c.HtmConfig, inputVectorIndexes, -1);
            //ProximalDendrite.setConnectedSynapsesForTest(c, connections);
        }


        /**
         * {@inheritDoc}
         * @param otherColumn     the {@code Column} to compare to
         * @return
         */
        //@Override
        //public int compareTo(Column otherColumn)
        //    {
        //        return boxedIndex(otherColumn.boxedIndex);
        //    }


        private readonly int m_Hashcode;


        public override int GetHashCode()
        {
            if (m_Hashcode == 0)
            {
                int prime = 31;
                int result = 1;
                result = prime * result + Index;
                return result;
            }
            return m_Hashcode;
        }

        public bool Equals(Column obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;

            if (Index != obj.Index)
                return false;
            else
                return true;
        }

        public int CompareTo(Column other)
        {
            if (this.Index < other.Index)
                return -1;
            else if (this.Index > other.Index)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Gets readable version of cell.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Column: Indx:{this.Index}, Cells:{this.Cells.Length}";
        }
    }
}
