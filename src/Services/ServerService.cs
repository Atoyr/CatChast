using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Medoz.CatChast.Clients;
using Medoz.CatChast.Data;
using Medoz.CatChast.Server;

using Discord.Rest;

namespace Medoz.CatChast.Services;

internal class ServerService : IServerService, IDisposable
{

    private readonly WebApi _webApi;

    private readonly int basePort = 22222;

    public ServerService()
    {
        _webApi = new WebApi();
    }

    public ServerService(WebApi webApi)
    {
        _webApi = webApi;
    }

    public async Task StartWebApiAsync(uint? port = null)
    {
        if (port.HasValue)
        {
            if (port.Value <= 0 || port.Value > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port number must be between 1 and 65535.");
            }
            await _webApi.StartAsync((int)port.Value);
            return;
        }
        await _webApi.StartAsync(basePort);
    }
    public void StopWebApi()
    {
        _webApi.Stop();
    }

    public void RegisterRequestAction(RequestAction action)
    {
        _webApi.RegisterDataReceivedAction(action);
    }

    public void Dispose()
    {
        _webApi.Dispose();
        // Dispose resources if any
    }
}
