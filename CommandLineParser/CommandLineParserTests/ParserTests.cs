using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLineParser;

namespace CommandLineParser.Tests
{
    [TestClass]
    public class ParserTests
    {
        public static ProgramSettings GetTestSetting()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('a', "aa", false);
            settings.AddOption('b', "bb", false);
            settings.AddOption('c', "cc", false);
            settings.AddOption('d', "dd", false, 
                new IntParameter("PARAM", false, 2, 4));
            settings.AddOption('e', "ee", false, 
                new IntParameter("PARAM", true));
            settings.AddOption('f', "ff", false, 
                new StringParameter("PARAM", false));
            settings.AddOption('g', "gg", false, 
                new StringParameter("PARAM", true, new[] {"hello", "hi"}));

            settings.AddOptionDependency("c", "d");

            return settings;
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void InvalidOptionName()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('x', "xx666", false);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MandatoryOptionMissingNoArgs()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('x', "xx", true);
            Parser.Parse(new string[0], settings);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MandatoryOptionMissing()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('x', "xx", true);
            Parser.Parse(new [] {"b", "c", "hello" }, settings);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void DependentOptions()
        {
            var s = GetTestSetting();
            s.AddOptionDependency("b", "a");
            Parser.Parse(new[] {"-b"}, s);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ConflictingOptions()
        {
            var settings = GetTestSetting();
            settings.AddOptionConflict("a", "b");
            Parser.Parse(new[] {"-a", "--bb"}, settings);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void ConflictingOptionNames()
        {
            var s = GetTestSetting();
            s.AddOption('a', "aa", false);
            s.AddOption('b', "aa", false);
            s.AddOption('a', "bb", false);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void WrongParameterTypeGiven()
        {
            Parser.Parse(new[] {"-e", "123not_a_valid_int"}, GetTestSetting());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MissingMandatoryOption()
        {
            var settings = GetTestSetting();
            settings.AddOption('z', "zz", true);
            Parser.Parse(new[] {"--bb", "42" }, settings);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TooFewPlainArgs()
        {
            var s = new ProgramSettings("program", 3, 5);
            Parser.Parse(new[] {"--", "arg1", "arg2"}, s);
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TooManyPlainArgs()
        {
            var s = new ProgramSettings("program", 3, 5);
            Parser.Parse(new[] {"--", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6"}, s);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MissingMandatoryParameter()
        {
            Parser.Parse(new[] {"-b", "--cc"}, GetTestSetting());
        }

        [TestMethod]
        public void OptionsNotGivenNotParsed()
        {
            var result = Parser.Parse(new [] { "-a", "-b" }, GetTestSetting());
            Assert.IsFalse(result.WasParsed("c"));
            Assert.IsFalse(result.WasParsed("d"));
            Assert.IsFalse(result.WasParsed("cc"));
            Assert.IsFalse(result.WasParsed("dd"));
            Assert.IsFalse(result.WasParsed("random_string"));
            Assert.IsFalse(result.WasParsed("m"));
        }

        [TestMethod]
        public void OptionsNotGivenHaveNullValue()
        {
            var result = Parser.Parse(new [] { "-a", "-b" }, GetTestSetting());

            Assert.IsNull(result.GetParameterValue("cc"));
            Assert.IsNull(result.GetParameterValue("dd"));
            Assert.IsNull(result.GetParameterValue("example"));
        }

        [TestMethod]
        public void ParsedOptionContainsAllNames()
        {
            var result = Parser.Parse(new[] {"-a", "--bb"}, GetTestSetting());
            Assert.IsTrue(result.ParsedOptions.First().Names.Contains("a"));
            Assert.IsTrue(result.ParsedOptions.First().Names.Contains("aa"));
            Assert.IsTrue(result.ParsedOptions.Last().Names.Contains("b"));
            Assert.IsTrue(result.ParsedOptions.Last().Names.Contains("bb"));
        }

        [TestMethod]
        public void ParsedOptionHasCorrectValue()
        {
            var result = Parser.Parse(new[] {"-e", "42"}, GetTestSetting());
            Assert.AreEqual((int) result.ParsedOptions.First().Value, 42);
        }

        [TestMethod]
        public void OptionsWithoutParametersHaveNullValue()
        {
            var result = Parser.Parse(new [] { "-a", "--bb" }, GetTestSetting());

            Assert.IsNull(result.GetParameterValue("a"));
            Assert.IsNull(result.GetParameterValue("b"));
            Assert.IsNull(result.GetParameterValue("aa"));
            Assert.IsNull(result.GetParameterValue("bb"));
        }

        [TestMethod]
        public void OptionsHaveCorrectValues()
        {
            var result = Parser.Parse(new [] { "--ee=42",  "-f", "hello", "hi", "ciao" }, GetTestSetting());

            Assert.AreEqual(42, (int) result.GetParameterValue("e"));
            Assert.AreEqual(42, (int)result.GetParameterValue("ee"));
            Assert.AreEqual("hello", (string)result.GetParameterValue("f"));
            Assert.AreEqual("hello", (string)result.GetParameterValue("ff"));
        }

        [TestMethod]
        public void GroupedParameterLessOptions()
        {
            var result = Parser.Parse(new[] { "-ab", "--cc", "-d" }, GetTestSetting());

            Assert.IsTrue(result.WasParsed("a"));
            Assert.IsTrue(result.WasParsed("b"));
            Assert.IsTrue(result.WasParsed("c"));
        }

        [TestMethod]
        public void GroupedOptionsLastWithParameter()
        {
            var result = Parser.Parse(new[] { "-abde", "42" }, GetTestSetting());

            Assert.IsTrue(result.WasParsed("a"));
            Assert.IsTrue(result.WasParsed("b"));
            Assert.IsTrue(result.WasParsed("d"));
            Assert.IsTrue(result.WasParsed("e"));

            Assert.AreEqual((int)result.GetParameterValue("e"), 42);
        }

        [TestMethod]
        public void NegativeIntegerParameter()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('h', "hh", true, new IntParameter("par", true, -10, 10));
            var result = Parser.Parse(new[] { "-h=-5", "--", "abraka", "dabra" }, settings);
            Assert.IsTrue(result.WasParsed("hh"));
        }

        [TestMethod]
        public void ParameterWithCommasSeparate()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('h', "hh", true, new StringParameter("par", true));
            var result = Parser.Parse(new[] { "-h", "1,2,3", "--", "abraka", "dabra" }, settings);
            Assert.IsTrue(result.WasParsed("hh"));
            Assert.AreEqual((string)result.GetParameterValue("hh"), "1,2,3");
        }

        [TestMethod]
        public void ParameterWithCrazyCharacters()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('h', "hh", true, new StringParameter("par", true));
            var result = Parser.Parse(new[] { "-h", "a-b$*^v78*/-_ěščř", "--", "abraka", "dabra" }, settings);
            Assert.IsTrue(result.WasParsed("hh"));
        }

        [TestMethod]
        public void ParameterWithCommasJoint()
        {
            var settings = new ProgramSettings("program");
            settings.AddOption('h', "hh", true, new StringParameter("par", true));
            var result = Parser.Parse(new[] { "-h=1,2,3", "--", "abraka", "dabra" }, settings);
            Assert.IsTrue(result.WasParsed("hh"));
            Assert.AreEqual((string)result.GetParameterValue("h"), "1,2,3");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParameterOutOfBoundsRefused()
        {
            Parser.Parse(new [] {"-d", "5"}, GetTestSetting());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParameterNotInDomainRefused()
        {
            Parser.Parse(new[] {"--gg=bumblebee"}, GetTestSetting());
        }

        [TestMethod]
        public void ParameterInDomainParsed()
        {
            var result = Parser.Parse(new[] { "--gg=hello" }, GetTestSetting());
            Assert.IsTrue(result.WasParsed("gg"));
        }

        [TestMethod]
        public void ParameterInDomainAccepted()
        {
            Parser.Parse(new[] {"-g", "hello"}, GetTestSetting());
        }

        [TestMethod]
        public void OptionsAfterDoubleMinusArePlainArguments()
        {
            var result = Parser.Parse(new[] {"-e", "3", "--", "-a", "--bb"}, GetTestSetting());

            Assert.IsFalse(result.WasParsed("a"));
            Assert.IsFalse(result.WasParsed("aa"));
            Assert.IsFalse(result.WasParsed("b"));
            Assert.IsFalse(result.WasParsed("bb"));

            Assert.AreEqual(3, (int) result.GetParameterValue("e"));

            Assert.IsTrue(result.PlainArguments.Contains("-a"));
            Assert.IsTrue(result.PlainArguments.Contains("--bb"));

            Assert.IsFalse(result.PlainArguments.Contains("--"));
            Assert.IsTrue(result.PlainArguments.Count == 2);

            Assert.IsTrue(result.ParsedOptions.Count == 1);
        }

        
    }
}
