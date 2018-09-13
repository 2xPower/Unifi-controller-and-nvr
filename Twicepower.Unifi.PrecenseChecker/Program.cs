using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TwicePower.Unifi.PrecenseChecker
{
    class Program
    {
        internal const string configFileName = "appsettings.json";

        public static readonly string InstalledPath;

        static Program()
        {
            // Build configuration
            var installedPath = new FileInfo(typeof(Program).Assembly.CodeBase).Directory.FullName;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                if (installedPath.Contains(':'))
                {
                    installedPath = installedPath.Split(':')[1];
                }
            }
            else
            {
                var indexOfFile = installedPath.IndexOf("file:", StringComparison.InvariantCultureIgnoreCase);
                if (indexOfFile > 0)
                {
                    installedPath = new Uri(installedPath.Substring(indexOfFile)).AbsolutePath;
                }
            }

            InstalledPath = installedPath;
        }

        public static int Main(string[] args)
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Run app
            return serviceProvider.GetService<App>().Run(args).Result;


        }


        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            serviceCollection.AddSingleton<ILoggerFactory>(new LoggerFactory()
                .AddConsole()
                .AddSerilog()
                .AddDebug());
            serviceCollection.AddLogging();
            
            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.RollingFile($"{Path.Combine(InstalledPath, typeof(Program).Namespace)}.-{{Date}}.log", Serilog.Events.LogEventLevel.Debug)
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            serviceCollection.AddTransient<Microsoft.Extensions.Logging.ILogger>((provider) => { return provider.GetService<ILoggerFactory>().CreateLogger("console app"); });

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(typeof(IConfigurationRoot), (serviceProvider) => { return GetConfigFromFile(serviceProvider); });

            // Add app
            serviceCollection.AddTransient<App, App>();
        }

        private static IConfigurationRoot GetConfigFromFile(IServiceProvider serviceProvider)
        {
            
            var configFilePath = Path.Combine(InstalledPath, "appsettings.json");

           serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger>().LogInformation($"using config at {configFilePath}");

            //if (!File.Exists(configFilePath))
            //    throw new Exception("appsettings.json not found");

            return new ConfigurationBuilder()
                    .SetBasePath(InstalledPath)
                    .AddJsonFile(configFileName, optional: true, reloadOnChange: false)
                    //.AddUserSecrets<Program>()
                    .Build();
        }


    }
}
