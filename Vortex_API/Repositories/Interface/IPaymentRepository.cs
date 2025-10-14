
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentResponse> CreatePayment(PaymentRequest dto);
        Task<bool> HandleMomoReturn(string orderId, string resultCode);
        Task<bool> HandleMomoNotify(string orderId, string resultCode);
    }
}
