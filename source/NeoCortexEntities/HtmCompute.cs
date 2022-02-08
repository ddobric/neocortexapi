// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    public static class HtmCompute
    {
        /// <summary>
        /// Calculates multidimensional coordinates from flat array index.
        /// </summary>
        /// <param name="index">Flat index.</param>
        /// <returns>Coordinates in multidimensional space.</returns>
        public static int[] GetCoordinatesFromIndex(int index, HtmModuleTopology topology)
        {
            int[] returnVal = new int[topology.NumDimensions];
            int baseNum = index;
            for (int i = 0; i < topology.DimensionMultiplies.Length; i++)
            {
                int quotient = baseNum / topology.DimensionMultiplies[i];
                baseNum %= topology.DimensionMultiplies[i];
                returnVal[i] = quotient;
            }
            return topology.IsMajorOrdering ? Reverse(returnVal) : returnVal;
        }


        /// <summary>
        /// Reurns reverse array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int[] Reverse(int[] input)
        {
            int[] retVal = new int[input.Length];
            for (int i = input.Length - 1, j = 0; i >= 0; i--, j++)
            {
                retVal[j] = input[i];
            }
            return retVal;
        }


        /// <summary>
        /// Maps a column to its input bits. This method encapsulates the topology of the region. It takes the index of the column as an argument and calculates 
        /// indexes of the input vector that represent column's potential pool. The return value is a list containing the indices of 
        /// connected input bits. The current implementation of the base class only supports a 1 dimensional topology of columns with a 1 dimensional topology of inputs. 
        /// To extend this class to support 2-D topology you will need to override this method. Examples of the expected output of this method:
        /// <list type="bullet">
        ///     <item>
        ///     If the potentialRadius is greater than or equal to the entire input space, (global visibility), then this method returns an array filled with 
        ///     all the indices
        ///     </item>
        ///     <item>
        ///     If the topology is one dimensional, and the potentialRadius is 5, this method will return an array containing 5 consecutive values centered on 
        ///     the index of the column (wrapping around if necessary).
        ///     </item>
        ///     <item>
        ///     If the topology is two dimensional (not implemented), and the potentialRadius is 5, the method should return an array containing 25 '1's, where 
        ///     the exact indices are to be determined by the mapping from 1-D index to 2-D position.
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="htmConfig">The configuration used in <see cref="Connections"/>.</param>
        /// <param name="columnIndex">The index identifying a column in the permanence, potential and connectivity matrices.</param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static int[] MapPotential(HtmConfig htmConfig, int columnIndex, Random rnd)
        {
            int centerInput = MapColumn(columnIndex, htmConfig.ColumnModuleTopology, htmConfig.InputModuleTopology);

            // Here we have Receptive Field (RF)
            int[] columnInputs = HtmCompute.GetInputNeighborhood(htmConfig.WrapAround, htmConfig.InputModuleTopology, centerInput, htmConfig.PotentialRadius);

            // Select a subset of the receptive field to serve as the the potential pool.
            int numPotential = (int)(columnInputs.Length * htmConfig.PotentialPct + 0.5);
            int[] retVal = new int[numPotential];

            var data = ArrayUtils.Sample(columnInputs, retVal, rnd);

            return data;
        }

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
        /// Uniform Column Mapping <br></br>
        /// Maps a column to its respective input index, keeping to the topology of the region. It takes the index of the column as an argument and determines 
        /// what is the index of the flattened input vector that is to be the center of the column's potential pool. It distributes the columns over the inputs 
        /// uniformly. The return value is an integer representing the index of the input bit. Examples of the expected output of this method:
        /// <list type="bullet">
        ///     <item>
        ///     If the topology is one dimensional, and the column index is 0, this method will return the input index 0. If the column index is 1, and there are 
        ///     3 columns over 7 inputs, this method will return the input index 3.
        ///     </item>
        ///     <item>If the topology is two dimensional, with column dimensions [3, 5] and input dimensions [7, 11], and the column index is 3, the method returns
        ///     input index 8.
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="columnIndex">The index identifying a column in the permanence, potential and connectivity matrices.</param>
        /// <param name="colTop"></param>
        /// <param name="inpTop"></param>
        /// <returns>Flat index of mapped column.</returns>
        public static int MapColumn(int columnIndex, HtmModuleTopology colTop, HtmModuleTopology inpTop)
        {
            int[] columnCoords = AbstractFlatMatrix.ComputeCoordinates(colTop.NumDimensions,
                colTop.DimensionMultiplies, colTop.IsMajorOrdering, columnIndex);

            double[] colCoords = ArrayUtils.ToDoubleArray(columnCoords);

            double[] columnRatios = ArrayUtils.Divide(
                colCoords, ArrayUtils.ToDoubleArray(colTop.Dimensions), 0, 0);

            double[] inputCoords = ArrayUtils.Multiply(
                ArrayUtils.ToDoubleArray(inpTop.Dimensions), columnRatios, 0, 0);

            var colSpanOverInputs = ArrayUtils.Divide(
                        ArrayUtils.ToDoubleArray(inpTop.Dimensions),
                        ArrayUtils.ToDoubleArray(colTop.Dimensions), 0, 0);

            inputCoords = ArrayUtils.AddOffset(inputCoords, ArrayUtils.Multiply(colSpanOverInputs, 0.5));

            // Makes sure that inputCoords are in range [0, inpDims]
            int[] inputCoordInts = ArrayUtils.Clip(ArrayUtils.ToIntArray(inputCoords), inpTop.Dimensions, -1);

            return AbstractFlatMatrix.ComputeIndex(inputCoordInts, inpTop.Dimensions, inpTop.NumDimensions,
                 inpTop.DimensionMultiplies, inpTop.IsMajorOrdering, true);
        }


        /// <summary>
        /// Gets indexes of neighborhood cells within centered radius  
        /// </summary>
        /// <param name="centerIndex">The index of the point. The coordinates are expressed as a single index by
        /// using the dimensions as a mixed radix definition. For example, in dimensions 42x10, the point [1, 4] is index 1*420 + 4*10 = 460.
        /// </param>   
        /// <param name="radius"></param>
        /// <param name="topology"></param>
        /// <returns>The points in the neighborhood, including centerIndex.</returns>
        public static int[] GetWrappingNeighborhood(int centerIndex, int radius, HtmModuleTopology topology)
        {
            int[] cp = HtmCompute.GetCoordinatesFromIndex(centerIndex, topology);

            // Dims of columns
            IntGenerator[] intGens = new IntGenerator[topology.Dimensions.Length];
            for (int i = 0; i < topology.Dimensions.Length; i++)
            {
                intGens[i] = new IntGenerator(cp[i] - radius, Math.Min((cp[i] - radius) + topology.Dimensions[i] - 1, cp[i] + radius) + 1);
            }

            List<List<int>> result = new List<List<int>>();

            result.Add(new List<int>());

            List<List<int>> interim = new List<List<int>>();

            int k = 0;
            foreach (IntGenerator gen in intGens)
            {
                interim.Clear();
                interim.AddRange(result);
                result.Clear();

                foreach (var lx in interim)
                {
                    gen.Reset();

                    for (int y = 0; y < gen.Size(); y++)
                    {
                        int py = ArrayUtils.Modulo(gen.Next(), topology.Dimensions[k]);
                        //int py = gen.next() % dimensions[k];

                        List<int> tl = new List<int>();
                        tl.AddRange(lx);
                        tl.Add(py);
                        result.Add(tl);
                    }
                }

                k++;
            }

            return result.Select((tl) => GetFlatIndexFromCoordinates(tl.ToArray(), topology)).ToArray();
        }

        /// <summary>
        /// Gets the list of neighborhood columns around the centar with the given radius in the specified topology.
        /// </summary>
        /// <param name="centerIndex"></param>
        /// <param name="radius"></param>
        /// <param name="topology"></param>
        /// <returns></returns>
        public static int[] GetNeighborhood(int centerIndex, int radius, HtmModuleTopology topology)
        {
            var centerPosition = HtmCompute.GetCoordinatesFromIndex(centerIndex, topology);

            IntGenerator[] intGens = new IntGenerator[topology.Dimensions.Length];
            for (int i = 0; i < topology.Dimensions.Length; i++)
            {
                intGens[i] = new IntGenerator(Math.Max(0, centerPosition[i] - radius),
                    Math.Min(topology.Dimensions[i] - 1, centerPosition[i] + radius) + 1);
            }

            List<List<int>> result = new List<List<int>>();

            result.Add(new List<int>());

            List<List<int>> interim = new List<List<int>>();

            foreach (IntGenerator gen in intGens)
            {
                interim.Clear();
                interim.AddRange(result);
                result.Clear();

                foreach (var lx in interim)
                {
                    gen.Reset();

                    for (int y = 0; y < gen.Size(); y++)
                    {
                        int py = gen.Next();
                        List<int> tl = new List<int>();
                        tl.AddRange(lx);
                        tl.Add(py);
                        result.Add(tl);
                    }
                }
            }

            return result.Select((tl) => GetFlatIndexFromCoordinates(tl.ToArray(), topology)).ToArray();
        }


        /// <summary>
        /// Returns a flat index computed from the specified coordinates.
        /// </summary>
        /// <param name="coordinates">  The index of the point</param>
        /// <param name="topology"></param>
        /// <returns>using the dimensions as a mixed radix definition.For example, in dimensions 
        /// 42x10, the point [1, 4] is index 1*420 + 4*10 = 460.</returns>
        public static int GetFlatIndexFromCoordinates(int[] coordinates, HtmModuleTopology topology)
        {
            int[] localMults = topology.IsMajorOrdering ? HtmCompute.Reverse(topology.DimensionMultiplies) : topology.DimensionMultiplies;
            int baseNum = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                baseNum += (localMults[i] * coordinates[i]);
            }
            return baseNum;
        }


        /// <summary>
        /// Gets a neighborhood of inputs. Simply calls topology.wrappingNeighborhood or topology.neighborhood. A subclass can insert different topology behavior by overriding this method.
        /// </summary>
        /// <param name="isWrapAround"></param>
        /// <param name="inputTopology"></param>
        /// <param name="centerInput">The center of the neighborhood.</param>
        /// <param name="potentialRadius">Span of the input field included in each neighborhood</param>
        /// <returns>The input's in the neighborhood. (1D)</returns>
        public static int[] GetInputNeighborhood(bool isWrapAround, HtmModuleTopology inputTopology, int centerInput, int potentialRadius)
        {
            return isWrapAround ?
                GetWrappingNeighborhood(centerInput, potentialRadius, inputTopology) :
                    GetNeighborhood(centerInput, potentialRadius, inputTopology);
        }


        /// <summary>
        /// Initializes the permanences of a column. The method returns a 1-D array the size of the input, where each entry in the array represents the initial 
        /// permanence value between the input bit at the particular index in the array, and the column represented by the 'index' parameter.
        /// </summary>
        /// <param name="htmConfig">An array specifying the potential pool of the column. Permanence values will only be generated for input bits corresponding to 
        ///                         indices for which the mask value is 1. <b>WARNING</b>: potentialPool is sparse, not an array of "1's"
        ///                         </param>
        /// <param name="potentialPool"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static double[] InitSynapsePermanences(HtmConfig htmConfig, int[] potentialPool, Random random)
        {
            //Random random = new Random();
            double[] perm = new double[htmConfig.NumInputs];

            //foreach (int idx in column.ProximalDendrite.ConnectedInputs)
            foreach (int idx in potentialPool)
            {
                if (random.NextDouble() <= htmConfig.InitialSynapseConnsPct)
                {
                    perm[idx] = InitPermConnected(htmConfig.SynPermMax, htmConfig.SynPermMax, random);
                }
                else
                {
                    perm[idx] = InitPermNonConnected(htmConfig.SynPermConnected, random);
                }

                perm[idx] = perm[idx] < htmConfig.SynPermTrimThreshold ? 0 : perm[idx];

            }

            return perm;
        }


        /// <summary>
        /// Returns a randomly generated permanence value for a synapse that is initialized in a connected state. The basic idea here is to initialize
        /// permanence values very close to <c>synPermConnected</c> so that a small number of learning steps could make it disconnected or connected.
        /// </summary>
        /// <param name="synPermMax"></param>
        /// <param name="synPermConnected"></param>
        /// <param name="rnd"></param>
        /// <returns>a randomly generated permanence value</returns>
        /// <remarks>
        /// Note: experimentation was done a long time ago on the best way to initialize permanence values, but the history for this particular scheme has been lost.
        /// </remarks>
        public static double InitPermConnected(double synPermMax, double synPermConnected, Random rnd)
        {
            //double p = c.getSynPermConnected() + (c.getSynPermMax() - c.getSynPermConnected()) * c.random.NextDouble();
            double p = synPermConnected + (synPermMax - synPermConnected) * rnd.NextDouble();

            // Note from Python implementation on conditioning below:
            // Ensure we don't have too much unnecessary precision. A full 64 bits of
            // precision causes numerical stability issues across platforms and across
            // implementations
            p = ((int)(p * 100000)) / 100000.0d;
            return p;
        }


        /// <summary>
        /// Returns a randomly generated permanence value for a synapses that is to be initialized in a non-connected state.
        /// </summary>
        /// <param name="synPermConnected"></param>
        /// <param name="rnd">Random generator to be used to generate permanence.</param>
        /// <returns>Permanence value.</returns>
        public static double InitPermNonConnected(double synPermConnected, Random rnd)
        {
            //double p = c.getSynPermConnected() * c.getRandom().NextDouble();
            double p = synPermConnected * rnd.NextDouble();

            // Note from Python implementation on conditioning below:
            // Ensure we don't have too much unnecessary precision. A full 64 bits of
            // precision causes numerical stability issues across platforms and across
            // implementations
            p = ((int)(p * 100000)) / 100000.0d;
            return p;
        }

        /// <summary>
        /// It traverses all connected synapses of the column and calculates the span, which synapses
        /// spans between all input bits. Then it calculates average of spans accross all dimensions. 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="htmConfig">Topology</param>
        /// <returns></returns>
        public static double CalcAvgSpanOfConnectedSynapses(Column column, HtmConfig htmConfig)
        {
            // Gets synapses connected to input bits.(from pool of the column)
            int[] connected = column.ProximalDendrite.GetConnectedSynapsesSparse();

            if (connected == null || connected.Length == 0) return 0;

            int[] maxCoord = new int[htmConfig.InputModuleTopology.Dimensions.Length];
            int[] minCoord = new int[maxCoord.Length];

            ArrayUtils.FillArray(maxCoord, -1);
            ArrayUtils.FillArray(minCoord, ArrayUtils.Max(htmConfig.InputModuleTopology.Dimensions));

            //
            // Traverse all connected input neurons.
            for (int i = 0; i < connected.Length; i++)
            {
                var coordinates = AbstractFlatMatrix.ComputeCoordinates(htmConfig.InputModuleTopology.Dimensions.Length,
                  htmConfig.InputModuleTopology.DimensionMultiplies, htmConfig.InputModuleTopology.IsMajorOrdering, connected[i]);

                maxCoord = ArrayUtils.MaxBetween(maxCoord, coordinates);

                minCoord = ArrayUtils.MinBetween(minCoord, coordinates);


                //maxCoord = ArrayUtils.MaxBetween(maxCoord, AbstractFlatMatrix.ComputeCoordinates(htmConfig.InputModuleTopology.Dimensions.Length,
                //   htmConfig.InputModuleTopology.DimensionMultiplies, htmConfig.InputModuleTopology.IsMajorOrdering, connected[i]));

                //minCoord = ArrayUtils.MinBetween(minCoord, AbstractFlatMatrix.ComputeCoordinates(htmConfig.InputModuleTopology.Dimensions.Length,
                //   htmConfig.InputModuleTopology.DimensionMultiplies, htmConfig.InputModuleTopology.IsMajorOrdering, connected[i]));
            }

            var spans = ArrayUtils.Subtract(maxCoord, minCoord);

            return ArrayUtils.Average(ArrayUtils.Add(spans, 1));
        }


        /// <summary>
        /// This method ensures that each column has enough connections to input bits
        /// to allow it to become active. Since a column must have at least
        /// 'stimulusThreshold' overlaps in order to be considered during the
        /// inhibition phase, columns without such minimal number of connections, even
        /// if all the input bits they are connected to turn on, have no chance of
        /// obtaining the minimum threshold. For such columns, the permanence values
        /// are increased until the minimum number of connections are formed.        
        /// </summary>
        /// <param name="htmConfig"></param>
        /// <param name="permanences">An array of permanence values for a column. The array is "dense", i.e. it contains an entry for each input bit, even if the permanence value is 0.</param>
        /// <param name="potentialIndexes">The indexes of inputs in the specified <see cref="Column"/>'s pool.</param>
        public static void BoostProximalSegment(HtmConfig htmConfig, double[] permanences, int[] potentialIndexes)
        {
            // TODO. Consider moving this condition to the initialization of the SP.
            if (potentialIndexes.Length < htmConfig.StimulusThreshold)
            {
                throw new ArgumentException("StimulusThreshold as number of required connected synapses cannot be greather than number of neurons in receptive field.");
            }

            ArrayUtils.EnsureBetweenMinAndMax(permanences, htmConfig.SynPermMin, htmConfig.SynPermMax);

            while (true)
            {
                // Gets number of synapses with permanence value grather than 'SynPermConnected' = Connected Synapses.
                int numConnected = ArrayUtils.GreaterThanAtIndex(htmConfig.SynPermConnected, permanences, potentialIndexes);

                // If enough synapses are connected, all ok.
                if (numConnected >= htmConfig.StimulusThreshold)
                    return;

                // If number of connected synapses is below threshold, 
                // then permanences of all synapses will be incremented (raised) until column is connected.
                ArrayUtils.RaiseValuesBy(htmConfig.SynPermBelowStimulusInc, permanences, potentialIndexes);
            }
        }

        public static void RaisePermanenceToThresholdSparse(HtmConfig htmConfig, double[] perm)
        {
            ArrayUtils.EnsureBetweenMinAndMax(perm, htmConfig.SynPermMin, htmConfig.SynPermMax);
            while (true)
            {
                int numConnected = ArrayUtils.ValueGreaterCount(htmConfig.SynPermConnected, perm);
                if (numConnected >= htmConfig.StimulusThreshold) return;
                ArrayUtils.RaiseValuesBy(htmConfig.SynPermBelowStimulusInc, perm);
            }
        }

        /// <summary>
        /// This method updates the permanences with a column's new permanence values. The column is identified by its index, which reflects the row in
        /// the matrix, and the permanence is given in 'sparse' form, i.e. an array whose members are associated with specific indexes. It is in charge of 
        /// implementing 'clipping' - ensuring that the permanence values are always between 0 and 1 - and 'trimming' - enforcing sparseness by zeroing out
        /// all permanence values below 'synPermTrimThreshold'. It also maintains the consistency between 'permanences' (the matrix storing the permanence values), 
        /// 'connectedSynapses', (the matrix storing the bits each column is connected to), and 'connectedCounts' (an array storing the number of input bits each 
        /// column is connected to). Every method wishing to modify the permanence matrix should do so through this method.
        /// </summary>
        /// <param name="htmConfig">the configuration used in <see cref="Connections"/>.</param>
        /// <param name="perm">An array of permanence values for a column. The array is "dense", i.e. it contains an entry for each input bit, even if the permanence value is 0.</param>
        /// <param name="column">The column to be updated.</param>
        /// <param name="potentialIndexes">The indexes of inputs in the specified <see cref="Column"/>'s pool.</param>
        /// <param name="raisePerm">a boolean value indicating whether the permanence values</param>
        public static void UpdatePermanencesForColumn(HtmConfig htmConfig, double[] perm, Column column, int[] potentialIndexes, bool raisePerm)
        {
            if (raisePerm)
            {
                // During every learning cycle, this method ensures that every column 
                // has enough connections (perm > SynPermConnected) to the iput space.
                BoostProximalSegment(htmConfig, perm, potentialIndexes);
            }

            // Here we set all permanences to 0 if the permanence value is less than SynPermTrimThreshold.
            ArrayUtils.LessOrEqualXThanSetToY(perm, htmConfig.SynPermTrimThreshold, 0);

            ArrayUtils.EnsureBetweenMinAndMax(perm, htmConfig.SynPermMin, htmConfig.SynPermMax);

            column.SetPermanences(htmConfig, perm);
        }
    }
}
