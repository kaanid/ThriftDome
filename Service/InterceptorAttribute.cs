
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.Service
{
    public class InterceptorAttribute : AbstractInterceptorAttribute
    {
        [FromContainer]
        public virtual IThriftServiceStatistics Statistics { set; get; }

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Task.Run(() => {
                Statistics.Add(context.ServiceMethod.Name.ToString());
            });
            //Console.WriteLine($"InterceptorAttribute {DateTime.Now} context.Method:{context.ServiceMethod.Name.ToString()}  next.Method:{next.Method.Name} context.Method:{context.ImplementationMethod.Name.ToString()}");

            return next(context);
        }
    }
}
