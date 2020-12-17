using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;

namespace NeoCortexApi.Entities
{
    /// <summary>
    /// Used as container for properties, which define topology of HTM module.
    /// </summary>
    //public class HtmModuleTopology
    //{
    //    public HtmEnpointTopology InputTopology { get; set; }

    //    public HtmEnpointTopology ColumnTopology { get; set; } 
    //}

    /// <summary>
    /// Used as container for properties, which define topology of HTM module.
    /// </summary>
    public class HtmModuleTopology
    {
        public HtmModuleTopology()
        {

        }

        public HtmModuleTopology(int[] dimensions, bool isMajorOrdering)
        {
            this.Dimensions = dimensions;
            this.IsMajorOrdering = isMajorOrdering;
            this.DimensionMultiplies = AbstractFlatMatrix.InitDimensionMultiples(dimensions);
        }
        [JsonProperty("Dimensions")]
        public int[] Dimensions { get; set; }
        [JsonProperty("IsMajorOrdering")]
        public bool IsMajorOrdering { get; set; }
        [JsonProperty("DimensionMultiplies")]
        public int[] DimensionMultiplies { get; set; }
        [JsonProperty("NumDimensions")]
        public int NumDimensions { get { return Dimensions.Length; } }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                if (property.PropertyType.IsArray)
                {
                    sb.AppendLine();
                    sb.Append("<?");
                    sb.Append(property.Name);
                    sb.Append(":");

                    string array = JsonConvert.SerializeObject(property.GetValue(this));

                    sb.Append(array);



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
