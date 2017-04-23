using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.ViewBot;

namespace TwitchBot
{
    class Program
    {
        static void Main(string[] args)
        {
            string configFile;

            if (args.Length < 1)
            {
                Console.WriteLine("Please enter config file name.");
                configFile = Console.ReadLine();
            }
            else
            {
                configFile = args[0];
                Console.WriteLine(string.Format("Running on config: {0}", configFile));
            }

            if (configFile != null)
                Bot.Start(configFile);
        }
    }
}
