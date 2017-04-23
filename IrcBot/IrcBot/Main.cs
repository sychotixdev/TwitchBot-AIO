using System;

namespace TwitchBot
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            try
            {
                IRCBot.Log.StartLogging("streambot.log", 1000);
                string configName;
                if (args.Length < 1)
                {
                    Console.WriteLine("Please enter user settings name: ");
                    configName = Console.ReadLine();
                }
                else
                {
                    configName = args[0];
                    Console.WriteLine(string.Format("Running on config: {0}", configName));
                }
                

                if (!string.IsNullOrWhiteSpace(configName)) 
                    IRCBot.Bot.Start(configName);
            }
        	catch (Exception e)
        	{
            	IRCBot.Log.AddErrorMessage(e.Message);  
            }
        }
    }
}
