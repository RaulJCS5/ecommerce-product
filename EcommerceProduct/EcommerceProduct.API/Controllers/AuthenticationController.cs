using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceProduct.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AuthenticationController(IConfiguration configuration, IUserService userService, IMapper mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registrationDto">User registration information</param>
        /// <returns>Created user information</returns>
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user already exists
                if (await _userService.UserExistsAsync(registrationDto.Username, registrationDto.Email))
                {
                    return Conflict("Username or email already exists.");
                }

                // Map DTO to User entity
                var user = _mapper.Map<User>(registrationDto);

                // Register the user (password will be hashed in service)
                var createdUser = await _userService.RegisterUserAsync(user, registrationDto.Password);

                // Map back to DTO for response (without sensitive information)
                var userDto = _mapper.Map<UserDto>(createdUser);

                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred during registration: {ex.Message}");
            }
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate user credentials
                var user = await _userService.ValidateUserAsync(loginDto.Username, loginDto.Password);
                if (user == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                // Update last login time
                await _userService.UpdateLastLoginAsync(user.Id);

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                var response = new AuthenticationResponse
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred during authentication: {ex.Message}");
            }
        }

        /// <summary>
        /// Legacy authenticate endpoint (for backward compatibility)
        /// </summary>
        /// <param name="authenticationRequestBody">Legacy authentication request</param>
        /// <returns>JWT token string</returns>
        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate([FromBody] AuthenticationRequestBody authenticationRequestBody)
        {
            var loginDto = new UserLoginDto
            {
                Username = authenticationRequestBody.Username,
                Password = authenticationRequestBody.Password
            };

            var result = await Login(loginDto);

            if (result.Result is OkObjectResult okResult && okResult.Value is AuthenticationResponse response)
            {
                return Ok(response.Token);
            }

            return result.Result ?? BadRequest("Authentication failed");
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretForKey"] ??
                    throw new InvalidOperationException("Authentication:SecretForKey is required")));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("unique_name", user.Username),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName),
                new Claim("email", user.Email),
                new Claim("city", user.City),
                new Claim("role", user.Role)
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        // Legacy class for backward compatibility
        public class AuthenticationRequestBody
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
