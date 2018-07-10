using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Configuration;

namespace kin_stellar_dashboard.Services.Impl
{

    public class DatabaseService : IDatabaseService
    {
        private readonly IConfigurationRoot _config;
        private readonly ILog _logger;
        public DatabaseService(IConfigurationRoot config)
        {
            _config = config;
            _logger = LogManager.GetLogger(typeof(DatabaseService));
        }

        public Task<string> Query(string queryString)
        {
            return Task.FromResult("0");
        }
    }
}
