using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kaa.ThriftDemo.Service.Thrift;

namespace Kaa.ThriftDemo.Framework.Server
{
    public class ThriftService : Calculator.ISync,Calculator.IAsync
    {
        public int add(int num1, int num2)
        {
            return num1 + num2;
        }

        public Task<int> addAsync(int num1, int num2)
        {
            return Task.FromResult(num1 + num2);
        }

        public int calculate(int logid, Work w)
        {
            return logid;
        }

        public Task<int> calculateAsync(int logid, Work w)
        {
            return Task.FromResult(logid);
        }

        public void ping()
        {
            
        }

        public Task pingAsync()
        {
            return Task.FromResult(1);
        }

        public void zip()
        {
            throw new NotImplementedException();
        }

        public Task zipAsync()
        {
            throw new NotImplementedException();
        }
    }
}
