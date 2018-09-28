
using AspectCore.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.Service
{
    public class InterceptorAttribute : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine("InterceptorAttribute");
            return next(context);
        }
    }
}
