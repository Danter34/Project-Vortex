using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface INewsRepository
    {
        Task<IEnumerable<News>> GetAllNews();
        Task<News?> GetNewsById(int id);
        Task<News> CreateNews(NewsDTO newsDto);
        Task<News?> UpdateNewsbyId(int id, EdditNewsDTO newsDto);
        Task<bool> DeleteNews(int id);
    }
}
