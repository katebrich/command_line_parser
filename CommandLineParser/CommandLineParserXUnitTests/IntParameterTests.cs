using CommandLineParser;
using Xunit;

namespace CommandLineParserTests
{
    public class IntParameterTests
    {
        #region IsMandatory getter

        [Fact]
        public void IntParameter_Mandatory()
        {
            var ip = new IntParameter("name", true);
            Assert.True(ip.IsMandatory);
        }

        [Fact]
        public void IntParameter_NotMandatory()
        {
            var ip = new IntParameter("name", false);
            Assert.False(ip.IsMandatory);
        }

        #endregion

        #region TryParse()

        [Fact]
        public void TryParse_Null()
        {
            var ip = new IntParameter("name", false);
            Assert.False(ip.TryParse(null, out object result));
        }

        [Fact]
        public void TryParse_Empty()
        {
            var ip = new IntParameter("name", false);
            Assert.False(ip.TryParse("", out object result));
        }

        [Fact]
        public void TryParse_NonEmpty()
        {
            var ip = new IntParameter("name", false);
            Assert.True(ip.TryParse("0", out object result));
        }

        [Fact]
        public void TryParse_NonEmptyType()
        {
            var ip = new IntParameter("name", false);
            ip.TryParse("0", out object result);
            Assert.IsType<int>(result);
        }

        [Fact]
        public void TryParse_NonEmptyValue()
        {
            var ip = new IntParameter("name", false);
            ip.TryParse("0", out object result);
            Assert.Equal(0, (int)result);
        }

        #endregion
    }
}
