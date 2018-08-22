using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using shared.d;
using Kaa.ThriftDemo.Service.Thrift;

namespace ConsoleApp33ThriftService
{
    public class ThriftService : Calculator.IAsync
    {
        public Task<int> addAsync(int num1, int num2, CancellationToken cancellationToken)
        {
            return Task.FromResult(num1+num2);
            //return Task.Run(()=> num1 + num2);
        }

        public Task<int> calculateAsync(int logid, Work w, CancellationToken cancellationToken)
        {
            return Task.FromResult(logid);
        }

        public Task<SharedStruct> getStructAsync(int key, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SharedStruct());
        }

        public Task pingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }

        public Task zipAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }
    }
}
