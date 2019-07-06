using System;
using System.Collections;
using System.Collections.Generic;

namespace CommandLineParser
{
    /// <summary>
    /// ParseException is thrown when:
    ///     - a mandatory option is missing,
    ///     - wrong type of an option parameter is given,
    ///     - wrong number of plain arguments is given,
    ///     - wrong number of option parameters is given,
    ///     - dependency between options is unmet,
    ///     - conflicting options are given,
    ///     - format of command is invalid.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
