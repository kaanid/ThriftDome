using ConsoleApp33ThriftService;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.Service
{
    public class ThriftHost : Microsoft.Extensions.Hosting.BackgroundService
    {
        private ILogger<ThriftHost> _log;
        private IConfiguration _config;
        private readonly ThriftServerConfig _thriftServerConfig = null;
        private ISample _sample;

        public ThriftHost(ILogger<ThriftHost> log, IConfiguration config, ISample sample)
        {
            _log = log;
            _config = config;
            _thriftServerConfig = _config.GetSection("ThriftService").Get<ThriftServerConfig>();
            _sample = sample;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _sample.Call();
            _sample.Call();
            //_log.LogInformation("Press any key to stop...");
            _log.LogInformation("ThriftServe start...");
            await ServerStartup.Init<ThriftService, Kaa.ThriftDemo.Service.Thrift.Calculator.AsyncProcessor>(_thriftServerConfig, stoppingToken);
            //await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ServerStartup.Stop(_thriftServerConfig, _log, cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}