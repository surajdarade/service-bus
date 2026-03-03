using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignmate.PaymentService
{
    public class PaymentMessageProcessor(
        ServiceBusClient client,
        IConfiguration config,
        ILogger<PaymentMessageProcessor> logger) : BackgroundService
    {
        private readonly ServiceBusClient _client = client;
        private readonly IConfiguration _config = config;
        private readonly ILogger<PaymentMessageProcessor> _logger = logger;
        private ServiceBusProcessor _processor;

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

            _logger.LogInformation("Payment Service received: {Body}", body);

            // Simulate payment processing
            await Task.Delay(2000);

            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation("Payment processing completed.");
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Message processing error");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}
