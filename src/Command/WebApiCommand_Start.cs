using Medoz.CatChast.Speakers;
using Medoz.CatChast.Data;
using Medoz.CatChast.Services;

using Microsoft.Extensions.Logging;
namespace Medoz.CatChast.Command;

public class WebApiCommand_Start : ICommand
{
    public string CommandName => "start";
    public string HelpText => "start webapi Server.";

    private readonly IConfigService _configService;
    private readonly IServerService _serverService;
    private readonly ILogger _logger;


    public WebApiCommand_Start(
        IConfigService configService,
        IServerService serverService,
        ILogger logger
        )
    {
        _configService = configService;
        _serverService = serverService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        var config = _configService.GetConfig();
        await _serverService.StartWebApiAsync(config.WebApiConfig.Port);

        _logger.LogInformation("WebApi server started.");
        await Task.CompletedTask;
    }
}
