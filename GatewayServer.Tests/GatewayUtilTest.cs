using Microsoft.VisualStudio.TestTools.UnitTesting;
using GatewayServer.Utils;
using System.Text.RegularExpressions;

namespace GatewayServer.Tests
{
    [TestClass]
    public class GatewayUtilTest
    {
        [TestMethod]
        public void TestGenerateRandomString()
        {
            var rndStr = GatewayUtil.GenerateRandomString();
            Assert.AreEqual(32, rndStr.Length);
            // 不会有中横线
            Assert.AreEqual(-1, rndStr.IndexOf("-"));
            Assert.IsTrue(Regex.IsMatch(rndStr, "^[a-zA-Z0-9]+$"));
        }
    }
}