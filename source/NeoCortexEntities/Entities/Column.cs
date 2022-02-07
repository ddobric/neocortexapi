// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Utility;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Implementation of the mini-column.
    /// </summary>
    public class Column : IEquatable<Column>, IComparable<Column>
    {
        public AbstractSparseBinaryMatrix connectedInputCounter;

        /// <summary>
        /// TODO: There is no need for this matrix. It should be removed. All required synapses atr no in the Proximal Dendrite Segment => Pool.
        /// </summary>
        public AbstractSparseBinaryMatrix ConnectedInputCounterMatrix { get { return connectedInputCounter; } set { connectedInputCounter = value; } }

        public int[] ConnectedInputBits
        {
            get
            {
                if (connectedInputCounter != null)
                    return (int[])this.connectedInputCounter.GetSlice(0);
                else
                    return new int[0];
            }
        }

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
        /// CellId
        /// </summary>
        public int CellId { get; set; }

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
        /// Creates connections between mini-columns and input neurons.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="inputVectorIndexes">Sensory cells providing spatial input that will be learned by SP.</param>
        /// <param name="startSynapseIndex">Starting index.</param>
        /// <returns></returns>
        public Pool CreatePotentialPool(HtmConfig htmConfig, int[] inputVectorIndexes, int startSynapseIndex)
        {
            this.ProximalDendrite.Synapses.Clear();

            var pool = new Pool(inputVectorIndexes.Length, htmConfig.NumInputs);

            this.ProximalDendrite.RFPool = pool;

            for (int i = 0; i < inputVectorIndexes.Length; i++)
            {
                var synapse = this.ProximalDendrite.CreateSynapse(startSynapseIndex + i, inputVectorIndexes[i]);

                // All permanences are at the begining set to 0.
                this.SetPermanence(synapse, htmConfig.SynPermConnected, 0);
            }

            return pool;
        }

        /// <summary>
        /// Used by SpatialPooler when learning spatial patterns. Spatial patterns are learned by synapses between 
        /// proximal dendrite segment and input neurons.
        /// </summary>
        /// <param name="synapse"></param>
        /// <param name="synPermConnected">The synapse is the connected oneif its permanence value is greather than this threshold.</param>
        /// <param name="perm">The permanence value of the synapse.</param>
        private void SetPermanence(Synapse synapse, double synPermConnected, double perm)
        {
            synapse.Permanence = perm;

            //
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
            this.ProximalDendrite.RFPool.ResetConnections();

            // Every column contians a single row at index 0.
            this.ConnectedInputCounterMatrix.ClearStatistics(0);

            foreach (Synapse synapse in this.ProximalDendrite.Synapses)
            {
                this.SetPermanence(synapse, htmConfig.SynPermConnected, perms[synapse.InputIndex]);

                if (perms[synapse.InputIndex] >= htmConfig.SynPermConnected)
                {
                    this.ConnectedInputCounterMatrix.set(1, 0 /*this.Index*/, synapse.InputIndex);
                }
            }
        }


        /// <summary>
        /// Sets the permanences on the <see cref="ProximalDendrite"/> <see cref="Synapse"/>s
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="permanences">floating point degree of connectedness</param>
        /// <param name="inputVectorIndexes"></param>
        public void SetProximalPermanencesSparse(HtmConfig htmConfig, double[] permanences, int[] inputVectorIndexes)
        {
            this.ProximalDendrite.SetPermanences(this.ConnectedInputCounterMatrix, htmConfig, permanences, inputVectorIndexes);
        }

        /// <summary>
        /// This method updates the permanence matrix with a column's new permanence values. The column is identified by its index, which reflects the row in
        /// the matrix, and the permanence is given in 'sparse' form, (i.e. an array whose members are associated with specific indexes). It is in charge of 
        /// implementing 'clipping' - ensuring that the permanence values are always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out all 
        /// permanence values below 'synPermTrimThreshold'. Every method wishing to modify the permanence matrix should do so through this method.
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="perm">An array of permanence values for a column. The array is "sparse", i.e. it contains an entry for each input bit, even if the permanence value is 0.</param>
        /// <param name="maskPotential">Indexes of potential connections to input neurons.</param>
        /// <param name="raisePerm">a boolean value indicating whether the permanence values</param>
        public void UpdatePermanencesForColumnSparse(HtmConfig htmConfig, double[] perm, int[] maskPotential, bool raisePerm)
        {
            if (raisePerm)
            {
                HtmCompute.RaisePermanenceToThresholdSparse(htmConfig, perm);
            }

            ArrayUtils.LessOrEqualXThanSetToY(perm, htmConfig.SynPermTrimThreshold, 0);
            ArrayUtils.EnsureBetweenMinAndMax(perm, htmConfig.SynPermMin, htmConfig.SynPermMax);
            SetProximalPermanencesSparse(htmConfig, perm, maskPotential);
        }

        /// <summary>
        /// Trace synapse permanences.
        /// </summary>
        /// <returns></returns>
        public string Trace()
        {
            double permSum = 0.0;

            StringBuilder sb = new StringBuilder();

            foreach (var syn in this.ProximalDendrite.Synapses)
            {
                sb.AppendLine($"{syn.InputIndex} - {syn.Permanence}");
                permSum += syn.Permanence;
            }

            sb.AppendLine($"Col: {this.Index}\t Synapses: {this.ProximalDendrite.Synapses.Count} \t PermSum: {permSum}");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the min,max and avg permanence of all connected synapses.
        /// </summary>
        /// <returns></returns>
        public HtmStatistics GetStatistics(HtmConfig config)
        {
            double permSum = 0.0;

            double max = 0.0;

            double min = 2.0;

            int connectedSynapses = 0;

            foreach (var syn in this.ProximalDendrite.Synapses.Where(s => s.Permanence > config.SynPermConnected))
            {
                permSum += syn.Permanence;

                if (syn.Permanence < min)
                    min = syn.Permanence;

                if (syn.Permanence > max)
                    max = syn.Permanence;

                connectedSynapses++;
            }

            return new HtmStatistics
            {
                SynapticActivity = (double)connectedSynapses / (double)this.ProximalDendrite.Synapses.Count,
                AvgPermanence = permSum / connectedSynapses,
                MinPermanence = min,
                MaxPermanence = max,
                ConnectedSynapses = connectedSynapses,
                Synapses = this.ProximalDendrite.Synapses.Count
            };
        }

        /// <summary>
        /// Calculates the overlapp of the column.
        /// </summary>
        /// <param name="inputVector"></param>
        /// <param name="stimulusThreshold">Overlap will be 0 if it is less than this value.</param>
        /// <returns>The overlap of the column. 0 if it is less than stimulus threshold.</returns>
        public int CalcMiniColumnOverlap(int[] inputVector, double stimulusThreshold)
        {
            int result = 0;

            // Gets the synapse map  between this column and the input vector.
            int[] synMap = (int[])this.connectedInputCounter.GetSlice(0);

            //
            // Step through all synapses between the mini-column and input vector.
            for (int inpBitIndx = 0; inpBitIndx < synMap.Length; inpBitIndx++)
            {
                // Result (overlap) is 1 if input bit is 1 and the mini-column is connected.
                result += (inputVector[inpBitIndx] * synMap[inpBitIndx]);

                //
                // After the overlap is calculated, we set it on 0 if it is under stimulus threshold.
                if (inpBitIndx == synMap.Length - 1)
                {
                    // If the overlap (num of connected synapses to TRUE input) is less than stimulusThreshold then we set result on 0.
                    // If the overlap (num of connected synapses to TRUE input) is greather than stimulusThreshold then result remains as calculated.
                    // This ensures that only overlaps are calculated, which are over the stimulusThreshold. All less than stimulusThreshold are set on 0.
                    result -= result < stimulusThreshold ? result : 0;
                }
            }

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

            if (connectedInputCounter == null)
            {
                if (obj.connectedInputCounter != null)
                    return false;
            }
            else if (!connectedInputCounter.Equals(obj.connectedInputCounter))
                return false;
            if (ConnectedInputCounterMatrix == null)
            {
                if (obj.ConnectedInputCounterMatrix != null)
                    return false;
            }
            else if (!ConnectedInputCounterMatrix.Equals(obj.ConnectedInputCounterMatrix))
                return false;
            if (ProximalDendrite == null)
            {
                if (obj.ProximalDendrite != null)
                    return false;
            }
            else if (!ProximalDendrite.Equals(obj.ProximalDendrite))
                return false;
            if (obj.Cells != null && Cells != null)
            {

                if (!obj.Cells.SequenceEqual(Cells))
                    return false;
            }
            if (Index != obj.Index)
                return false;
            if (CellId != obj.CellId)
                return false;

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
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(Column), writer);

            ser.SerializeValue(this.CellId, writer);
            ser.SerializeValue(this.Index, writer);


            if (this.connectedInputCounter != null)
            {
                this.connectedInputCounter.Serialize(writer);
            }

            if (this.ConnectedInputCounterMatrix != null)
            {
                this.ConnectedInputCounterMatrix.Serialize(writer);
            }

            if (this.ProximalDendrite != null)
            {
                this.ProximalDendrite.Serialize(writer);
            }
            ser.SerializeValue(this.Cells, writer);
            ser.SerializeEnd(nameof(Column), writer);
        }
        public static Column Deserialize(StreamReader sr)
        {
            Column column = new Column();

            HtmSerializer2 ser = new HtmSerializer2();


            while (!sr.EndOfStream)
            {
                string data = sr.ReadLine();
                if (data == String.Empty || data == ser.ReadBegin(nameof(Column)) || data == ser.ValueDelimiter)
                {
                    continue;
                }
                else if (data == ser.ReadBegin(nameof(SparseBinaryMatrix)))
                {
                    column.connectedInputCounter = SparseBinaryMatrix.Deserialize(sr);
                }
                //else if (data == ser.ReadBegin(nameof(SparseBinaryMatrix)))
                //{
                //    column.ConnectedInputCounterMatrix = SparseBinaryMatrix.Deserialize(sr);
                //}
                else if (data == ser.ReadBegin(nameof(ProximalDendrite)))
                {
                    column.ProximalDendrite = ProximalDendrite.Deserialize(sr);
                }
                else if (data == ser.ReadBegin(nameof(Cell)))
                {
                    column.Cells = ser.DeserializeCellArray(data, sr);
                }
                else if (data == ser.ReadEnd(nameof(Column)))
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
                                    column.CellId = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            case 1:
                                {
                                    column.Index = ser.ReadIntValue(str[i]);
                                    break;
                                }
                            default:
                                { break; }

                        }
                    }
                }
            }
            return column;
        }
    }
}
