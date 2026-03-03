using System;
using System.Collections.Generic;
using System.Text;

namespace Assignmate.AIService
{
    public sealed record AssignmentCreatedEvent(
        string EventId,
        DateTime OccurredOn,
        string AssignmentId,
        string UserId,
        string Type,
        decimal Price,
        string Currency
    );
}
