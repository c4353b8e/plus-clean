namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;
    using Users.Authenticator;

    internal class GiveUserBadgeBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectGiveUserBadge;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public GiveUserBadgeBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Badge = Packet.PopString();

            StringData = Badge;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                return false;
            }

            var Owner = HabboFactory.GetHabboById(Item.UserID);
            if (Owner == null || !Owner.GetPermissions().HasRight("room_item_wired_rewards"))
            {
                return false;
            }

            var Player = (Habbo)Params[0];
            if (Player == null || Player.GetClient() == null)
            {
                return false;
            }

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            if (Player.GetBadgeComponent().HasBadge(StringData))
            {
                Player.GetClient().SendPacket(new WhisperComposer(User.VirtualId, "Oops, it appears you have already recieved this badge!", 0, User.LastBubble));
            }
            else
            {
                Player.GetBadgeComponent().GiveBadge(StringData, true, Player.GetClient());
                Player.GetClient().SendNotification("You have recieved a badge!");
            }

            return true;
        }
    }
}