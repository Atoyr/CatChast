using Medoz.CatChast.Clients;
using Medoz.CatChast.Speakers;

namespace Medoz.CatChast.Data;

/// <summary>
/// </summary>
public class VoicevoxConfig : IConfig
{
    private readonly DynamicConfig _config;
    // (IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
    public VoicevoxConfig(DynamicConfig config)
    {
        _config = config;
    }

    private VoicevoxConfig()
    {
        _config = new();
    }

    public uint SpeakerId => _config.TryGetValue<uint>("SpeakerId", out var speakerId) ? speakerId : 0;

    string? Url => _config.TryGetValue<string>("Url", out var url) ? url : null;

    public VoicevoxSpeakerOptions ToVoicevoxSpeakerOptions()
        => new VoicevoxSpeakerOptions()
        {
            SpeakerId = SpeakerId,
            Url = Url,
            // FIXME: BindingKeysの取得
            BindingKeys = ["_", "default"],
        };
}
