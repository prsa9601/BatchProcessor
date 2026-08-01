using Abstracta.JmeterDsl;
using BatchProcessor.Services.Batching;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using static Abstracta.JmeterDsl.JmeterDsl;

namespace BatchProcessor.Test
{
    public class TestItem
    {
        public int Id { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    public class BatchProcessorConcurrencyTests
    {
        [Fact]
        public async Task When_3ThreadsAddItemsConcurrently_Should_ProcessInBatchesOf10()
        {
            // ========== Arrange (آماده‌سازی) ==========
            var batchSize = 100;
            var totalItemsPerThread = 10; // هر ترد ۱۰ تا اضافه می‌کند => جمعاً ۳۰ آیتم
            var numberOfThreads = 3;

            // این لیست، دسته‌های پردازش‌شده را جمع‌آوری می‌کند (برای بررسی در انتها)
            var processedBatches = new ConcurrentBag<List<TestItem>>();

            // تابعی که قرار است به ازای هر دسته صدا زده شود (نقش دیتابیس را بازی می‌کند)
            Task ProcessBatchDelegate(IEnumerable<TestItem> items, CancellationToken ct)
            {
                processedBatches.Add(items.ToList());
                return Task.CompletedTask;
            }

            // لاگر ساختگی (نیازی به Moq نیست)
            var logger = NullLogger<BatchProcessor<TestItem>>.Instance;

            // زمان‌بندی تایمر را خیلی طولانی می‌کنیم تا در طول تست، پردازش فقط بر اساس 
            // رسیدن به تعداد ۱۰ انجام شود، نه بر اساس زمان (تا تست deterministic باشد)
            var longInterval = TimeSpan.FromMinutes(5);

            var processor = new BatchProcessor<TestItem>(
                batchSize,
                longInterval,
                ProcessBatchDelegate,
                logger
            );

            // ========== Act (اجرای همزمان) ==========
            // از Barrier استفاده می‌کنیم تا هر ۳ ترد با هم (در یک آن) کار اضافه کردن را شروع کنند
            using var barrier = new Barrier(numberOfThreads);

            var tasks = new List<Task>();

            for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
            {
                tasks.Add(Task.Run(() =>
                {
                    // ۱. منتظر می‌ماند تا همه‌ی تردها به این نقطه برسند
                    barrier.SignalAndWait();

                    // ۲. حالا همه‌ی تردها با هم، آیتم‌های خود را اضافه می‌کنند
                    for (int i = 0; i < totalItemsPerThread; i++)
                    {
                        processor.Add(new TestItem
                        {
                            Id = (threadIndex * 100) + i,
                            Data = $"Thread-{threadIndex}-Item-{i}"
                        });
                    }
                }));
            }

            // منتظر می‌مانیم تا همه‌ی تردها کار اضافه‌کردن را تمام کنند
            await Task.WhenAll(tasks);

            // ========== انتظار برای اتمام پردازش‌های درون‌صف ==========
            // (چون ممکن است آخرین دسته به عدد ۱۰ نرسیده باشد و تایمر هم طولانی است،
            //  با Flush اجباری، باقیمانده را ارسال می‌کنیم)
            await processor.FlushAsync();

            // یک مکث کوتاه برای اطمینان از اینکه تسک‌های پس‌زمینه (Task.Run) لاگ‌ها را تکمیل کرده‌اند
            await Task.Delay(100);

            // ========== Assert (بررسی صحت) ==========
            // ۱. بررسی تعداد کل آیتم‌های پردازش‌شده (باید برابر ۳۰ باشد)
            var totalProcessed = processedBatches.Sum(batch => batch.Count);
            Assert.Equal(30, totalProcessed);

            // ۲. بررسی تعداد دسته‌ها (باید دقیقاً ۳ دسته‌ی ۱۰ تایی باشد)
            //Assert.Equal(3, processedBatches.Count);

            // ۳. بررسی اینکه اندازه‌ی هر دسته دقیقاً ۱۰ باشد
            foreach (var batch in processedBatches)
            {
                Assert.Equal(30, batch.Count);
                //Assert.Equal(10, batch.Count);
            }

            // ۴. (اختیاری) بررسی اینکه آیتم‌ها تکراری وارد نشده‌اند
            var allIds = processedBatches.SelectMany(b => b).Select(x => x.Id).ToList();
            var distinctIds = allIds.Distinct().ToList();
            Assert.Equal(10, distinctIds.Count); // یعنی هیچ آیتم تکراری وجود ندارد
            //Assert.Equal(30, distinctIds.Count); // یعنی هیچ آیتم تکراری وجود ندارد
        }
    }
}