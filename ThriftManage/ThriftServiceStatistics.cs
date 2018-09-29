using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ThriftServiceStatistics : IThriftServiceStatistics
    {
        private const int UPDATACOUNT = 100;
        private static object objLock = new object();
        public readonly Dictionary<string, CountModel> dict;
        private readonly ThriftServerConfig  _config;
        private readonly ILogger<ThriftServiceStatistics> _log;
        public ThriftServiceStatistics(IConfiguration config, ILogger<ThriftServiceStatistics> log)
        {
            dict = new Dictionary<string, CountModel>();
            _config = config.GetSection("ThriftService").Get<ThriftServerConfig>();
            _log = log;
        }

        public void Add(string methodName)
        {
            CountModel model = null;
            if (!dict.ContainsKey(methodName))
            {
                lock (objLock)
                {
                    if (!dict.ContainsKey(methodName))
                    {
                        model = new CountModel { Count = 1 };
                        dict.Add(methodName, model);
                    }
                }
            }

            if (model == null)
            {
                model = dict[methodName];
                model.Count++;
                dict[methodName] = model;
            }


            if (_config.Consul != null && model.Count > model.NextUpdate)
            {
                model.NextUpdate = model.NextUpdate/UPDATACOUNT< 100? model.NextUpdate + UPDATACOUNT: model.NextUpdate + UPDATACOUNT*2;
               
                Task.Run(async () => {
                    await UpdateConsulKv(methodName, model);
                });
            }

        }

        private async Task UpdateConsulKv(string methodName, CountModel model)
        {
            _log.LogTrace($"UpdateConsulKv methodName:{methodName} model:{model.Count} next:{model.NextUpdate}");
            //更新
            using (var manage = new ConsulManage(_config.GetConsulUri()))
            {
                var flag = await manage.AddKVServiceMethod(_config.Name,_config.Port, methodName, model.Count.ToString());
            }
        }
    }

    public class CountModel
    {
        public long Count { set; get; }
        public long NextUpdate { set; get; }
    }
}
