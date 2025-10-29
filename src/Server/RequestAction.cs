// WebServer.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Medoz.CatChast.Server;

public class RequestAction
{
    private readonly Action<Request> _action;

    private readonly string _route;

    public string Route => _route;

    private RequestAction() { _route = string.Empty; _action = (_) => { }; }

    public RequestAction(string route, Action<Request> action)
    {
        _action = action;
        _route = route;
    }

    public void Invoke(Request request)
    {
        _action(request);
    }
}