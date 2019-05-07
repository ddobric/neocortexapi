using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NeoCortexApi.Entities;

namespace NeoCortexApi.Entities
{

    /**
     * Abstraction of both an input bit and a columnal collection of
     * {@link Cell}s which have behavior associated with membership to
     * a given {@code Column}
     * 
     * @author Chetan Surpur
     * @author David Ray
     *
     */
     //[Serializable]
    public class Column : IEquatable<Column>, IComparable<Column>
    {
      
        /// <summary>
        /// Column index
        /// </summary>
        public int Index { get; set; }

        /** Stored boxed form to eliminate need for boxing on the fly */
       // private readonly Integer boxedIndex;
        /** Configuration of cell count */
        //private readonly int numCells;

        /// <summary>
        /// Dendrites connected to <see cref="SpatialPooler"/> input pools
        /// </summary>
        public ProximalDendrite ProximalDendrite { get; set; }

        /// <summary>
        /// All cells of the column.
        /// </summary>
        public Cell[] Cells { get; set; }

        //private ReadOnlyCollection<Cell> cellList;

        private readonly int hashcode;

        public Column()
        {

        }

        /// <summary>
        /// Creates a new collumn with specified number of cells and a single proximal dendtrite segment.
        /// </summary>
        /// <param name="numCells">Number of cells in the column.</param>
        /// <param name="index">Colun index.</param>
        /// <param name="synapsePermConnected">Permanence threshold value to declare synapse as connected.</param>
        /// <param name="numInputs">Number of input neorn cells.</param>
        public Column(int numCells, int index, double synapsePermConnected, int numInputs)
        {
            //this.numCells = numCells;
            this.Index = index;
            //this.boxedIndex = index;
            this.hashcode = GetHashCode();
            Cells = new Cell[numCells];
            for (int i = 0; i < numCells; i++)
            {
                Cells[i] = new Cell(this, i);
            }

            //cellList = new ReadOnlyCollection<Cell>(Cells);

            ProximalDendrite = new ProximalDendrite(index, synapsePermConnected, numInputs);
        }

        /**
         * Returns the {@link Cell} residing at the specified index.
         * <p>
         * <b>IMPORTANT NOTE:</b> the index provided is the index of the Cell within this
         * column and is <b>not</b> the actual index of the Cell within the total
         * list of Cells of all columns. Each Cell maintains it's own <i><b>GLOBAL</i></b>
         * index which is the index describing the occurrence of a cell within the
         * total list of all cells. Thus, {@link Cell#getIndex()} returns the <i><b>GLOBAL</i></b>
         * index and <b>not</b> the index within this column.
         * 
         * @param index     the index of the {@link Cell} to return.
         * @return          the {@link Cell} residing at the specified index.
         */
        //public Cell getCell(int index)
        //{
        //    return Cells[index];
        //}

        

        /**
         * Returns the index of this {@code Column}
         * @return  the index of this {@code Column}
         */
        public int getIndex()
        {
            return Index;
        }

        /**
         * Returns the configured number of cells per column for
         * all {@code Column} objects within the current {@link TemporalMemory}
         * @return
         */
        public int getNumCellsPerColumn()
        {
            return Cells.Length;
        }

        /**
         * Returns the {@link Cell} with the least number of {@link DistalDendrite}s.
         * 
         * @param c         the connections state of the temporal memory
         * @param random
         * @return
         */
        public Cell getLeastUsedCell(Connections c, Random random)
        {
            List<Cell> leastUsedCells = new List<Cell>();
            int minNumSegments = Integer.MaxValue;

            foreach (var cell in Cells)
            {
                
                int numSegments = cell.getSegments(c).Count;

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

        /**
         * Returns this {@code Column}'s single {@link ProximalDendrite}
         * @return
         */
        public ProximalDendrite getProximalDendrite()
        {
            return ProximalDendrite;
        }

        /**
         * This method creates connections between columns and inputs.
         * It delegates the potential synapse creation to the one {@link ProximalDendrite}.
         * 
         * @param c						the {@link Connections} memory
         * @param inputVectorIndexes	indexes specifying the input vector bit
         */
        public Pool createPotentialPool(Connections c, int[] inputVectorIndexes)
        {
            //var pool = ProximalDendrite.createPool(c, inputVectorIndexes);
            this.ProximalDendrite.Synapses.Clear();
            var pool = new Pool(inputVectorIndexes.Length, c.NumInputs);
            for (int i = 0; i < inputVectorIndexes.Length; i++)
            {
                var cnt = c.getProximalSynapseCount();
                //var synapse = createSynapse(c, c.getSynapses(this), null, this.RFPool, synCount, inputIndexes[i]);
                var synapse = this.ProximalDendrite.createSynapse(null, pool, cnt, inputVectorIndexes[i]);
                synapse.setPermanence(c.getSynPermConnected(), 0);
                c.setProximalSynapseCount(cnt + 1);
            }

            this.ProximalDendrite.RFPool = pool;

            c.getPotentialPools().set(this.Index, pool);

            return pool;
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

        /**
         * Sets the permanences on the {@link ProximalDendrite} {@link Synapse}s
         * 
         * @param c				the {@link Connections} memory object
         * @param permanences	floating point degree of connectedness
         */
        public void setProximalPermanencesSparse(Connections c, double[] permanences, int[] inputVectorIndexes)
        {
            ProximalDendrite.setPermanences(c, permanences, inputVectorIndexes);
           
        }

        /**
         * Delegates the call to set synapse connected indexes to this 
         * {@code Column}'s {@link ProximalDendrite}
         * @param c
         * @param connections
         */
        public void setProximalConnectedSynapsesForTest(Connections c, int[] inputVectorIndexes)
        {
            this.ProximalDendrite.RFPool = createPotentialPool(c, inputVectorIndexes);
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
            return $"Column: Indx:{this.getIndex()}, Cells:{this.Cells.Length}";
        }
    }
}
