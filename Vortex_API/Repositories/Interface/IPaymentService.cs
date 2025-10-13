using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(PaymentRequestDTO dto);
        Task<Payment?> GetByIdAsync(int id);
        Task ConfirmPaymentAsync(int paymentId);
    }
}
