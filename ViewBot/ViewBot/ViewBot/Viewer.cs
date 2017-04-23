using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TwitchBot.ViewBot
{
    public class Viewer
    {
        private Proxy Proxy;
        private DateTime TimeToDie;
        private string Url;
        private Timer _timer;

        public static int AliveViewerCount;
        public static readonly Object ViewerCountLock = new object();
        private Viewer(Proxy proxy, string url)
        {
            Proxy = proxy;
            TimeToDie = DateTime.Now.AddSeconds((StaticRandom.Rand() % (Settings.ViewerMaxLifespan - Settings.ViewerMinLifespan)) + Settings.ViewerMinLifespan);
            Url = url;
            _timer = new Timer(Ping, null, TimeSpan.FromSeconds(0), Timeout.InfiniteTimeSpan);
            
            lock (ViewerCountLock)
            {
                AliveViewerCount++;
            }
        }

        /// <summary>
        /// Creates a viewer
        /// </summary>
        public static void CreateViewer()
        {
            try
            {
                ViewBot.Proxy proxy = ViewBot.Proxy.GetProxy();
                if (proxy == null)
                {
                    return;
                }
                   

                //Console.WriteLine("Got proxy");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.twitch.tv/api/channels/" + Settings.Channel + "/access_token");
                WebProxy myproxy = new WebProxy(proxy.Ip, proxy.Port);
                myproxy.BypassProxyOnLocal = false;
                request.Proxy = myproxy;
                request.Method = "GET";
                request.Timeout = 10000;
                
                request.BeginGetResponse(ar =>
                {
                    try
                    {
                        string responseText = string.Empty;
                        string sig = string.Empty, token = string.Empty;
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar))
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                Console.WriteLine(DateTime.Now + ": Couldn't Get Token/Sig");
                                return;
                            }

                            //Console.WriteLine("Status OK");
                            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                            {
                                responseText = reader.ReadToEnd();
                            }
                        }
                        JObject json = JObject.Parse(responseText);

                        JToken temp;

                        json.TryGetValue("token", out temp);
                        token = (string)temp;
                        json.TryGetValue("sig", out temp);
                        sig = (string)temp;

                        if (string.IsNullOrWhiteSpace(sig) || string.IsNullOrWhiteSpace(token))
                        {
                            Console.WriteLine(DateTime.Now + ": Sig and/or token returned an empty string.");
                            return;
                        }

                        request = (HttpWebRequest)WebRequest.Create("http://usher.twitch.tv/select/" + Settings.Channel + ".json?nauthsig=" + sig + "&nauth=" + token + "&type=any");
                        myproxy = new WebProxy(proxy.Ip, proxy.Port);
                        myproxy.BypassProxyOnLocal = false;
                        request.Proxy = myproxy;
                        request.Method = "GET";
                        request.Timeout = 10000;
                        request.BeginGetResponse(ar2 =>
                        {
                            try
                            {
                                string url = string.Empty;
                                responseText = string.Empty;
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar2))
                                {
                                    if (response.StatusCode != HttpStatusCode.OK)
                                    {
                                        Console.WriteLine(DateTime.Now + ": Couldn't Get URL");
                                        return;
                                    }

                                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                                    {
                                        responseText = reader.ReadToEnd();
                                    }
                                }

                                url = responseText.Split('\n').LastOrDefault(x => !string.IsNullOrWhiteSpace(x));

                                if (string.IsNullOrWhiteSpace(url) || url.Equals("[]"))
                                {
                                    Console.WriteLine(DateTime.Now + ": Error Getting URL, Stream Offline?");
                                    return;
                                }

                                Viewer v = new Viewer(proxy, url);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(DateTime.Now + ": Exception when creating viewer.");
                                return;
                            }
                            
                        }, null);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(DateTime.Now + ": Exception when creating viewer.");
                        return;
                    }
                }, null);
                
            }
            catch (Exception)
            {
               Console.WriteLine(DateTime.Now + ": Exception when creating viewer.");
                return;
            }
            //Console.WriteLine("Viewer Created successfully.");
            return;
        }

        /// <summary>
        /// Sends the heartbeat request to keep our viewer alive
        /// </summary>
        /// <param name="sender"></param>
        public void Ping(object sender)
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                if (TimeToDie < DateTime.Now)
                {
                    CleanUp(false);
                    Console.WriteLine(DateTime.Now + ": Viewer too old, killing.");
                    return;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                WebProxy myproxy = new WebProxy(Proxy.Ip, Proxy.Port);
                myproxy.BypassProxyOnLocal = false;
                request.Proxy = myproxy;
                request.Method = "GET";
                request.Timeout = 10000;
                ServicePointManager.Expect100Continue = false;

                System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                request.BeginGetResponse(ar =>
                {
                    try
                    {
                        string responseText;
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar))
                        {
                            stopwatch.Stop();
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                CleanUp(true);

                                Console.WriteLine(DateTime.Now + ": Error pinging URL");
                                return;

                            }

                            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
                            {
                                responseText = reader.ReadToEnd();
                            }
                            if (string.IsNullOrWhiteSpace(responseText) || !responseText.Contains("#EXTM3U"))
                            {
                                Console.WriteLine(DateTime.Now + ": URL Ping did return expected results");
                                return;
                            }
                        }
                        _timer.Change(stopwatch.Elapsed > TimeSpan.FromSeconds(Settings.ViewerUpdateInterval) ? TimeSpan.FromSeconds(0) : (TimeSpan.FromSeconds(Settings.ViewerUpdateInterval) - stopwatch.Elapsed), Timeout.InfiniteTimeSpan);

                    }
                    catch (Exception)
                    {
                        CleanUp(true);
                        Console.WriteLine(DateTime.Now + ": Exception when pinging URL");
                        return;
                    }
                }, null);
            }
            catch (Exception)
            {
                CleanUp(true);
                Console.WriteLine(DateTime.Now + ": Exception when pinging URL");
                return;
            }
        }

        /// <summary>
        /// Cleans used resources
        /// </summary>
        /// <param name="wasError"></param>
        public void CleanUp(bool wasError)
        {
            Proxy.FreeProxy(Proxy, wasError);
            _timer.Dispose();
            lock (ViewerCountLock)
            {
                AliveViewerCount--;
            }
        }
    }
}
