using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using Telegram.Bot;
using WooperUtility;
using WooperUtility.Models;
using WooperUtility.Services;
using WooperUtility.Utility;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((context, services) =>
    {
        // Register Bot configuration
        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.Configuration));

        services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddSingleton<List<Reply>>(JsonSerializer.Deserialize<List<Reply>>(Utils.LoadJson(Path.Combine(AppContext.BaseDirectory, "Messages.json"))));
        //remove you're ok with everyone to use your bot or you want to manage a banlist with a db or something else
        services.AddSingleton<List<long>>(JsonSerializer.Deserialize<List<long>>(Utils.LoadJson(Path.Combine(AppContext.BaseDirectory, "BannedUsers.json"))));
        services.AddHostedService<PollingService>();

    })
    .Build();

await host.RunAsync();

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; set; } = "";
}
