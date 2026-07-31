
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Data;
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Services
{
    public class OutBoxBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutBoxBackgroundService> _logger;
        public OutBoxBackgroundService(IServiceProvider serviceProvider, ILogger<OutBoxBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                    var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                    var messages = await context.OutBoxMessages
                            .Where(m => m.ProcessedAt == null && m.RetryCount<3)
                            .OrderBy(m => m.OccuredAt)
                            .Take(20)
                            .ToListAsync(stoppingToken);
                    
                    foreach(var message in messages)
                    {
                        try
                        {
                            await messagePublisher.PublishAsync(
                                message.Type,
                                message.Payload,
                                stoppingToken
                            );
                            message.ProcessedAt = DateTime.UtcNow;
                            message.Error = null;
                        }
                        catch (Exception e)
                        {
                            
                            _logger.LogError(e, "Failed To Publish OutBox Message {MessageId}" ,message.Id);
                            message.RetryCount ++;
                            message.Error = e.Message;
                        }
                    }
                    if(messages.Count > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                    
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An Error proceesing while outbox Message");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            
        }
    }
}