// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;

namespace NeoCortexApi.Network
{
    public class HtmModuleNet
    {
        List<SpatialPooler> m_Poolers = new List<SpatialPooler>();
        List<Connections> m_Connections = new List<Connections>();
        List<int[]> m_ActiveArrays = new List<int[]>();

        public int Layers { get { return this.m_Connections.Count; } }

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
                    levelIn = m_Connections[levelIndx - 1].HtmConfig.ColumnDimensions[0];
                    levelOut = levels[levelIndx];
                }

                parameters.setInputDimensions(new int[] { levelIn, levelIn });
                parameters.setColumnDimensions(new int[] { levelOut, levelOut });
                parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 0.1 * levelOut * levelOut);

                var mem = new Connections();
                parameters.apply(mem);

                this.m_ActiveArrays.Add(new int[levelOut * levelOut]);

                this.m_Connections.Add(mem);

                SpatialPooler sp = new SpatialPooler();
                sp.Init(mem, null);

                m_Poolers.Add(sp);
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
                    numCols *= colDims[i];
                }

                this.m_ActiveArrays.Add(new int[numCols]);

                this.m_Connections.Add(mem);

                SpatialPooler sp = new SpatialPooler();

                sp.Init(mem, null);

                m_Poolers.Add(sp);
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
            if (levelIndx >= this.m_Poolers.Count)
                throw new ArgumentException("Invalid level index.");

            return this.m_ActiveArrays[levelIndx];
        }

        public Connections GetMemory(int levelIndx)
        {
            if (levelIndx >= this.m_Connections.Count)
                throw new ArgumentException("Invalid level index.");

            return this.m_Connections[levelIndx];
        }

    }
}
