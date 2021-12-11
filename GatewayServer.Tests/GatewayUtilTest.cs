using Microsoft.VisualStudio.TestTools.UnitTesting;
using GatewayServer.Utils;

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
        }
    }
}