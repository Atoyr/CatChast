using System.CodeDom;

namespace Medoz.CatChast.Clients;

public interface IClientOptions
{
    string this[string key] { get; set; }
}
