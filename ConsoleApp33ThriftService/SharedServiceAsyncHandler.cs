using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using shared.d;

namespace ConsoleApp33ThriftService
{
    public class SharedServiceAsyncHandler : shared.d.SharedService.IAsync
    {
        private static readonly ILogger Logger = new LoggerFactory().AddConsole(LogLevel.Trace).CreateLogger(nameof(SharedServiceAsyncHandler));
        public  Task<SharedStruct> getStructAsync(int key, CancellationToken cancellationToken)
        {
            Logger.LogInformation("GetStructAsync({0})", key);
            return Task.FromResult(new SharedStruct()
            {
                Key = key,
                Value = "GetStructAsync"
            });
        }
    }
}
