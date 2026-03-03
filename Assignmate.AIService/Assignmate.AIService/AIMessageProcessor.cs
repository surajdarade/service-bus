using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Assignmate.AIService
{
    public class AIMessageProcessor : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<AIMessageProcessor> _logger;
        private ServiceBusProcessor _processor;

        public AIMessageProcessor(
            ServiceBusClient client,
            IConfiguration config,
            ILogger<AIMessageProcessor> logger)
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

            _logger.LogInformation("AI Service received: {Body}", body);

            // Deserialize event
            var assignment = JsonSerializer.Deserialize<AssignmentCreatedEvent>(body);

            // Example: Only process digital assignments
            if (assignment?.Type == "Digital")
            {
                _logger.LogInformation("Running AI evaluation for Assignment {Id}", assignment.AssignmentId);

                // Simulate AI processing
                await Task.Delay(3000);

                _logger.LogInformation("AI evaluation completed.");
            }
            else
            {
                _logger.LogInformation("Skipping non-digital assignment.");
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "AI processing error");
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
