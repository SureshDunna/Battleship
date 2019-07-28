using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Battleship.Infrastructure.Database;
using DbUp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battleship.IntegrationTests.Infrastructure
{
    public class ServerFixture : IDisposable
    {
        private readonly Action<IConfigurationBuilder> _configuration;
        private readonly int _databasePort;
        public string Url { get; set; }

        private IWebHost _server;
        public HttpClient Client;

        private readonly string _databaseName;
        private readonly Action<IServiceCollection> _services;

        public ServerFixture(Action<IConfigurationBuilder> configuration = null, int databasePort = default(int), string databaseName = null, Action<IServiceCollection> services = null)
        {
            _configuration = configuration;
            _databasePort = databasePort;
            _databaseName = databaseName;
            _services = services;
            StartServer();
        }

        private void StartServer()
        {
            Url = "http://localhost:" + GetNextPort();
            _server = StartMainHost();
            Client = new HttpClient { BaseAddress = new Uri(Url) };
        }

        private IWebHost StartMainHost()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseEnvironment("IntegrationTests")
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                    var databaseName = _databaseName ?? $"BattleshipTest_{uniqueId}";
                    var connectionString = $"Data Source=localhost,{_databasePort};Initial Catalog={databaseName};user id=sa;password=Password01!;MultipleActiveResultSets=true";

                    var configurationValues = new Dictionary<string, string>
                    {
                        ["ConnectionStrings:BattleshipDbConnection"] = connectionString,
                    };

                    configuration.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .AddInMemoryCollection(configurationValues);

                    _configuration?.Invoke(configuration);
                })
                .ConfigureServices(services =>
                {
                    _services?.Invoke(services);
                })
                .ConfigureLogging((context, logger) =>
                {
                    logger.AddConsole();
                    logger.AddConfiguration(context.Configuration.GetSection("Logging"));
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseUrls(Url);

            webHostBuilder.UseStartup<TestStartup>();

            var webHost = webHostBuilder.Build();
            webHost.Start();
            return webHost;
        }

        public ServerArrangement Arrange()
        {
            var database = _server.Services.GetRequiredService<IDatabase>();

            return new ServerArrangement(database);
        }

        public ServerAssertion Assert()
        {
            return _server.Services.GetRequiredService<ServerAssertion>();
        }

        public void Dispose()
        {
            var databaseConfiguration = _server.Services.GetRequiredService<IDatabaseConfiguration>();
            DropDatabase.For.SqlDatabase(databaseConfiguration.BattleshipConnectionString);

            _server.Dispose();
            _server = null;
        }

        // Copied from https://github.com/aspnet/KestrelHttpServer/blob/47f1db20e063c2da75d9d89653fad4eafe24446c/test/Microsoft.AspNetCore.Server.Kestrel.FunctionalTests/AddressRegistrationTests.cs#L508
        private static int GetNextPort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // Let the OS assign the next available port. Unless we cycle through all ports
                // on a test run, the OS will always increment the port number when making these calls.
                // This prevents races in parallel test runs where a test is already bound to
                // a given port, and a new test is able to bind to the same port due to port
                // reuse being enabled by default by the OS.
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}