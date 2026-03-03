using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignmate.NotificationService
{
    public class NotificationMessageProcessor : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationMessageProcessor> _logger;
        private ServiceBusProcessor _processor;

        public NotificationMessageProcessor(
            ServiceBusClient client,
            IConfiguration config,
            ILogger<NotificationMessageProcessor> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["ServiceBus:TopicName"];
            var subscription = _config["ServiceBus:SubscriptionName"];

            _processor = _client.CreateProcessor(topic, subscription);

            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            _logger.LogInformation("Notification Service received: {Body}", body);

            // Simulate sending notification (email/SMS/etc.)
            await Task.Delay(1000);

            _logger.LogInformation("Notification sent successfully.");

            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Notification processing error");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }
        }
    }
}
