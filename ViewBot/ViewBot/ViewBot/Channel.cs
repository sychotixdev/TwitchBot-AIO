using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace TwitchBot.ViewBot
{
    class Channel
    {
        public static int Viewers { get; set; }
        private static Timer _timer;
        /// <summary>
        /// Starts the channel updater
        /// </summary>
        public static void StartUpdater()
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = new Timer(Update, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Settings.ChannelInfoUpdateInterval));
        }

        /// <summary>
        /// Pulls information from the target channel
        /// </summary>
        /// <param name="sender"></param>
        public static void Update(object sender)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/streams/" + Settings.Channel);
                request.Method = "GET";
                string responseText = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return;
                    }

                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                    {
                        responseText = reader.ReadToEnd();
                    }
                    if (string.IsNullOrWhiteSpace(responseText))
                    {
                        return;
                    }

                    JObject jobject = JObject.Parse(responseText);
                    JToken token = jobject["stream"];

                    if (token != null)
                        Viewers = (int)(token["viewers"] ?? 0);
                    else
                    {
                        Console.WriteLine("Error getting viewercount.");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception when updating channel info. Channel Offline?");
            }
            
        }
    }
}
