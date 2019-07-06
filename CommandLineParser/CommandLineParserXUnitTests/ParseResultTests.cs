using CommandLineParser;
using System;
using System.Collections.Generic;
using Xunit;

namespace CommandLineParserTests
{
    public class ParseResultTests : IDisposable
    {
        private ProgramSettings ps;

        public ParseResultTests()
        {
            ps = new ProgramSettings("command");
            ps.AddOption('a', "aopt", false, new IntParameter("name", true, 0, 10));
            ps.AddOption('b', "bopt", false, new IntParameter("name", true, -5, 10));
            ps.AddOptionDependency("a", "b");

            var domain = new List<string>
            {
                "some", "string", "params"
            };
            ps.AddOption('s', "sopt", false, new StringParameter("name", true, domain));
        }

        public void Dispose()
        {
            ps = null;
        }

        #region ParsedOptions

        [Fact]
        public void ParsedOptions_Null()
        {
            var result = Parser.Parse("command --", ps);
            Assert.Empty(result.ParsedOptions);
        }

        [Fact]
        public void ParsedOptions_NotNull()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.NotNull(result.ParsedOptions);
        }

        [Fact]
        public void ParsedOptions_Count()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.Equal(2, result.ParsedOptions.Count);
        }

        #endregion

        #region PlainArguments getter

        [Fact]
        public void ParseResult_NoPlainArguments()
        {
            var result = Parser.Parse("command --", ps);
            Assert.Empty(result.PlainArguments);
        }

        [Fact]
        public void ParseResult_SinglePlainArgument()
        {
            var result = Parser.Parse("command -- plain", ps);
            var expected = new List<string>
            {
                "plain"
            };
            Assert.Equal(expected, result.PlainArguments);
        }

        [Fact]
        public void ParseResult_MultiplePlainArguments()
        {
            var result = Parser.Parse("command -- plain argument", ps);
            var expected = new List<string>
            {
                "plain", "argument"
            };
            Assert.Equal(expected, result.PlainArguments);
        }

        [Fact]
        public void ParseResult_MultiplePlainArgumentsReverse()
        {
            var result = Parser.Parse("command -- plain argument", ps);
            var expected = new List<string>
            {
                "argument", "plain"
            };
            expected.Reverse();
            Assert.Equal(expected, result.PlainArguments);
        }

        [Fact]
        public void ParseResult_MultiplePlainArgumentsFail()
        {
            var result = Parser.Parse("command -- plain argument and some more", ps);
            var expected = new List<string>
            {
                "plain", "argument", "and", "some", "more"
            };
            Assert.Equal(expected, result.PlainArguments);
        }

        #endregion

        #region GetParameterValue()
        [Fact]
        public void GetParameterValue_NullParameter()
        {
            var result = Parser.Parse("command", ps);
            Assert.Throws<ArgumentException>(() => result.GetParameterValue(null));
        }

        [Fact]
        public void GetParameterValue_InvalidParameter()
        {
            var result = Parser.Parse("command", ps);
            Assert.Null(result.GetParameterValue("invalid_option"));
        }

        [Fact]
        public void GetParameterValue_Valid()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.Equal(0, (int)result.GetParameterValue("a"));
        }

        [Fact]
        public void GetParameterValue_ValidLongName()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.Equal(0, (int)result.GetParameterValue("aopt"));
        }

        #endregion

        #region WasParsed()

        [Fact]
        public void WasParsed_NullOption()
        {
            var result = Parser.Parse("command", ps);
            Assert.Throws<ArgumentException>(() => result.WasParsed(null));
        }

        [Fact]
        public void WasParsed_InvalidOption()
        {
            var result = Parser.Parse("command", ps);
            Assert.False(result.WasParsed("invalid_option"));
        }

        [Fact]
        public void WasParsed_NotPresentOption()
        {
            var result = Parser.Parse("command", ps);
            Assert.False(result.WasParsed("a"));
        }

        [Fact]
        public void WasParsed_PresentOption()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.True(result.WasParsed("a"));
        }

        [Fact]
        public void WasParsed_PresentOptionLongName()
        {
            var result = Parser.Parse("command -a 0 -b 0", ps);
            Assert.True(result.WasParsed("aopt"));
        }

        #endregion
    }
}
