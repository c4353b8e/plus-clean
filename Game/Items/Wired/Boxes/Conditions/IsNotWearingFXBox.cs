namespace Plus.Game.Items.Wired.Boxes.Conditions
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class IsNotWearingFXBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionIsWearingFX;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public IsNotWearingFXBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopInt();

            StringData = Unknown2.ToString();
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

            if (Player.Effects().CurrentEffect != int.Parse(StringData))
            {
                return true;
            }

            return false;
        }
    }
}
