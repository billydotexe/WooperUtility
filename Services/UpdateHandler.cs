using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using Newtonsoft.Json.Converters;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WooperUtility.Datacontext;
using WooperUtility.Models;
using WooperUtility.Utility;

namespace WooperUtility.Services;

public class UpdateHandler(
	ITelegramBotClient botClient,
	List<Reply> replies,
	// Collection<long> bannedUsers,
	WooperContext context,
	ILogger<UpdateHandler> logger
) : IUpdateHandler
{
	//this is just meme for the banned users
	private readonly List<Button> _fuckKeyboard = new List<Button>()
		{
			new Button()
			{
				Text = "Assistance",
				Url = @"https://t.me/wooperutility_bot"
			},
			new Button(){
				Text = "Complaint 🛃",
				Url = @"https://www.youtube.com/watch?v=xvFZjo5PgG0"
			}
		};

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(update);
		var handler = update switch
		{
			{ Message: { } message } => BotOnMessageReceived(message, cancellationToken),
			{ CallbackQuery: { } callbackQuery } when callbackQuery.Data is not null && callbackQuery.Data.StartsWith("users", StringComparison.OrdinalIgnoreCase) => BotOnUsersCallbackQueryReceived(callbackQuery, cancellationToken),
			{ CallbackQuery: { } callbackQuery } => BotOnGenericCallbackQueryReceived(callbackQuery, cancellationToken),
			_ => UnknownUpdateHandlerAsync(update, cancellationToken)
		};

		await handler;
	}

	private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
	{
        logger.LogInformation("User {UserId} with tag {Tag} sent {Message}", message.From!.Id, message.From.Username, message.Text);
		var handler = message switch
		{
			{ Text: string text }
					when text.Equals("/users", StringComparison.OrdinalIgnoreCase)
					&& context.Admins.Any(x => x.User.Id == message.From!.Id)
					=> GetUserList(message, cancellationToken),
			{ Text: string text } when text.StartsWith("/ban ", StringComparison.OrdinalIgnoreCase)
					&& context.Admins.Any(x => x.User.Id == message.From!.Id)
					=> BanUser(message, cancellationToken),
			{ Text: string text } when text.StartsWith("/unban ", StringComparison.OrdinalIgnoreCase)
					&& context.Admins.Any(x => x.User.Id == message.From!.Id)
					=> UnbanUser(message, cancellationToken),
			_ => GenericMessageHandler(message, cancellationToken)
		};

		await handler;
	}

	private async Task UnbanUser(Message message, CancellationToken cancellationToken)
	{
		var unbanlist = message.Text!.Split(" ", StringSplitOptions.RemoveEmptyEntries).Skip(1);
		List<string> unUnbannable = [];


        logger.LogInformation("Admin {UserId} with tag {Tag} requested unban for {ToBan}", message.From!.Id, message.From.Username, unbanlist);

        foreach (var toUnban in unbanlist)
		{
			if(long.TryParse(toUnban, out var id))
			{
				var user = context.BannedUsers.Where(x => x.User == context.Users.Where(x => x.Id == id).First()).FirstOrDefault();
				if(user is not null)
                {
                    logger.LogError("User {User} unbanned", toUnban);
                    context.BannedUsers.Remove(user);
				}
				else
				{
                    logger.LogError("Cannot unban User {User}", toUnban);
					unUnbannable.Add(toUnban);
				}
			}
			else
			{
				unUnbannable.Add(toUnban);
			}
		}


        await botClient.SendTextMessageAsync(chatId: message.From!.Id, text: $"Unbanned all users except: {string.Join(", ", unUnbannable)}", cancellationToken: cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
	}
	private async Task BanUser(Message message, CancellationToken cancellationToken)
	{
		var banlist = message.Text!
						.Replace('\n', ' ')
						.Split(" ", StringSplitOptions.RemoveEmptyEntries)
						.Skip(1);

		List<string> unbannable = [];

        logger.LogInformation("Admin {UserId} with tag {Tag} requested unban for {ToBan}", message.From!.Id, message.From.Username, banlist);

        foreach (var toBan in banlist)
		{
			if(long.TryParse(toBan, out var id))
			{
				var user = context.Users.Where(x => x.Id == id).FirstOrDefault();
				if(user is not null)
                {
                    logger.LogError("User {User} banned", toBan);
                    context.BannedUsers.Add( new BannedUser(){ User = user, BanDate = DateTime.UtcNow, BannedBy = context.Users.Where(x => x.Id == message!.From!.Id).First()});
				}
				else
                {
                    logger.LogError("Cannot unban User {User}", toBan);
                    unbannable.Add(toBan);
				}
			}
			else
			{
				unbannable.Add(toBan);
			}
		}
		await botClient.SendTextMessageAsync(chatId: message.From!.Id, text: $"Banned all users except: {string.Join(", ", unbannable)}", cancellationToken: cancellationToken);
		await context.SaveChangesAsync(cancellationToken);
	}
	private async Task GetUserList(Message message, CancellationToken cancellationToken)
	{
		var users = context.Users.Take(20);
		var msg = string.Join("\n", users.Select(x => $@"`{x.Id}` \| `@{x.Username}` \| {x.LastActivity:yyyy:MM:dd}"));
		var sentMessage = await botClient.SendTextMessageAsync(chatId: message.From!.Id, text: msg, parseMode: ParseMode.MarkdownV2,  replyMarkup: APIHelper.CreateNavigationKeyboard(context.Users.Count()), cancellationToken: cancellationToken);
		logger.LogInformation("New message sent with id: {SentMessageId}", sentMessage.MessageId);
	}

	private async Task GenericMessageHandler(Message message, CancellationToken cancellationToken)
	{
		if(message.From is not { } fromUser)
		{
			return;
		}
		if(context.Users.Any(x => x.Id == fromUser.Id))
		{
			var user = context.Users.Where(x => x.Id == fromUser.Id).First();
			user.LastActivity = DateTime.UtcNow;
			user.Username = fromUser.Username;
			context.Users.Update(user);
		}
		else
		{
			var user = new Datacontext.User(){ Id = fromUser.Id, JoinDate = DateTime.UtcNow, LastActivity = DateTime.UtcNow, Username = fromUser.Username };
			context.Users.Add(user);
		}
		await context.SaveChangesAsync(cancellationToken);
		//let's just say that if someone is spamming something else from text messages
		//or if someone somehow manage to add the bot to a bot we ignore it
		if (message.Text is not { } messageText)
		{
			return;
		}

		if (message.Chat.Type != ChatType.Private)
		{
			return;
		}

        logger.LogInformation("User {Id} with tag {Username} sent message {Message}", message.From.Id, message.From.Username, message.Text);

		//that's easy, we just need to send the first menu voice
		var reply = replies.Where(x => x.Id == 0).First();

		var caption = APIHelper.CreateCaption(reply.Message!, reply.Parameters, message.From!);
		//again, banned users meme
		if (context.BannedUsers.Any(x => x.User.Id == fromUser.Id))
		{
			var sentMessage = await botClient.SendTextMessageAsync(chatId: fromUser.Id, text: "💩", replyMarkup: APIHelper.CreateKeyboard(_fuckKeyboard), cancellationToken: cancellationToken);
			context.Requests.Add(new BotRequest(){ User = context.Users.Where(x => x.Id == fromUser.Id).First(), RequestType = "BannedMessage", RequestDate = DateTime.UtcNow });
			logger.LogInformation("The fuck message was sent with id: {SentMessageId}", sentMessage.MessageId);
		}
		else
		{
			using FileStream fs = new(reply.ImgPath!, FileMode.Open, FileAccess.Read);

			var sentMessage = await botClient.SendPhotoAsync(
			chatId: fromUser.Id,
			photo: InputFile.FromStream(fs, reply.ImgPath),
			caption: caption,
			replyMarkup: APIHelper.CreateKeyboard(reply.Buttons),
			cancellationToken: cancellationToken);
			context.Requests.Add(new BotRequest(){ User = context.Users.Where(x => x.Id == fromUser.Id).First(), RequestType = reply.Id.ToString(CultureInfo.InvariantCulture), RequestDate = DateTime.UtcNow });

			logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	private async Task BotOnUsersCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
	{
		var pageIndex = int.Parse(callbackQuery.Data!.Split(".")![1], CultureInfo.InvariantCulture);
		var users = context.Users.Skip(20*pageIndex).Take(20);
		var msg = string.Join("\n", users.Select(x => $@"`{x.Id}` \| `@{x.Username}` \| {x.LastActivity:yyyy:MM:dd}"));
		var sentMessage = await botClient.EditMessageTextAsync(chatId: callbackQuery.From!.Id,
																	messageId: callbackQuery.Message!.MessageId,
																	text: msg,
																	parseMode: ParseMode.MarkdownV2,
																	replyMarkup: APIHelper.CreateNavigationKeyboard(context.Users.Count(), pageIndex),
																	cancellationToken: cancellationToken);
		logger.LogInformation("New list sent with id: {SentMessageId}", sentMessage.MessageId);
	}

	// Process Inline Keyboard callback data
	private async Task BotOnGenericCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
	{
		logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

		if(callbackQuery.From is not { } fromUser)
		{
			return;
		}
		if(context.Users.Any(x => x.Id == fromUser.Id))
		{
			var user = context.Users.Where(x => x.Id == fromUser.Id).First();
			user.LastActivity = DateTime.UtcNow;
			user.Username = fromUser.Username;
			context.Users.Update(user);
		}
		else
		{
			var user = new Datacontext.User(){ Id = fromUser.Id, JoinDate = DateTime.UtcNow, LastActivity = DateTime.UtcNow, Username = fromUser.Username };
			context.Users.Add(user);
		}
		await context.SaveChangesAsync(cancellationToken);

		var page = 0;
		var index = 0;

		//with a . we have both the page (menu voice id) and index (page of the menu voice)
		if (callbackQuery.Data!.Contains('.', StringComparison.InvariantCulture))
		{
			var data = callbackQuery.Data.Split(".");
			page = int.Parse(data[0], CultureInfo.InvariantCulture);
			index = int.Parse(data[1], CultureInfo.InvariantCulture);
		}
		//otherwhise we just have the menu voice id
		else
		{
			page = int.Parse(callbackQuery.Data, CultureInfo.InvariantCulture);
		}

		//it was a bit harder but now we can get the right reply
		var reply = replies.Where(x => x.Id == page).First();
		var filename = Path.GetFileName(reply.ImgPath);

		var caption = APIHelper.CreateCaption(reply.Message!, reply.Parameters, callbackQuery.From);

		//idk if it's the library or telegram API's fault but it looks like we need to send 2 requests
		//the first one to edit the image and the second one with the new caption and keyboard
		using FileStream fs = new(reply.ImgPath!, FileMode.Open, FileAccess.Read);
		await botClient.EditMessageMediaAsync(
			chatId: callbackQuery.Message!.Chat.Id,
			messageId: callbackQuery.Message.MessageId,
			media: new InputMediaPhoto(InputFile.FromStream(fs, filename)),
			cancellationToken: cancellationToken
		);

		await botClient.EditMessageCaptionAsync(chatId: callbackQuery.Message!.Chat.Id,
												messageId: callbackQuery.Message.MessageId,
												caption: caption,
												replyMarkup: APIHelper.CreateKeyboard(reply.Buttons, page, index, reply.ParentId ?? -1),
												cancellationToken: cancellationToken);

		context.Requests.Add(new BotRequest(){ User = context.Users.Where(x => x.Id == fromUser.Id).First(), RequestType = reply.Id.ToString(CultureInfo.InvariantCulture), RequestDate = DateTime.UtcNow });
		await context.SaveChangesAsync(cancellationToken);
	}

	private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
	{
		logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
		return Task.CompletedTask;
	}

	public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
	{
		var errorMessage = exception switch
		{
			ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
			_ => exception.ToString()
		};

		logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

		// Cooldown in case of network connection error
		if (exception is RequestException)
		{
			await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
		}
	}
}
