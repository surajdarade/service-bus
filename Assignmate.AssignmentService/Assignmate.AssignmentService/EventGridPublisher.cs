using Azure;
using Azure.Messaging.EventGrid;

namespace Assignmate.AssignmentService
{
    public class EventGridPublisher
    {
        private readonly EventGridPublisherClient _client;

        public EventGridPublisher(IConfiguration config)
        {
            var endpoint = new Uri(config["EventGrid:TopicEndpoint"]);
            var key = new AzureKeyCredential(config["EventGrid:AccessKey"]);

            _client = new EventGridPublisherClient(endpoint, key);
        }

        public async Task PublishAssignmentCreated(string assignmentId)
        {
            var eventData = new
            {
                assignmentId = assignmentId,
                type = "Digital"
            };

            var gridEvent = new EventGridEvent(
                subject: $"assignments/{assignmentId}",
                eventType: "Assignment.Created",
                dataVersion: "1.0",
                data: eventData);

            await _client.SendEventAsync(gridEvent);
        }
    }
}
