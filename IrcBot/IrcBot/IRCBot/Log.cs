using System;
using System.IO;
using System.Linq;

namespace TwitchBot.IRCBot
{
    public class Log
    {
        public static string filename;
        public static int maxlen;

        public static void StartLogging(string filename, int maxlen)
        {
            Log.filename = filename;
            Log.maxlen = maxlen;
            if (File.Exists(filename))
                File.Delete(filename);
        }

        public static void AddMessage(string message)
        {
            lock (Log.filename) {
                string msg = DateTime.Now.ToString("[HH:mm] ") + message;
                File.AppendAllText(Log.filename, msg + "\n");
                CleanFile();
            }
        }

        public static void AddErrorMessage(string message)
        {
            lock (Log.filename)
            {
                string msg = DateTime.Now.ToString("[HH:mm]") + " *ERROR* " + message;
                File.AppendAllText(Log.filename, msg + "\n");
                CleanFile();
            }
        }

        private static void CleanFile()
        {
            int len = File.ReadAllLines(filename).Length;
            if (len > maxlen)
                File.WriteAllLines(filename, File.ReadAllLines(filename).Skip(len - maxlen).ToArray());
        }
    }
}

