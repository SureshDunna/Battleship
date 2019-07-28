using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace Battleship
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Build().Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();

                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .ConfigureServices((services) =>
                {
                    services.AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc("v1",
                        new Info
                        {
                            Title = "Battleship",
                            Version = "1.0"
                        });

                        options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Battleship.xml"));
                    });
                });
    }
}
