using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchBot.IRCBot
{
    public class User
    {
        public string Username;
        public string Password;

        private static List<User> _userList = new List<User>();
        private static List<User>.Enumerator _enumerator;

        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public static void LoadUsers()
        {
            List<string> file = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.txt")).OrderBy(x => Guid.NewGuid()).ToList();
            _userList.Capacity = file.Count;
            foreach (var line in file)
            {
                string []split = line.Split(new char[] {':'}, 2);
                if (split.Length < 2 || string.IsNullOrWhiteSpace(split.ElementAtOrDefault(0)) || string.IsNullOrWhiteSpace(split.ElementAtOrDefault(1))) 
                    continue;

                _userList.Add(new User(split.ElementAtOrDefault(0) ?? string.Empty, split.ElementAtOrDefault(1) ?? string.Empty));
            }
            _enumerator = _userList.GetEnumerator();

            Console.WriteLine(@"Parsed {0} users.", _userList.Count);
        }

        
        public static User GetUser()
        {
            try
            {
                lock (_userList)
                {
                    if (_userList.Count == 0 || !_enumerator.MoveNext())
                    {
                        return null;
                    }
                    return _enumerator.Current;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
