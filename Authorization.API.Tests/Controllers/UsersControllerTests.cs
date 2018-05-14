using Authorization.API.Controllers;
using Authorization.API.Data;
using Authorization.API.DTO;
using Authorization.API.Middleware;
using Authorization.API.Model;
using Authorization.API.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Authorization.API.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly SignUp _infoSignUp;
        private readonly SignIn _infoSignIn;

        private readonly User _userValid;
        private readonly User _userInvalid;
        private readonly Guid _userId;

        public UsersControllerTests()
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

            _infoSignUp = new SignUp()
            {
                Name = "Full Name",
                Email = "email@domain.com",
                Password = "pass123",
                Telephones = new List<Telephone>() { new Telephone() { Number = "+353 20 911 3682" } }
            };
            _infoSignIn = new SignIn()
            {
                Email = "email@domain.com",
                Password = "pass123"
            };

            _userId = Guid.NewGuid();
        }

        [Fact]
        [Trait("UsersController", "SignUp_Error")]
        public async void SignUp_ThrowException_gReturnInternalServerError() {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.AnyByEmailAsync(It.IsAny<String>())).Throws(new Exception());

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignUp(_infoSignUp);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        [Trait("UsersController", "SignUp_Error")]
        public async void SignUp_ModelInvalid_ReturnBadRequestErrorList()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            uc.ModelState.AddModelError("", "");
            var result = await uc.SignUp(_infoSignUp);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        [Fact]
        [Trait("UsersController", "SignUp_Error")]
        public async void SignUp_EmailAlreadyExists_ReturnBadRequestDuplicatedErrorMessage()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.AnyByEmailAsync(_infoSignUp.Email)).ReturnsAsync(true);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignUp(_infoSignUp);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.Duplicated }, ((BadRequestObjectResult)result).Value);
        }

        [Fact]
        [Trait("UsersController", "SignUp_Success")]
        public async void SignUp_EmailDoesNotExists_ReturnCreatedUriEndpointNewUser()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIConfiguration.SetupGet(m => m["JWT:SigningKey"]).Returns("FE8A4a!3YL`ct@wL;&0vpSc1qF`.`M");
            mockIUserService.Setup(m => m.AnyByEmailAsync(_infoSignUp.Email)).ReturnsAsync(false);
            mockIUserService.Setup(m => m.AddAsync(new User())).ReturnsAsync(It.IsAny<int>);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            uc.ControllerContext.HttpContext = new DefaultHttpContext();
            uc.ControllerContext.HttpContext.Request.Scheme = "http";
            uc.ControllerContext.HttpContext.Request.Host = new HostString("localhost", 50123);
            uc.ControllerContext.HttpContext.Request.Path = new PathString("/api/Users/SignUp");
            var result = await uc.SignUp(_infoSignUp);

            //Assert
            mockIUserService.Verify(m => m.AddAsync(It.IsAny<User>()));
            Assert.IsType<CreatedResult>(result);
            Assert.Equal("http://localhost:50123/api/Users/SignUp", ((CreatedResult)result).Location);
            Assert.IsType<User>(((CreatedResult)result).Value);
        }

        [Fact]
        [Trait("UsersController", "SignIn_Error")]
        public async void SignIn_ThrowException_gReturnInternalServerError()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.FirstOrDefaultByEmailAsync(It.IsAny<String>())).Throws(new Exception());

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignIn(_infoSignIn);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        [Trait("UsersController", "SignIn_Error")]
        public async void SignIn_ModelInvalid_ReturnBadRequestErrorList()
        {
            //Arrange
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIUserService = new Mock<IUserService>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            uc.ModelState.AddModelError("", "");
            var result = await uc.SignIn(_infoSignIn);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("UsersController", "SignIn_Error")]
        public async void SignIn_EmailDoesNotExists_ReturnUnauthorizedInvalidUserErrorMessage()
        {
            //Arrange
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIUserService = new Mock<IUserService>();

            mockIConfiguration.SetupGet(m => m["JWT:SigningKey"]).Returns("FE8A4a!3YL`ct@wL;&0vpSc1qF`.`M");
            User user = null;
            mockIUserService.Setup(m => m.FirstOrDefaultByEmailAsync(_infoSignIn.Email)).ReturnsAsync(user);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignIn(_infoSignIn);

            //Assert
            mockIUserService.Verify(m => m.FirstOrDefaultByEmailAsync(It.IsAny<String>()));
            Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.InvalidUser }, ((ObjectResult)result).Value);
        }

        [Fact]
        [Trait("UsersController", "SignIn_Error")]
        public async void SignIn_EmailExistsPasswordIncorrect_ReturnUnauthorizedInvalidUserErrorMessage()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.FirstOrDefaultByEmailAsync(_infoSignIn.Email)).ReturnsAsync(_userValid);
            mockIHashHelper.Setup(m => m.CompareStringToSHA256(It.IsAny<String>(), It.IsAny<byte[]>())).Returns(false);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignIn(_infoSignIn);

            //Assert
            Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, ((ObjectResult)result).StatusCode);
            Assert.Equal(new ErrorMessage() { Message = ErrorMessage.InvalidUser }, ((ObjectResult)result).Value);
        }

        [Fact]
        [Trait("UsersController", "SignIn_Success")]
        public async void SignIn_EmailPasswordCorrect_ReturnOkUserData()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIConfiguration.SetupGet(m => m["JWT:SigningKey"]).Returns("FE8A4a!3YL`ct@wL;&0vpSc1qF`.`M");
            mockIUserService.Setup(m => m.FirstOrDefaultByEmailAsync(_infoSignIn.Email)).ReturnsAsync(_userValid);
            mockIHashHelper.Setup(m => m.CompareStringToSHA256(It.IsAny<String>(), It.IsAny<byte[]>())).Returns(true);

            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            var result = await uc.SignIn(_infoSignIn);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            mockIUserService.Verify(m => m.UpdateAsync(_userValid));
            Assert.Equal(_userValid, ((ObjectResult)result).Value);
        }

        [Fact]
        [Trait("UsersController", "Search_Authorization")]
        public void Search_JwtAuthorizeAttribueExists_Exists()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);

            // Act
            var type = uc.GetType();
            var methodInfo = type.GetMethod("Search");
            var attributes = methodInfo.GetCustomAttributes(typeof(JwtAuthorizeAttribute), true);

            // Assert
            Assert.Single(attributes);
        }

        [Fact]
        [Trait("UsersController", "Search_Success")]
        public async void Search_HeaderAuthenticationExistentUserTokenValidSessionValid_ReturnOkUserDtoData()
        {
            //Arrange
            var mockIUserService = new Mock<IUserService>();
            var mockIConfiguration = new Mock<IConfiguration>();
            var mockIHashHelper = new Mock<IHashHelper>();
            var mockILogger = new Mock<ILogger<UsersController>>();

            mockIUserService.Setup(m => m.FirstOrDefaultByIdAsync(It.IsAny<Guid>())).ReturnsAsync(_userValid);
            mockIHashHelper.Setup(m => m.CompareStringToSHA256(It.IsAny<String>(), It.IsAny<byte[]>())).Returns(true);
   
            //Act
            var uc = new UsersController(mockIUserService.Object, mockIConfiguration.Object, mockIHashHelper.Object, mockILogger.Object);
            uc.ControllerContext.HttpContext = new DefaultHttpContext();
            uc.ControllerContext.HttpContext.Request.Headers.Add("Authentication", "Bearer Token");
            var result = await uc.Search(_userId);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new UserDTO(_userValid), ((OkObjectResult)result).Value);
        }
    }
}