namespace MarbleQualityBot.Core.Domain.Entities;

public class TextMessage
{
    public long ChatId { get; set; }

    public int MessageId { get; set; }

    public string MessageText { get; set; }

    public TextMessage(long chatId, int messageId, string messageText)
    {
        ChatId = chatId;
        MessageId = messageId;
        MessageText = messageText;
    }
}