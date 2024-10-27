namespace MarbleQualityBot.Core.Domain.Entities;

public class ImageMessage
{
    public long ChatId { get; set; }

    public int MessageId { get; set; }

    public string FileId { get; set; }

    public ImageMessage(long chatId, string fileId, int messageId)
    {
        ChatId = chatId;
        FileId = fileId;
        MessageId = messageId;
    }
}