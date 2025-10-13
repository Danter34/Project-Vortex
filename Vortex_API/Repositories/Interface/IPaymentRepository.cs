using Vortex_API.Model.Domain;

namespace Vortex_API.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<Payment> CreatePaymentAsync(Payment p);
        Task<Payment?> GetByIdAsync(int id);
        Task UpdateAsync(Payment p);
    }
}
