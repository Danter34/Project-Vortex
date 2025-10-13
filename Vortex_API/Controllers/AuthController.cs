using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var result = await _authRepository.RegisterAsync(dto);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var token = await _authRepository.LoginAsync(dto);
            if (token == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { token });
        }
    }
}
