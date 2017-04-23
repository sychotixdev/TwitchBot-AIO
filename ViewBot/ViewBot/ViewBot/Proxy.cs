using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TwitchBot.ViewBot
{
    public class Proxy
    {
        private static List<Proxy> _proxyList = new List<Proxy>();
        private static Timer _timer;


        public string Ip { get; private set; }
        public int Port { get; private set; }

        private DateTime _timeOfDeath;
        private int _errorCount;
        private bool _isUsed;
       
        public Proxy(string ip, int port)
        {
            Ip = ip;
            Port = port;

            _timeOfDeath = DateTime.MinValue;
        }

        /// <summary>
        /// Loads the proxy list from proxies.txt
        /// </summary>
        /// <param name="configFile">Location of proxies file</param>
        public static void LoadProxies(string configFile)
        {
            string[] file = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "proxies.txt"));
            foreach (var s in file)
            {
                string [] proxy = s.Split(new [] {':'}, 2);
                if (proxy.Length == 2 && !string.IsNullOrWhiteSpace(proxy[0]) && !string.IsNullOrWhiteSpace(proxy[1]))
                    _proxyList.Add(new Proxy(proxy[0], Convert.ToInt32(proxy[1])));
            }
            _proxyList = _proxyList.OrderBy(x => Guid.NewGuid()).ToList();
            Console.WriteLine(@"Parsed {0} proxies.", _proxyList.Count);

            if (Settings.ShouldRefreshProxies)
                _timer = new Timer(RefreshProxies, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Settings.ProxyRefreshTime));
        }

        
        /// <summary>
        /// Retrieves the next usable proxy from the list
        /// </summary>
        /// <returns></returns>
        public static Proxy GetProxy()
        {
            lock (_proxyList)
            {
                Proxy p = _proxyList.FirstOrDefault(x => !x._isUsed && x._errorCount < Settings.ProxyMaxErrorCount);
                if (p != null)
                    p._isUsed = true;
                return p;
            }
        }

        /// <summary>
        /// Frees up the proxy for another use if no error occured
        /// </summary>
        /// <param name="proxy">Which proxy is being freed</param>
        /// <param name="wasError">True if freed due to error</param>
        public static void FreeProxy(Proxy proxy, bool wasError)
        {
            lock (_proxyList)
            {
                Proxy p = _proxyList.FirstOrDefault(x => x._isUsed && x.Ip == proxy.Ip && proxy.Port == x.Port);
                if (p != null)
                {
                    p._isUsed = false;
                    if (wasError)
                    {
                        p._errorCount++;
                        if (p._errorCount > Settings.ProxyMaxErrorCount)
                            p._timeOfDeath = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes dead proxies for another attempt
        /// </summary>
        /// <param name="sender"></param>
        public static void RefreshProxies(object sender)
        {
            lock (_proxyList)
            {
                foreach (var proxy in _proxyList.Where(x => x._timeOfDeath != DateTime.MinValue && x._timeOfDeath.AddSeconds(Settings.ProxyRefreshTime) < DateTime.Now))
                {
                    proxy._errorCount = 0;
                    proxy._isUsed = false;
                    proxy._timeOfDeath = DateTime.MinValue;
                }
            }
        }

    }
}
