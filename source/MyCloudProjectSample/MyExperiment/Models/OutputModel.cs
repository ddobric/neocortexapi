using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment.Models
{
    public class OutputModel
    {
        /// <summary>
        /// This class represents the output model of the SE project
        /// </summary>
        private int[] NextValues { get; set; }

        public OutputModel(int[] nextValues)
        {
            NextValues = nextValues;
        }
    }
}
