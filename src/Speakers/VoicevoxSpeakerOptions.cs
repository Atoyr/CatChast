namespace Medoz.CatChast.Speakers;


public class VoicevoxSpeakerOptions : ISpeakerOptions
{
    private readonly Dictionary<string, string> _options = new();

    public uint SpeakerId
    {
        get
        {
            if(!_options.ContainsKey("speaker_id"))
            {
                return 0;
            }
            return uint.Parse(_options["speaker_id"]);
        }
        set => _options["speaker_id"] = value.ToString();
    }

    public string? Url
    {
        get
        {
            if(!_options.ContainsKey("url"))
            {
                return null;
            }
            return _options["url"];
        }

        set
        {
            if(value is null)
            {
                if (_options.ContainsKey("url"))
                {
                    _options.Remove("url");
                }
            }
            else
            {
                _options["url"] = value;
            }
        }
    }

    private const string BINDING_KEY = "binds";

    public IEnumerable<string> BindingKeys
    {
        get
        {
            if (_options.ContainsKey(BINDING_KEY))
            {
                return _options[BINDING_KEY].Split(',');
            }
            return Array.Empty<string>();
        }
        set
        {
            _options[BINDING_KEY] = string.Join(',', value);
        }
    }

    public string this[string key]
    {
        get => _options[key];
        set => _options[key] = value;
    }
}