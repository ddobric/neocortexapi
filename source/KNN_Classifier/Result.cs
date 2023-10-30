using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/************************************************************************************************************************************

################################################# CLASS SUMMARY ######################################################################

Class Result:

- The Result class is a simple data class with two properties: label and result. It has a constructor that takes in a string and a 
double value to initialize the properties. 

- The purpose of this class is to represent the class label and result represents the confidence score or probability associated 
with that label.

*************************************************************************************************************************************/

namespace textClassification
{
    /// <summary>
<<<<<<< HEAD
    /// Represents the result of a classification model prediction.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// The label assigned to the predicted class.
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// The confidence score associated with the predicted class.
        /// </summary>
        public double result { get; set; }

        /// <summary>
        /// Initializes a new instance of the Result class with the specified label and confidence score.
        /// </summary>
        /// <param name="label">The label assigned to the predicted class.</param>
        /// <param name="result">The confidence score associated with the predicted class.</param>
=======
    /// Represents a classification result, which includes a label and a corresponding classification score.
    /// </summary>

    public class Result
    {
        /// <summary>
        /// The label of the classification result.
        /// </summary>
        
        public String label { get; set; }
        /// <summary>
        /// The classification score of the label.
        /// </summary>
        /// 
        public double result { get; set; }
        /// <summary>
        /// Creates a new Result object with the specified label and classification score.
        /// </summary>
        /// <param name="label">The label of the classification result.</param>
        /// <param name="result">The classification score of the label.</param>
>>>>>>> 7305e039302f6b4e10550ca91874c0d57699aba4
        public Result(string label, double result)
        {
            this.label = label;
            this.result = result;
        }
    }
}
