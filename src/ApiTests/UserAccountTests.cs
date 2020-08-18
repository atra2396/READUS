using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using READUS.Commands;
using READUS.Controllers;
using READUS.Crypto;
using READUS.Models;
using Storage;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;

namespace ApiTests
{
    [TestClass]
    public class UserAccountTests
    {
        UserAccountController userAccountController;

        [TestInitialize]
        public void TestInit()
        {
            var cryptoOptions = new Mock<IOptions<CryptoConfigs>>();
            cryptoOptions.Setup(x => x.Value).Returns(new CryptoConfigs { Salt = "salty salty" });

            var jwtOptions = new Mock<IOptions<JwtConfigs>>();
            jwtOptions.Setup(x => x.Value).Returns(new JwtConfigs { SymmetricKey = "symmetrytryemmys" });

            this.userAccountController = new UserAccountController(new MemoryDataContext(), cryptoOptions.Object, jwtOptions.Object);
        }

        [TestMethod]
        public void Integration_AccountCreation_SuccessfulLogin()
        {
            var controllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            this.userAccountController.ControllerContext = controllerContext;

            var accountId = this.userAccountController.CreateAccount(new CreateAccountCommand { Username = "username", Password = "password@123" }).Value;
            this.userAccountController.Login(new AuthRequest { Username = "username", Password = "password@123" });

            var responseHeaders = controllerContext.HttpContext.Response.Headers;
            Assert.IsTrue(responseHeaders.ContainsKey("Set-Cookie"));

            var claimUserId = Guid.Parse(GetJwtClaim(GetTokenFromHeader(responseHeaders["Set-Cookie"]), "user_id"));
            Assert.AreEqual(accountId, claimUserId);
        }

        [TestMethod]
        public void Integration_AccountCreation_FailedLogin()
        {
            var controllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() };
            this.userAccountController.ControllerContext = controllerContext;

            this.userAccountController.CreateAccount(new CreateAccountCommand { Username = "username", Password = "password@123" });
            var result = this.userAccountController.Login(new AuthRequest { Username = "username", Password = "wrong_password" });

            Assert.IsNotNull(result as UnauthorizedObjectResult);
        }

        private string GetJwtClaim(string token, string claim)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            return jwt.Claims.First(x => x.Type == claim).Value;
        }

        private string GetTokenFromHeader(string header)
        {
            return header.Split('=')[1].Split(';')[0];   // Janky but good enough for now
        }
    }
}
