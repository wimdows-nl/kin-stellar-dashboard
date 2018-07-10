using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Configuration;

namespace kin_stellar_dashboard.Services.Impl
{
    public class StartupService
    {
        private readonly IConfigurationRoot _config;
        private readonly IDatabaseService _databaseService;
        private readonly IStellarService _stellarService;
        private readonly ILog _logger;
        public StartupService(IConfigurationRoot config, IDatabaseService databaseService, IStellarService stellarService)
        {
            _config = config;
            _databaseService = databaseService;
            _stellarService = stellarService;
            _logger = LogManager.GetLogger(typeof(StartupService));
         
        }

        public async Task StartAsync()
        {
            await _stellarService.StartAsync();
        }
    }
}
