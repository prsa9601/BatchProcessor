using BatchProcessor.Dtos;
using BatchProcessor.Services.Abstractions;

namespace BatchProcessor.Services.Hosted
{

    public class BatchProcessorFlushService : IHostedService
    {
        private readonly IBatchProcessor<PostLikeDto> _processor;

        public BatchProcessorFlushService(IBatchProcessor<PostLikeDto> processor) => _processor = processor;

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.FlushAsync();
            (_processor as IDisposable)?.Dispose();
        }
    }
}
