using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using kin_stellar_dashboard.Services;
using kin_stellar_dashboard.Services.Impl;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace kin_stellar_dashboard
{
    public class Startup
    {
        private static readonly ILog Logger;
        private readonly IConfigurationRoot Configuration;
        static Startup()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("kin_stellar_dashboard.dll.config"));
            Logger = LogManager.GetLogger(typeof(Startup));
        }
        private Startup(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("_configuration.json", true, true);
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            try
            {
                var startup = new Startup(args);
                await startup.RunAsync();
            }
            catch (Exception e)
            {
               Logger.Error(e.Message, e);
                Console.ReadLine();
                throw;
            }
        }

        private async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            ServiceProvider provider = services.BuildServiceProvider();

            await provider.GetRequiredService<StartupService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(Configuration) 
                .AddSingleton<IDatabaseService, DatabaseService>()
                .AddSingleton<IStellarService, StellarService>()
                .AddSingleton<StartupService>();
        }
    }
}