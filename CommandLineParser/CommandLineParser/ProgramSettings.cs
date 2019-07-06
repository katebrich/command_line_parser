using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandLineParser
{
    /// <summary>
    /// Option class represents an option, possibly with a corresponding parameter, as referred to by a ProgramSettings object.
    /// Options can be added to a ProgramSettings object via its AddOption method by the client, do not call the Option 
    /// constructor directly.
    /// </summary>
    class Option
    {
        internal ICollection<string> Names { get; }

        internal bool IsMandatory { get; }

        internal IParameter Parameter { get; }

        internal string HelpString { get; }

        public Option(ICollection<string> names, bool isMandatory, IParameter parameter = null, string helpString = null)
        {
            this.Names = names;
            this.IsMandatory = isMandatory;
            this.Parameter = parameter;
            this.HelpString = helpString;
        }
    }

    /// <summary>
    /// PlainArgument class represents a plain argument, as referred to by a ProgramSettings object. This is for purposes of 
    /// providing documentation for a specific plain argument (identified by its position in the user's command).
    /// Annotated plain arguments can be added to a ProgramSettings object via its AddPlainArgument method by the client, 
    /// do not call the PlainArgument constructor directly.
    /// </summary>
    class PlainArgument
    {
        internal int Position { get; }

        internal string Name { get; }

        internal string HelpString { get; }

        internal PlainArgument(int position, string name, string helpString)
        {
            this.Position = position;
            this.Name = name;
            this.HelpString = helpString;          
        }
    }

    /// <summary>
    /// ProgramSettings class defines a set of rules for the valid use of a program. The class includes the program name,
    /// a collection of possible options, collections specifying dependencies and conflicts between options a minimum and 
    /// maximum bound for number of plain arguments accepted by the program. An optional description of the program, for 
    /// the purposes of documentation, can be included, as well as documentation strings for options and plain arguments 
    /// accepted by the program.
    /// </summary>
    public class ProgramSettings
    {
        internal List<Option> Options = new List<Option>();
                                                           
        internal List<Option> MandatoryOptions = new List<Option>();
        internal Dictionary<string, Option> OptionsDictionary = new Dictionary<string, Option>();
        internal Dictionary<Option, Option> OptionDependencies = new Dictionary<Option, Option>();
        internal List<List<Option>> OptionConflicts = new List<List<Option>>();

        internal string programName;
        internal int minPlainArgs;
        internal int maxPlainArgs;
        private string helpString;
        private List<PlainArgument> plainArguments = new List<PlainArgument>();

        private string indentSpace = "    "; // for printing help

        /// <summary>
        /// Constructor. Called to instantiate a ProgramSettings object for the purpose of specifying rules which govern 
        /// the eventual parsing of a user's command.
        /// </summary>
        /// <param name="programName">Must be non-empty. Otherwise, throws ArgumentException.</param>
        /// <param name="minPlainArgs">Minimum number of plain arguments needed. If larger than maxPlainArgs or negative, throws ArgumentException.</param>
        /// <param name="maxPlainArgs">Maximum number of plain arguments needed. If smaller than maxPlainArgs, throws ArgumentException.</param>
        /// <param name="helpString">A help string for printing documentation. Optional.</param>
        public ProgramSettings(string programName,  int minPlainArgs = 0, int maxPlainArgs = int.MaxValue, string helpString = null)
        {
            if (string.IsNullOrEmpty(programName))
                throw new ArgumentException(Properties.Resources.ProgramSettings_ProgramNameNotDefined);
            if (minPlainArgs < 0)
                throw new ArgumentException(Properties.Resources.ProgramSettings_PlainArgsCountNegative);
            if (minPlainArgs > maxPlainArgs)
                throw new ArgumentException(Properties.Resources.ProgramSettings_PlainArgsMinMaxViolation);

            this.programName = programName;
            this.minPlainArgs = minPlainArgs;
            this.maxPlainArgs = maxPlainArgs;
            this.helpString = helpString;
        }

        /// <summary>
        /// Adds option to a ProgramSettings object. Option must have at least one name: short (1-character) or long (2+).
        /// Omitting one of the names (passing null) is allowed. 
        /// Option names must be comprised only of letters (lowercase ur uppercase) without diacritics.
        /// Do not include '-' or '--' at beginning of the name ("version" is the recommended format,
        /// rather than "--version"). Duplicit names are forbidden.
        /// </summary>
        /// <param name="shortName">Short name of option (null if absent).</param>
        /// <param name="longName">Long name of option (null if absent).</param>
        /// <param name="isMandatory">Indicates whether option needs to be included in user's command to be valid.</param>
        /// <param name="parameter">Parameter object (implements IParameter). Null if option is Boolean.</param>
        /// <param name="helpString">Option-specific help string which is incorporated in the ProgramSettings object documentation.</param>
        /// <exception cref="System.ArgumentException">Thrown if no name provided (both short name and long name are null), option names contain invalid characters or duplicit option names detected.</exception>
        public void AddOption(char? shortName, string longName, bool isMandatory, IParameter parameter = null, string helpString = null)
        {
            List<string> names;
            string message;
            if (checkAndGetNames(shortName, longName, out names, out message))
            {
                Option option = new Option(names, isMandatory, parameter, helpString);

                Options.Add(option);
                if (isMandatory)
                    MandatoryOptions.Add(option);

                foreach (string name in names)
                    OptionsDictionary.Add(name, option);
            }
            else
                throw new ArgumentException(message);
        }

        /// <summary>
        /// Adds option to a ProgramSettings object. Option must have at least one name: short (1-character) or long (2+).
        /// Option names must be comprised only of letters (lowercase ur uppercase) without diacritics.
        /// Do not include '-' or '--' at beginning of the name ("version" is the recommended format,
        /// rather than "--version"). Duplicit names are forbidden.
        /// </summary>
        /// <param name="names">Non-null, non-empty collection of option names.</param>
        /// <param name="isMandatory">Indicates whether option needs to be included in user's command to be valid.</param>
        /// <param name="parameter">Parameter object (implements IParameter). Null if option is Boolean.</param>
        /// <param name="helpString">Option-specific help string which is incorporated in the ProgramSettings object documentation.</param>
        /// <exception cref="System.ArgumentException">Thrown if no name provided (names list is null or of length zero), option names contain invalid characters or duplicit option names detected.</exception>
        public void AddOption(ICollection<string> names, bool isMandatory, IParameter parameter = null, string helpString = null)
        {
            string message;
            if (checkNames(names, out message))
            {
                Option option = new Option(names, isMandatory, parameter, helpString);

                Options.Add(option);
                if (isMandatory)
                    MandatoryOptions.Add(option);

                foreach (string name in names)
                    OptionsDictionary.Add(name, option);
            }
            else
                throw new ArgumentException(message);
        }

        /// <summary>
        /// Adds dependency between options. Then, if dependent option is present in user command, independent option
        /// must be present also. Otherwise, command is invalid and parsing will be unsuccessful.
        /// </summary>
        /// <param name="dependentOptionName">Any name of the dependent option .</param>
        /// <param name="independentOptionName">Any name of the independent option.</param>
        /// <exception cref="ConstraintException">Thrown if any of the option names is not defined
        /// in the ProgramSettings object, if dependent and independent option is the same (synonymous names are
        /// used) or if dependency cannot be met due to the existence of a conflict (see AddOptionConflict method
        /// of ProgramSettings) which is incompatible with the dependency.</exception>
        public void AddOptionDependency(string dependentOptionName, string independentOptionName)
        {
            if (!isValidOptionName(dependentOptionName) || !isValidOptionName(independentOptionName))
                    throw new ArgumentException(Properties.Resources.ProgramSettings_OptionNameIllegalCharacters);

            string message;
            if (!tryAddOptionDependency(dependentOptionName, independentOptionName, out message))
                throw new ConstraintException(message);       
        }

        /// <summary>
        /// Adds an exclusive relationship between options. From these options, one at most may be present
        /// in the command. Otherwise, user command is invalid.
        /// </summary>
        /// <param name="optionName1">Any name of the first of the conflicting options.</param>
        /// <param name="optionName2">Any name of the second of the conflicting options.</param>
        /// <param name="otherOptionsNames">Any names of other conflicting options.</param>
        /// <exception cref="ConstraintException">Thrown either if any of option names are not defined
        /// in the ProgramSettings object, if any synonymous names are passed as parameters or if conflict cannot
        /// be enforced, due to the existence of a dependency violating the rule (see AddOptionDependency method of
        /// ProgramSettings).</exception>
        /// <exception cref="System.ArgumentException">Thrown if any of the provided names is invalid (null, empty or contains illegal characters).</exception>
        public void AddOptionConflict(string optionName1, string optionName2, params string[] otherOptionsNames)
        {
            List<string> allNames = new List<string>();
            allNames.Add(optionName1);
            allNames.Add(optionName2);
            foreach (string i in otherOptionsNames)
            {
                allNames.Add(i);
            }

            foreach (var name in allNames)
            {
                if (!isValidOptionName(name))
                    throw new ArgumentException(Properties.Resources.ProgramSettings_OptionNameIllegalCharacters);
            }

            string message;
            if (!tryAddOptionConflict(allNames, out message))
            {
                throw new ConstraintException(message);
            }
         
        }

        /// <summary>
        /// Specify plain argument for the purposes of documentation. Name and help text will be displayed
        /// by the PrintHelp method. This is only for the purposes of documentation, since interpreting plain 
        /// arguments is the client's responsibility, not the parser's.
        /// </summary>
        /// <param name="position">Position (0-based) of the select plain argument in command structure. </param>
        /// <param name="name">Name of plain argument.</param>
        /// <param name="helpString">Argument-specific help string which is incorporated in the ProgramSettings object documentation.</param>
        /// <exception cref="System.ArgumentException">Thrown if plain argument name is null or empty.</exception>
        public void AddPlainArgument(int position, string name, string helpString = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(Properties.Resources.ProgramSettings_AddPlainArgument_NameEmpty);
            if (position < 0 || position > this.maxPlainArgs)
                throw new ArgumentException(String.Format(Properties.Resources.ProgramSettings_AddPlainArgument_InvalidPosition, position)); 
            if (plainArguments.Any(x => x.Position == position))
                throw new ArgumentException(String.Format(Properties.Resources.ProgramSettings_AddPlainArgument_ArgumentAlreadyAdded, position));
            plainArguments.Add(new PlainArgument(position, name, helpString));
        }

        /// <summary>
        /// Writes documentation string using a given TextWriter object.
        /// Uses HelpString properties of Option and ProgramSettings objects.
        /// </summary>
        /// <param name="writer">System.IO.TextWriter object used to write documentation.</param>
        /// <exception cref="System.ArgumentException">Thrown if writer is null.</exception>
        public void PrintHelp(System.IO.TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentException(Properties.Resources.ProgramSettings_PrintHelp_WriterNull);
            printHelpHeader(writer);
            writer.WriteLine();

            printGNUOptions(writer);
            writer.WriteLine();

            printGNUStandardOptions(writer);
            writer.WriteLine();

            printPlainArguments(writer);
            writer.WriteLine();

            writer.Flush();
        }

        /// <summary>
        /// Checks whether options with given names are defined, whether they are not synonymous
        /// and whether there is no dependency among them. If these conditions are met, adds conflict.
        /// </summary>
        /// <param name="optionNames">List of option names.</param>
        /// <param name="message">String to which an error message is assigned in case of failure. Otherwise, becomes an empty string.</param>
        /// <returns>Returns Boolean: was adding proposed option conflict successful?</returns>
        private bool tryAddOptionConflict(List<string> optionNames, out string message)
        {
            List<Option> conflicting = new List<Option>();

            foreach (string opt in optionNames)
            {
                Option option;
                if (!OptionsDictionary.TryGetValue(opt, out option))
                {
                    message = Properties.Resources.ProgramSettings_OptionNameNotDefined;
                    return false;
                }
                conflicting.Add(option);
            }

            if (conflicting.Count != conflicting.Distinct().Count())
            {
                message = Properties.Resources.ProgramSettings_SynonymousNames;
                return false;
            }

            // The following loop checks if the proposed conflict is not negated by an existing dependency.
            // This scales badly with more dependencies, but is okay for typical usage.
            foreach (var item in OptionDependencies)
            {
                if (conflicting.Contains(item.Key) && conflicting.Contains(item.Value))
                {
                    message = Properties.Resources.ProgramSettings_OptionsDependent;
                    return false;
                }
            }

            OptionConflicts.Add(conflicting);
            message = null;
            return true;
        }

        /// <summary>
        /// Checks whether options with given names are defined, whether they are not synonymous
        /// and whether there is no conflict between them. If these conditions are met, adds dependency.
        /// </summary>
        /// <param name="dependentOptionName">Any name of the dependent option.</param>
        /// <param name="independentOptionName">Any name of the independent option.</param>
        /// <param name="message">String to which an error message is assigned, in case of failure. Otherwise, becomes an empty string.</param>
        /// <returns>Returns Boolean: was adding proposed option dependency successful?</returns>
        private bool tryAddOptionDependency(string dependentOptionName, string independentOptionName, out string message)
        {
            Option dependentOption, independentOption;

            if (!OptionsDictionary.TryGetValue(dependentOptionName, out dependentOption)
                || !OptionsDictionary.TryGetValue(independentOptionName, out independentOption))
            {
                message = Properties.Resources.ProgramSettings_OptionNameNotDefined;
                return false;
            }

            if (dependentOption == independentOption)
            {
                message = Properties.Resources.ProgramSettings_SynonymousNames;
                return false;
            }

            // The following loop checks if the proposed dependency is not negated by an existing conflict.
            // This scales badly with more conflicts, but is okay for typical usage.
            foreach (List<Option> item in OptionConflicts)
            {
                if (item.Contains(dependentOption) && item.Contains(independentOption))
                {
                    message = Properties.Resources.ProgramSettings_OptionsInConflict;
                    return false;
                }
            }

            OptionDependencies.Add(dependentOption, independentOption);
            message = null;
            return true;
        }

        /// <summary>
        /// Checks whether at least one of given names for an option is not null,
        /// whether there are no illegal characters in names (only letters permitted) and 
        /// whether the given names are unique. All non-null names are collected in a list.
        /// </summary>
        /// <param name="shortName">Short name of option.</param>
        /// <param name="longName">Long name of option.</param>
        /// <param name="message">String to which an error message is assigned, in case of failure. Otherwise, becomes an empty string.</param>
        private bool checkAndGetNames(char? shortName, string longName, out List<string> names, out string message)
        {
            if (shortName == null && longName == null)
            {
                message = Properties.Resources.ProgramSettings_OptionNameNotDefined;
                names = null;
                return false;
            }
            foreach (string key in OptionsDictionary.Keys)
            {
                if (key == shortName.ToString() || key == longName)
                {
                    message = Properties.Resources.ProgramSettings_OptionNameDuplicit;
                    names = null;
                    return false;
                }
            }
            names = new List<string>();
            if (shortName != null)
            {
                if (isValidOptionName(shortName.ToString()))
                {
                    names.Add(shortName.ToString());
                }
                else
                {
                    names = null;
                    message = Properties.Resources.ProgramSettings_OptionNameIllegalCharacters;
                    return false;
                }
            }
            if (longName != null)
            {
                if (isValidOptionName(longName))
                {
                    names.Add(longName);
                }
                else
                {
                    names = null;
                    message = Properties.Resources.ProgramSettings_OptionNameIllegalCharacters;
                    return false;
                }
            }
            message = null;
            return true;
        }

        /// <summary>
        /// Checks whether there are no illegal characters in given option names (only letters are permitted).
        /// </summary>
        /// <param name="names">Collection of option names.</param>
        /// <param name="message"></param>
        /// <returns>String to which an error message is assigned, in case of failure. Otherwise, becomes an empty string.</returns>
        private bool checkNames(ICollection<string> names, out string message)
        {
            if (names == null || names.Count == 0)
            {
                message = Properties.Resources.ProgramSettings_OptionNameNotDefined;
                return false;
            }
            foreach (string item in names)
            {
                if (!isValidOptionName(item))
                {
                    message = Properties.Resources.ProgramSettings_OptionNameIllegalCharacters;
                    return false;
                }
            }
            message = null;
            return true;
        }

        /// <summary>
        /// Writes documentation header using a TextWriter object.
        /// </summary>
        /// <param name="writer">TextWriter object for outputting a documentation header.</param>
        private void printHelpHeader(TextWriter writer)
        {
            writer.WriteLine(indentSpace + programName + " [options] command [ -- arguments...]");
        }

        /// <summary>
        /// Writes GNU standard options documentation using a TextWriter object.
        /// </summary>
        /// <param name="writer">TextWriter object for output.</param>
        private void printGNUStandardOptions(TextWriter writer)
        {
            writer.WriteLine(indentSpace + "GNU Standard Options");

            writer.WriteLine(indentSpace + indentSpace + "--help Print a usage message on standard output and exit successfully.");
            writer.WriteLine(indentSpace + indentSpace + "- V, --version");
            writer.WriteLine(indentSpace + indentSpace + "Print version information on standard output, then exit successfully.");
            writer.WriteLine(indentSpace + indentSpace + "--Terminate option list.");
        }

        /// <summary>
        /// Writes GNU options documentation using a TextWriter object.
        /// </summary>
        /// <param name="writer">TextWriter object for output.</param>
        private void printGNUOptions(TextWriter writer)
        {
            writer.WriteLine(indentSpace + "GNU Options");

            foreach (Option opt in Options)
            {
                printOption(writer, opt);
            }
        }

        /// <summary>
        /// Writes plain arguments documentation using a TextWriter object.
        /// </summary>
        /// <param name="writer">TextWriter object for output.</param>
        private void printPlainArguments(TextWriter writer)
        {
            writer.WriteLine(indentSpace + "Arguments (following the delimiter --)");

            string word = "arguments";

            if (minPlainArgs != 0) //lower bound defined
            {
                if (minPlainArgs == 1)
                    word = "argument";
                writer.WriteLine(indentSpace + indentSpace + "At least " + minPlainArgs + " plain " + word + " must be defined.");
            }

            if (maxPlainArgs != int.MaxValue) //upper bound defined
            {
                if (maxPlainArgs == 1)
                    word = "argument";
                writer.WriteLine(indentSpace + indentSpace + "At most " + maxPlainArgs + " plain " + word + " can be defined.");
            }

            foreach (var arg in plainArguments)
            {
                writer.WriteLine(indentSpace + indentSpace + "Argument at position " + arg.Position + ": " + arg.Name);
                if (arg.HelpString != null)
                    writer.WriteLine(indentSpace + indentSpace + indentSpace + arg.HelpString);
            }

        }

        /// <summary>
        /// Writes option documentation using a TextWriter object.
        /// </summary>
        /// <param name="writer">TextWriter object for output.</param>
        /// <param name="option">Option object for which documentation is to be printed.</param>
        private void printOption(TextWriter writer, Option option)
        {           
            StringBuilder sb = new StringBuilder();
            string prefix = "";
            foreach (var name in option.Names)
            {
                sb.Append(prefix);
                prefix = ", ";
                if (name.Length == 1) //short option
                {
                    sb.Append("-" + name);
                    if (option.Parameter != null)
                        sb.Append(" " + option.Parameter.Name);
                }
                else //long option
                {
                    sb.Append("--" + name);
                    if (option.Parameter != null)
                        sb.Append("=" + option.Parameter.Name);
                }
            }
            writer.Write(indentSpace + indentSpace + sb.ToString());
            writer.WriteLine();
            if (option.HelpString != null)
            {
                writer.Write(indentSpace + indentSpace + indentSpace);
                writer.WriteLine(option.HelpString);
            }
        }

        /// <summary>
        /// Checks whether an option name is valid, i.e. is comprised only of letters.
        /// </summary>
        /// <param name="name">Name of option to be checked.</param>
        /// <returns>Boolean, is option name valid?</returns>
        internal static bool isValidOptionName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            return Regex.IsMatch(name, @"^[a-zA-Z]+$");
        }
    }
}
