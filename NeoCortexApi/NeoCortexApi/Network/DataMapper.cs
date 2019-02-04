
using NeoCortexApi.Encoders;
using NeoCortexApi.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningFoundation.DataMappers
{
    /// <summary>
    /// Class for asigning set of properties for each feature (data column)
    /// </summary>
    internal class DataMapper
    {
        private DataDescriptor descriptor;

        private CortexNetworkContext context;

        /// <summary>
        /// Main constructor
        /// </summary>
        public DataMapper(DataDescriptor descriptor, CortexNetworkContext context)
        {
            this.context = context;
            this.descriptor = descriptor;

            foreach (var feature in descriptor.Features)
            {
                if (feature.EncoderSettings != null)
                {
                    var encoder = this.context.CreateEncoder(feature.EncoderSettings);

                }
                else
                {
                    throw new ArgumentException("Encoder settings not specified.");
                }
            }
        }


        /// <summary>
        /// Transform the featureVector from natural format in to double format. 
        /// ** AFTER MAPPING:  'LABEL COLUMN IS THE LAST ELEMENT IN ARRAY'**
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public int[] Run(object[] vector)
        {
            //sort features by id
            Array.Sort(this.descriptor.Features, (x, y) => x.Id.CompareTo(y.Id));

            //enumerate all dataset data
            List<int> output = new List<int>();

            //
            // Transform rawData in to raw of Features with proper type, normalization value, and corect binary and catogery type 
            // during enumeration Features are sorted by Id property
            //for (int featureIndx = 0; featureIndx < data[0].Length; featureIndx++)
            foreach (var featureIndx in this.descriptor.Features.OrderBy(x => x.Id).Select(x => x.Index))
            {
                var col = this.descriptor.Features[featureIndx];
                if (col.Encoder == null)
                    col.Encoder = context.CreateEncoder(col.EncoderSettings);

                int[] encodedValue = col.Encoder.Encode(vector[featureIndx]);

                output.AddRange(encodedValue);
                ////skip string columns.
                //if (col.Type == ColumnType.STRING)
                //    continue;


                ////first mapp the features
                //if (this.descriptor.LabelIndex != featureIndx)
                //{
                //    var featValues = MapFeature(vector[featureIndx], col);
                //    raw.AddRange(featValues);
                //}
            }

            
            //after real data is transformed in to numeric format, then we can calculate number of feature
            // ctx.DataDescriptor.NumOfFeatures = rows.FirstOrDefault().Count;

            // Returns rows of double value feture vectors
            return output.ToArray();
        }


        /// <summary>
        /// Mapping Feature real value in to numeric representation
        /// </summary>
        /// <param name="val"> real value</param>
        /// <param name="col"> column metadata</param>
        /// <returns></returns>
        private List<double> MapFeature(object val, Column col)
        {
            List<double> raw = new List<double>();

            if (col.Type == ColumnType.STRING)
                throw new ArgumentException("string column type is invalid here.");
            else if (col.Type == ColumnType.NUMERIC)//numeric column
            {
                double value = double.NaN;//initial value of the feature

                //in case of invalid (missing) value, value must be replaced with defaultMissing value
                if (val == null || !double.TryParse(val.ToString(), out value))
                    value = col.DefaultMissingValue;

                //
                raw.Add(value);
            }
            else if (col.Type == ColumnType.BOOLEAN)//binary column
            {
                if (val == null || string.IsNullOrEmpty(val.ToString()))
                    raw.Add(col.DefaultMissingValue);
                if (col.Values[0].Equals(val))
                    raw.Add(0);
                else if (col.Values[1].Equals(val))
                    raw.Add(1);
                else//in case of invalid (missing) value, value must be replaced with defaultMIssing value
                    raw.Add(col.DefaultMissingValue);
            }
            else if (col.Type == ColumnType.CLASS)//multiclass column
            {
                //add as many columns as number of categories
                //eg. red, greeen,blue -  categories
                // for red -> 0
                // for green -> 1
                // for blue -> 2


                // Converts category value in to binary values with creation additional feature columns as meny as number of classes             
                // Feature Classes (Red, Greeen, Blue): threee addition features
                //     Features      (R; G; B)
                //          Blue  =  (0; 0; 1)  - three values which sum is 1,
                //          Red   =  (1; 0; 0)
                //          Green =  (0; 1; 0)

                // number of classes
                var numClasses = col.Values.Length;
                //inidex initial value
                int index;

                //in case of missing value
                if (val == null || string.IsNullOrEmpty(val.ToString()))
                    index = (int)col.DefaultMissingValue;
                else
                {
                    //find index of the class
                    index = Array.IndexOf(col.Values, val);

                    //missing value for class column must be index of the class value
                    // DefaultMissingValue = 0 -> red
                    // DefaultMissingValue = 1 -> green, ...
                    if (index < 0)
                        index = (int)col.DefaultMissingValue;
                }

                //after index is calculated add features category
                for (int j = 0; j < numClasses; j++)
                {
                    //when the indexes are equal this column assign to 1 , otherwize 0
                    if (index == j)
                    {
                        raw.Add(1);
                    }
                    else
                    {
                        raw.Add(0);
                    }
                }
            }
            else if (col.Type == ColumnType.STRING)
                throw new ArgumentException("string column type is invalid here.");
            else if (col.Type == ColumnType.NUMERIC)//numeric column
            {
                double value = double.NaN;//initial value of the feature

                //in case of invalid (missing) value, value must be replaced with defaultMissing value
                if (val == null || !double.TryParse(val.ToString(), out value))
                    value = col.DefaultMissingValue;

                //
                raw.Add(value);
            }

            return raw;
        }


        /// <summary>
        /// Mapping label column in to numerical representation
        /// </summary>
        /// <param name="val"> real value</param>
        /// <param name="col"> column metadata</param>
        /// <returns></returns>
        private double MapLabel(object val, Column col)
        {
            List<double> raw = new List<double>();

            if (col.Type == ColumnType.BOOLEAN || col.Type == ColumnType.NUMERIC)//numeric or binary column
            {
                var value = MapNumericOrBinaryColType(val, col);
                //
                return value;
            }

            else if (col.Type == ColumnType.CLASS)//multiclass column mapper for the Feature
            {
                //Fransform lable class column in to ceresponded numeric column
                // Each class in the Column are replaced with integer in acdedenting order: 0,1,2,3.... 
                //eg. red, greeen,blue -  categories
                // for red -> 0
                // for green -> 1
                // for blue -> 2


                // Converts category value in to numeric values
                // it creates array which has length of categories count.
                // Example: Red, Gree, Blue - 3 categories  - real values
                //             0,  1,  2    - 3 numbers     - numeric values
                //             

                // number of classes
                var numClasses = col.Values.Length;
                //inidex initial value
                int index;

                //in case of missing value
                if (val == null || string.IsNullOrEmpty(val.ToString()))
                    index = (int)col.DefaultMissingValue;
                else
                {
                    //find index of the class
                    index = Array.IndexOf(col.Values, val);

                    //missing value for class column must be index of the class value
                    // DefaultMissingValue = 0 -> red
                    // DefaultMissingValue = 1 -> green, ...
                    if (index < 0)
                        index = (int)col.DefaultMissingValue;
                }

                //after index is calculated add features category
                return index;
            }
            else
                throw new ArgumentException("Invalid column type.");
        }

        /// <summary>
        /// Maps Numeric and Binary column types in to coresponded numeric value.
        /// This method is identical for Faeture and Lebel columns.
        /// </summary>
        /// <param name="val"> real value</param>
        /// <param name="col"> column metadata</param>
        /// <returns></returns>
        private double MapNumericOrBinaryColType(object val, Column col)
        {
            if (col.Type == ColumnType.NUMERIC)//numeric column
            {
                double value = double.NaN;//initial value of the feature

                //in case of invalid (missing) value, value must be replaced with defaultMissing value
                if (val == null || !double.TryParse(val.ToString(), out value))
                    value = col.DefaultMissingValue;

                //
                return value;
            }
            else if (col.Type == ColumnType.BOOLEAN)//binary column
            {
                if (val == null || string.IsNullOrEmpty(val.ToString()))
                    return col.DefaultMissingValue;
                if (col.Values[0].Equals(val))
                    return 0;
                else if (col.Values[1].Equals(val))
                    return 1;
                else//in case of invalid (missing) value, value must be replaced with defaultMIssing value
                    return col.DefaultMissingValue;
            }
            else
                throw new ArgumentException("nonnumeric and nonbinary column type is invalid here.");
        }

    }

    /// <summary>
    /// Implementation of the data column used in Data Mapper 
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Feature Id. Features are sorted by this property
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Feature (Column) name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Feature position in trainData/testData
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The Type of the column (feature)
        /// </summary>
        public ColumnType Type { get; set; }

        /// <summary>
        /// Assembly qualified name of the column encoder. If specified, encoder is used for encoding of the value.
        /// </summary>
        public Dictionary<String, Object> EncoderSettings { get; set; }

        /// <summary>
        /// Instance of encoder used for this column.
        /// </summary>
        public EncoderBase Encoder { get; set; }


        /// <summary>
        /// In case of binary and Category type, values represent class values enumerated in ascedenting order
        /// binary:
        /// {false,true} - mean: 0->false, 1->true
        /// {no, yes}; - mean: 0->no, 1->yes  
        /// {0, 1}; - mean: 0->0, 1->1
        /// 
        /// multiclass: 1->n representation 
        /// {Red, Green, Blue}; - mean: (Red=0, Green=1, Blue=2) normalized values: Red-> (1,0,0), Green ->(0,1,0), Blue ->(0,0,1) 
        /// </summary>
        public string[] Values { get; set; }

        /// <summary>
        /// Replaces the missing value in the cell
        /// </summary>
        public double DefaultMissingValue { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ColumnType
    {
        STRING,// 0 - string (avoided in training and testing)
        NUMERIC,// 1 - numeric
        BOOLEAN,// 2 - binary
        CLASS,// 3 - multiclass with x categories,
        DATETIME
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResultMapping
    {
        public Dictionary<string, double> Mappings { get; set; }
    }
}
