using CommandLineParser;
using System;
using Xunit;

namespace CommandLineParserTests
{
    public class ParserTests
    {
        [Fact]
        public void Parse_NoSettings_String()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse("command", null);
            });
        }

        [Fact]
        public void Parse_NoSettings_Array()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse(new string[] { "command" }, null);
            });
        }

        [Fact]
        public void Parse_NoSettings_Reader()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse(new System.IO.StringReader("command"), null);
            });
        }

        [Fact]
        public void Parse_NullCommand()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse((string)null, ps);
            });
        }

        [Fact]
        public void Parse_NullArray()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse((string[])null, ps);
            });
        }

        [Fact]
        public void Parse_NullReader()
        {
            var ps = new ProgramSettings("command");
            Assert.Throws<ArgumentException>(delegate
            {
                Parser.Parse((System.IO.StringReader)null, ps);
            });
        }

        [Fact]
        public void Parse_MandatoryOptionMissing()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "mandatoryOption" }, true);
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("", ps);
            });
        }

        [Fact]
        public void Parse_WrongTypeOptionParameter()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "intOption" }, true, new IntParameter("name", true, 0, 10));
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -intOption someStringParameter", ps);
            });
        }

        [Fact]
        public void Parse_WrongNumberOfOptionParameters()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "intOption" }, true, new IntParameter("name", true, 0, 10));
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -intOption", ps);
            });
        }

        [Fact]
        public void Parse_OptionDependencyErrorFirst()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "a" }, true);
            ps.AddOption(new string[] { "b" }, true);
            ps.AddOptionDependency("a", "b");
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -a", ps);
            });
        }

        [Fact]
        public void Parse_OptionDependencyErrorSecond()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "a" }, true);
            ps.AddOption(new string[] { "b" }, true);
            ps.AddOptionDependency("a", "b");
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -b", ps);
            });
        }

        [Fact]
        public void Parse_ConflictingOptions()
        {
            ProgramSettings ps = new ProgramSettings("command");
            ps.AddOption(new string[] { "a" }, true);
            ps.AddOption(new string[] { "b" }, true);
            ps.AddOptionConflict("a", "b");
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -a -b", ps);
            });
        }

        [Fact]
        public void Parse_LowMinPlainArgs()
        {
            var ps = new ProgramSettings("command", minPlainArgs: 1);
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command --", ps);
            });
        }

        [Fact]
        public void Parse_HighMaxPlainArgs()
        {
            var ps = new ProgramSettings("command", maxPlainArgs: 1);
            Assert.Throws<ParseException>(delegate
            {
                Parser.Parse("command -- first second", ps);
            });
        }
    }
}
