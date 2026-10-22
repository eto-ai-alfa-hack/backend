namespace AlfaBankInsight.Hangfire.Domain.Entities;

public class NewsArticle
{
    public required string Source { get; init; }

    public required string ExternalId { get; init; }

    public required string Url { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Content { get; init; }

    public string? ImageUrl { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public IReadOnlyCollection<string> Categories { get; init; } = [];

    public DateTimeOffset RetrievedAt { get; init; } = DateTimeOffset.UtcNow;
}