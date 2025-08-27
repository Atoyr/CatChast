
using Medoz.CatChast.Clients;
using Medoz.CatChast.Data;

namespace Medoz.CatChast.Services;

public interface IConfigService
{
    void Save();
    void Reload();
    Config GetConfig();
    void SaveConfig();
    event EventHandler? ConfigChanged;

    Secret GetSecret();
    void SaveSecret();

    event EventHandler? SecretChanged;
}
