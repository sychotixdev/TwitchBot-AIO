using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwitchBot.IRCBot
{
    public class Settings
    {
        private static readonly Regex SectionRegex = new Regex(@"^\[(.+)\]$", RegexOptions.Compiled);

        public static string Server { get; private set; }
        public static int Port { get; private set; }
        public static List<string> Channels { get; private set; }

        public static int UserInitialWait { get; private set; }
        public static int UserChatInterval { get; private set; }
        public static int UserChatVariance { get; private set; }

        public static int ChatterCount { get; private set; }
        public static int LurkerCount { get; private set; }

        public static bool ShouldGreet { get; private set; }

        public static int TimeBetweenConnections { get; private set; }

        static Settings()
        {
            Channels = new List<string>();
        }

        public static void LoadConfig(string configFile)
        {
            string[] file = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + configFile + "_settings.txt"));
            string section = String.Empty;
            string tempParse = string.Empty;
            foreach (var line in file)
            {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                if (SectionRegex.IsMatch(line))
                {
                    section = SectionRegex.Match(line).Groups[1].Value;
                    continue;
                }

                if (section == "ConnectionSettings")
                {
                    string[] pair = line.Split(new char[] {'='}, 2);

                    switch (pair[0])
                    {
                        case "server":
                            Server = pair[1];
                            break;
                        case "port":
                            Port = Convert.ToInt32(pair[1]);
                            break;
                    }
                }

                if (section == "Channels")
                {
                    Channels.Add(line.ToLower());
                }

                if (section == "BotSettings")
                {
                    string[] pair = line.Split(new char[] { '=' }, 2);

                    switch (pair[0])
                    {
                        case "userInitialWait":
                            UserInitialWait = Convert.ToInt32(pair[1]);
                            break;
                        case "userChatInterval":
                            UserChatInterval = Convert.ToInt32(pair[1]);
                            break;
                        case "userChatVariance":
                            UserChatVariance = Convert.ToInt32(pair[1]);
                            break;
                        case "chatterCount":
                            ChatterCount = Convert.ToInt32(pair[1]);
                            break;
                        case "lurkerCount":
                            LurkerCount = Convert.ToInt32(pair[1]);
                            break;
                        case "shouldGreet":
                            ShouldGreet = Convert.ToBoolean(pair[1]);
                            break;
                        case "timeBetweenConnection":
                            TimeBetweenConnections = Math.Max(Convert.ToInt32(pair[1]), 300);
                            break;

                    }
                }

            }
            Console.WriteLine(@"Parsed {0} config file.", Channels.Count);
        }

    }
}

