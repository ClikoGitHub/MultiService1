using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Net;

namespace MultiService
{
    class Program
    {
        public int IAm = 0;
        static void Main(string[] args)
        {
            // получить ip4 адрес локального хоста 
            IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(ipAddress => ipAddress.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork);
            // создать два базовых адреса для конечных точек (две привязки будем использовать)
            Uri[] baseAddresses = {new Uri(string.Format("net.pipe://{0}/{1}/",ip,Guid.NewGuid().ToString())),
                                  new Uri(string.Format("net.tcp://{0}:{1}/{2}/",ip,System.Configuration.ConfigurationManager.AppSettings["PortNum"],Guid.NewGuid().ToString()))};
            //WhoIsService whoIsService = new WhoIsService();   // такое возможно только для InstanceContextMode.Single
            //whoIsService.IAm=1;

            ServiceHost serviceHost = new ServiceHost(typeof(WhoIsService), baseAddresses); // создать хостинг
            try
            {
                // добавить две конечные точки для net.pipe и net.tcp
                ServiceEndpoint netPipeEndpoint = serviceHost.AddServiceEndpoint(typeof(IWhoIsService), new NetNamedPipeBinding(), string.Empty);
                ServiceEndpoint netTcpEndpoint = serviceHost.AddServiceEndpoint(typeof(IWhoIsService), new NetTcpBinding(SecurityMode.None), string.Empty); // отключенная безопасность


                // добавить возможность обнаружения сервиса путем широковещательного udp запроса
                ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();
                // добавить возможность извещать остальные сервисы о входе/выходе сервиса
                serviceDiscoveryBehavior.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());  // конечная точка для оповещения

                serviceHost.Description.Behaviors.Add(serviceDiscoveryBehavior);
                serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint()); // конечная точка для этого

                serviceHost.Open(); // запустить хост для прослушивания запросов


                Console.WriteLine("WhoIs Service started at {0} под номером {1}", baseAddresses[0], System.Configuration.ConfigurationManager.AppSettings["MyNum"]);
                Console.WriteLine("WhoIs Service started at {0} под номером {1}", baseAddresses[1], System.Configuration.ConfigurationManager.AppSettings["MyNum"]);
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate the service.");
                Console.WriteLine();
                Console.ReadLine();

                serviceHost.Close();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }

            if (serviceHost.State != CommunicationState.Closed)
            {
                Console.WriteLine("Aborting service...");
                serviceHost.Abort();
            }
        }
    }
}
