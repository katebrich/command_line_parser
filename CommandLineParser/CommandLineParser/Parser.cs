using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace CommandLineParser
{
    /// <summary>
    /// Parses given command following rules defined by a ProgramSettings object.
    /// Returns a ParseResult object, which is a compact representation of information obtained by parsing.
    /// </summary>
    /// <exception cref="CommandLineParser.ParseException">Thrown with corresponding message, if:
    ///     - a mandatory option is missing,
    ///     - a parameter is given for an option which does not accept any parameter,
    ///     - wrong type of an option parameter is given,
    ///     - wrong number of plain arguments is given,
    ///     - wrong number of option parameters is given,
    ///     - dependency between options is unmet,
    ///     - conflicting options are given,
    ///     - format of command is invalid.</exception>
    public class Parser
    {
        /// <summary>
        /// Takes array of strings (representing command arguments) and parses
        /// the arguments according to rules defined in a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="args">String arguments array.</param>
        /// <param name="settings">Corresponding ProgramSettings object.</param>
        /// <returns>Returns a ParseResult object, which is a compact representation of information obtained by parsing.</returns>
        /// <exception cref="System.ArgumentException">Thrown if settings are undefined.</exception>
        /// <exception cref="CommandLineParser.ParseException">Thrown if command is illegal.</exception>
        public static ParseResult Parse(string[] args, ProgramSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_SettingsNull);
            }
            if (args == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_ArgumentsNull);
            }

            if (!tryParse(args, settings, out string message, out ParseResult result))
            {
                throw new ParseException(message);
            }
            return result;
        }

        /// <summary>
        /// Parses given string representing a command according to rules defined in a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="cmdLine">Single command string.</param>
        /// <param name="settings">Corresponding ProgramSettings object.</param>
        /// <returns>Returns a ParseResult object, which is a compact representation of information obtained by parsing.</returns>
        /// <exception cref="System.ArgumentException">Thrown if settings are undefined.</exception>
        /// <exception cref="CommandLineParser.ParseException">Thrown if command is illegal.</exception>
        public static ParseResult Parse(string cmdLine, ProgramSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_SettingsNull);
            }
            if (cmdLine == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_CmdLineNull);
            }

            if (!tryParse(argumentsArray(cmdLine), settings, out string message, out ParseResult result))
            {
                throw new ParseException(message);
            }
            return result;
        }

        /// <summary>
        /// Reads a TextReader stream (representing a command) and parses arguments according
        /// to rules defined a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="reader">System.IO.TextReader Object.</param>
        /// <param name="settings">Corresponding ProgramSettings object.</param>
        /// <returns>Returns a ParseResult object, which is a compact representation of information obtained by parsing.</returns>
        /// <exception cref="System.ArgumentException">Thrown if settings are undefined.</exception>
        /// <exception cref="CommandLineParser.ParseException">Thrown if command is illegal.</exception>
        public static ParseResult Parse(System.IO.TextReader reader, ProgramSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_SettingsNull);
            }
            if (reader == null)
            {
                throw new ArgumentException(Properties.Resources.Parser_Parse_ReaderNull);
            }

            if (!tryParse(argumentsArray(reader.ReadLine()), settings, out string message, out ParseResult result))
            {
                throw new ParseException(message);
            }
            return result;
        }

        /*###########################################################################################
         *############# PARSING SCHEME: which internal methods are called and when. #################
         *############# An arrow from method (A) to method (B) means 'A calls B'... #################
         *###########################################################################################
         * 
         * ####################  ########################  ######################
         * # Parse(string...) #  # Parse(TextReader...) #  # Parse(string[]...) #
         * ####################  ########################  ######################
         *           |                       |                        |
         *            -----------------------                         |        
         *                      |                                     |
         *                      V                                     |
         *         #############################                      |
         *         #      argumentsArray       #                      |
         *         #############################                      |
         *                      |                                     |
         *                       -------------------------------------
         *                                         |
         *                                         V
         *                                    ############
         *                                    # tryParse #
         *                                    ############
         *                                         |
         *               -----------------------------------------------
         *              |                   |                           |
         *              V                   V                           V
         *  #####################  ###################            ##################################
         *  # tryParsePlainArgs #  # tryParseOptions #            #      mandatoryOptionsParsed    #
         *  #####################  ###################            ##################################
         *                                  |                     #         dependenciesMet        #
         *                   ---------------------                ##################################
         *                  |                     |               #        noConflictsPresent      #
         *                  V                     V               ##################################
         *  ##########################  ########################  #  plainArgumentsCountAdmissible #
         *  # tryParseGroupedOptions #  # tryParseSingleOption #  ##################################     
         *  ##########################  ######################## 
         *                  |                     ^
         *                  |                     |
         *                   ---------------------
         *         
         *  (...tryParseGroupedOptions takes a group of options (eg. '-abc') and parses all but the
         *  last option, which is then parsed by tryParseSingleOption. This is since only
         *  tryParseSingleOption can parse an option with a parameter value, either included in the
         *  same argument or in the next argument.)        
         *         
        */

        /// <summary>
        /// Converts a single string of arguments (separated by whitespace) to an array of single argument strings.
        /// </summary>
        /// <param name="cmdLine">Command string.</param>
        /// <returns>Returns a string array of arguments.</returns>
        internal static string[] argumentsArray(string cmdLine) => Regex.Split(cmdLine, @"\s+");

        /// <summary>
        /// Tries to parse a string array of arguments, checking compliance with the rules formulated by a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="args">A string array of arguments.</param>
        /// <param name="settings">Corresponding ProgramSettings object which specifies rules to which the parsed arguments must conform.</param>
        /// <param name="message">String to which an error message is assigned if any rules are found to be violated in the process of parsing. Otherwise, becomes an empty string.</param>
        /// <param name="result">ParseResult object to which the results of parsing are assigned if all rules are met. Otherwise, NULL.</param>
        /// <returns>Returns a ParseResult object, which is a compact representation of information obtained by parsing.</returns>
        internal static bool tryParse(string[] args, ProgramSettings settings, out string message, out ParseResult result)
        {
            result = new ParseResult();
            if (args.Length == 0)
            {
                message = Properties.Resources.Parser_tryParse_NoArguments;
                return false;
            }

            int argsIterator = 0;

            if (args[0] == settings.programName)
                ++argsIterator;

            while (argsIterator < args.Length)
            {
                if (tryParsePlainArgs(ref args, argsIterator, ref result))
                    break;

                if (tryParseOptions(ref args, argsIterator, ref result, ref settings, out message))
                    ++argsIterator;
                else
                    return false;
            }

            if (!mandatoryOptionsParsed(ref result, ref settings, out message))
                return false;

            if (!dependenciesMet(ref result, ref settings, out message))
                return false;

            if (!noConflictsPresent(ref result, ref settings, out message))
                return false;

            if (!plainArgumentsCountAdmissible(ref result, ref settings, out message))
                return false;

            message = "";
            return true;
        }

        /// <summary>
        /// Provided a reference to a string array of arguments and an iterator (index), tries to parse plain arguments which 
        /// may be included as part of a user's command. If plain arguments are found at given position, they are added 
        /// into a ParseResult object, via reference passed to the method.
        /// </summary>
        /// <param name="args">String array of arguments.</param>
        /// <param name="argsIterator">Index of element in the arguments array which is evaluated (if this is a separator, what follows is considered plain arguments and parsed).</param>
        /// <param name="result">ParseResult object. If plain arguments are present, they are added to the object via its addPlainArgs method.</param>
        /// <returns>Returns a Boolean value: were plain arguments parsed successfully?</returns>
        internal static bool tryParsePlainArgs(ref string[] args, int argsIterator, ref ParseResult result)
        {
            if (args[argsIterator] == Properties.Resources.Parser_tryParsePlainArgs_ArgumentsSeparator) // if at separator
            {
                List<string> plainArgs = new List<string>();
                for (int i = (argsIterator + 1); i < args.Length; ++i)
                    plainArgs.Add(args[i]);
                result.addPlainArgs(plainArgs);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Provided a reference to a string array of arguments, an iterator (index), a reference to a corresponding ParseResult object 
        /// and a reference to the relevant ProgramSettings object, tries to parse all options and their parameter values within the 
        /// sequence of arguments.
        /// </summary>
        /// <param name="args">String array of arguments.</param>
        /// <param name="argsIterator">Index of element in the arguments array which is being evaluated.</param>
        /// <param name="result">ParseResult object to which parsed options are to be added.</param>
        /// <param name="settings">Corresponding ProgramSettings object, specifying the rules by which the processes which check legality of parsed options are governed.</param>
        /// <param name="message">String to which a potential error message is assigned. Otherwise, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: were the evaluated arguments parsed successfully as options and, potentially, their parameter values?</returns>
        internal static bool tryParseOptions(ref string[] args, int argsIterator, ref ParseResult result, ref ProgramSettings settings, out string message)
        {
            Regex parameterRegex = new Regex(@Properties.Resources.Parser_tryParseOptions_ParameterRegexString);
            if (parameterRegex.Match(args[argsIterator]).Success)
            { // if iterator at parameter, skip (this is handled with options directly)
                message = "";
                return true;
            }

            Regex singleOptionRegex = new Regex(@Properties.Resources.Parser_tryParseOptions_SingleOptionRegexString);
            Regex groupedOptionsRegex = new Regex(@Properties.Resources.Parser_tryParseOptions_GroupedOptionsRegexString);

            string optionName = "";
            string parameterString = "";

            var groupedOptionsMatch = groupedOptionsRegex.Match(args[argsIterator]);
            var singleOptionMatch = singleOptionRegex.Match(args[argsIterator]);

            if (groupedOptionsMatch.Success)
            { // if iterator at group of options
                if (!tryParseGroupedOptions(ref groupedOptionsMatch, ref result, ref settings, out message, out optionName, out parameterString))
                    return false;
            }
            else if (singleOptionMatch.Success)
            {
                optionName = singleOptionMatch.Groups["optionName"].ToString();
                parameterString = singleOptionMatch.Groups["parameterString"].ToString();
            }
            else
            {
                message = String.Format(Properties.Resources.Parser_tryParseOptions_InvalidOption, argsIterator, args[argsIterator]);
                return false;
            }

            if (!tryParseSingleOption(ref parameterRegex, optionName, parameterString, ref args, argsIterator, ref result, ref settings, out message))
                return false;

            message = "";
            return true;
        }


        /// <summary>
        /// Provided a reference to a Match object, which is resulted from matching an argument against a grouped option regex, 
        /// a reference to a ParseResult object and a reference to the corresponding ProgramSettings object, tries to parse a 
        /// group of options, except for the last option. Furthermore, the name of the last option and its string parameter value 
        /// (if it is provided as part of the same argument) are assigned to out-parameters of the method. (The method 
        /// tryParseSingleOption is to be then used for the last option, since that method handles the possibility of a 
        /// parameter value provided in a separate argument.)
        /// </summary>
        /// <param name="groupedOptionsMatch">A regex Match object generated by matching an argument string against a Regex object for grouped options.</param>
        /// <param name="result">ParseResult object. If legal options are detected, they are added to the object via its addOption method.</param>
        /// <param name="settings">Corresponding ProgramSettings object, used for checking if detected options exist.</param>
        /// <param name="message">String to which a potential error message is assigned. Otherwise, becomes an empty string.</param>
        /// <param name="lastOptionName">String to which name of the last option within the evaluated group of options is assigned. Otherwise if parsing fails, becomes an empty string.</param>
        /// <param name="parameterString">String to which string parameter value of the last option is assigned. Otherwise if parsing fails, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: were grouped options parsed successfully?</returns>
        internal static bool tryParseGroupedOptions(ref Match groupedOptionsMatch, ref ParseResult result, ref ProgramSettings settings, out string message, out string lastOptionName, out string parameterString)
        {
            char[] groupedOptionNames = groupedOptionsMatch.Groups["optionName"].ToString().ToCharArray();

            for (int i = 0; i < (groupedOptionNames.Length - 1); ++i)
            { // try adding all options but the last (these do not have any parameters specified)
                Option groupedOption;
                string groupedOptionName = groupedOptionNames[i].ToString();
                if (!settings.OptionsDictionary.TryGetValue(groupedOptionName, out groupedOption)) // get option name
                { // if option name not recognised, error
                    message = String.Format(Properties.Resources.Parser_tryParseGroupedOptions_InvalidOptionName, groupedOptionName);
                    lastOptionName = "";
                    parameterString = "";
                    return false;
                }
                else
                { // if recognised
                    if (groupedOption.Parameter == null || !groupedOption.Parameter.IsMandatory)
                    { // if no parameter needed, add it to results
                        ParsedOption parsedOption = new ParsedOption(groupedOption.Names, value: null);
                        result.addOption(parsedOption);
                    }
                    else
                    { // otherwise if parameter needed, error
                        message = String.Format(Properties.Resources.Parser_tryParseGroupedOptions_NoParameterValue, groupedOptionName);
                        lastOptionName = "";
                        parameterString = "";
                        return false;
                    }
                }
            }
            message = "";
            lastOptionName = groupedOptionNames[groupedOptionNames.Length - 1].ToString();
            parameterString = groupedOptionsMatch.Groups["parameterString"].ToString();
            return true;
        }

        /// <summary>
        /// Provided a reference to a Regex object for matching an argument string (possibly with parameter value), an option name string, 
        /// a parameter value string, reference to a string array of arguments, an iterator (index), a reference to a ParseResult 
        /// object and a reference to a corresponding ProgramSettings object, tries to parse a single option, with a possible parameter value.
        /// If successful, adds the option to the given ParseResult object using its addOption method.
        /// </summary>
        /// <param name="parameterRegex">Regex for matching a parameter value string.</param>
        /// <param name="optionName">Name of the option which should be parsed.</param>
        /// <param name="parameterString">Relevant parameter string value.</param>
        /// <param name="args">String array of arguments.</param>
        /// <param name="argsIterator">Index of element in the arguments array which is being evaluated.</param>
        /// <param name="result">ParseResult object. If option is legal, it is added, possibly with a corresponding parameter value, to the result via its addOption method.</param>
        /// <param name="settings">Corresponding ProgramSettings object, used for checking whether the parsed option and its potential parameter value are legal.</param>
        /// <param name="message">String to which a potential error message is assigned. Otherwise, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: was the option (and, potentially, its parameter value) parsed successfully?</returns>
        internal static bool tryParseSingleOption(ref Regex parameterRegex, string optionName, string parameterString, ref string[] args, int argsIterator, ref ParseResult result, ref ProgramSettings settings, out string message)
        {
            Option option;
            object parameterValue = null;
            if (!settings.OptionsDictionary.TryGetValue(optionName, out option)) // get option name
            { // if option name not recognised, error
                message = String.Format(Properties.Resources.Parser_tryParseSingleOption_InvalidOptionName, optionName);
                return false;
            }

            if (parameterString == "" && option.Parameter != null)
            { // if no parameter value was provided yet and option accepts parameter
                if ((argsIterator + 1) < args.Length)
                { // look for separate parameter value in the following position
                    var parameterMatch = parameterRegex.Match(args[argsIterator + 1]);
                    if (parameterMatch.Success)
                    { // if separate parameter value found
                        parameterString = parameterMatch.Groups["parameterString"].ToString();
                        if (!option.Parameter.TryParse(parameterString, out parameterValue)) // get parameter value
                        { // if it is invalid, error
                            message = String.Format(Properties.Resources.Parser_tryParseSingleOption_InvalidParameterValue, optionName);
                            return false;
                        }
                    } // otherwise OK
                    else
                    { // else if value not found in the following position
                        if (option.Parameter.IsMandatory)
                        { // if it is mandatory, error
                            message = String.Format(Properties.Resources.Parser_tryParseSingleOption_NoParameterValue, optionName);
                            return false;
                        }
                        parameterValue = null;
                    } // otherwise OK
                }
            }
            else
            { // else if parameter value was provided, OR was not provided BUT option does not accept parameter...
                if (parameterString != "" && option.Parameter == null)
                { // if option does not accept parameter and parameter is provided, error
                    message = String.Format(Properties.Resources.Parser_tryParseSingleOption_TooManyParameterValue, optionName);
                    return false;
                }
                // else
                if (parameterString != "")
                {
                    if (!option.Parameter.TryParse(parameterString, out parameterValue)) // get parameter value
                    { // if parameter value is invalid, error
                        message = String.Format(Properties.Resources.Parser_tryParseSingleOption_InvalidParameterValue, optionName);
                        return false;
                    }
                }
                else
                { // otherwise OK
                    parameterValue = null;
                }
            }
            ParsedOption parsedOption = new ParsedOption(option.Names, parameterValue);
            result.addOption(parsedOption);
            message = "";
            return true;
        }

        /// <summary>
        /// Examines a ParseResult object and determines whether all mandatory options were parsed, with respect to rules formulated 
        /// by a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="result">ParseResult object to be examined.</param>
        /// <param name="settings">Corresponding ProgramSettings object which specifies which options are mandatory.</param>
        /// <param name="diagnosis">String to which an error message is assigned if any mandatory options are missing. Otherwise, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: were all mandatory options parsed?</returns>
        internal static bool mandatoryOptionsParsed(ref ParseResult result, ref ProgramSettings settings, out string diagnosis)
        {
            foreach (Option option in settings.MandatoryOptions)
            {
                string name = option.Names.AsEnumerable().ElementAt(0);
                if (!result.WasParsed(name))
                {
                    diagnosis = String.Format(Properties.Resources.Parser_mandatoryOptionsParsed_mandatoryOptionMissing, name);
                    return false;
                }
            }
            diagnosis = "";
            return true;
        }

        /// <summary>
        /// Examines a ParseResult object and determines whether all option dependencies were met, with respect to rules formulated by a 
        /// corresponding ProgramSettings object.
        /// </summary>
        /// <param name="result">ParseResult object to be examined.</param>
        /// <param name="settings">Corresponding ProgramSettings object which specifies option dependencies.</param>
        /// <param name="diagnosis">String to which an error message is assigned if any dependencies are unmet. Otherwise, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: were all dependencies met?</returns>
        internal static bool dependenciesMet(ref ParseResult result, ref ProgramSettings settings, out string diagnosis)
        {
            foreach (KeyValuePair<Option, Option> entry in settings.OptionDependencies)
            {
                string dependentName = entry.Key.Names.AsEnumerable().ElementAt(0);
                if (result.WasParsed(dependentName))
                {
                    string independentName = entry.Value.Names.AsEnumerable().ElementAt(0);
                    if (!result.WasParsed(independentName))
                    {
                        diagnosis = String.Format(Properties.Resources.Parser_dependenciesMet_dependencyUnmet, dependentName, independentName);
                        return false;
                    }
                }
            }
            diagnosis = "";
            return true;
        }

        /// <summary>
        /// Examines a ParseResult object and determines whether any conflicting options were parsed, with respect 
        /// to rules formulated by a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="result">ParseResult object to be examined.</param>
        /// <param name="settings">Corresponding ProgramSettings object which specifies which sets of options are in conflict.</param>
        /// <param name="diagnosis">String to which an error message is assigned if any conflicting options are present. Otherwise, becomes an 
        /// empty string.</param>
        /// <returns>Returns a Boolean value: is the ParseResult object free of any option conflicts?</returns>
        internal static bool noConflictsPresent(ref ParseResult result, ref ProgramSettings settings, out string diagnosis)
        {
            foreach (List<Option> conflictingOptions in settings.OptionConflicts)
            {
                string[] conflicting = new string[2];
                int mutuallyExclusiveOptionsCount = 0;
                foreach (Option exclusiveOption in conflictingOptions)
                {
                    string name = exclusiveOption.Names.AsEnumerable().ElementAt(0);
                    if (result.WasParsed(name))
                    {
                        conflicting[mutuallyExclusiveOptionsCount] = name;
                        ++mutuallyExclusiveOptionsCount;
                    }
                    if (mutuallyExclusiveOptionsCount > 1)
                        break;
                }
                if (mutuallyExclusiveOptionsCount > 1)
                {
                    diagnosis = String.Format(Properties.Resources.Parser_noConflictingOptions_conflictPresent, conflicting[0], conflicting[1]);
                    return false;
                }
            }
            diagnosis = "";
            return true;
        }

        /// <summary>
        /// Examines a ParseResult object and determines whether the number of parsed plain arguments is admissible, 
        /// with respect to minimum and maximum counts formulated by a corresponding ProgramSettings object.
        /// </summary>
        /// <param name="result">ParseResult object to be examined.</param>
        /// <param name="settings">Corresponding ProgramSettings object which specifies the minimum and maximum number of plain arguments to be parsed.</param>
        /// <param name="diagnosis">String to which an error message is assigned if plain arguments count is not admissible. Otherwise, becomes an empty string.</param>
        /// <returns>Returns a Boolean value: is plain arguments count admissible?</returns>
        internal static bool plainArgumentsCountAdmissible(ref ParseResult result, ref ProgramSettings settings, out string diagnosis)
        {
            int count = result.PlainArguments.Count;
            if (count < settings.minPlainArgs || count > settings.maxPlainArgs)
            {
                diagnosis = Properties.Resources.Parser_plainArgumentsCountAdmissible_illegalNumberArgs;
                return false;
            }
            diagnosis = "";
            return true;
        }
    }
}
