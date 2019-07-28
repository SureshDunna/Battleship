using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Battleship.IntegrationTests.Infrastructure
{
    public class DatabaseFixture : IDisposable
    {
        private static readonly string ExeSuffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
        private string _pathToDockerExecutable;
        private string _containerName;
        private bool _useExistingContainer;
        public int Port { get; private set; }
        private const int LocalSqlServerPort = 1433;
        public bool IsDockerPresent => !string.IsNullOrWhiteSpace(_pathToDockerExecutable);

        public DatabaseFixture()
        {
            if (IsSqlServerInstalledLocally())
            {
                Port = LocalSqlServerPort;
                return;
            }

            // Currently Windows Server 2016 doesn't support linux containers
            if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR"), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            SetDockerLocation();

            if (IsDockerPresent)
            {
                Start();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public ServerFixture CreateServerFixture(Action<IConfigurationBuilder> configuration = null, Action<IServiceCollection> services = null)
        {
            return new ServerFixture(databasePort: Port, configuration: configuration, services: services);
        }

        private void Start()
        {
            _useExistingContainer = TryGetExistingContainerPort(_pathToDockerExecutable, out var port);
            if (_useExistingContainer)
            {
                Port = port;
                return;
            }

            _containerName = "test-mssql-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            Port = GetNextPort();

            RunDockerProcess($"run -d --rm --name {_containerName} -e \"ACCEPT_EULA=Y\" -e \"SA_PASSWORD=Password01!\" -p {Port}:1433 microsoft/mssql-server-windows-express");
            Thread.Sleep(TimeSpan.FromSeconds(200)); // it takes a while for containers to warm up
        }

        private void Stop()
        {
            if (_useExistingContainer || IsDockerPresent == false)
            {
                return;
            }

            RunDockerProcess($"stop {_containerName}");
        }

        private static bool IsSqlServerInstalledLocally()
        {
            try
            {
                var connectionString = $"Data Source=localhost,{LocalSqlServerPort};user id=sa;password=Password01!";
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetDockerLocation()
        {
            foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
            {
                var candidate = Path.Combine(dir, "docker" + ExeSuffix);
                if (File.Exists(candidate))
                {
                    _pathToDockerExecutable = candidate;
                }
            }
        }

        private int RunDockerProcess(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pathToDockerExecutable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };

            var output = new StringBuilder();
            var error = new StringBuilder();
            process.OutputDataReceived += (_, outLine) =>
            {
                if (string.IsNullOrEmpty(outLine.Data))
                {
                    return;
                }
                output.AppendLine(outLine.Data);
            };
            process.ErrorDataReceived += (_, outLine) =>
            {
                if (string.IsNullOrEmpty(outLine.Data))
                {
                    return;
                }
                error.AppendLine(outLine.Data);
            };

            var exitCode = 0;
            process.Exited += (_, __) => exitCode = process.ExitCode;

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit(15000);

            Console.WriteLine($"Process[{process.StartInfo.FileName} {process.StartInfo.Arguments}]\r\nOutput: {output}\r\nError: {error}");

            return exitCode;
        }

        private bool TryGetExistingContainerPort(string dockerFilePath, out int port)
        {
            const string containerName = "battleship-mssql";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = dockerFilePath,
                    Arguments = "port " + containerName,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();

            if (process.WaitForExit(5000))
            {
                const string sqlPort = "1433";
                foreach (var line in process.StandardOutput.ReadToEnd().Split('\r', '\n'))
                {
                    if (line.StartsWith(sqlPort + "/"))
                    {
                        var exposedPortString = line.Substring(line.LastIndexOf(':') + 1);
                        if (int.TryParse(exposedPortString, out int exposedPort))
                        {
                            port = exposedPort;
                            return true;
                        }
                    }
                }
            }

            port = 0;
            return false;
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
