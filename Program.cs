using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TweetsCountRealData
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Runner().Execute();
        }
    }

        public static class ServiceProviderFactory
        {
            public static IConfiguration Configuration { get; private set; }

            public static IServiceProvider ServiceProvider { get; }

            static ServiceProviderFactory()
            {
                GetAppSettingsFile();
                ServiceCollection sc = new ServiceCollection();
                ConfigureServices(sc);
                ServiceProvider = sc.BuildServiceProvider();
            }

            private static void ConfigureServices(IServiceCollection services)
            {
                services.AddLogging(configure => configure.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger()));
            }
            private static void GetAppSettingsFile()
            {
                var builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                Configuration = builder.Build();
            }
        }
    }

