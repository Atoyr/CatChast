using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Medoz.CatChast.Messaging;

public sealed class QueuedAsyncEventBus : IAsyncEventBus, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Type, IEventPipe> _pipes = new();
    private readonly CancellationTokenSource _cts = new();
    // 満杯時の方針
    public enum OverflowPolicy { Wait, DropNewest, DropOldest }

    private readonly int _capacity;
    private readonly OverflowPolicy _policy;

    public QueuedAsyncEventBus(int capacity = 256, OverflowPolicy policy = OverflowPolicy.Wait)
    {
        _capacity = capacity;
        _policy = policy;
    }

    public IDisposable Subscribe<T>(Action<T> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        var pipe = (EventPipe<T>)_pipes.GetOrAdd(typeof(T), _ => new EventPipe<T>(_capacity, _policy, _cts.Token));
        var wrappedHandler = new Func<T, Task>(item => Task.Run(() => handler(item)));
        return pipe.Subscribe(wrappedHandler);
    }

    public IDisposable SubscribeAsync<T>(Func<T, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        var pipe = (EventPipe<T>)_pipes.GetOrAdd(typeof(T), _ => new EventPipe<T>(_capacity, _policy, _cts.Token));
        return pipe.Subscribe(handler);
    }


    public Task PublishAsync<T>(T eventData)
    {
        if (eventData is null) throw new ArgumentNullException(nameof(eventData));
        var pipe = (EventPipe<T>)_pipes.GetOrAdd(typeof(T), _ => new EventPipe<T>(_capacity, _policy, _cts.Token));

        // 呼び出し元はブロックさせない方針
        pipe.TryPost(eventData);
        return Task.CompletedTask;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        if (_pipes.TryGetValue(typeof(T), out var pipe))
        {
            ((EventPipe<T>)pipe).Unsubscribe(new Func<T, Task>(item => Task.Run(() => handler(item))));
        }
    }

    public void UnsubscribeAsync<T>(Func<T, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        if (_pipes.TryGetValue(typeof(T), out var pipe))
        {
            ((EventPipe<T>)pipe).Unsubscribe(new Func<T, Task>(item => Task.Run(() => handler(item))));
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        var tasks = _pipes.Values.Select(p => p.CompleteAsync()).ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _cts.Dispose();
    }

    // -------- 内部実装 --------

    private interface IEventPipe
    {
        Task CompleteAsync();
    }

    private sealed class EventPipe<T> : IEventPipe
    {
        private readonly Channel<T> _channel;
        private readonly List<Func<T, Task>> _handlers = new();
        private readonly object _gate = new();
        private readonly CancellationToken _token;
        private readonly Task _worker;

        public EventPipe(int capacity, QueuedAsyncEventBus.OverflowPolicy policy, CancellationToken token)
        {
            _token = token;

            // Channel 設定（背圧/ドロップ方針）
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = policy switch
                {
                    QueuedAsyncEventBus.OverflowPolicy.DropNewest => BoundedChannelFullMode.DropWrite,
                    QueuedAsyncEventBus.OverflowPolicy.DropOldest => BoundedChannelFullMode.DropOldest,
                    _ => BoundedChannelFullMode.Wait
                }
            };
            _channel = Channel.CreateBounded<T>(options);

            // 単一コンシューマ・ワーカー
            _worker = Task.Run(WorkerAsync);
        }

        public IDisposable Subscribe(Func<T, Task> handler)
        {
            lock (_gate)
            {
                _handlers.Add(handler);
            }
            return new SubscriptionToken(() =>
            {
                lock (_gate)
                {
                    _handlers.Remove(handler);
                }
            });
        }

        public void Unsubscribe(Func<T, Task> handler)
        {
            lock (_gate)
            {
                _handlers.Remove(handler);
            }
        }

        public bool TryPost(T item)
        {
            // 呼び出し元を待たせない：TryWrite。Wait 方針でも Publish 側は待たない。
            return _channel.Writer.TryWrite(item);
        }

        private async Task WorkerAsync()
        {
            try
            {
                var reader = _channel.Reader;
                while (await reader.WaitToReadAsync(_token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var item))
                    {
                        List<Func<T, Task>> handlers;
                        lock (_gate)
                        {
                            handlers = _handlers.ToList();
                        }

                        // 「イベント1件」→「登録ハンドラを順に」→ 完了してから次のイベントへ
                        foreach (var h in handlers)
                        {
                            try
                            {
                                await h(item).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                // TODO: ログ出力に置き換え
                                System.Diagnostics.Debug.WriteLine($"[EventPipe<{typeof(T).Name}>] handler error: {ex}");
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EventPipe<{typeof(T).Name}>] worker error: {ex}");
            }
        }

        public async Task CompleteAsync()
        {
            _channel.Writer.TryComplete();
            try { await _worker.ConfigureAwait(false); } catch { /* swallow */ }
        }
    }

    private sealed class SubscriptionToken : IDisposable
    {
        private readonly Action _dispose;
        private int _disposed;
        public SubscriptionToken(Action dispose) => _dispose = dispose;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0) _dispose();
        }
    }
}
