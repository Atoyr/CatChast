using Medoz.CatChast.Services;
using Medoz.CatChast.Clients;
using Medoz.CatChast.Data;
using Medoz.CatChast.Auth;
using Medoz.CatChast.Messaging;
using Microsoft.Extensions.Logging;

namespace Medoz.CatChast.Command;

public class TwitchCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start twitch client";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;
    private readonly IAsyncEventBus _asyncEventBus;
    private readonly ILogger _logger;

    public TwitchCommand_Start(
        IClientService clientService,
        IConfigService configService,
        IAsyncEventBus asyncEventBus,
        ILogger logger)
    {
        _configService = configService;
        _clientService = clientService;
        _asyncEventBus = asyncEventBus;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        // FIXME: TwitchClientの生成プロセスが複雑なので修正する
        var config = _configService.GetConfig();
        var twitchClientConfig = config.Clients.TryGetValue<TwitchConfig>("twitch", out var clientConfig) ? clientConfig : null;

        if (twitchClientConfig == null)
        {
            _logger.LogError("Twitch client configuration not found.");
            return;
        }

        var oauth = new TwitchOAuthWithImplicit(new TwitchOAuthOptions(twitchClientConfig.ClientId ?? "", 53919));
        var token = await oauth.AuthorizeAsync();
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            _logger.LogError("Failed to get Twitch OAuth token.");
            return;
        }

        var twitchClient = _clientService.GetOrCreateClient<TwitchTextClient>(
            twitchClientConfig.ToTwitchOptions(token.AccessToken),
            "twitch",
            async message =>
            {
                try
                {
                    await _asyncEventBus.PublishAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while publishing message from Twitch client.");
                }
            });

        _ = Task.Run(async () =>
        {
            try
            {
                await twitchClient.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twitch client crashed unexpectedly.");
            }
        });
        await Task.CompletedTask;
    }
}
