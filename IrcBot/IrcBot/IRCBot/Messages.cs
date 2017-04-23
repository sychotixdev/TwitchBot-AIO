using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TwitchBot.IRCBot
{
    public class Messages
    {
        private static List<string> _messageList;
        private static List<string>.Enumerator _messageEnumerator;

        private static List<string> _greetingList;
        private static List<string>.Enumerator _greetingEnumerator;

        static Messages()
        {
            _messageList = new List<string>();
            _greetingList = new List<string>();
        }

        public static void LoadMessages(string configFile)
        {
            _messageList.AddRange(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + configFile + "_messages.txt")).OrderBy(x => Guid.NewGuid()));
            _messageEnumerator = _messageList.GetEnumerator();
            Console.WriteLine(@"Parsed {0} messages.", _messageList.Count);
        }

        public static void LoadGreetings(string configFile)
        {
            _greetingList.AddRange(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + configFile + "_greetings.txt")).OrderBy(x => Guid.NewGuid()));
            _greetingEnumerator = _greetingList.GetEnumerator();
            Console.WriteLine(@"Parsed {0} greetings.", _greetingList.Count);
        }

        
        public static string GetMessage()
        {
            lock (_messageList)
            {
                if (!_messageEnumerator.MoveNext())
                {
                    _messageList = _messageList.OrderBy(x => Guid.NewGuid()).ToList();
                    _messageEnumerator = _messageList.GetEnumerator();
                }
                return _messageEnumerator.Current;
            }
        }

        public static string GetGreeting()
        {
            lock (_greetingList)
            {
                if (!_greetingEnumerator.MoveNext())
                {
                    _greetingList = _greetingList.OrderBy(x => Guid.NewGuid()).ToList();
                    _greetingEnumerator = _greetingList.GetEnumerator();
                }
                return _greetingEnumerator.Current;
            }
        }
    }
}
