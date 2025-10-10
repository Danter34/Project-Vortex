using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IBannerRepository
    {
        Task<IEnumerable<Banner>> GetAllBanner();
        Task<Banner?> GetBannerById(int id);
        Task<Banner> CreateBanner(BannerDTO bannerDto);
        Task<Banner?> UpdateBanner(int id, EdditBannerDTO bannerDto);
        Task<bool> DeleteBanner(int id);
    }
}
