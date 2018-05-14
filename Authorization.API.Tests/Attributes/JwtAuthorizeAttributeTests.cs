using Authorization.API.Controllers;
using Authorization.API.Data;
using Authorization.API.DTO;
using Authorization.API.Middleware;
using Authorization.API.Model;
using Authorization.API.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using Xunit;
namespace Authorization.API.Tests.Attributes
{
    public class JwtAuthorizeAttributeTests
    {

        private readonly User _userValid;
        private readonly User _userInvalid;
        private readonly Guid _userId;

        public JwtAuthorizeAttributeTests()
        {
            _userValid = new User()
            {
                CreatedOn = DateTime.Now,
                Email = "mail@domain.com",
                Id = new Guid("2A4716C9-1E5D-4599-8EB4-2BDC6091CD2E"),
                LastLoginOn = DateTime.Now,
                LastUpdatedOn = DateTime.Now,
                Name = "Full Name",
                Password = new byte[] { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 },
                Telephones = new List<Telephone>() { new Telephone() { Number = "+353 20 911 3682" } },
                TokenHashed = new byte[] { }
            };
            _userInvalid = new User()
            {
                CreatedOn = DateTime.Now,
                Email = "mail@domain.com",
                Id = new Guid("2A4716C9-1E5D-4599-8EB4-2BDC6091CD2E"),
                LastLoginOn = DateTime.Now.AddMinutes(-3600),
                LastUpdatedOn = DateTime.Now,
                Name = "Full Name",
                Password = new byte[] { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 },
                Telephones = new List<Telephone>() { new Telephone() { Number = "+353 20 911 3682" } },
                TokenHashed = new byte[] { }
            };

            _userId = Guid.NewGuid();

        }

        [Fact]
        [Trait("JwtAuthorizeAttribute", "OnAuthorization")]
        public void OnAuthorization_NoAuthHeader_ReturnUnauthorized()
        {
            //Arrange
            JwtAuthorizeAttribute jwtAuthorize = new JwtAuthorizeAttribute();
            AuthorizationFilterContext context = new AuthorizationFilterContext(new ActionContext() { HttpContext = new DefaultHttpContext(), RouteData = new RouteData(), ActionDescriptor = new ActionDescriptor() }, new List<IFilterMetadata>());

            //Act
            jwtAuthorize.OnAuthorization(context);

            //Assert
            Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)context.Result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.Unauthorized }, ((ObjectResult)context.Result).Value);
        }

        [Fact]
        [Trait("JwtAuthorizeAttribute", "OnAuthorization")]
        public void OnAuthorization_HeaderInvalid_ReturnUnauthorized()
        {
            //Arrange
            JwtAuthorizeAttribute jwtAuthorize = new JwtAuthorizeAttribute();
            AuthorizationFilterContext context = new AuthorizationFilterContext(new ActionContext() { HttpContext = new DefaultHttpContext(), RouteData = new RouteData(), ActionDescriptor = new ActionDescriptor() }, new List<IFilterMetadata>());
            context.HttpContext.Request.Headers.Add("Authorization", "Bearer WrongToken");
            context.HttpContext.Request.Headers.Add("Authentication", "Bearer WrongToken");

            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.SetupGet(m => m.IsAuthenticated).Returns(false);
            context.HttpContext.User = new ClaimsPrincipal(mockIdentity.Object);

            //Act
            jwtAuthorize.OnAuthorization(context);

            //Assert
            Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)context.Result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.Unauthorized }, ((ObjectResult)context.Result).Value);
        }

        [Fact]
        [Trait("JwtAuthorizeAttribute", "OnAuthorization")]
        public void OnAuthorization_HeaderValidExpired_ReturnUnauthorized3()
        {
            //Arrange
            JwtAuthorizeAttribute jwtAuthorize = new JwtAuthorizeAttribute();
            AuthorizationFilterContext context = new AuthorizationFilterContext(new ActionContext() { HttpContext = new DefaultHttpContext(), RouteData = new RouteData(), ActionDescriptor = new ActionDescriptor() }, new List<IFilterMetadata>());
            context.HttpContext.Request.Headers.Add("Authorization", "Bearer WrongToken");
            context.HttpContext.Request.Headers.Add("Authentication", "Bearer WrongToken");

            var mockClaims = new Mock<ClaimsPrincipal>();
            mockClaims.Setup(m => m.FindFirst(It.IsAny<string>())).Returns(new Claim("exp", DateTimeOffset.Now.AddMinutes(-1).ToString()));
            mockClaims.Setup(m => m.Identity.IsAuthenticated).Returns(true);

            context.HttpContext.User = mockClaims.Object;

            //Act
            jwtAuthorize.OnAuthorization(context);

            //Assert
            Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)context.Result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.InvalidSession }, ((ObjectResult)context.Result).Value);



            /*
             
        [Fact]
        [Trait("JwtAuthorizeAttribute", "OnAuthorization")]
        public async void Search_HeaderAuthenticationExistentUserTokenValidSessionExpired_ReturnUnauthorizedIvalidSessionErrorMessage()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.FirstOrDefaultByIdAsync(It.IsAny<Guid>())).ReturnsAsync(_userInvalid);
            mockIHashHelper.Setup(m => m.CompareStringToSHA256(It.IsAny<String>(), It.IsAny<byte[]>())).Returns(true);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            uc.ControllerContext.HttpContext = new DefaultHttpContext();
            uc.ControllerContext.HttpContext.Request.Headers.Add("Authentication", "Bearer Token");
            var result = await uc.Search(_userId);

            //Assert
            Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.InvalidSession }, ((ObjectResult)result).Value);
        }
             */
        }

        [Fact]
        [Trait("JwtAuthorizeAttribute", "OnAuthorization")]
        public async void OnAuthorization_ThrowException_gReturnInternalServerError()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.FirstOrDefaultByIdAsync(It.IsAny<Guid>())).Throws(new Exception());

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.Search(_userId);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }

    }
}
