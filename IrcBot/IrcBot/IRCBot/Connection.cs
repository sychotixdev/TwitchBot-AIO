using System;
using System.Net.Sockets;
using System.Threading;
using Meebey.SmartIrc4net;

namespace TwitchBot.IRCBot
{
    public class Connection
    {
        private readonly IrcClient _irc = new IrcClient();
        private Timer _timer;
        private User _user;
        private CountdownEvent _countdownEvent;
        private bool _waitingOnMessage;
        private bool _hasGreeted;
        private bool _shouldDisconnect;

        public void Create(User user, CountdownEvent countdownEvent, bool shouldChat)
        {
            _user = user;
            _countdownEvent = countdownEvent;

            _irc.Encoding = System.Text.Encoding.UTF8;
            _irc.SendDelay = 1000;
            _irc.ActiveChannelSyncing = true;
            _irc.AutoReconnect = true;
            _irc.AutoRejoinOnKick = true;
            _irc.AutoRetry = true;
            _irc.AutoRetryDelay = 30000;
            _irc.AutoRelogin = true;

            _irc.OnError += new ErrorEventHandler(OnError);
            _irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

            try
            {
                _irc.Connect(Settings.Server, Settings.Port);
            }
            catch (Exception e)
            {
                Log.AddErrorMessage(e.Message);
                _countdownEvent.Signal();
                System.Environment.Exit(1);
            }

            try
            {
                _irc.Login(_user.Username, _user.Username, 0, _user.Username, _user.Password);

                foreach (var channel in Settings.Channels)
                    _irc.RfcJoin(channel);

                if (shouldChat)
                    _timer = new Timer(streamTimer, null, 
                        TimeSpan.FromSeconds(Math.Max(Settings.UserInitialWait + (StaticRandom.Rand() % (Settings.UserChatVariance * 2)) - Settings.UserChatVariance, 0)),
                        TimeSpan.FromSeconds(Math.Max(Settings.UserChatInterval + (StaticRandom.Rand() % (Settings.UserChatVariance * 2)) - Settings.UserChatVariance, 0)));

                while (!_shouldDisconnect)
                    _irc.ListenOnce();

                
            }
            catch (Exception e)
            {
                Log.AddErrorMessage(e.Message);
                //System.Environment.Exit(2);
            }
            finally
            {
                _countdownEvent.Signal();
                _irc.Disconnect();
            }

        }

        private void OnConnectionError(object sender, EventArgs e)
        {
            Console.WriteLine("Connection Error! ");
        }

        private void OnRawMessage(object sender, IrcEventArgs e)
        {
            if (_waitingOnMessage)
            {
                if (_irc.IsMe(e.Data.Nick) && e.Data.Message != null)
                {
                    _waitingOnMessage = false;
                }
                Log.AddMessage(e.Data.Message); 
            }
            
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Log.AddErrorMessage(e.ErrorMessage);
            _shouldDisconnect = true;
        }

        private void streamTimer(object sender)
        {
            //if (_waitingOnMessage)
            //{
            //    Log.AddErrorMessage(string.Format("User {0} tried to send another message before the last one was recieved. He will stop sending messages.", _user.Username));
            //    _timer.Dispose();
            //    _shouldDisconnect = true;
            //    return;
            //}
            string msg;
            if (!_hasGreeted)
            {
                msg = Messages.GetGreeting();
                _hasGreeted = true;
            }
            else
                msg = Messages.GetMessage();
            foreach (var channel in Settings.Channels)
            {
                _waitingOnMessage = true;
                _irc.SendMessage(SendType.Message, channel, msg);
            }
        }
    }
}

