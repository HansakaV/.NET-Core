
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Services
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly ILogger<MessagePublisher> _logger;
        public MessagePublisher(ILogger<MessagePublisher> logger)
        {
            _logger = logger;
        }

        public async Task PublishAsync(string messageType, string payload, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Publishing Outbox Event -> Type: {MessageType}, Payload: {Payload}", messageType,payload);
            await Task.Delay(100, cancellationToken);
        }
    }
}