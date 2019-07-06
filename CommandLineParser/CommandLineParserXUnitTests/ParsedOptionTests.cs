using CommandLineParser;
using System;
using System.Collections.Generic;
using Xunit;

namespace CommandLineParserTests
{
    public class ParsedOptionTests : IDisposable
    {
        private ProgramSettings ps;

        public ParsedOptionTests()
        {
            ps = new ProgramSettings("command");
            ps.AddOption('o', "opt", false, new StringParameter("name", true));
        }

        public void Dispose()
        {
            ps = null;
        }

        [Fact]
        public void NamesGetter()
        {
            var result = Parser.Parse("command -o string_parameter", ps);
            var names = new List<string>
            {
                "o", "opt"
            };
            foreach(var opt in result.ParsedOptions)
            {
                Assert.Equal(names, opt.Names);
            }
        }

        [Fact]
        public void ValueGetterNull()
        {
            var result = Parser.Parse("command -o", ps);
            foreach(var opt in result.ParsedOptions)
            {
                Assert.Null(opt.Value);
            }
        }

        [Fact]
        public void ValueGetterType()
        {
            var result = Parser.Parse("command -o string_parameter", ps);
            foreach(var opt in result.ParsedOptions)
            {
                Assert.IsType<string>(opt.Value);
            }
        }

        [Fact]
        public void ValueGetter()
        {
            var result = Parser.Parse("command -o string_parameter", ps);
            foreach(var opt in result.ParsedOptions)
            {
                Assert.Equal("string_parameter", (string)opt.Value);
            }
        }
    }
}
