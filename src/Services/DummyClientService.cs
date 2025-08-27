using Medoz.CatChast.Messaging;
using Medoz.CatChast.Clients;

using Microsoft.Extensions.Logging;

namespace Medoz.CatChast.Services;

/// <summary>
/// クライアントの管理を行うクラス
/// </summary>
public class DummyClientService : IClientService
{
    private readonly Dictionary<string, ITextClient> _clients = new();

    private readonly string _defaultClient = "_";

    private readonly ILogger _logger;

    /// <summary>
    /// クライアントの管理を行うクラス
    /// </summary>
    /// <param name="asyncEventBus"></param>
    /// <param name="configService"></param>
    public DummyClientService(
        ILogger<DummyClientService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// クライアントの取得
    /// </summary>
    public ITextClient GetClient(string? name)
    {
        throw new ArgumentException($"Client {name} is not registered.");
    }

    public bool TryGetClient(string? name, out ITextClient? client)
    {
        var clientName = string.IsNullOrEmpty(name) ? _defaultClient : name;
        return _clients.TryGetValue(clientName, out client);
    }

    public T GetOrCreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage = null
        ) where T : ITextClient
    {
        if (_clients.TryGetValue(name, out var registeredClient))
        {
            if (registeredClient is T typedClient)
            {
                return typedClient;
            }
            else
            {
                throw new ArgumentException($"Client {name} is already registered with a different type.");
            }
        }

        return CreateClient<T>(options, name, onReceiveMessage);
    }

    public T CreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage = null
        ) where T : ITextClient
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }

        var client = ClientFactory.Create<T>(options);
        _clients.Add(name, client);

        if (onReceiveMessage is null)
        {
        }
        else
        {
            client.OnReceiveMessage += onReceiveMessage;
        }

        client.OnReady += async () =>
        {
            _logger.LogInformation($"{name} client started successfully.");
            await Task.CompletedTask;
        };

        return client;
    }

    /// <summary>
    /// クライアントの登録
    /// </summary>
    public void RegisterClient(string name, ITextClient client)
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }
        _clients.Add(name, client);
    }

    /// <summary>
    /// クライアントの削除
    /// </summary>
    public void RemoveClient(string name)
    {
        if (_clients.ContainsKey(name))
        {
            _clients.Remove(name);
        }
        else
        {
            throw new ArgumentException($"Client {name} is not registered.");
        }
    }
}
