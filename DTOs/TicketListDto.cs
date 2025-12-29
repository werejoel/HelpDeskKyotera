namespace HelpDeskKyotera.DTOs
{
    // DTOs/TicketDto.cs
    public record TicketListDto(
        Guid Id,
        string TicketNumber,
        string Title,
        string Priority,
        string Status,
        string Requester,
        string? AssignedTo,
        DateTime CreatedOn
    );

    public record CreateTicketDto(string Title, string Description, Guid CategoryId, Guid PriorityId);
}
