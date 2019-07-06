using System;
using System.Collections.Generic;
using CommandLineParser;

/*#######################################################################################
 *############################# How CommandLineParser works #############################
 *#######################################################################################
 * 
 *    #############################    @ @ @ @ @ @
 *    #      ProgramSettings      #    @ args... @
 *    #############################    @ @ @ @ @ @       throws    ##################
 *              | contains      :         :           - - - - - -> # ParseException # 
 *              V                - - - - -           :             ##################
 *          ##########               :               :
 *          # Option #               V               :
 *          ##########            #############      :     ###############
 *              | contains        #  Parser   #- - - - - > # ParseResult #
 *              V                 #############  output    ###############
 *        ##############                                          | contains
 *        # IParameter #                                          V
 *        ##############                                   ################
 *              | implemented by                           # ParsedOption #
 *           -----------------                             ################
 *          |                 |                  
 *          V                 V               (Option objects contain rules to which
 * ###################  ################      options present in a user command must
 * # StringParameter #  # IntParameter #      conform. ParsedOption objects do not
 * ###################  ################      contain these rules, since they are not
 *                                            needed anymore--parsing was successful.)
 */

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            runTimeCommandExample();
            runNumactlExample();
        }

        private static void runTimeCommandExample()
        {
            ProgramSettings s = new ProgramSettings("time");
            
            s.AddOption('f', "format", false, new StringParameter("FORMAT", true),
                "Specify output format, possibly overriding the format specified in the environment variable TIME.");
            s.AddOption('p', "portability", false, null,
                "Use the portable output format.");
            s.AddOption('o', "output", false, new StringParameter("FILE", true),
                "Do not send the results to stderr, but overwrite the specified file.");
            s.AddOption('a', "append", false, null,
                "(Used together with -o.) Do not overwrite but append.");
            s.AddOption('v', "verbose", false, null,
                "Give very verbose output about all the program knows about.");
            s.AddOption('V', "version", false, null,
                "Print version information on standard output, then exit successfully.");
            s.AddOption(null, "help", false, null,
                "Print a usage message on standard output and exit successfully.");

            s.AddOptionDependency("a", "o"); // both of these options must be present if first option is present

            string cmd = "time -o output_file -a --verbose -- first_file second_file";
            ParseResult result = Parser.Parse(cmd, s);

            // get parameter value for specific option (cast needed)
            string output = (string)result.GetParameterValue("o");
            output = (string)result.GetParameterValue("output");

            // find out whether this option was present (good for non-parametric /Boolean/ options)
            result.WasParsed("v"); // true
            result.WasParsed("p"); // false

            // enumerate all parsed options
            foreach (var item in result.ParsedOptions)
            {
                if (item.Names.Contains("v"))
                {
                    // ...
                }
                else if (item.Names.Contains("o"))
                {
                    string outFile = (string)item.Value;
                    // printOutput(outFile);
                }
                // ...
            }

            // get all the plain arguments in the order they appeared in the command
            List<string> plainArgs = result.PlainArguments;

            // print help using specified TextWriter
            s.PrintHelp(Console.Out);
            //s.PrintHelp(new System.IO.StreamWriter("myfile.txt"));
        }

        private static void runNumactlExample()
        {
            ProgramSettings settings = new ProgramSettings("numactl", 0, 3);
            
            // note the user-defined parameter class: IntListParameter
            settings.AddOption('i', "interleave", false, new IntListParameter("Numa nodes", true, 0, 3), "Interleave memory allocation across given nodes.");
            settings.AddOption('p', "preferred", false, new IntParameter("Numa node", true, 0, 3), "Prefer memory allocation from given node.");
            settings.AddOption('m', "membind", false, new IntListParameter("Numa nodes", true, 0, 3), "Allocate memory allocation from given nodes only.");
            settings.AddOption('C', "physcpubind", false, new IntListParameter("CPUs", true, 0, 31), "Run on given CPUs only.");
            settings.AddOption('S', "show", false, null, "Show current NUMA policy.");
            settings.AddOption('H', "hardware", false, null, "Print hardware configuration.");

            // define conflicting options
            settings.AddOptionConflict("m", "p", "i");

            // Invalid usage: conflicting options. Throws ParseException.
            Console.WriteLine("Examples of ParseException...");
            string command = "numactl -i=0,1,3 -p=0-3";
            try
            {
                ParseResult result = Parser.Parse(command, settings);
            }
            catch (ParseException ex)
            {
                Console.WriteLine("COMMAND: "+command);
                Console.WriteLine("ERROR MESSAGE: " + ex.Message);
            }

            command = "numactl -i 0,1,4";
            // Invalid usage: parameter values out of range. Throws ParseException.
            try
            {
                ParseResult result = Parser.Parse(command, settings);
            }
            catch (ParseException ex)
            {
                Console.WriteLine("COMMAND: " + command);
                Console.WriteLine("ERROR MESSAGE: " + ex.Message);
            }
            Console.WriteLine();

            // Valid usage, grouped options are allowed
            try
            {
                ParseResult result = Parser.Parse("numactl -SH", settings);

                result.WasParsed("S"); // true
                result.WasParsed("hardware"); // true
                result.WasParsed("i"); // false
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Valid usage
            try
            {
                ParseResult result = Parser.Parse("numactl -i 0,1,3 --physcpubind 15-17 -S -- my_file", settings);

                // get parameter value for specific option (explicit cast needed)
                List<int> interleave = (List<int>)result.GetParameterValue("i");

                result.WasParsed("S"); // true
                result.WasParsed("H"); // false

                // enumerate all parsed options
                foreach (var item in result.ParsedOptions)
                {
                    if (item.Names.Contains("i"))
                    {
                        // ...
                    }
                    else if (item.Names.Contains("m"))
                    {
                        // ...
                    }
                    // ...
                }

                // get all plain arguments in the order they appeared in the command
                List<string> plainArgs = result.PlainArguments;
            }
            // check documentation to see all the cases when this exception is thrown
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message);
                settings.PrintHelp(Console.Out);
            }

            // add plain arguments and show their help strings in program documentation
            settings.AddPlainArgument(0, "Output file", "Specify the output file.");
            settings.AddPlainArgument(1, "Output directory", "Specify the output directory.");
            settings.AddPlainArgument(2, "Another argument...");
            settings.PrintHelp(Console.Out);
        }
    }

    /// <summary>
    /// Implemetation of IParameter interface for integer list parameters.
    /// Supports formats such as
    ///     "1,3,8,2"
    ///     "1-5,8,10-17"
    ///     "all" (whole range)
    /// BEWARE: This is only for the purpose of this example. Bad usage is not checked (lower bound higher than upper bound, repeating items in the list, overlapping ranges, null arguments, etc.) 
    /// </summary>
    class IntListParameter : IParameter
    {
        private readonly int minValue;
        private readonly int maxValue;

        public bool IsMandatory { get; }

        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="minValue">Lower range bound, inclusive</param>
        /// <param name="maxValue">Upper range bound, inclusive</param>
        public IntListParameter(string name, bool isMandatory, int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.IsMandatory = isMandatory;
            this.Name = name;
        }

        public bool TryParse(string value, out object result)
        {
            List<int> res = new List<int>();
            result = null;

            //"all" indicates that we want the whole range
            if (value.ToLowerInvariant() == "all")
            {
                for (int i = minValue; i <= maxValue; i++)
                {
                    res.Add(i);
                }
                result = res;
                return true;
            }
            //split lists such as "1,3,8,2" or "1-5,8,10-17" 
            foreach (var nodeArg in value.Split(','))
            {
                if (!nodeArg.Contains('-')) //only one number
                {
                    int i;
                    if (int.TryParse(nodeArg, out i) && i >= minValue && i <= maxValue)
                        res.Add(i);
                    else return false;
                }
                else //range of numbers
                {
                    var numbers = nodeArg.Split('-');
                    if (numbers.Length != 2)
                    {
                        return false;
                    }
                    else
                    {
                        int lower;
                        int upper;
                        if (int.TryParse(numbers[0], out lower) && int.TryParse(numbers[1], out upper)
                            && lower < upper && lower >= minValue && upper <= maxValue) //valid range
                        {
                            for (int i = lower; i <= upper; i++)
                            {
                                res.Add(i);
                            }
                        }
                        else return false;
                    }
                }
            }

            result = res;
            return true;
        }
    }

}


