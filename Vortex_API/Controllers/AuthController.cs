using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthRepository authRepository, ILogger<AuthController> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

            var result = await _authRepository.RegisterAsync(dto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}", dto.Email, string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var token = await _authRepository.LoginAsync(dto);

            if (token == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", dto.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
            return Ok(new { token });
        }
    }
}
