using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TwitchBotService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBotService" in both code and config file together.
    [ServiceContract]
    public interface IBotService
    {
        [OperationContract]
        int StartUser(string userName, bool hasViewBot, bool hahsIrcBot);

        [OperationContract]
        int StopUser(string userName);

        [OperationContract]
        int StartViewBot(string userName);

        [OperationContract]
        int StopViewBot(string userName);

        [OperationContract]
        int StartIrcBot(string userName);

        [OperationContract]
        int StopIrcBot(string userName);

        [OperationContract]
        User GetUserInfo(string userName);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "TwitchBotService.ContractType".
    [DataContract]
    public class User
    {
        public User(string userName)
        {
            this.UserName = userName;
            this.ViewBot = new ViewBotInfo();
        }

        // Exposed Public Members
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public ViewBotInfo ViewBot { get; set; }

        [DataMember]
        public IrcBotInfo IrcBot { get; set; }

        [DataMember]
        public bool HasStopped { get; set; }
    }

    [DataContract]
    public class ViewBotInfo
    {
        public ViewBotInfo()
        {
            // Nothing needs initializing
        }

        // Internal Public Members
        public int ProcessId { get; set; }

        // Exposed Public Members
        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public bool HasAccess { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public bool Running { get; set; }
    }

    [DataContract]
    public class IrcBotInfo
    {
        public IrcBotInfo()
        {
            // Nothing needs initializing
        }
        // Internal Public Members
        public int ProcessId { get; set; }


        // Exposed Public Members
        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public bool HasAccess { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public bool Running { get; set; }
    }
}
