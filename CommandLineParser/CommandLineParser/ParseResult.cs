using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    /// <summary>
    /// Compact representation of information obtained by parsing a command.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// Collection of all options which were present in user's command.
        /// </summary>
        public List<ParsedOption> ParsedOptions { get; private set; } = new List<ParsedOption>();

        /// <summary>
        /// List of plain arguments in the same order as they appeared in the command.
        /// </summary>
        public List<string> PlainArguments { get; private set; } = new List<string>();

        // Multi-key dictionary for fast lookup of parsed options.
        private Dictionary<string, ParsedOption> parsedOptions = new Dictionary<string, ParsedOption>();

        /// <summary>
        /// Returns value of parameter (if any). The type of the returned object corresponds
        /// to the specific IParameter implementation (IntParameter returns int etc.) Explicit
        /// cast is needed to assign the value to an object of the corresponding type.
        /// </summary>
        /// <param name="optionName">Any of the names of a select option.</param>
        /// <returns>Parameter value. Null if not found.</returns>
        /// <exception cref="System.ArgumentException">Thrown if option name is null or empty.</exception>
        public object GetParameterValue(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
                throw new ArgumentException(Properties.Resources.ParseResult_OptionNameNull);
            ParsedOption opt;
            if (parsedOptions.TryGetValue(optionName, out opt))
            {
                return opt.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns whether a select option was parsed (present in the command and valid).
        /// </summary>
        /// <param name="optionName">Any of the names of a select option.</param>
        /// <returns>Boolean, whether option was parsed.</returns>
        /// <exception cref="System.ArgumentException">Thrown if option name is null or empty.</exception>
        public bool WasParsed(string optionName)
        {
            if (string.IsNullOrEmpty(optionName))
                throw new ArgumentException(Properties.Resources.ParseResult_OptionNameNull);
            return parsedOptions.ContainsKey(optionName);
        }

        /// <summary>
        /// Adds parsed option to a ParseResult object. This method is called by Parser.
        /// </summary>
        /// <param name="parsedOption">ParsedOption object to be added to a ParseResult object.</param>
        internal void addOption(ParsedOption parsedOption)
        {
            ParsedOptions.Add(parsedOption);

            foreach (var name in parsedOption.Names)
            {
                parsedOptions.Add(name, parsedOption);
            }
        }

        /// <summary>
        /// Adds parsed plain arguments to a ParseResult object. This method is called by Parser.
        /// </summary>
        /// <param name="plainArgs">List of parsed plain arguments (their string values) to be added to a ParseResult object.</param>
        internal void addPlainArgs(List<string> plainArgs)
        {
            PlainArguments = plainArgs;
        }
    }
}
