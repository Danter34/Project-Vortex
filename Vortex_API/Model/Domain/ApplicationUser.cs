using Microsoft.AspNetCore.Identity;

namespace Vortex_API.Model.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public decimal Balance { get; set; }
        public string? FullName { get; set; }
    }

}
