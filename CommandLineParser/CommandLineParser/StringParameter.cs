using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommandLineParser
{
    public class StringParameter : IParameter
    {
        public string Name { get; }
        public bool IsMandatory { get; }

        private ICollection<string> domain = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Parameter name. (For the purposes of documentation.)</param>
        /// <param name="isMandatory">Boolean, indicates whether parameter must be present.</param>
        /// <param name="domain">Admissible values for this parameter. Null if unlimited.</param>
        /// <exception cref="System.ArgumentException">Thrown if name is null or empty.</exception>
        public StringParameter(string name, bool isMandatory = true, ICollection<string> domain = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(Properties.Resources.StringParameter_ParameterNameNotDefined);
            this.Name = name;
            this.IsMandatory = isMandatory;
            this.domain = domain;
        }

        /// <summary>
        /// Checks whether a value can be converted to string and falls into defined domain.
        /// If parsing is successful, returns true and result out-parameter is assigned the parsed string 
        /// value. If parsing is not successful, returns false and the result object is assigned null. 
        /// Beware that the result out-parameter is of type object, use string cast for assigning to a string.
        /// </summary>
        /// <param name="value">String input value.</param>
        /// <param name="result">Result object.</param>
        /// <returns>Bool indicating whether parsing was successful.</returns> 
        /// <exception cref="System.ArgumentException">Thrown if input value is null or empty.</exception>
        public bool TryParse(string value, out object result)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(Properties.Resources.StringParameter_TryParse_ValueEmpty);

            string str = value;

            if (domain != null) //we need to check the domain
            {
                foreach (var item in domain)
                {
                    if (str == item)
                    {
                        result = str;
                        return true;
                    }
                }
                //no item in domain equals given string
                result = null;
                return false;
            }
            else //domain is unlimited
            {
                result = str;
                return true;
            }
        }

    }
}
