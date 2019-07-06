using CommandLineParser;
using Xunit;
using System;

namespace CommandLineParserTests
{
    public class StringParameterTests
    {
        #region IsMandatory getter

        [Fact]
        public void StringParameter_Mandatory()
        {
            var sp = new StringParameter("name", true);
            Assert.True(sp.IsMandatory);
        }

        [Fact]
        public void StringParameter_NotMandatory()
        {
            var sp = new StringParameter("name", false);
            Assert.False(sp.IsMandatory);
        }

        #endregion

        #region TryParse()

        [Fact]
        public void TryParse_Null()
        {
            var sp = new StringParameter("name", false);
            Assert.Throws<ArgumentException>(() => sp.TryParse(null, out object result));
        }

        [Fact]
        public void TryParse_Type()
        {
            var ip = new StringParameter("name", false);
            ip.TryParse("string", out object result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void TryParse_Value()
        {
            var ip = new StringParameter("name", false);
            ip.TryParse("string", out object result);
            Assert.Equal("string", (string)result);
        }

        #endregion
    }
}
