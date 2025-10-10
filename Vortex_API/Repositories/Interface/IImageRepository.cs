using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
namespace Vortex_API.Repositories.Interface
{
    public interface IImageRepository
    {
        Task<IEnumerable<Image>> GetByProductIdAsync(int productId);
        Task<Image?> UploadImage(int productId, ImageUploadDTO dto);
        Task<bool> DeleteImage(int imageId);
    }
}
