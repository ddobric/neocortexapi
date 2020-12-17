using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Calculus of a temporal cycle.
    /// </summary>
    //[Serializable]
    public class SegmentActivity
    {
        /// <summary>
        /// Contains the index of segments with number of synapses with permanence higher than threshol (connectedPermanence),
        /// which makes synapse active.
        /// Dictionary[segment index, number of active synapses].
        /// </summary>
        public Dictionary<int, int> ActiveSynapses = new Dictionary<int, int>();

        /// <summary>
        /// Dictionary, which holds the number of potential synapses of every segment.
        /// Potential synspses are all established synapses between receptor cell and the segment's cell. Receprot cell was active cell in the previous cycle.
        /// Dictionary [segment index, number of potential synapses].
        /// </summary>
        public Dictionary<int, int> PotentialSynapses = new Dictionary<int, int>();

        public SegmentActivity()
        {

        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
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
