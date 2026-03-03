namespace Assignmate.AssignmentService.Contracts.Events
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