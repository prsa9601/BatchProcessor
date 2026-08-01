using BatchProcessor.Dtos;
using BatchProcessor.Services.Abstractions;
using BatchProcessor.Services.Batching;
using BatchProcessor.Services.DataBase;
using BatchProcessor.Services.Hosted;

namespace BatchProcessor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBatchingServices(this IServiceCollection services)
        {
            // ثبت BatchProcessor به صورت Singleton
            services.AddSingleton<IBatchProcessor<PostLikeDto>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BatchProcessor<PostLikeDto>>>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                return new BatchProcessor<PostLikeDto>(
                    batchSize: 10000,
                    interval: TimeSpan.FromSeconds(5),
                    processBatchAsync: async (items, ct) =>
                    {
                        // ایجاد اسکوپ جدید برای جلوگیری از مشکل DbContext
                        using var scope = scopeFactory.CreateScope();
                        var dbService = scope.ServiceProvider.GetRequiredService<IPostLikeService>();
                        await dbService.BulkLikePostsAsync(items, ct);
                    },
                    logger
                );
            });

            // ثبت سرویس دیتابیس به صورت Scoped (معمول)
            services.AddScoped<IPostLikeService, PostLikeService>();

            // ثبت سرویس Flush برای زمان خاموشی
            services.AddHostedService<BatchProcessorFlushService>();

            return services;
        }
    }
}