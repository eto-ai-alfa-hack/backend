namespace AlfaBankInsight.Hangfire.Application.Abstractions;

public interface INewsCollectionService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}