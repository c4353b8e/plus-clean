namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class IsWearingBadgeBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionIsWearingBadge;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public IsWearingBadgeBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var BadgeCode = Packet.PopString();

            StringData = BadgeCode;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var Player = (Habbo)Params[0];
            if (Player == null)
            {
                return false;
            }

            if (!Player.GetBadgeComponent().GetBadges().Contains(Player.GetBadgeComponent().GetBadge(StringData)))
            {
                return false;
            }

            foreach (var Badge in Player.GetBadgeComponent().GetBadges().ToList())
            {
                if (Badge.Slot <= 0)
                {
                    continue;
                }

                if (Badge.Code == StringData)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
