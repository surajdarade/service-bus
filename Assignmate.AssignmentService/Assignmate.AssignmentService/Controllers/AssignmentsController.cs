using Assignmate.AssignmentService.Contracts.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Assignmate.AssignmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly ServiceBusSender _sender;
        private readonly EventGridPublisher _eventGridPublisher;

        public AssignmentsController(ServiceBusSender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment()
        {
            var assignmentId = Guid.NewGuid().ToString();

            var evt = new AssignmentCreatedEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredOn: DateTime.UtcNow,
                AssignmentId: assignmentId,
                UserId: "U456",
                Type: "Digital",
                Price: 500,
                Currency: "INR"
            );

            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(evt))
            {
                MessageId = evt.EventId,
                CorrelationId = evt.AssignmentId,
                ContentType = "application/json"
            };

            message.ApplicationProperties["eventType"] = "AssignmentCreated";
            message.ApplicationProperties["assignmentType"] = evt.Type;

            var sendMessageTask = _sender.SendMessageAsync(message);

            var publishEventTask = _eventGridPublisher.PublishAssignmentCreated(assignmentId);

            await Task.WhenAll(sendMessageTask, publishEventTask);

            return Ok("AssignmentCreated event published.");
        }
    }
}
