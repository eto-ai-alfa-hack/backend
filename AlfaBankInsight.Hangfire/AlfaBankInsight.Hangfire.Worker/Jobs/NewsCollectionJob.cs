using AlfaBankInsight.Hangfire.Application.Abstractions;

namespace AlfaBankInsight.Hangfire.Worker.Jobs;

public class NewsCollectionJob(INewsCollectionService collectionService, ILogger<NewsCollectionJob> logger)
{
    public async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Запущено задание сбора новостей");

        await collectionService.ExecuteAsync(cancellationToken);

        logger.LogInformation("Задание сбора новостей завершено");
    }
}