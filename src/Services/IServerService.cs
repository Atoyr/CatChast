using Medoz.CatChast.Server;

namespace Medoz.CatChast.Services;

public interface IServerService
{
    Task StartWebApiAsync(uint? port = null);
    void StopWebApi();

    void RegisterRequestAction(RequestAction action);
}

