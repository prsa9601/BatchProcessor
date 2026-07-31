using BatchProcessor.Services.Abstractions;
using System.Collections.Concurrent;

namespace BatchProcessor.Services.Batching
{
    public class BatchProcessor<T> : IBatchProcessor<T>, IDisposable
    {
        private readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly Func<IEnumerable<T>, CancellationToken, Task> _processBatchAsync;
        private readonly CancellationTokenSource _cts = new();
        private readonly TimeSpan _interval;
        private readonly int _batchSize;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private bool _disposed; 


        public BatchProcessor(
              int batchSize,
              TimeSpan interval,
              Func<IEnumerable<T>, CancellationToken, Task> processBatchAsync,
              ILogger<BatchProcessor<T>> logger)
        {
            _batchSize = batchSize;
            _interval = interval;
            _processBatchAsync = processBatchAsync;
            _logger = logger;

            // تایمر برای ارسال دوره‌ای
            _timer = new Timer(async _ => await ProcessQueueAsync(), null, interval, interval);
        }

        public void Add(T item)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BatchProcessor<T>));
            _queue.Enqueue(item);

            // اگر تعداد به آستانه رسید، بلافاصله پردازش کن (اختیاری)
            if (_queue.Count >= _batchSize)
            {
                Task.Run(() => ProcessQueueAsync());
            }
        }

        public async Task FlushAsync()
        {
            await ProcessQueueAsync(force: true);
        }

        private async Task ProcessQueueAsync(bool force = false)
        {
            // جلوگیری از هم‌پوشانی چند پردازش همزمان
            if (!await _processingLock.WaitAsync(0)) return;
             
            try
            {
                var items = new List<T>();
                while (_queue.TryDequeue(out var item))
                {
                    items.Add(item);
                    if (!force && items.Count >= _batchSize)
                        break;
                }

                if (items.Any())
                {
                    _logger.LogInformation("Processing {Count} items", items.Count);
                    await _processBatchAsync(items, _cts.Token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch");
                // در صورت خطا، آیتم‌ها را دوباره به صف برمی‌گردانیم؟ 
                // (بسته به سناریو، می‌توانید آن‌ها را به صف برگردانید یا لاگ کنید)
            }
            finally
            {
                _processingLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cts.Cancel();
            _timer?.Dispose();
            // ارسال باقیمانده‌ها قبل از بستن
            ProcessQueueAsync(force: true).GetAwaiter().GetResult();
            _processingLock.Dispose();
            _cts.Dispose();
        }
    } 
}