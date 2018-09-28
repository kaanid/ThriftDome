using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transports;
using Thrift.Transports.Client;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class TSocketClientTransportSupportDns : TSocketClientTransport
    {
        private string _hostName;

        public TSocketClientTransportSupportDns(IPAddress host, int port, int timeout=0,string hostName = null) : base(host, port, timeout)
        {
            _hostName = hostName;
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }

            if (IsOpen)
            {
                throw new TTransportException(TTransportException.ExceptionType.AlreadyOpen, "Socket already connected");
            }

            if (Port <= 0)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot open without port");
            }

            if (TcpClient == null)
            {
                throw new InvalidOperationException("Invalid or not initialized tcp client");
            }

            if (_hostName == null)
            {
                await TcpClient.ConnectAsync(Host, Port);
            }
            else
            {
                await TcpClient.ConnectAsync(_hostName, Port);
            }

            InputStream = TcpClient.GetStream();
            OutputStream = TcpClient.GetStream();
        }


    }
}
