using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace TwitchBotHost
{
    class Program
    {
        private static void Main(string[] args)
        {
            Uri u1 = new Uri("http://localhost:8733/Design_Time_Addresses/TwitchBotService/BotService/");

            Uri u2 = new Uri("http://localhost:8733/Design_Time_Addresses/TwitchBotService/BotService/mex");
            BasicHttpBinding binding = new BasicHttpBinding();
            ServiceHost host = new ServiceHost(typeof(TwitchBotService.BotService), u1);
            ServiceMetadataBehavior meta = new ServiceMetadataBehavior();
            meta.HttpGetEnabled = true;
            host.AddServiceEndpoint(typeof(TwitchBotService.IBotService), binding, u1);
            host.Description.Behaviors.Add(meta);
            host.Open();
            Console.WriteLine("Service is runing.Press Any key to stop");
            Console.ReadKey();
            host.Close();
        }
    }
}
