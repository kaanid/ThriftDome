using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Thrift;
using Thrift.Transports.Server;
using tutorial.c;

namespace ConsoleApp33ThriftService
{
    public class HttpServerSample
    {
        public void Run(CancellationToken cancellationToken)
        {
            var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                    .Build();

            var host = new WebHostBuilder()
                   .UseConfiguration(config)
                   .UseKestrel()
                   .UseUrls("http://localhost:9090")
                   .UseContentRoot(Directory.GetCurrentDirectory())
                   .UseStartup<Startup>()
                   .Build();

            host.RunAsync(cancellationToken).GetAwaiter().GetResult();
        }

        public class Startup
        {
            public Startup(IHostingEnvironment env)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();
            }

            public IConfigurationRoot Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient<Calculator.IAsync, ThriftService>();
                services.AddTransient<ITAsyncProcessor, Calculator.AsyncProcessor>();
                services.AddTransient<THttpServerTransport, THttpServerTransport>();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env,
                ILoggerFactory loggerFactory)
            {
                app.UseMiddleware<THttpServerTransport>();
            }
        }
    }

}
