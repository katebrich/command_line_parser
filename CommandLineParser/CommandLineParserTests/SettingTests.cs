using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLineParser;

namespace CommandLineParser.Tests
{
    [TestClass]
    public class SettingTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyProgramName()
        {
            var s = new ProgramSettings("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NullProgramName()
        {
            var s = new ProgramSettings(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MinPlainArgsLargerThanMaxPlainArgs()
        {
            var s = new ProgramSettings("program", 10, 9);
        }

        [TestMethod]
        public void HelpPrintsSomething()
        {
            var s = ParserTests.GetTestSetting();
            var writer = new StringWriter();
            s.PrintHelp(writer);
            writer.Flush();
            Assert.IsFalse(string.IsNullOrEmpty(writer.ToString()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NoNames()
        {
            var s = new ProgramSettings("program");
            s.AddOption(null, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DuplicitShortNames()
        {
            var s = new ProgramSettings("program");
            s.AddOption('a',null, false);
            s.AddOption('a',"aa", false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DuplicitLongNames()
        {
            var s = new ProgramSettings("program");
            s.AddOption(null, "aaa", false);
            s.AddOption('a', "aaa", false);
        }
    }
}
