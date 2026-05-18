namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Input for the GetRecipeFeed use case.
/// UserId is extracted from auth claims by the controller; page params come from the query string.
/// </summary>
public record GetRecipeFeedRequest(Guid UserId, int PageNumber = 1, int PageSize = 20);
