using CommandLineParser;
using System;
using Xunit;

namespace CommandLineParserTests
{
    public class ProgramSettingsTests
    {
        #region Constructor

        [Fact]
        public void Constructor_NullName()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                var ps = new ProgramSettings(null);
            });
        }

        [Fact]
        public void Constructor_EmptyName()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                var ps = new ProgramSettings("");
            });
        }

        [Fact]
        public void Constructor_MinPlainArgsGreaterThanMaxPlainArgs()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                var ps = new ProgramSettings("command", minPlainArgs: 1, maxPlainArgs: 0);
            });
        }

        #endregion

        #region AddOption()

        [Fact]
        public void AddOption_NoName()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddOption(null, null, false);
            });
        }

        [Fact]
        public void AddOption_NoNameCollection()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddOption(null, false);
            });
        }

        [Fact]
        public void AddOption_DuplicateNames()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddOption('a', "a", false);
            });
        }

        [Fact]
        public void AddOption_DuplicateNamesCollection()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddOption(new string[] { "a", "a" }, false);
            });
        }

        #endregion

        #region AddOptionDependency()

        [Fact]
        public void AddOptionDependency_OptionNotDefined()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('o', "option", false);
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionDependency("o", "x");
            });
        }

        [Fact]
        public void AddOptionDependency_Synonyms()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('o', "option", false);
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionDependency("o", "option");
            });
        }

        [Fact]
        public void AddOptionDependency_OptionConflict()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('a', "aOption", false);
            ps.AddOption('b', "bOption", false);
            ps.AddOptionConflict("a", "b");
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionDependency("a", "b");
            });
        }

        #endregion

        #region AddOptionConflict()

        [Fact]
        public void AddOptionConflict_OptionNotDefined()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('o', "option", false);
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionConflict("o", "x");
            });
        }

        [Fact]
        public void AddOptionConflict_Synonyms()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('o', "option", false);
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionConflict("o", "option");
            });
        }

        [Fact]
        public void AddOptionConflict_ConflictingOptionDependency()
        {
            var ps = new ProgramSettings("command");
            ps.AddOption('a', "aOption", false);
            ps.AddOption('b', "bOption", false);
            ps.AddOptionDependency("a", "b");
            Assert.Throws<ConstraintException>(delegate
            {
                ps.AddOptionConflict("a", "b");
            });
        }

        #endregion

        #region AddPlainArgument()

        [Fact]
        public void AddPlainArgument_NullName()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddPlainArgument(0, null);
            });
        }

        [Fact]
        public void AddPlainArgument_EmptyName()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.AddPlainArgument(0, "");
            });
        }

        #endregion

        #region PrintHelp()

        [Fact]
        public void PrintHelp_NullWriter()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                ps.PrintHelp(null);
            });
        }

        [Fact]
        public void PrintHelp()
        {
            string help = "help string";

            var ps = new ProgramSettings("command");
            ps.AddOption('o', "option", false, helpString: help);

            var writer = new System.IO.StringWriter();
            ps.PrintHelp(writer);
            Assert.Contains(help, writer.ToString());
        }

        #endregion
    }
}
