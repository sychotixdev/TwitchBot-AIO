using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TwitchBotService
{
    public static class Manager
    {
        private static ConcurrentDictionary<string, User> _userDict = new ConcurrentDictionary<string, User>();

        private static readonly string ViewBotPath = "ViewBot\\Viewbot.exe";
        private static readonly string IrcBotPath = "IrcBot\\IrcBot.exe";

        #region Public Static Methods

        /// <summary>
        /// Attempts to start a user
        /// </summary>
        /// <param name="userName">Name of user to start</param>
        /// <returns>
        /// 0 : On Success
        /// -1 : On user already exists
        /// -2 : Failed to add to dictionary
        /// -99 : Unhandled exception occured
        /// </returns>
        public static int StartUser (string userName, bool hasViewBot, bool hasIrcBot)
        {
            try
            {
                if (_userDict.ContainsKey(userName))
                    return -1;

                User user = new User(userName);

                user.ViewBot = new ViewBotInfo();
                user.ViewBot.HasAccess = hasViewBot;

                user.IrcBot = new IrcBotInfo();
                user.IrcBot.HasAccess = hasIrcBot;

                if (!_userDict.TryAdd(userName, user))
                    return -2;

                return 0;
            }
            catch (Exception)
            {
                return -99;
            }
        }

        /// <summary>
        /// Attempts to stop a started user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 0 : On Success
        /// -1 : User was not in list
        /// </returns>
        public static int StopUser(string userName)
        {
            try
            {
                User user;
                if (!_userDict.TryRemove(userName, out user))
                    return -1;

                // Only need to lock the user and change the HasStopped value to true
                lock (user)
                {
                    user.HasStopped = true;
                }

                StopViewBot(user);
                StopIrcBot(user);

                return 0;
            }
            catch (Exception)
            {
                return -99;
            }
        }

        /// <summary>
        /// Attempts to start the viewbot
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 0 : On Success
        /// -1 : User does not exist
        /// -2 : User does not have access
        /// -3 : Viewbot already running
        /// -4 : Failed to start process
        /// -5 : User has been stopped
        /// </returns>
        public static int StartViewBot(string userName)
        {
            User user;
            if (!_userDict.TryGetValue(userName, out user))
                return -1;

            lock (user)
            {
                if (user.HasStopped)
                    return -5;
                    
                if (!user.ViewBot.HasAccess)
                    return -2;

                GetUserInfo(userName);

                if (user.ViewBot.Enabled && user.ViewBot.Running)
                    return -3;

                Process proc;
                if (!StartProcess(ViewBotPath, user.UserName, out proc) || proc == null)
                    return -4;

                user.ViewBot.Enabled = true;
                user.ViewBot.ProcessId = proc.Id;
                user.ViewBot.StartTime = DateTime.Now;
                user.ViewBot.Running = true;

                return 0;
            }
        }

        /// <summary>
        /// Attempts to start the viewbot
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 0 : On Success
        /// -1 : User does not exist
        /// -2 : User does not have access
        /// -3 : Viewbot already running
        /// -4 : Failed to start process
        /// -5 : User has been stopped
        /// </returns>
        public static int StartIrcBot(string userName)
        {
            User user;
            if (!_userDict.TryGetValue(userName, out user))
                return -1;

            lock (user)
            {
                if (user.HasStopped)
                    return -5;

                if (!user.IrcBot.HasAccess)
                    return -2;

                GetUserInfo(userName);

                if (user.IrcBot.Enabled && user.IrcBot.Running)
                    return -3;

                Process proc;
                if (!StartProcess(IrcBotPath, user.UserName, out proc) || proc == null)
                    return -4;

                user.IrcBot.Enabled = true;
                user.IrcBot.ProcessId = proc.Id;
                user.IrcBot.StartTime = DateTime.Now;
                user.IrcBot.Running = true;

                return 0;
            }
        }

        /// <summary>
        /// Attempts to stop the viewbot of the given user
        /// </summary>
        /// <param name="userName">Username to search for</param>
        /// <returns>
        /// 0 : On Success
        /// -1 : User not found in list
        /// (See private StopViewBot for more returns)
        /// </returns>
        public static int StopViewBot(string userName)
        {
            User user;
            if (!_userDict.TryGetValue(userName, out user))
                return -1;

            return StopViewBot(user);
        }

        /// <summary>
        /// Attempts to stop the ircbot of the given user
        /// </summary>
        /// <param name="userName">Username to search for</param>
        /// <returns>
        /// 0 : On Success
        /// -1 : User not found in list
        /// (See private StopIrcBot for more returns)
        /// </returns>
        public static int StopIrcBot(string userName)
        {
            User user;
            if (!_userDict.TryGetValue(userName, out user))
                return -1;

            return StopIrcBot(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// 
        /// </returns>
        public static User GetUserInfo(string userName)
        {
            User user;
            if (!_userDict.TryGetValue(userName, out user))
                return null;

            lock (user)
            {
                if (user.ViewBot.Enabled)
                {
                    try
                    {
                        // Update 
                        Process proc = Process.GetProcessById(user.ViewBot.ProcessId);
                        user.ViewBot.Running = true;
                    }
                    catch (Exception)
                    {
                        user.ViewBot.Running = false;
                        user.ViewBot.ProcessId = 0;
                    }
                }
                if (user.IrcBot.Enabled)
                {
                    try
                    {
                        // Update 
                        Process proc = Process.GetProcessById(user.IrcBot.ProcessId);
                        user.IrcBot.Running = true;
                    }
                    catch (Exception)
                    {
                        user.IrcBot.Running = false;
                        user.IrcBot.ProcessId = 0;
                    }
                }
                return user;
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Attempts to kill a process given the process id
        /// </summary>
        /// <param name="processId">Id of process to kill</param>
        /// <returns>True if process was killed</returns>
        private static bool KillProcess(int processId)
        {
            try
            {
                Process proc = Process.GetProcessById(processId);
                if (proc == null)
                    return false;

                proc.Kill();

                return true;
            }
            catch (Exception)
            {
                // Unhandled exception. Do not throw any exceptions.
                return false;
            }
        }

        /// <summary>
        /// Attempts to start a process given the processPath
        /// </summary>
        /// <param name="processPath">Path of process to start</param>
        /// <returns>Process created. Null if unsuccessful</returns>
        private static bool StartProcess(string processPath, string parameters, out Process proc)
        {
            proc = null;
            try
            {
                proc = Process.Start(processPath, parameters);
                if (proc == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to stop the viewbot for the given user
        /// This method exists because StopUser needs the functionality, but has already removed user from the list
        /// </summary>
        /// <param name="user">User to attempt to stop the viewbot</param>
        /// <returns>
        /// 0 : On Success
        /// -2 : 
        /// -99 : Unhandled Exception
        /// </returns>
        private static int StopViewBot(User user)
        {
            try
            {
                lock (user)
                {

                    if (user.ViewBot.Enabled && (user.ViewBot.ProcessId == 0 || KillProcess(user.ViewBot.ProcessId)))
                    {
                        // Perform cleanup
                        user.ViewBot.Enabled = false;
                        user.ViewBot.EndTime = DateTime.Now;
                        user.ViewBot.ProcessId = 0;
                        user.ViewBot.Running = false;
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return -99;
            }
        }

        /// <summary>
        /// Attempts to stop the viewbot for the given user
        /// This method exists because StopUser needs the functionality, but has already removed user from the list
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// 0 : On Success
        /// -99 : Unhandled Exception
        /// </returns>
        private static int StopIrcBot(User user)
        {
            try
            {
                lock (user)
                {
                    if (user.IrcBot.Enabled && (user.ViewBot.ProcessId == 0 || KillProcess(user.IrcBot.ProcessId)))
                    {
                        // Perform cleanup
                        user.IrcBot.Enabled = false;
                        user.IrcBot.EndTime = DateTime.Now;
                        user.IrcBot.ProcessId = 0;
                        user.IrcBot.Running = false;
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return -99;
            }
        }

        #endregion
    }
}
