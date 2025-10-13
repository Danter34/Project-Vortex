using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
namespace Vortex_API.Repositories.Service
{
    public class PaymentRepository: IPaymentRepository
    {
        private readonly AppDbContext _context;
        public PaymentRepository(AppDbContext ctx) { _context = ctx; }

        public async Task<Payment> CreatePaymentAsync(Payment p)
        {
            _context.Payments.Add(p);
            await _context.SaveChangesAsync();
            return p;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(Payment p)
        {
            _context.Payments.Update(p);
            await _context.SaveChangesAsync();
        }
    }
}
