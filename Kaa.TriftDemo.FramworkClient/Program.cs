using Kaa.ThriftDemo.Service.Thrift;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Kaa.TriftDemo.FramworkClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //生成类库
            //thrift -r --gen csharp:async,nullable tutorial.thrift

            Task.Run(async ()=> {
                await Run();
            }).Wait();
        }

        static async Task Run()
        {
            try
            {
                
                TTransport transport = new TSocket("localhost", 9090, 20000);
                //TBufferedTransport transport2 = new TBufferedTransport(transport, 2048);
                TProtocol protocol = new TBinaryProtocol(transport);

                transport.Open();

                Calculator.Client client = new Calculator.Client(protocol);
                //var processor = new Calculator.AsyncProcessor(client);
                //THttpTaskAsyncHandler transprt111 = new THttpTaskAsyncHandler(processor);

                Console.WriteLine($"Starting client... host:localhost port:9090");

                int testCount = 1000;
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var m in Enumerable.Range(0, testCount))
                {
                    int sum2 = client.add(1, 1);
                    //int sum =await client.addAsync(1, 1);
                }
                sw.Stop();
                Console.WriteLine($"1 TTransport Execute client.add(1, 1) do:{testCount} ms:{sw.ElapsedMilliseconds}");

                sw.Restart();
                foreach (var m in Enumerable.Range(0, testCount))
                {
                    //int sum2 = client.add(1, 1);
                    int sum = await client.addAsync(1, 1);
                }
                sw.Stop();
                Console.WriteLine($"2 TTransport Execute client.addAsync(1, 1) do:{testCount} ms:{sw.ElapsedMilliseconds}");
                transport.Close();

                TTransport transport2 = new TSocket("localhost", 9090, 100000);
                TBufferedTransport transport22 = new TBufferedTransport(transport2, 2048);
                TProtocol protocol2 = new TBinaryProtocol(transport22);
                transport22.Open();
                Calculator.Client client2 = new Calculator.Client(protocol2);

                sw.Restart();
                foreach (var m in Enumerable.Range(0, testCount))
                {
                    int sum2 = client2.add(1, 1);
                }
                sw.Stop();
                Console.WriteLine($"3 TBufferedTransport Execute client.addA(1, 1) do:{testCount} ms:{sw.ElapsedMilliseconds}");

                sw.Restart();
                foreach (var m in Enumerable.Range(0, testCount))
                {
                    int sum = await client2.addAsync(1, 1);
                }
                sw.Stop();
                Console.WriteLine($"4 TBufferedTransport Execute client.addAsync(1, 1) do:{testCount} ms:{sw.ElapsedMilliseconds}");

                client2.ping();

                //Work work = new Work();
                //work.Op = Operation.DIVIDE;
                //work.Num1 = 1;
                //work.Num2 = 0;
                //try
                //{
                //    int quotient = client.calculate(1, work);
                //    Console.WriteLine("Whoa we can divide by 0");
                //}
                //catch (InvalidOperation io)
                //{
                //    Console.WriteLine("Invalid operation: " + io.Why);
                //}

                transport2.Close();


            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (TTransportException tte)
            {
                Console.WriteLine(tte.Message);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
            }
            catch (IOException ie)
            {
                if(ie.InnerException!=null)
                {
                    if(ie.InnerException is SocketException sex)
                    {
                        Console.WriteLine(sex);
                    }
                }
                Console.WriteLine(ie);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
