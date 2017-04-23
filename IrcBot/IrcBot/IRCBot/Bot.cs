using System;
using System.Collections.Generic;
using System.Threading;

namespace TwitchBot.IRCBot
{
    public class Bot
    {
        public static void Start(string configName)
        {
            Settings.LoadConfig(configName);
            Messages.LoadMessages(configName);
            Messages.LoadGreetings(configName);
            User.LoadUsers();
            
            
            using (CountdownEvent chatterCountdownEvent = StartTasks(Settings.ChatterCount, true))
            using (CountdownEvent lurkerCountdownEvent = StartTasks(Settings.LurkerCount, false))
            {
                bool usersEmpty = false;
                while (true)
                {
                    if (chatterCountdownEvent.CurrentCount > 1)
                        chatterCountdownEvent.Wait(250);
                    else if (lurkerCountdownEvent.CurrentCount > 1)
                        chatterCountdownEvent.Wait(250);
                    else break;

                    if (!usersEmpty)
                    {
                        while (!usersEmpty && chatterCountdownEvent.CurrentCount < chatterCountdownEvent.InitialCount)
                        {
                            User u = User.GetUser();
                            if (u == null)
                                usersEmpty = true;
                            else
                            {
                                CountdownEvent temp = chatterCountdownEvent;
                                ThreadPool.QueueUserWorkItem(_ => new Connection().Create(u, temp, true));
                                chatterCountdownEvent.AddCount();
                            }
                        }
                        while (usersEmpty && lurkerCountdownEvent.CurrentCount < lurkerCountdownEvent.InitialCount)
                        {
                            User u = User.GetUser();
                            if (u == null)
                                usersEmpty = true;
                            else
                            {
                                CountdownEvent temp = lurkerCountdownEvent;
                                ThreadPool.QueueUserWorkItem(_ => new Connection().Create(u, temp, true));
                                lurkerCountdownEvent.AddCount();
                            }
                        }
                    }
                }
                    
            }
            Console.WriteLine("All threads finished. Press enter to continue.");
            Console.Read();
        }

        static CountdownEvent StartTasks(int count, bool shouldChat)
        {
            CountdownEvent countDown = new CountdownEvent(Settings.ChatterCount+1);

            for (int i = 0; i < count; i++)
            {
                User user = User.GetUser();
                if (user == null)
                {
                    countDown.Signal();
                }
                else ThreadPool.QueueUserWorkItem(_ => new Connection().Create(user, countDown, shouldChat));
                Thread.Sleep(TimeSpan.FromSeconds(Settings.TimeBetweenConnections));
            }
            return countDown;
        }
    }
}

