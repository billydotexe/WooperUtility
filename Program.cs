using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.Json;
using Telegram.Bot;
using WooperUtility;
using WooperUtility.Datacontext;
using WooperUtility.Models;
using WooperUtility.Services;
using WooperUtility.Utility;

var host = Host.CreateDefaultBuilder(args)
	.UseSystemd()
	.ConfigureServices(async (context, services) =>
	{
		// Register Bot configuration
		services.Configure<BotConfiguration>(
			context.Configuration.GetSection(BotConfiguration.Configuration));

		services.AddHttpClient("telegram_bot_client")
				.AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
				{
					var botConfig = sp.GetRequiredService<IOptionsMonitor<BotConfiguration>>().CurrentValue;
					TelegramBotClientOptions options = new(botConfig.BotToken);
					return new TelegramBotClient(options, httpClient);
				});


        //db & migration
        services.AddDbContext<WooperContext>();

        var serviceProvider = services.BuildServiceProvider();
        var requiredService = serviceProvider.GetRequiredService<WooperContext>();
        var migrator = requiredService.GetService<IMigrator>();
        await migrator.MigrateAsync(null);

        services.AddScoped<UpdateHandler>();
		services.AddScoped<ReceiverService>();
		services.AddSingleton(JsonSerializer.Deserialize<List<Reply>>(Utils.LoadJson(Path.Combine(AppContext.BaseDirectory, "Messages.json"))) ?? throw new DataException("Couldn't load messages"));
		//remove you're ok with everyone to use your bot or you want to manage a banlist with a db or something else
		services.AddSingleton(JsonSerializer.Deserialize<List<long>>(Utils.LoadJson(Path.Combine(AppContext.BaseDirectory, "BannedUsers.json"))) ?? throw new DataException("Couldn't load banned users"));
		services.AddHostedService<PollingService>();

	})
	.Build();

await host.RunAsync();
