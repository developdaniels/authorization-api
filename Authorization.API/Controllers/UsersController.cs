using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authorization.API.Data;
using Authorization.API.Model;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Authorization.API.Util;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Authorization.API.DTO;

namespace Authorization.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Users/[Action]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly String _jwtSigningKey;
        private readonly Double _authSessionInMinutes;
        private readonly IHashHelper _hashHelper;
        private readonly ILogger<UsersController> _logger;

        private const String _authentication = "Authentication";
        private const String _bearer = "Bearer ";

        private const String _claimIdKey = "id";
        private const String _claimLastLoginOnKey = "lastLoginOn";

        public UsersController(IUserService userService, IConfiguration configuration, IHashHelper hashHelper, ILogger<UsersController> logger)
        {
            _logger = logger;
            _jwtSigningKey = configuration["JWT:SigningKey"];
            _authSessionInMinutes = Double.TryParse(configuration["Auth:SessionInMinutes"], out _authSessionInMinutes) ? _authSessionInMinutes : 30.0;

            _userService = userService;
            _hashHelper = hashHelper;
        }

        // POST: api/Users/SingUp
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUp infoSignUp)
        {
            try
            {
                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "SignUp", "Begin   - " + infoSignUp.Email ?? ""));

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "SignUp", "Failure - Invalid payload"));
                    return BadRequest(ModelState);
                }

                bool userAlreadyExist = await _userService.AnyByEmailAsync(infoSignUp.Email);

                if (userAlreadyExist)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "SignUp", "Failure - Email already exists"));
                    return BadRequest(new ErrorMessage() { Message = ErrorMessage.Duplicated });
                }

                byte[] passwordAsHash256 = _hashHelper.ComputeSha256FromString(infoSignUp.Password);

                User newUser = new User(infoSignUp.Name, infoSignUp.Email, passwordAsHash256, infoSignUp.Telephones);

                List<Claim> exampleClaims = new List<Claim>() {
                new Claim(_claimIdKey, newUser.Id.ToString()),
                new Claim(_claimLastLoginOnKey, newUser.LastLoginOn.ToString())
            };

                String jwtToken = JwtTokenHelper.WriteJwtToken(exampleClaims, _jwtSigningKey);

                newUser.Token = jwtToken;
                newUser.TokenHashed = _hashHelper.ComputeSha256FromString(_bearer + jwtToken);
                await _userService.AddAsync(newUser);

                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "SignUp", "Success"));

                return Created(
                    new UriBuilder(HttpContext.Request.Scheme, HttpContext.Request.Host.Host, HttpContext.Request.Host.Port ?? 80, HttpContext.Request.Path.Value).ToString(),
                    newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(String.Format("Action: {0} | Status: {1}", "SignUp", "Exception"));
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorMessage() { Message = ex.Message });
            }
        }

        // POST: api/Users/SignIn
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] SignIn infoSignIn)
        {
            try
            {
                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "SignIn", "Begin   - " + infoSignIn.Email ?? ""));


                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "SignIn", "Failure - Invalid payload"));
                    return BadRequest(ModelState);
                }

                var existentUser = await _userService.FirstOrDefaultByEmailAsync(infoSignIn.Email);

                if (existentUser == null)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "SignIn", "Failure - Invalid user"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.InvalidUser });
                }

                if (!_hashHelper.CompareStringToSHA256(infoSignIn.Password, existentUser.Password))
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "SignIn", "Failure - Invalid password"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.InvalidUser });
                }

                existentUser.LastLoginOn = DateTime.Now;
                List<Claim> exampleClaims = new List<Claim>() {
                new Claim(_claimIdKey, existentUser.Id.ToString()),
                new Claim(_claimLastLoginOnKey, existentUser.LastLoginOn.ToString())
            };

                String jwtToken = JwtTokenHelper.WriteJwtToken(exampleClaims, _jwtSigningKey);
                existentUser.Token = jwtToken;
                existentUser.TokenHashed = _hashHelper.ComputeSha256FromString(_bearer + jwtToken);

                await _userService.UpdateAsync(existentUser);

                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "SignIn", "Success"));
                return Ok(existentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(String.Format("Action: {0} | Status: {1}", "SignUp", "Exception"));
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorMessage() { Message = ex.Message });
            }
        }

        // GET: api/Users/Search/00000000-0000-0000-0000-000000000000
        //[Route("{userId:guid}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> Search(Guid userId)
        {
            try
            {
                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "Search", "Begin   - " + userId.ToString()));

                if (!Request.Headers.Any(h => h.Key == _authentication))
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "Search", "Failure - Header authentication not found"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.Unauthorized });
                }

                User user = await _userService.FirstOrDefaultByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "Search", "Failure - User not found"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.Nonexistent });
                }

                if (!_hashHelper.CompareStringToSHA256(Request.Headers[_authentication], user.TokenHashed))
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "Search", "Failure - Invalid Token"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.Unauthorized });
                }

                if (user.LastLoginOn.AddMinutes(_authSessionInMinutes) <= DateTime.Now)
                {
                    _logger.LogWarning(String.Format("Action: {0} | Status: {1}", "Search", "Failure - Invalid Session"));
                    return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorMessage() { Message = ErrorMessage.InvalidSession });
                }

                UserDTO userDto = new UserDTO(user);

                _logger.LogInformation(String.Format("Action: {0} | Status: {1}", "Search", "Success"));
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(String.Format("Action: {0} | Status: {1}", "SignUp", "Exception"));
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorMessage() { Message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userService.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}