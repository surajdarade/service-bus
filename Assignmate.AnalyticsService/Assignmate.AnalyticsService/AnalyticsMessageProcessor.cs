using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignmate.AnalyticsService
{
    public class AnalyticsMessageProcessor : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<AnalyticsMessageProcessor> _logger;
        private ServiceBusProcessor _processor;

        public AnalyticsMessageProcessor(
            ServiceBusClient client,
            IConfiguration config,
            ILogger<AnalyticsMessageProcessor> logger)
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

            _logger.LogInformation("Analytics Service received: {Body}", body);

            // Simulate analytics processing
            await Task.Delay(1500);

            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation("Analytics processing completed.");
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Analytics processing error");
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
