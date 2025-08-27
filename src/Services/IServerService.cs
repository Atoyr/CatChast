using System.Reactive.Disposables;

using Medoz.CatChast.Clients;
using Medoz.CatChast.Data;

namespace Medoz.CatChast.Services;

public interface IServerService
{
    Task StartWebApiAsync();
    void StopWebApi();

    event EventHandler<string>? WebApiMessageReceived;
}

