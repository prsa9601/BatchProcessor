using BatchProcessor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BatchProcessor.Test
{
    public class BatchProcessorTest
    {
        private IServiceProvider serviceProvider;
        private AppDbContext context;
        public BatchProcessorTest()
        {
            var service = new ServiceCollection();
            //Set Configuration
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=BatchProcessor;integrated security=true;TrustServerCertificate=True; Trusted_Connection=True;MultipleActiveResultSets=true"
            })
            .AddEnvironmentVariables()
            .Build();
            service.AddSingleton<IConfiguration>(configuration);
            //var fdsa = configuration["ConnectionStrings:Sohabil"];

            // ✅ اصلاح: تنظیم UseSqlServer
            service.AddDbContext<AppDbContext>((provider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });
            serviceProvider = service.BuildServiceProvider();
            context = serviceProvider.GetService<AppDbContext>();

        }

        [Fact]
        public void Test()
        {
            // 1. غیرفعال‌سازی SSL
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var httpClient = new HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromSeconds(10); // افزایش زمان‌بندی

            // 2. سناریو با لاگ خطا
            var scenario = Scenario.Create("stress_test_scenario", async context =>
            {
                var request = Http.CreateRequest("POST",
                    "http://localhost:5225/api/Post/8099a0e5-8109-485d-b0b1-c8ad21d6bb09/like")
                    ////"https://localhost:7121/api/Post/8099a0e5-8109-485d-b0b1-c8ad21d6bb09/like")
                                  .WithHeader("Accept", "application/json");

                var response = await Http.Send(httpClient, request);

                if (!response.IsError)
                    return Response.Ok();
                else
                    return Response.Fail(500, $"Error: {response.Message}");
            })
            .WithLoadSimulations(
                Simulation.KeepConstant(copies: 15, during: TimeSpan.FromSeconds(5))
            );

            // 3. اجرا و نمایش گزارش
            var result = NBomberRunner
                .RegisterScenarios(scenario)
                .Run();

            Console.WriteLine(result.AllRequestCount);
            Console.WriteLine($"RequestCount: {result.AllRequestCount}");
            Console.WriteLine($"AllOkCount: {result.AllOkCount}");
            Console.WriteLine($"FailCount: {result.AllFailCount}");

            // 4. Assert معقول
            Assert.True(result.AllFailCount == 0, "برخی درخواست‌ها با خطا مواجه شدند.");
        }
    }
}
