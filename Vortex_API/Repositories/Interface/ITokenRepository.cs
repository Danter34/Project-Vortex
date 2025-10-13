using Vortex_API.Model.Domain;

namespace Vortex_API.Repositories.Interface
{
    public interface ITokenRepository
    {
        string CreateJwtToken(ApplicationUser user, IList<string> roles);
    }
}
