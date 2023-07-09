// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information

using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// The HTM area of Mini-Columns.
    /// </summary>
    public class MinicolumnArea
    {
        /// <summary>
        /// Mini-Columns
        /// </summary>
        public List<Column> Columns { get; set; } = new List<Column>();

        /// <summary>
        /// The name of the area.
        /// </summary>
        public string Name { get; private set; }


        public List<Cell> AllCells
        {
            get
            {
                return this.Columns.SelectMany(c => c.Cells).ToList();
            }
        }

        public Segment[] AllDistalDendrites
        {
            get
            {
                return AllCells.SelectMany(c => c.DistalDendrites).ToArray();
            }
        }



        /// <summary>
        /// Initializes the mini-columns from the configuration.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public MinicolumnArea(string name, HtmConfig config)
        {
            this.Name = name;
            Init(config);
        }


        /// <summary>
        /// Create a flat list of mini-columns in a property <see cref="Columns"/>. 
        /// </summary>
        /// <param name="config"></param>
        private void Init(HtmConfig config)
        {
            int numColumns = 1;

            foreach (var item in config.ColumnDimensions)
            {
                numColumns *= item;
            }

            for (int i = 0; i < numColumns; i++)
            {
                Column column = new Column(config.CellsPerColumn, i, config.SynPermConnected, config.NumInputs);

                Columns.Add(column);
            }
        }

        /// <inheritdoc/>     
        public override string ToString()
        {
            return $"{Name} - Cols: {this.Columns.Count}";
        }
    }
}
