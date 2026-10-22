using AlfaBankInsight.Hangfire.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AlfaBankInsight.Hangfire.Infrastructure.Services;

public class NewsCollectionService(IEnumerable<INewsParser> parsers, ILogger<NewsCollectionService> logger) : INewsCollectionService
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var parserList = parsers.ToArray();

        if (parserList.Length == 0)
        {
            throw new InvalidOperationException("Не зарегистрировано ни одного парсера новостей.");
        }

        var successfulSources = 0;
        var failedSources = 0;

        foreach (var parser in parserList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                logger.LogInformation("Начинаем сбор новостей. Источник: {Source}", parser.Source);

                var articles = await parser.ParseAsync(cancellationToken);

                foreach (var article in articles)
                {
                    logger.LogInformation(
                        """
                        Новость
                        Источник: {Source}
                        Заголовок: {Title}
                        Дата: {PublishedAt}
                        Ссылка: {Url}
                        Категории: {Categories}
                        Описание: {Summary}
                        Полный текст: {Content}
                        ----------------------------------------
                        """,
                        article.Source,
                        article.Title,
                        article.PublishedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Не указана",
                        article.Url,
                        string.Join(", ", article.Categories),
                        article.Summary ?? "Нет описания",
                        article.Content ?? "Полный текст отсутствует");
                }

                successfulSources++;

                logger.LogInformation("Сбор завершён. Источник: {Source}. Новостей: {Count}", parser.Source, articles.Count);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedSources++;

                logger.LogError(exception, "Не удалось получить новости из источника {Source}", parser.Source);
            }
        }

        logger.LogInformation(
            "Сбор завершён. Успешных источников: {Successful}; " +
            "источников с ошибкой: {Failed}",
            successfulSources,
            failedSources);

        if (successfulSources == 0)
        {
            throw new InvalidOperationException("Не удалось получить новости ни из одного источника.");
        }
    }
}