using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public AuthenticationController(IConfiguration configuration, IUserService userService, ICustomerService customerService, IMapper mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
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
        /// Delete user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound("User not found.");
            }

            return NoContent();
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
        [Authorize(Policy = "MustBeAdmin")]
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

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>All users information</returns>
        [HttpGet(Name = "GetAllUsers")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        /// <summary>
        /// Get user with customer profile by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information with customer profile</returns>
        [HttpGet("{id}/customer")]
        [Authorize]
        public async Task<ActionResult<UserWithCustomerDto>> GetUserWithCustomer(int id)
        {
            // Get user first
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Get customer profile
            var customer = await _customerService.GetCustomerByUserIdAsync(id);

            // Create a combined DTO
            var userWithCustomerDto = _mapper.Map<UserDto>(user);
            var customerDto = customer != null ? _mapper.Map<CustomerDto>(customer) : null;

            var result = new UserWithCustomerDto
            {
                // Map user properties
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                Customer = customerDto
            };

            return Ok(result);
        }

        /// <summary>
        /// Create a customer profile for a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="createCustomerDto">Customer profile information</param>
        /// <returns>Created customer profile</returns>
        [HttpPost("{id}/customer/profile")]
        [Authorize]
        public async Task<ActionResult<CustomerDto>> CreateCustomerProfile(int id, [FromBody] CreateCustomerProfileDto createCustomerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user exists
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Check if user already has a customer profile
                if (await _customerService.UserHasCustomerProfileAsync(id))
                {
                    return Conflict("User already has a customer profile.");
                }

                // Map DTO to Customer entity
                var customer = _mapper.Map<Customer>(createCustomerDto);

                // Create customer profile
                var createdCustomer = await _customerService.CreateCustomerProfileAsync(id, customer);

                // Map back to DTO for response
                var customerDto = _mapper.Map<CustomerDto>(createdCustomer);

                return CreatedAtAction(nameof(GetUserWithCustomer), new { id = id }, customerDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating customer profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a user has a customer profile
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Boolean indicating if user has customer profile</returns>
        [HttpGet("{id}/customer/profile/check")]
        public async Task<ActionResult<bool>> HasCustomerProfile(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var hasProfile = await _customerService.UserHasCustomerProfileAsync(id);
            return Ok(hasProfile);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretForKey"] ??
                                    throw new InvalidOperationException("Authentication:SecretForKey is required")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("role_name", user.Role));

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                //DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddYears(2),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return tokenToReturn;
        }

        // Legacy class for backward compatibility
        public class AuthenticationRequestBody
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
