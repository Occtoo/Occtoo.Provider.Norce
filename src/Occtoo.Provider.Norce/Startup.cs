using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Occtoo.Provider.Norce;
using Occtoo.Provider.Norce.Formatter;
using Occtoo.Provider.Norce.Services;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Occtoo.Provider.Norce
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOcctooService, OcctooService>();
            services.AddTransient<INorceService, NorceService>();
            services.AddTransient<IBlobService, BlobService>();
            services.AddTransient<IProductFormatter, ProductFormatter>();
            services.AddTransient<IMediaFormatter, MediaFormatter>();
            services.AddTransient<IPriceFormatter, PriceFormatter>();
            services.AddTransient<IStockFormatter, StockFormatter>();
            services.AddTransient<ITableService, TableService>();
            services.AddTransient<ILogService, LogService>();

            services.AddHttpClient<IBlobService, BlobService>((client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            }))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddHttpClient<IOcctooService, OcctooService>(
                (client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                }))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddHttpClient<INorceService, NorceService>(
                (client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                }))
                .AddPolicyHandler(GetRetryPolicy());
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
