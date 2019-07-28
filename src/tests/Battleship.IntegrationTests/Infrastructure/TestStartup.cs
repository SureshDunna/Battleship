using Battleship.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battleship.IntegrationTests.Infrastructure
{
    public class TestStartup
    {
        private readonly Startup _startup;

        public TestStartup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _startup = new Startup(configuration, environment);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _startup.ConfigureServices(services);

            services.AddSingleton(typeof(ServerAssertion));
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IDbUpgradeChecker dbUpgradeChecker)
        {
            _startup.Configure(app, loggerFactory, dbUpgradeChecker);
        }
    }
}
