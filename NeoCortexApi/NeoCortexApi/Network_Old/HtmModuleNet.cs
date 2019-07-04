
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi.Network
{
    public class HtmModuleNet
    {
        List<SpatialPooler> poolers = new List<SpatialPooler>();
        List<Connections> connections = new List<Connections>();
        List<int[]> activeArrays = new List<int[]>();

        public int Layers { get { return this.connections.Count; } }

        public HtmModuleNet(Parameters parameters, int[] levels)
        {
            for (int levelIndx = 0; levelIndx < levels.Length; levelIndx++)
            {
                int levelIn;
                int levelOut;

                if (levelIndx == 0)
                    levelIn = levelOut = levels[levelIndx];
                else
                {
                    levelIn = connections[levelIndx - 1].getColumnDimensions()[0];
                    levelOut = levels[levelIndx];
                }

                parameters.setInputDimensions(new int[] { levelIn, levelIn });
                parameters.setColumnDimensions(new int[] { levelOut, levelOut });
                parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * levelOut * levelOut);

                var mem = new Connections();
                parameters.apply(mem);

                this.activeArrays.Add(new int[levelOut * levelOut]);

                this.connections.Add(mem);

                SpatialPooler sp = new SpatialPooler();
                sp.init(mem, null);

                poolers.Add(sp);
            }
        }

        public HtmModuleNet(Parameters[] parametersList)
        {
            foreach (var prms in parametersList)
            {
                var mem = new Connections();
                prms.apply(mem);

                var colDims = prms.Get<int[]>(KEY.COLUMN_DIMENSIONS);
                int numCols = 1;
                for (int i = 0; i < colDims.Length; i++)
                {
                    numCols = numCols * colDims[i];
                }

                this.activeArrays.Add(new int[numCols]);

                this.connections.Add(mem);

                SpatialPooler sp = new SpatialPooler();

                sp.init(mem, null);

                poolers.Add(sp);
            }
        }

        public void Compute(int[] inputVector, bool train)
        {
            throw new NotImplementedException();
            //for (int i = 0; i < poolers.Count; i++)
            //{
            //    poolers[i].compute(connections[i], i == 0 ? inputVector : activeArrays[i - 1],
            //        this.activeArrays[i], train);
            //}
        }

        public int[] GetActiveColumns(int levelIndx)
        {
            if (levelIndx >= this.poolers.Count)
                throw new ArgumentException("Invalid level index.");
           
            return this.activeArrays[levelIndx];
        }

        public Connections GetMemory(int levelIndx)
        {
            if (levelIndx >= this.connections.Count)
                throw new ArgumentException("Invalid level index.");

            return this.connections[levelIndx];
        }

    }
}
