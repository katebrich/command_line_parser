using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    /// <summary>
    /// Thrown by ProgramSettings.AddOptionDependency or ProgramSettings.AddOptionConflict if adding 
    /// option dependency or conflict is unsuccessful due to reference to a non-existing option or 
    /// due to logical constraints. This occurs if
    ///     - any of the option names in a proposed dependency or conflict is not defined in the 
    ///       corresponding ProgramSettings object,
    ///     - dependent and inpedendent option names in a dependency are synonyms,
    ///     - option dependency is negated by an option conflict,
    ///     - option conflict is negated by an option dependency.
    /// </summary>
    public class ConstraintException : Exception
    {
        public ConstraintException()
        {
        }

        public ConstraintException(string message) : base(message)
        {
        }

        public ConstraintException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
