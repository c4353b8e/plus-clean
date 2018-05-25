namespace Plus.HabboHotel.Users.Messenger
{
    using System;
    using System.Linq;
    using Communication.Packets.Outgoing;
    using GameClients;
    using Relationships;
    using Rooms;

    public class MessengerBuddy
    {
        #region Fields

        public int UserId;
        public bool mAppearOffline;
        public bool mHideInroom;
        public int mLastOnline;
        public string mLook;
        public string mMotto;

        public GameClient client;
        public string mUsername;

        #endregion

        #region Return values

        public int Id => UserId;

        public bool IsOnline => client != null && client.GetHabbo() != null && client.GetHabbo().GetMessenger() != null &&
                                !client.GetHabbo().GetMessenger().AppearOffline;

        public bool InRoom => CurrentRoom != null;

        public Room CurrentRoom { get; set; }
        #endregion

        #region Constructor

        public MessengerBuddy(int UserId, string pUsername, string pLook, string pMotto, int pLastOnline,
                                bool pAppearOffline, bool pHideInroom)
        {
            this.UserId = UserId;
            mUsername = pUsername;
            mLook = pLook;
            mMotto = pMotto;
            mLastOnline = pLastOnline;
            mAppearOffline = pAppearOffline;
            mHideInroom = pHideInroom;
        }

        #endregion

        #region Methods
        public void UpdateUser(GameClient client)
        {
            this.client = client;
            if (client != null && client.GetHabbo() != null)
            {
                CurrentRoom = client.GetHabbo().CurrentRoom;
            }
        }

        public void Serialize(ServerPacket Message, GameClient Session)
        {
            Relationship Relationship = null;

            if(Session != null && Session.GetHabbo() != null && Session.GetHabbo().Relationships != null)
            {
                Relationship = Session.GetHabbo().Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(UserId)).Value;
            }

            var y = Relationship == null ? 0 : Relationship.Type;

            Message.WriteInteger(UserId);
            Message.WriteString(mUsername);
            Message.WriteInteger(1);
            Message.WriteBoolean(!mAppearOffline || Session.GetHabbo().GetPermissions().HasRight("mod_tool") ? IsOnline : false);
            Message.WriteBoolean(!mHideInroom || Session.GetHabbo().GetPermissions().HasRight("mod_tool") ? InRoom : false);
            Message.WriteString(IsOnline ? mLook : "");
            Message.WriteInteger(0); // categoryid
            Message.WriteString(mMotto);
            Message.WriteString(string.Empty); // Facebook username
            Message.WriteString(string.Empty);
            Message.WriteBoolean(true); // Allows offline messaging
            Message.WriteBoolean(false); // ?
            Message.WriteBoolean(false); // Uses phone
            Message.WriteShort(y);
        }

        #endregion
    }
}