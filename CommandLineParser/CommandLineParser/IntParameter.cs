using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    public class IntParameter : IParameter
    {
        public string Name { get; }
        public bool IsMandatory { get; }

        private int lowerBound;
        private int upperBound;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Parameter name. (For the purposes of documentation.)</param>
        /// <param name="isMandatory">Boolean, indicates whether parameter must be present.</param>
        /// <param name="lowerBound">Minimum admissible value, inclusive.</param>
        /// <param name="upperBound">Maximum admissible value, inclusive.</param>
        /// <exception cref="System.ArgumentException">Thrown is name is null or empty, if lower bound is higher than upper bound or if upper bound is lower than lower bound.</exception>
        public IntParameter(string name, bool isMandatory, int lowerBound = int.MinValue, int upperBound = int.MaxValue)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(Properties.Resources.StringParameter_ParameterNameNotDefined);
            this.Name = name;
            this.IsMandatory = isMandatory;

            if (lowerBound > upperBound)
                throw new ArgumentException(Properties.Resources.IntParameter_LowerUpperBoundViolation);

            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }

        /// <summary>
        /// Checks whether a given value can be converted to integer and falls into correct range, as specified for IntParameter object.
        /// If parsing is successful, returns true and the result out-parameter is assigned the parsed integer value. If parsing 
        /// is not successful, returns false and the result out-parameter is assigned null. Beware that the result out-parameter is
        /// of type object, use integer cast for assigning to an int.
        /// </summary>
        /// <param name="value">String input value.</param>
        /// <param name="result">Result object.</param>
        /// <returns>Boolean: was parsing successful?</returns>
        public bool TryParse(string value, out object result)
        {
            int res;
            if (int.TryParse(value, out res) && res <= upperBound && res >= lowerBound)
            {
                result = res;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

        }

    }
}
