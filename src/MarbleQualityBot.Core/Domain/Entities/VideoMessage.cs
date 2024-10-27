namespace MarbleQualityBot.Core.Domain.Entities;

public class VideoMessage
{
    public long ChatId { get; set; }

    public int MessageId { get; set; }

    public string FileId { get; set; }

    public VideoMessage(long chatId, string fileId, int messageId)
    {
        ChatId = chatId;
        FileId = fileId;
        MessageId = messageId;
    }
}