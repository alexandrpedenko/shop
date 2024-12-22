using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shop.API.Contracts.Requests.Auth;
using Shop.API.Contracts.Responses;
using Shop.Core.Helpers.JWT;

namespace Shop.API.Controllers
{
    /// <summary>
    /// Auth controller
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                return BadRequest($"Role '{request.Role}' does not exist.");
            }

            var user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.Name
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to register user",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Failed to assign role", Errors = roleResult.Errors.Select(e => e.Description) });
            }


            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Authenticate and login a user
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", request.Email);

                return Unauthorized("Invalid email or password.");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Invalid password for user with email {Email}", request.Email);

                return Unauthorized("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = JwtTokenHelper.GenerateToken(user, roles, _configuration);


            _logger.LogInformation("User with email {Email} logged in", request.Email);

            return Ok(new LoginResponseDto { Token = token });
        }
    }
}
