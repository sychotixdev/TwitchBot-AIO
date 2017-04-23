using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TwitchBotService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class BotService : IBotService
    {
        public int StartUser(string userName, bool hasViewBot, bool hasIrcBot)
        {
            return Manager.StartUser (userName, hasViewBot, hasIrcBot);
        }

        public int StopUser(string userName)
        {
            return Manager.StopUser(userName);
        }

        public int StartViewBot(string userName)
        {
            return Manager.StartViewBot (userName);
        }

        public int StopViewBot(string userName)
        {
            return Manager.StopViewBot(userName);
        }

        public int StartIrcBot(string userName)
        {
            return Manager.StartIrcBot(userName);
        }

        public int StopIrcBot(string userName)
        {
            return Manager.StopIrcBot(userName);
        }

        public User GetUserInfo(string userName)
        {
            return Manager.GetUserInfo (userName);
        }
    }
}
