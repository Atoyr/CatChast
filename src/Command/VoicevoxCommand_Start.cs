using Medoz.CatChast.Speakers;
using Medoz.CatChast.Data;
using Medoz.CatChast.Services;

using Microsoft.Extensions.Logging;
namespace Medoz.CatChast.Command;

public class VoicevoxCommand_Start : ICommand
{
    public string CommandName => "start";
    public string HelpText => "[textClientName?] start voicevox client for textClient.";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;
    private readonly ISpeakerService _speakerService;
    private readonly ILogger _logger;


    public VoicevoxCommand_Start(
        IClientService clientService,
        IConfigService configService,
        ISpeakerService speakerService,
        ILogger logger
        )
    {
        _configService = configService;
        _clientService = clientService;
        _speakerService = speakerService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length <= 1;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        string? clientName = null;
        if (args.Length > 0)
        {
            clientName = args[0];
        }

        var config = _configService.GetConfig();
        DynamicConfig? clientConfig;
        config.Clients.TryGetValue("voicevox", out clientConfig);
        if (clientConfig == null)
        {
            _logger.LogError($"Voicevox client {clientName ?? ""} config not found.");
            return;
        }
        var voicevoxSpeackerOptions = new VoicevoxConfig(clientConfig).ToVoicevoxSpeakerOptions();

        _speakerService.GetOrCreateSpeaker<VoicevoxSpeaker>(
            voicevoxSpeackerOptions,
            clientName ?? "_");

        await Task.CompletedTask;
    }
}