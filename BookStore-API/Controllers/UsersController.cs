using BookStore_API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Users login and register acount
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _loggerService;
        private readonly IConfiguration _configuration;
        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILoggerService loggerService, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _loggerService = loggerService;
            _configuration = configuration;
        }
        /// <summary>
        /// User register endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var location = GetControllerNames();
            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;
                _loggerService.LogInfo($"{location}: Registration Attempt for {username} ");
                var user = new IdentityUser { Email = username, UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _loggerService.LogError($"{location}: {error.Code} {error.Description}");
                    }
                    return InternalError($"{location}: {username} User Registration Attempt Failed");
                }
                await _userManager.AddToRoleAsync(user, "Customer");
                return Created("login", new { result.Succeeded });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerNames();
            try
            {
                var username = userDTO.EmailAddress;
                var password = userDTO.Password;
                _loggerService.LogInfo($"{location}: Login attempted from user {username}");
                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    _loggerService.LogInfo($"{location}: {username} Successfully authenticated");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebTokens(user);
                    return Ok(new { token = tokenString });
                }
                _loggerService.LogInfo($"{location}:{username} Not authenticated");
                return Unauthorized(userDTO);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Generate tokens
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<string> GenerateJSONWebTokens(IdentityUser user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub , user.Email),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                null,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /// <summary>
        /// Get control names
        /// </summary>
        /// <returns></returns>
        private string GetControllerNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }
        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the administrator");
        }
    }
}