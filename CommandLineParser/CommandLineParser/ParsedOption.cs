using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    /// <summary>
    /// Class representing a single parsed option (defined by its names) and 
    /// parsed parameter value (if any).
    /// </summary>
    public class ParsedOption
    {
        /// <summary>
        /// All names of the option.
        /// </summary>
        public ICollection<string> Names { get; }

        /// <summary>
        /// Parsed value of option parameter. Null if no parameter was given for this option.
        /// Return type corresponds to the specific IParameter implementation that was given for
        /// this option (IntParameter returns int, etc.). Explicit cast is needed to assign Value
        /// to an object of the corresponding type.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Constructor. Do not call directly, this is done by Parser.
        /// </summary>
        public ParsedOption(ICollection<string> names, object value)
        {
            this.Names = names;
            this.Value = value;
        }
    }
}
