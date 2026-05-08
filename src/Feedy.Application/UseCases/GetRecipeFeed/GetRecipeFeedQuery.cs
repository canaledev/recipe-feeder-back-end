namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Query to fetch the Smart Feed for a user.
/// This is the input contract for the use case. Each use case has its own query/command.
/// </summary>
public class GetRecipeFeedQuery
{
    public Guid UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public GetRecipeFeedQuery(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        UserId = userId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
