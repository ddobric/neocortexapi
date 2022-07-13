using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    public class CorticalArea
    {
        public List<Column> Columns { get; set; } = new List<Column>();

        public string Name { get; private set; }

        public CorticalArea(string name, HtmConfig config)
        {
            this.Name = name;
            Init(config);
        }

        public override string ToString()
        {
            return $"{Name} - Cols: {this.Columns.Count}";
        }

        private void Init(HtmConfig config)
        {
            int numColumns = 1;

            foreach (var item in config.ColumnDimensions)
            {
                numColumns *= item;
            }

            Cell[] cells = new Cell[numColumns * config.CellsPerColumn];

            for (int i = 0; i < numColumns; i++)
            {
                Column column = new Column(config.CellsPerColumn, i, config.SynPermConnected, config.NumInputs);

                Columns.Add(column);
            }
        }

        public Cell[] AllCells
        {
            get
            {
                return this.Columns.SelectMany(c => c.Cells).ToArray();             
            }
        }

        public DistalDendrite[] AllDistalDendrites
        {
            get
            {
                return AllCells.SelectMany(c => c.DistalDendrites).ToArray();
            }
        }
    }
}
