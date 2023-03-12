using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WooperUtility.Models;
using WooperUtility.Utility;

namespace WooperUtility.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly List<Reply> _replies;
    private readonly List<long> _bannedUsers;

    private readonly List<Button> _fuckKeyboard;

    public UpdateHandler(ITelegramBotClient botClient, List<Reply> replies, List<long> bannedUsers, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
        _replies = replies;
        _bannedUsers = bannedUsers;

        //this is just meme for the banned users
        _fuckKeyboard = new List<Button>()
        {
            new Button(){
                Text = "Complaint 🛃",
                Url = @"https://www.youtube.com/watch?v=xvFZjo5PgG0"
            }
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        //let's just say that if someone is spamming something else from text messages
        //or if someone somehow manage to add the bot to a bot we ignore it
        if (message.Text is not { } messageText)
            return;
        if (message.Chat.Type != ChatType.Private)
            return;

        //that's easy, we just need to send the first menu voice
        var reply = _replies.Where(x => x.Id == 0).First();

        string caption = APIHelper.CreateCaption(reply.Message, reply.Parameters, message.From);
        //again, banned users meme
        if (_bannedUsers.Contains(message.Chat.Id))
        {
            Message sentMessage = await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "💩",
                replyMarkup: APIHelper.CreateKeyboard(_fuckKeyboard));

            _logger.LogInformation("The fuck message was sent with id: {SentMessageId}", sentMessage.MessageId);
        }
        else
        {
            using (FileStream fs = new(reply.ImgUrl, FileMode.Open, FileAccess.Read))
            {

                Message sentMessage = await _botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputFile(fs, reply.ImgUrl),//.InputOnlineFile(fs),//msg.url),
                caption: caption,
                replyMarkup: APIHelper.CreateKeyboard(reply.Buttons),
                cancellationToken: cancellationToken);

                _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
            }
        }
        
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        int page = 0;
        int index = 0;

        //with a . we have both the page (menu voice id) and index (page of the menu voice)
        if (callbackQuery.Data.Contains("."))
        {
            string[] data = callbackQuery.Data.Split(".");
            page = int.Parse(data[0]);
            index = int.Parse(data[1]);
        }
        //otherwhise we just have the menu voice id
        else
        {
            page = int.Parse(callbackQuery.Data);
        }

        //it was a bit harder but now we can get the right reply
        var reply = _replies.Where(x => x.Id == page).First();
        var filename = Path.GetFileName(reply.ImgUrl);

        string caption = APIHelper.CreateCaption(reply.Message, reply.Parameters, callbackQuery.From);

        //idk if it's the library or telegram API's fault but it looks like we need to send 2 requests
        //the first one to edit the image and the second one with the new caption and keyboard
        using (FileStream fs = new(reply.ImgUrl, FileMode.Open, FileAccess.Read))
        {
            await _botClient.EditMessageMediaAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                media: new InputMediaPhoto(new InputFile(fs, filename)),
                cancellationToken: cancellationToken
            );

            await _botClient.EditMessageCaptionAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                caption: caption,
                replyMarkup: APIHelper.CreateKeyboard(reply.Buttons, page, index, reply.ParentId ?? -1)
            );

        }
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
