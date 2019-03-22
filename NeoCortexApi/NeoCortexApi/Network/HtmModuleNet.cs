
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi.Network
{
    public class HtmModuleNet
    {
        List<SpatialPooler> graphs = new List<SpatialPooler>();
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

                var mem = new Connections();
                parameters.apply(mem);

                this.activeArrays.Add(new int[levelOut * levelOut]);

                this.connections.Add(mem);

                SpatialPooler sp = new SpatialPooler();
                sp.init(mem);

                graphs.Add(sp);
            }
        }

        public void Compute(Connections mem, int[] inputVector, bool train)
        {
            for (int i = 0; i < graphs.Count; i++)
            {
                graphs[i].compute(connections[i], i == 0 ? inputVector : activeArrays[i - 1],
                    this.activeArrays[i], train);
            }
        }

        public int[] GetActiveColumns(int levelIndx)
        {
            if (levelIndx >= this.graphs.Count)
                throw new ArgumentException("Invalid level index.");

            return this.activeArrays[levelIndx];
        }

    }
}
