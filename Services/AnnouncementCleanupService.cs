using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalRoomAPI.Services
{
    public class AnnouncementCleanupService : BackgroundService
    {
        private readonly IServiceProvider _provider;

        public AnnouncementCleanupService(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();

                var service = scope.ServiceProvider
                    .GetRequiredService<IAnnouncementService>();

                // ? this will now work after interface fix
                await service.RemoveExpiredAnnouncements();

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}