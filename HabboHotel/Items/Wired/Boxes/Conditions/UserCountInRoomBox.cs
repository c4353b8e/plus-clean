namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    using System;
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class UserCountInRoomBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionUserCountInRoom;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public UserCountInRoomBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var CountOne = Packet.PopInt();
            var CountTwo = Packet.PopInt();

            StringData = CountOne + ";" + CountTwo;
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

            var CountOne = StringData != null ? int.Parse(StringData.Split(';')[0]) : 1;
            var CountTwo = StringData != null ? int.Parse(StringData.Split(';')[1]) : 50;

            if (Instance.UserCount >= CountOne && Instance.UserCount <= CountTwo)
            {
                return true;
            }

            return false;
        }
    }
}