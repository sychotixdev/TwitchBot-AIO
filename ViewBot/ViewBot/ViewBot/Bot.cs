using System;
using System.Net;
using System.Threading;

namespace TwitchBot.ViewBot
{
    public class Bot
    {

        private static Timer _updateTimer;
        private static Timer _recreateViewersTimer;

        /// <summary>
        /// Entrypoint for ViewBot
        /// </summary>
        /// <param name="configFile">Location of configuration file</param>
        public static void Start(string configFile)
        {
            Console.WriteLine(ServicePointManager.DefaultConnectionLimit);
            Settings.LoadConfig(configFile);
            Proxy.LoadProxies(configFile);
            Channel.StartUpdater();

            ThreadPool.SetMinThreads(10, 0);

            _updateTimer = new Timer(UpdateTitle, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));

            System.Threading.Tasks.Parallel.For(0, Settings.Viewers, x => Viewer.CreateViewer());

            _recreateViewersTimer = new Timer(UpdateTitle, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));

            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Recreates viewers if they are below our desired viewer count
        /// </summary>
        /// <param name="sender"></param>
        private static void RecreateViewers(object sender)
        {
            _recreateViewersTimer.Change(Timeout.Infinite, Timeout.Infinite);
            int currentViewCount = 0;
            lock (Viewer.ViewerCountLock)
            {
                currentViewCount = Viewer.AliveViewerCount;
            }

            System.Threading.Tasks.Parallel.For(currentViewCount, Settings.Viewers, x => Viewer.CreateViewer());
            _recreateViewersTimer.Change(TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Updates the title of the console to show running statistics
        /// </summary>
        /// <param name="sender"></param>
        private static void UpdateTitle(object sender)
        {
            int currentViewCount = 0;
            lock (Viewer.ViewerCountLock)
            {
                currentViewCount = Viewer.AliveViewerCount;
            }
            Console.Title = string.Format("Channel: {0} TotalViewers: {1} FakeViewers: {2}", Settings.Channel,
                Channel.Viewers, currentViewCount);
        }
    }
}
