using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Medoz.CatChast.Data;

/// <summary>
/// アプリケーションの設定
/// </summary>
public class Config
{
    public Config()
    {
    }

    private readonly object _lock = new();

    private Dictionary<string, DynamicConfig> _clients = new Dictionary<string, DynamicConfig>();
    /// <summary>
    /// クライアントの設定
    /// </summary>
    [JsonIgnore]
    public IDictionary<string, DynamicConfig> Clients
    {
        get => _clients;
    }
    [JsonPropertyName("clients")]
    [JsonInclude]
    private Dictionary<string, DynamicConfig> _clientsRow
    {
        get => _clients;
        set => _clients = value;
    }


    private Dictionary<string, DynamicConfig> _speakers = new Dictionary<string, DynamicConfig>();
    /// <summary>
    /// スピーカーの設定
    /// </summary>
    [JsonIgnore]
    public IDictionary<string, DynamicConfig> Speakers
    {
        get => _speakers;
    }
    [JsonPropertyName("speakers")]
    [JsonInclude]
    private Dictionary<string, DynamicConfig> _speakersRow
    {
        get => _speakers;
        set => _speakers = value;
    }

    private DynamicConfig _WebApiConfig = new DynamicConfig();
    /// <summary>
    /// Web APIの設定
    /// </summary>
    [JsonIgnore]
    public WebApiConfig WebApiConfig
    {
        get
        {
            lock (_lock)
            {
                return new WebApiConfig(_WebApiConfig);
            }
        }
    }

    [JsonPropertyName("webApi")]
    [JsonInclude]
    private DynamicConfig _WebApiConfigRow
    {
        get => _WebApiConfig;
        set => _WebApiConfig = value;
    }

    private string _username = "";
    /// <summary>
    /// 自身の名前
    /// </summary>
    [JsonPropertyName("username")]
    [JsonInclude]
    public string Username
    {
        get
        {
            lock (_lock)
            {
                return _username;
            }
        }
        set
        {
            lock (_lock)
            {
                _username = value;
            }
        }
    }

    private string? _icon;
    /// <summary>
    /// 自身のアイコンファイルのパス
    /// </summary>
    [JsonPropertyName("icon")]
    [JsonInclude]
    public string? Icon
    {
        get
        {
            lock (_lock)
            {
                return _icon;
            }
        }
        set
        {
            lock (_lock)
            {
                _icon = value;
            }
        }
    }

    private double _width = 400;
    /// <summary>
    /// ウィンドウの幅
    /// </summary>
    [JsonPropertyName("width")]
    [JsonInclude]
    public double Width
    {
        get
        {
            lock (_lock)
            {
                return _width;
            }
        }
        set
        {
            lock (_lock)
            {
                _width = value;
            }
        }
    }

    private double _height = 800;
    /// <summary>
    /// ウィンドウの高さ
    /// </summary>
    [JsonPropertyName("height")]
    [JsonInclude]
    public double Height
    {
        get
        {
            lock (_lock)
            {
                return _height;
            }
        }
        set
        {
            lock (_lock)
            {
                _height = value;
            }
        }
    }

    public double _x = 100;
    /// <summary>
    /// ウィンドウの左位置
    /// </summary>
    [JsonPropertyName("x")]
    [JsonInclude]
    public double X
    {
        get
        {
            lock (_lock)
            {
                return _x;
            }
        }
        set
        {
            lock (_lock)
            {
                _x = value;
            }
        }
    }

    public double _y = 100;
    /// <summary>
    /// ウィンドウの上位置
    /// </summary>
    [JsonPropertyName("y")]
    [JsonInclude]
    public double Y
    {
        get
        {
            lock (_lock)
            {
                return _y;
            }
        }
        set
        {
            lock (_lock)
            {
                _y = value;
            }
        }
    }

    private MOD_KEY _modKey = MOD_KEY.CONTROL;
    private KEY _key = KEY.ENTER;

    /// <summary>
    /// ホットキーの修飾キー
    /// </summary>
    [JsonIgnore]
    public MOD_KEY ModKey
    {
        get
        {
            lock (_lock)
            {
                return _modKey;
            }
        }
        set
        {
            lock (_lock)
            {
                _modKey = value;
            }
        }
    }

    [JsonPropertyName("modKey")]
    [JsonInclude]
    private string _modKeyRow
    {
        // Jsonのシリアライズ/デシリアライズで使用するためロックはしない
        get => ModKeyExtension.ToString(_modKey);
        set => _modKey = ModKeyExtension.GetModKey(value);
    }

    /// <summary>
    /// ホットキーのキー
    /// </summary>
    [JsonIgnore]
    public KEY Key
    {
        get
        {
            lock (_lock)
            {
                return _key;
            }
        }
        set
        {
            lock (_lock)
            {
                _key = value;
            }
        }
    }

    [JsonPropertyName("key")]
    [JsonInclude]
    private string _keyRow
    {
        // Jsonのシリアライズ/デシリアライズで使用するためロックはしない
        get => KeyExtension.ToString(_key);
        set => _key = KeyExtension.GetKey(value);
    }

    private IEnumerable<string> _applications = new List<string>();
    /// <summary>
    /// アクティブに変更できるアプリケーション一覧
    /// </summary>
    [JsonIgnore]
    public IEnumerable<string> Applications
    {
        get
        {
            lock (_lock)
            {
                return _applications;
            }
        }
    }

    [JsonPropertyName("applications")]
    [JsonInclude]
    private IEnumerable<string> _applicationsRow
    {
        // Jsonのシリアライズ/デシリアライズで使用するためロックはしない
        get => _applications;
        set => _applications = value;
    }
}