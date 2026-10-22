using AlfaBankInsight.Hangfire.Domain.Entities;

namespace AlfaBankInsight.Hangfire.Application.Abstractions;

public interface INewsParser
{
    string Source { get; }

    Task<IReadOnlyCollection<NewsArticle>> ParseAsync(CancellationToken cancellationToken = default);
}