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

public record Request(
    int? Id,
    string? Type,
    string? ClientName,
    string? Message,
    string? Value
);

public record Response(
    bool Success,
    string Message,
    DateTime Timestamp
);

internal class WebApi : IDisposable
{
    private HttpListener? _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly List<RequestAction> _requestActions = new();

    public bool AllowExternalAccess { get; set; } = false;

    public WebApi()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void RegisterDataReceivedAction(RequestAction action)
    {
        // 既存のアクションに追加
        if (_requestActions.Any(a => a.Route == action.Route))
        {
            throw new InvalidOperationException($"Route '{action.Route}' is already registered.");
        }

        _requestActions.Add(action);
    }

    public async Task StartAsync(int port)
    {
        try
        {
            if (_listener is not null && _listener.IsListening)
                return;

            _listener = new HttpListener();
            // FIXME: 外部からのアクセスを許可する場合は、http://*:{port}/ などに変更
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();

            await ProcessRequestsAsync(_cancellationTokenSource.Token);
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    private async Task ProcessRequestsAsync(CancellationToken cancellationToken)
    {
        if (_listener is null || !_listener.IsListening)
            return;

        while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                // 非同期でリクエストを処理（UIをブロックしない）
                _ = Task.Run(() => ProcessRequest(context), cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                // サーバーが停止された場合
                break;
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995)
            {
                // サーバーが停止された場合
                break;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("リクエストの処理中にエラーが発生しました", ex);
            }
        }
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            // CORS対応
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // OPTIONSリクエスト（プリフライト）への対応
            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            if (request.HttpMethod == "POST" && _requestActions.Any(a => a.Route == request.Url?.AbsolutePath))
            {
                var action = _requestActions.First(a => a.Route == request.Url?.AbsolutePath);
                await HandlePostRequest(request, response, action);
            }
            // else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/")
            // {
            //     // TODO
            //     // Swagger
            // }
            else
            {
                // 404 Not Found
                response.StatusCode = 404;
                var errorResponse = JsonSerializer.Serialize(new Response(
                    false,
                    "エンドポイントが見つかりません",
                    DateTime.Now
                ));
                await WriteJsonResponse(response, errorResponse);
            }
        }
        catch
        {
            response.StatusCode = 500;
            var errorResponse = JsonSerializer.Serialize(new Response(
                false,
                "内部サーバーエラー",
                DateTime.Now
            ));
            await WriteJsonResponse(response, errorResponse);
        }
    }

    private async Task HandlePostRequest(HttpListenerRequest request, HttpListenerResponse response, RequestAction action)
    {
        try
        {
            // リクエストボディを読み取り
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
            var jsonString = await reader.ReadToEndAsync();

            // JSONをデシリアライズ
            var apiRequest = JsonSerializer.Deserialize<Request>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiRequest is null)
            {
                response.StatusCode = 400;
                var errorResponse = JsonSerializer.Serialize(new Response(
                    false,
                    "不正なリクエスト形式です",
                    DateTime.Now
                ));
                await WriteJsonResponse(response, errorResponse);
                return;
            }

            // WPFのUIスレッドで処理結果を表示
            System.Windows.Application.Current.Dispatcher.Invoke(() => action.Invoke(apiRequest));

            // 成功レスポンスを返す
            var successResponse = JsonSerializer.Serialize(new Response(
                true,
                "データを正常に受信しました",
                DateTime.Now
            ));

            response.StatusCode = 200;
            await WriteJsonResponse(response, successResponse);
        }
        catch
        {
            response.StatusCode = 400;
            var errorResponse = JsonSerializer.Serialize(new Response(
                false,
                "不正なJSON形式です",
                DateTime.Now
            ));
            await WriteJsonResponse(response, errorResponse);
        }
    }

    public void Stop()
    {
        if (_listener is null || !_listener.IsListening)
            return;

        _cancellationTokenSource.Cancel();
        _listener?.Stop();
        _listener?.Close();
    }

    private static async Task WriteJsonResponse(HttpListenerResponse response, string json)
    {
        response.ContentType = "application/json; charset=UTF-8";
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource.Dispose();
        _listener?.Close();
        _listener = null;
    }
}

