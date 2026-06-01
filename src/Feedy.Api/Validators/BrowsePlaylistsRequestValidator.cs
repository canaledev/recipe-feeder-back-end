namespace Feedy.Api.Validators;

using Feedy.Application.UseCases.BrowsePlaylists;
using FluentValidation;

public class BrowsePlaylistsRequestValidator : AbstractValidator<BrowsePlaylistsRequest>
{
    private static readonly string[] AllowedSortValues = ["relevance", "popularity", "newest", "matchPercentage"];
    private static readonly string[] AllowedDifficulties = ["Easy", "Medium", "Hard"];

    public BrowsePlaylistsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Sort)
            .Must(s => AllowedSortValues.Contains(s))
            .WithMessage($"Sort must be one of: {string.Join(", ", AllowedSortValues)}.");

        RuleFor(x => x.Difficulty)
            .Must(d => d is null || AllowedDifficulties.Contains(d))
            .WithMessage($"Difficulty must be one of: {string.Join(", ", AllowedDifficulties)}.");
    }
}
