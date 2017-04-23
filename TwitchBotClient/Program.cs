using System;
using System.Linq;
using System.Text;
using TwitchBotClient.TwitchBotService;

namespace TwitchBotClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (BotServiceClient client = new BotServiceClient("BasicHttpBinding_IBotService"))
            {
                client.Open();
                WriteMenuOptions();
                while (ProcessUserInput(client, Console.ReadLine()))
                    WriteMenuOptions();
                client.Close();
            }
        }
        /// <summary>
        /// Writes the menu options for the program
        /// </summary>
        private static void WriteMenuOptions()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Manual Options:");
            sb.AppendLine("start {USERNAME} - Initializes a user");
            sb.AppendLine("stop {USERNAME} - Stops and removes user");
            sb.AppendLine("viewbot {USERNAME} {on/off} - Turns viewbot on/off for user");
            sb.AppendLine("ircbot {USERNAME} {on/off} - Turns ircbot on/off for user");
            sb.AppendLine("userinfo {USERNAME} - Retreives info for user");
            sb.AppendLine("exit - Exits the program");
            Console.WriteLine(sb.ToString());
        }

        private static bool ProcessUserInput(BotServiceClient client, string input)
        {
            try
            {
                string[] str = input.Split(new char[] { ' ' });
                if (str.Length == 0)
                {
                    Console.WriteLine("Invalid Input");
                    return true;
                }

                switch (str.First().ToLower())
                {
                    case "start":
                        switch (str.ElementAtOrDefault(1) ?? string.Empty)
                        {
                            case "":
                                Console.WriteLine("Must specify a username.");
                                break;
                            default:
                                Console.WriteLine("Call returned: {0}", client.StartUser(str.ElementAtOrDefault(1), true, true));
                                break;
                        }
                        break;
                    case "stop":
                        switch (str.ElementAtOrDefault(1) ?? string.Empty)
                        {
                            case "":
                                Console.WriteLine("Must specify a username.");
                                break;
                            default:
                                Console.WriteLine("Call returned: {0}", client.StopUser(str.ElementAtOrDefault(1)));
                                break;
                        }
                        break;
                    case "viewbot":
                        switch (str.ElementAtOrDefault(1) ?? string.Empty)
                        {
                            case "":
                                Console.WriteLine("Must specify a username.");
                                break;
                            default:
                                switch ((str.ElementAtOrDefault(2) ?? string.Empty).ToLower())
                                {
                                    case "on":
                                        Console.WriteLine("Call returned: {0}", client.StartViewBot(str.ElementAtOrDefault(1)));
                                        break;
                                    case "off":
                                        Console.WriteLine("Call returned: {0}", client.StopViewBot(str.ElementAtOrDefault(1)));
                                        break;
                                    default:
                                        Console.WriteLine("Must specify on/off.");
                                        break;
                                }
                                break;
                        }
                        break;
                    case "ircbot":
                        switch (str.ElementAtOrDefault(1) ?? string.Empty)
                        {
                            case "":
                                Console.WriteLine("Must specify a username.");
                                break;
                            default:
                                switch ((str.ElementAtOrDefault(2) ?? string.Empty).ToLower())
                                {
                                    case "on":
                                        Console.WriteLine("Call returned: {0}", client.StartIrcBot(str.ElementAtOrDefault(1)));
                                        break;
                                    case "off":
                                        Console.WriteLine("Call returned: {0}", client.StartIrcBot(str.ElementAtOrDefault(1)));
                                        break;
                                    default:
                                        Console.WriteLine("Must specify on/off.");
                                        break;
                                }
                                break;
                        }
                        break;
                    case "userinfo":
                        switch (str.ElementAtOrDefault(1) ?? string.Empty)
                        {
                            case "":
                                Console.WriteLine("Must specify a username.");
                                break;
                            default:
                                User user = client.GetUserInfo(str.ElementAtOrDefault(1));
                                if (user == null) Console.WriteLine("User not found.");
                                else Console.WriteLine("User info - Username: {0} HasStopped: {1} Viewbot [ HasAccess: {2} Enabled: {3} Running: {4} StartTime: {5} EndTime: {6} ] IrcBot [ HasAccess: {7} Enabled: {8} Running: {9} StartTime: {10} EndTime: {11} ]", user.UserName, user.HasStopped, user.ViewBot.HasAccess, user.ViewBot.Enabled, user.ViewBot.Running, user.ViewBot.StartTime, user.ViewBot.EndTime, user.IrcBot.HasAccess, user.IrcBot.Enabled, user.IrcBot.Running, user.IrcBot.StartTime, user.IrcBot.EndTime);
                                break;
                        }
                        break;
                    case "exit":
                        return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("An exception occured when processing user input. Exception Message: {0}\nPress any key to exit.", e.Message));
                Console.ReadLine();
                return false;
            }

        }
    }
}
