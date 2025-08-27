namespace Medoz.CatChast.Clients;

public interface ITextClient: IClient
{
    event Func<ClientMessage, Task>? OnReceiveMessage;
    Task SendMessageAsync(ClientMessage message);
}