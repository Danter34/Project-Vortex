using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using QRCoder;

namespace Vortex_API.Repositories.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly AppDbContext _context;

        public PaymentService(IPaymentRepository repo, AppDbContext ctx)
        {
            _paymentRepo = repo;
            _context = ctx;
        }

        // create payment; controller will build the public paymentPageUrl and generate QR
        public async Task<Payment> CreatePaymentAsync(PaymentRequestDTO dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = order.TotalAmount,
                Method = dto.Method,
                Status = "Pending",
                BankName = "Vietinbank",
                ReceiverAccount = "1234567890",
                ReceiverName = "Dante",
                Memo = $"Thanh toan don hang #{dto.OrderId}"
            };

            var created = await _paymentRepo.CreatePaymentAsync(payment);
            return created;
        }

        public async Task<Payment?> GetByIdAsync(int id) => await _paymentRepo.GetByIdAsync(id);

        public async Task ConfirmPaymentAsync(int paymentId)
        {
            var p = await _paymentRepo.GetByIdAsync(paymentId);
            if (p == null) throw new Exception("Payment not found");
            if (p.Status == "Paid") return;

            p.Status = "Paid";
            await _paymentRepo.UpdateAsync(p);

            // update order status
            var order = await _context.Orders.FindAsync(p.OrderId);
            if (order != null)
            {
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }
        }
    }
}
