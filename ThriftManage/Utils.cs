using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Kaa.ThriftDemo.ThriftManage
{
    public abstract  class Utils
    {
        public static IEnumerable<UnicastIPAddressInformation> LocalIPAddressList()
        {
            var lists = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                  .Select(p => p.GetIPProperties())
                  .SelectMany(p => p.UnicastAddresses)
                  .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address));

            return lists;
        }

        public static IPAddress LocalIPAddress()
        {
            var lists = LocalIPAddressList();
            return lists?.FirstOrDefault()?.Address;
        }

        public static void LocalIPListPrint()
        {
            int i = 0;
            var lists = LocalIPAddressList();

            Console.WriteLine($"LocalIPList:");
            foreach (var ip in lists)
            {   
                Console.WriteLine($"{++i} ip:{ip.Address}");
            }
        }
    }
}
