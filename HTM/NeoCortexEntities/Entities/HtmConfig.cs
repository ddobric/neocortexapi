using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// HTM required configuration sent from Akka-client to Akka Actor.
    /// </summary>
    public class HtmConfig
    {
        //public int[] ColumnDimensions { get; set; }

        //public bool IsColumnMajor { get; set; } = false;

        /// <summary>
        /// Use -1 if real random generator has to be used with timestamp seed.
        /// </summary>
        public int RandomGenSeed { get; set; } = 42;

        public HtmModuleTopology ColumnTopology { get; set; }

        public HtmModuleTopology InputTopology { get; set; }
        
        public bool IsWrapAround { get; set; }

        /// <summary>
        /// The name of the actor as set by actor-client.
        /// </summary>
        public string Name { get; set; }

        public double PotentialPct { get; set; }

        public int PotentialRadius { get; set; }

        public double SynPermConnected { get; set; }

        public double StimulusThreshold { get; set; }        

        public int NumInputs { get;  set; }

        public int NumColumns { get; set; }

        public double SynPermMax { get; set; }

        public double SynPermMin { get; set; }

        public double InitialSynapseConnsPct { get; set; }

        public double SynPermTrimThreshold { get; set; }

        public double SynPermBelowStimulusInc { get; set; }

        public int CellsPerColumn { get; set; }

        public double SynPermInactiveDec { get; set; }

        public double PermanenceIncrement { get;  set; }

        public double PermanenceDecrement { get;  set; }
        public int MaxNewSynapseCount { get; set; }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                if (property.Name == "ColumnTopology")
                {
                    sb.Append(property.Name);
                    sb.Append(":");
                    sb.Append(this.ColumnTopology.Serialize());
                    sb.AppendLine();
                    sb.Append("<?");

                }

                else if (property.Name == "InputTopology")
                {
                    sb.Append(property.Name);
                    sb.Append(":");
                    sb.Append(this.InputTopology.Serialize());

                    sb.AppendLine();
                    sb.Append("<?");

                }

                else
                {
                    sb.AppendLine();
                    sb.Append("<?");

                    sb.Append(property.Name);
                    sb.Append(": ");
                    if (property.GetIndexParameters().Length > 0)
                    {
                        sb.Append("Indexed Property cannot be used");
                    }
                    else
                    {
                        sb.Append(property.GetValue(this, null));
                    }


                    sb.Append(Environment.NewLine);
                    sb.Append("<?");
                }
            }

            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                sb.AppendLine();
                sb.Append("<?");

                sb.Append(field.Name);
                sb.Append(": ");

                sb.Append(field.GetValue(this));

                sb.Append(Environment.NewLine);
                sb.Append("<?");

            }
            sb.AppendLine("}");
            return sb.ToString();


        }
    }
}
