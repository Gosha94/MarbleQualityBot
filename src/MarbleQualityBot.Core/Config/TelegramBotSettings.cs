namespace MarbleQualityBot.Core.Config;

public class TelegramBotSettings
{
    public string FileUrl { get; set; }

    public string LocalStoragePath { get; set; }

    public int MaxFileSizeMb { get; set; }

    public string BotToken { get; set; }
}