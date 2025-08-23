using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;

namespace Medoz.CatChast.Messaging;

public record LogMessage(
    string ClientType,
    string SourceName,
    string Content,
    LogLevel LogLevel = LogLevel.Information,
    DateTime createdAt = default,
    string? SpeakerName = null)
{
    public LogMessage(string clientType, string content)
        : this(clientType, "system", content)
    {

    }

    /// <summary>
    /// コンテンツだけのメッセージを作成します
    /// </summary>
    /// <param name="content"></param>
    public LogMessage(string content)
        : this("log", "system", content, LogLevel.Information, DateTime.Now)
    {
    }

    /// <summary>
    /// コンテンツだけのメッセージを作成します
    /// </summary>
    /// <param name="content"></param>
    public LogMessage(string content, LogLevel logLevel)
        : this("log", "system", content, logLevel, DateTime.Now)
    {
    }

    public string LogEntry => $"[{createdAt:yyyy-MM-dd HH:mm:ss}] [{LogLevel}] {Content}";
}

