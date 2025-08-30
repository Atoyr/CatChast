using Medoz.CatChast.Clients;

namespace Medoz.CatChast.Data;

/// <summary>
/// </summary>
public class TwitchConfig
{
    private readonly DynamicConfig _config;
    // (IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
    public TwitchConfig(DynamicConfig config)
    {
        _config = config;
    }

    private TwitchConfig()
    {
        _config = new();
    }

    public string ClientId => _config.TryGetValue<string>("clientId", out var clientId) ? clientId! : string.Empty;

    public string[] Channels => _config.TryGetValue<string[]>("channels", out var channels) ? channels! : Array.Empty<string>();

    public TwitchOptions ToTwitchOptions(string? token)
        => new TwitchOptions()
        {
            Token = token,
            Channels = Channels
        };
}