using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AlfaBankInsight.Hangfire.Application.Abstractions;
using AlfaBankInsight.Hangfire.Domain.Entities;

namespace AlfaBankInsight.Hangfire.Infrastructure.ParsingServices;

public class RbcRssParser(HttpClient httpClient) : INewsParser
{
    public string Source => "RBC";
    private const string FeedUrl = "https://rssexport.rbc.ru/rbcnews/news/30/full.rss";
    
    public async Task<IReadOnlyCollection<NewsArticle>> ParseAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(FeedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var items = document
            .Descendants()
            .Where(element => element.Name.LocalName == "item");

        return items
            .Select(ParseItem)
            .Where(article => article is not null)
            .Select(article => article!)
            .ToArray();
    }
    
    private NewsArticle? ParseItem(XElement item)
    {
        var title = GetValue(item, "title");
        var url = GetValue(item, "link");

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var externalId = GetValue(item, "guid") ?? url;

        var fullText = GetValue(item, "rbc_news_full-text", "full-text", "encoded");

        var summary = GetValue(item, "description");

        return new NewsArticle
        {
            Source = Source,
            ExternalId = externalId,
            Url = url,
            Title = CleanText(title),
            Summary = CleanOptionalText(summary),
            Content = CleanOptionalText(fullText),
            ImageUrl = GetImageUrl(item),
            PublishedAt = ParseDate(GetValue(item, "pubDate")),
            Categories = item.Elements()
                .Where(element => element.Name.LocalName == "category")
                .Select(element => CleanText(element.Value))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RetrievedAt = DateTimeOffset.UtcNow
        };
    }

    private static string? GetValue(XElement element, params string[] localNames)
    {
        var child = element.Elements().FirstOrDefault(candidate =>
            localNames.Contains(candidate.Name.LocalName, StringComparer.OrdinalIgnoreCase));

        if (child is null && localNames.Any(name => name.Contains("full-text", StringComparison.OrdinalIgnoreCase)))
        {
            child = element.Elements().FirstOrDefault(candidate =>
                candidate.Name.LocalName.EndsWith("full-text", StringComparison.OrdinalIgnoreCase));
        }

        return string.IsNullOrWhiteSpace(child?.Value) ? null : child.Value.Trim();
    }

    private static string? GetImageUrl(XElement item)
    {
        var enclosure = item.Elements().FirstOrDefault(element => element.Name.LocalName == "enclosure");

        var enclosureUrl = enclosure?.Attribute("url")?.Value;

        if (!string.IsNullOrWhiteSpace(enclosureUrl))
            return enclosureUrl;

        return GetValue(item, "image", "image_url", "url");
    }

    private static string CleanText(string value)
    {
        var decoded = WebUtility.HtmlDecode(value);
        var withoutTags = Regex.Replace(decoded, "<[^>]*>", " ");

        return Regex.Replace(withoutTags, @"\s+", " ").Trim();
    }

    private static string? CleanOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = CleanText(value);

        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var date)
                ? date
                : null;
    }
}