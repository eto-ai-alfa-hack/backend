using System.Text;
using AlfaBankInsight.Hangfire.Infrastructure;
using AlfaBankInsight.Hangfire.Worker.Jobs;
using Hangfire;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNewsParsers();

builder.Services.AddHangfire(configuration => { configuration.UseInMemoryStorage(); });
builder.Services.AddHangfireServer(options => { options.WorkerCount = 1; });

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Запускаем сбор сразу после старта.
    backgroundJobs.Enqueue<NewsCollectionJob>(job => job.ExecuteAsync(CancellationToken.None));

    // Затем запускаем сбор каждые 10 минут.
    recurringJobs.AddOrUpdate<NewsCollectionJob>(
        "news-collection",
        job => job.ExecuteAsync(CancellationToken.None),
        "*/3 * * * *");
}

await host.RunAsync();