🚀 BatchProcessor - پردازش دسته‌ای هوشمند برای دات‌نت
همه‌ی درخواست‌ها رو جمع‌وجور کن، بعد با یه تیر دو نشان بزن!
این پروژه یه Batch Processor فوق‌العاده برای برنامه‌های دات‌نت (مخصوصاً APIها) هست که درخواست‌های هم‌رنگ (مثل لایک‌های پست) رو جمع‌آوری می‌کنه و به‌صورت دسته‌ای پردازش می‌کنه تا فشار روی دیتابیس رو کم کنه و سرعت رو بالا ببره.

📦 ویژگی‌های کلیدی
پردازش دسته‌ای خودکار: درخواست‌ها رو توی صف می‌ریزه و وقتی تعداد به حد نصاب (مثلاً ۱۰۰۰۰) رسید یا زمان مشخص (مثلاً ۵ ثانیه) سپری شد، یک‌جا پردازش می‌کنه.

مقیاس‌پذیر و سبک: با ConcurrentQueue و SemaphoreSlim برای جلوگیری از تداخل هم‌زمان.

قابل تنظیم: سایز بسته و بازه‌ی زمانی رو خودت تعیین می‌کنی.

امن در برابر خطا: خطاها رو لاگ می‌کنه و در صورت نیاز می‌تونی آیتم‌ها رو به صف برگردونی.

Flush خودکار در خاموشی: وقتی برنامه بسته می‌شه، باقی‌مونده‌ی صف رو پردازش می‌کنه تا داده‌ای از دست نره.

تست‌پذیری: همراه با تست بار با NBomber برای شبیه‌سازی فشار بالا.

🛠️ تکنولوژی‌ها
.NET Core / .NET 6+

Entity Framework Core (SQL Server)

NBomber (برای تست بار)

Dependency Injection, Logging, Hosted Services

🚀 شروع سریع
۱. پیش‌نیازها
.NET SDK (نسخه ۶ یا بالاتر)

SQL Server (یا هر دیتابیس سازگار با EF Core)

۲. کلون کردن پروژه
bash
git clone https://github.com/your-username/BatchProcessor.git
cd BatchProcessor
۳. تنظیم رشته اتصال
فایل appsettings.json رو باز کن و ConnectionStrings:DefaultConnection رو با اطلاعات دیتابیس خودت پر کن.

۴. اجرای مهاجرت‌ها (Migration)
bash
dotnet ef database update
۵. اجرای پروژه
bash
dotnet run --launch-profile https
یا با http:

bash
dotnet run --launch-profile http
حالا API روی پورت‌های 5225 (http) و 7121 (https) در دسترسه. می‌تونی از Swagger هم استفاده کنی:
https://localhost:7121/swagger

🧩 ساختار پروژه
text
BatchProcessor/
├── Controllers/
│   └── PostController.cs          # مدیریت درخواست‌های لایک و ایجاد پست
├── Services/
│   ├── Abstractions/              # اینترفیس‌ها
│   ├── Batching/
│   │   └── BatchProcessor.cs      # هسته‌ی پردازش دسته‌ای (جنریک)
│   ├── DataBase/
│   │   ├── IPostLikeService.cs
│   │   └── PostLikeService.cs     # عملیات Bulk Insert
│   └── Hosted/
│       └── BatchProcessorFlushService.cs  # Flush خودکار در خاموشی
├── Data/
│   └── AppDbContext.cs            # DbContext
├── Entities/
│   ├── Post.cs
│   └── PostLike.cs
├── Dtos/
│   └── PostLikeDto.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # ثبت سرویس‌ها
├── Test/
│   └── BatchProcessorTest.cs      # تست بار با NBomber
└── appsettings.json
🔧 نحوه استفاده
ثبت سرویس در Program.cs
csharp
builder.Services.AddBatchingServices();
اضافه کردن آیتم به صف (در کنترلر)
csharp
[HttpPost("{postId}/like")]
public IActionResult LikePost(Guid postId)
{
    var likeDto = new PostLikeDto { PostId = postId, UserId = Guid.NewGuid(), LikedAt = DateTime.UtcNow };
    _likeProcessor.Add(likeDto);  // آیتم وارد صف می‌شه
    return Accepted("درخواست لایک با موفقیت در صف قرار گرفت.");
}
تنظیمات پردازشگر (در AddBatchingServices)
csharp
new BatchProcessor<PostLikeDto>(
    batchSize: 10000,                      // حداکثر تعداد در هر بسته
    interval: TimeSpan.FromSeconds(5),    // بازه‌ی زمانی ارسال
    processBatchAsync: async (items, ct) => { ... }, // متد پردازش
    logger
);
📊 تست بار (NBomber)
برای شبیه‌سازی فشار بالا روی API لایک، تست BatchProcessorTest رو اجرا کن:

bash
dotnet test
این تست با ۲۰ کاربر هم‌زمان به مدت ۶۵ ثانیه درخواست لایک می‌فرسته و نتیجه رو نمایش می‌ده. اگر همه‌ی درخواست‌ها با موفقیت پردازش بشن، تست سبز می‌شه.

💡 نکات مهم
مدیریت DbContext: برای جلوگیری از مشکل DbContext در سرویس‌های Singleton، از IServiceScopeFactory استفاده شده تا هر بار اسکوپ جدیدی ساخته بشه.

خطا و بازیابی: در صورت بروز خطا در پردازش، آیتم‌ها برنمی‌گردن به صف (مگر اینکه خودت پیاده‌سازی کنی). ولی لاگ کامل ثبت می‌شه.

مقیاس‌پذیری: برای بارهای خیلی سنگین، می‌تونی batchSize و interval رو تنظیم کنی یا حتی از چند پردازشگر موازی استفاده کنی.

🤝 مشارکت
اگر ایده‌ی بهبود داری، خوشحال می‌شیم Pull Request بفرستی.
قبلش یه Issue باز کن تا درباره‌ش حرف بزنیم.

📄 لایسنس
این پروژه تحت لایسنس MIT منتشر شده.

حالا برو و با خیال راحت هزاران لایک رو یک‌جا پردازش کن! 😉
