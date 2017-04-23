using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TwitchBot.ViewBot
{
    public class Settings
    {
        private static readonly Regex SectionRegex = new Regex(@"^\[(.+)\]$", RegexOptions.Compiled);

        public static string Channel { get; private set; }
        public static int ChannelInfoUpdateInterval{ get; private set; }

        public static int Viewers { get; private set; }
        public static int ViewerUpdateInterval { get; private set; }
        public static int ViewerMinLifespan { get; private set; }
        public static int ViewerMaxLifespan { get; private set; }
        public static int ViewerTimeBetweenCreation { get; private set; }

        public static int ProxyMaxErrorCount { get; private set; }
        public static bool ShouldRefreshProxies { get; private set; }
        public static int ProxyRefreshTime { get; private set; }
        

        static Settings()
        {
            ChannelInfoUpdateInterval = 30;
            Viewers = 100;
            ViewerUpdateInterval = 10;
            ViewerMinLifespan = 500;
            ViewerMaxLifespan = 1000;
            ViewerTimeBetweenCreation = 100;
            ProxyMaxErrorCount = 10;
            ShouldRefreshProxies = true;
            ProxyRefreshTime = 500;
        }

        /// <summary>
        /// Loads the configuration file
        /// </summary>
        /// <param name="configFile"></param>
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

                if (section == "ViewerSettings")
                {
                    string[] pair = line.Split(new char[] { '=' }, 2);

                    switch (pair[0])
                    {
                        case "UpdateInterval":
                            ViewerUpdateInterval = Convert.ToInt32(pair[1]);
                            break;
                        case "MinLifespan":
                            ViewerMinLifespan = Convert.ToInt32(pair[1]);
                            break;
                        case "MaxLifespan":
                            ViewerMaxLifespan = Convert.ToInt32(pair[1]);
                            break;
                        case "Viewers":
                            Viewers = Convert.ToInt32(pair[1]);
                            break;
                        case "TimeBetweenCreation":
                            ViewerTimeBetweenCreation = Math.Max(Convert.ToInt32(pair[1]), ViewerTimeBetweenCreation);
                            break;
                    }
                }

                if (section == "ChannelSettings")
                {
                    string[] pair = line.Split(new char[] { '=' }, 2);

                    switch (pair[0])
                    {
                        case "Channel":
                            Channel = pair[1];
                            break;
                        case "InfoUpdateInterval":
                            ChannelInfoUpdateInterval = Convert.ToInt32(pair[1]);
                            break;
                    }
                }

                if (section == "ProxySettings")
                {
                    string[] pair = line.Split(new char[] { '=' }, 2);

                    switch (pair[0])
                    {
                        case "MaxErrorCount":
                            ProxyMaxErrorCount = Convert.ToInt32(pair[1]);
                            break;
                        case "ShouldRefreshProxies":
                            ShouldRefreshProxies = Convert.ToBoolean(pair[1]);
                            break;
                        case "ProxyRefreshTime":
                            ProxyRefreshTime = Convert.ToInt32(pair[1]);
                            break;
                    }
                }

            }
        }

    }
}
