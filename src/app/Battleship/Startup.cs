using Battleship.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;
using Battleship.Features.Battleship;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog.Extensions.Logging;

namespace Battleship
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters = new List<JsonConverter> { new StringEnumConverter() };
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            services.AddSingleton<IDatabase, Database>();
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            services.AddTransient<IBoardService, BoardService>();
            services.AddTransient<IShipService, ShipService>();
            services.AddTransient<IAttackService, AttackService>();

            services.AddSwaggerGen();

            services.AddHealthChecksUI();
            services.AddHealthChecks()
                .AddSqlServer(_configuration.GetConnectionString("BattleshipDbConnection"), name: "Battleship DB", healthQuery: "select top 1 id from dbo.Board", failureStatus: HealthStatus.Unhealthy);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IDbUpgradeChecker dbUpgradeChecker)
        {
            loggerFactory.AddNLog();

            if (_environment.IsDevelopment() || _environment.IsEnvironment("IntegrationTests"))
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", GetType().GetTypeInfo().Assembly.GetName().Name);
            });

            app.UseMvc();

            dbUpgradeChecker.EnsureDatabaseUpToDate();

            app.UseHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            if (_environment.EnvironmentName.Equals("IntegrationTests") == false)
            {
                app.UseHealthChecksUI();
            }
        }
    }
}
