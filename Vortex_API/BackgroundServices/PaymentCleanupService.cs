using Vortex_API.Data;
using Vortex_API.Repositories.Service;

namespace Vortex_API.BackgroundServices
{
    public class PaymentCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var paymentRepo = new PaymentRepository(context, config);

                await paymentRepo.CheckFailedPaymentsAsync();

                // Chạy lại sau 2 phút
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
