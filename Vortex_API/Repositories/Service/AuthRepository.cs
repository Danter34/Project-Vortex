using Microsoft.AspNetCore.Identity;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class AuthRepository: IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthRepository(UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        public async Task<string?> LoginAsync(LoginDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            return _tokenRepository.CreateJwtToken(user, roles);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDTO dto)
        {
            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "User");

            return result;
        }
    }
}
