using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Enums;

namespace ProductManagement.Api.BackgroundServices
{
    public class SyncWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SyncWorker> _logger;

        private readonly TimeSpan waiting_time = TimeSpan.FromSeconds(30);

        public SyncWorker(IServiceScopeFactory scopeFactory, ILogger<SyncWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            _logger.LogInformation("SyncWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking database for not sync. product.");

                await SyncProductsAsync(stoppingToken);
            }

            await Task.Delay(waiting_time, stoppingToken);
        }

        internal async Task SyncProductsAsync(CancellationToken stoppingToken)
        {

            var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if(! await context.Database.CanConnectAsync(stoppingToken))
            {
                _logger.LogCritical("Connections to the database was not possible yet.");
                await Task.Delay(waiting_time,stoppingToken);
                return;
            }
            var unsyncedProducts = await context.Products.Where(p => p.SyncStatus != SyncStatus.Synchronized).Take(10).ToListAsync();
            if (!unsyncedProducts.Any()) 
            {
                _logger.LogInformation("All products are syncronized!");
                await Task.Delay(waiting_time, stoppingToken);
                return;
            }

            _logger.LogInformation("Found {count} intems unsynchronized.", unsyncedProducts.Count());

            foreach (var p in unsyncedProducts)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                _logger.LogInformation("Sending request to thirth party system to sync the product with Sku {Sku}", p.Sku);

                await Task.Delay(1000, stoppingToken);
                _logger.LogInformation("product with {Sku} was successfully synchronized.", p.Sku);

                p.SyncStatus = SyncStatus.Synchronized;

            }
            await context.SaveChangesAsync();
        }
    }
}
