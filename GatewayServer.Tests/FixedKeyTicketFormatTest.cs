using System;
using System.Security.Claims;
using System.Security.Cryptography;
using GatewayServer.ControlPlane.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GatewayServer.Tests
{
    [TestClass]
    public class FixedKeyTicketFormatTest
    {
        private static AuthenticationTicket MakeTicket()
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "admin"), new Claim(ClaimTypes.Role, "Owner")], "Cookies");
            return new AuthenticationTicket(new ClaimsPrincipal(identity), "Cookies");
        }

        [TestMethod]
        public void TestRoundTrip()
        {
            var key = RandomNumberGenerator.GetBytes(32);
            var format = new FixedKeyTicketFormat(key);

            var protectedText = format.Protect(MakeTicket(), "purpose1");
            var ticket = format.Unprotect(protectedText, "purpose1");

            Assert.IsNotNull(ticket);
            Assert.AreEqual("admin", ticket.Principal.Identity?.Name);
            Assert.IsTrue(ticket.Principal.IsInRole("Owner"));
        }

        [TestMethod]
        public void TestSameKeyAcrossInstances()
        {
            // 同一密钥的两个实例(模拟重启/多实例):A 加密的票据 B 能解
            var key = RandomNumberGenerator.GetBytes(32);
            var protectedText = new FixedKeyTicketFormat(key).Protect(MakeTicket());
            var ticket = new FixedKeyTicketFormat(key).Unprotect(protectedText);
            Assert.IsNotNull(ticket);
        }

        [TestMethod]
        public void TestWrongKeyReturnsNull()
        {
            var protectedText = new FixedKeyTicketFormat(RandomNumberGenerator.GetBytes(32)).Protect(MakeTicket());
            var ticket = new FixedKeyTicketFormat(RandomNumberGenerator.GetBytes(32)).Unprotect(protectedText);
            Assert.IsNull(ticket);
        }

        [TestMethod]
        public void TestTamperedPayloadReturnsNull()
        {
            var key = RandomNumberGenerator.GetBytes(32);
            var format = new FixedKeyTicketFormat(key);
            var protectedText = format.Protect(MakeTicket());
            var tampered = protectedText[..^2] + (protectedText[^2] == 'A' ? "BB" : "AA");
            Assert.IsNull(format.Unprotect(tampered));
        }

        [TestMethod]
        public void TestPurposeMismatchReturnsNull()
        {
            var key = RandomNumberGenerator.GetBytes(32);
            var format = new FixedKeyTicketFormat(key);
            var protectedText = format.Protect(MakeTicket(), "p1");
            Assert.IsNull(format.Unprotect(protectedText, "p2"));
        }

        [TestMethod]
        public void TestInvalidKeyLengthThrows()
        {
            Assert.ThrowsExactly<ArgumentException>(() => _ = new FixedKeyTicketFormat(new byte[16]));
        }
    }
}
