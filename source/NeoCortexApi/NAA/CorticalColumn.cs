using NeoCortexApi.DataMappers;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Cortical column that consists of cells. It does not contain mini-columns.
    /// </summary>
    public class CorticalColumn

    {
        public List<Cell> Cells { get; set; } = new List<Cell>();

        public string Name { get; private set; }

        public CorticalColumn(string name, int numCells)
        {
            this.Name = name;
            
            Cell[] cells = new Cell[numCells];            
        }

        public override string ToString()
        {
            return $"{Name} - Cells: {this.Cells.Count}";
        }

    
        public DistalDendrite[] AllDistalDendrites
        {
            get
            {
                return Cells.SelectMany(c => c.DistalDendrites).ToArray();
            }
        }
    }
}
