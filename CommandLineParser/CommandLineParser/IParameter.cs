using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    /// <summary>
    /// Represents parameter of an option.
    /// Custom parameters (lists, novel parameter types or parameters with complex rules of compliance) can be
    /// created by implementing this interface.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Name of parameter, used for documentation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Specifies if parameter is mandatory, i.e. parameter value must be included if the corresponding option is included in command.
        /// </summary>
        bool IsMandatory { get; }

        /// <summary>
        /// Tries to parse given string and tests whether its value conforms to defined rules, such as
        /// ranges for numerical values, domains of admissible values of a string parameter, correct
        /// formatting, etc. If parsing is successful, returns true and result out-parameter of type object 
        /// is assigned the parsed value. If parsing is not successful, returns false and the result object
        /// is assigned null.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Returns Boolean: was parsing successful?</returns>
        bool TryParse(string value, out object result);
    }

}
