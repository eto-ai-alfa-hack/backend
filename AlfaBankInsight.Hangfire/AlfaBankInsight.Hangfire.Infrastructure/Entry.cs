using AlfaBankInsight.Hangfire.Application.Abstractions;
using AlfaBankInsight.Hangfire.Infrastructure.ParsingServices;
using AlfaBankInsight.Hangfire.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlfaBankInsight.Hangfire.Infrastructure;

public static class Entry
{
    public static IServiceCollection AddNewsParsers(this IServiceCollection services)
    {
        services.AddHttpClient<RbcRssParser>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<INewsParser, RbcRssParser>();
        services.AddScoped<INewsCollectionService, NewsCollectionService>();
        

        return services;
    }
}