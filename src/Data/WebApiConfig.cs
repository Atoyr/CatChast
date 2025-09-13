using Medoz.CatChast.Clients;
using Medoz.CatChast.Speakers;

namespace Medoz.CatChast.Data;

/// <summary>
/// </summary>
public class WebApiConfig : IConfig
{
    private readonly DynamicConfig _config;
    // (IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
    public WebApiConfig(DynamicConfig config)
    {
        _config = config;
    }

    private WebApiConfig()
    {
        _config = new();
    }

    public uint Port => _config.TryGetValue<uint>("port", out var port) ? port : 8080;

    public bool IsAutoStart => _config.TryGetValue<bool>("autoStart", out var autoStart) ? autoStart : false;
}
