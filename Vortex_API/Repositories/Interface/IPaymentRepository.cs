
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentResponse> CreatePayment(PaymentRequest dto);
        Task<PaymentResponse> CreateVnPayPayment(PaymentRequest dto);
        Task<bool> HandleVnPayReturn(IDictionary<string, string> queryParams);
        Task<bool> HandleMomoReturn(string orderId, string resultCode);
        Task<bool> HandleMomoNotify(string orderId, string resultCode);
    }
}
