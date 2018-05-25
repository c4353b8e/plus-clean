namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class GiveRewardBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectGiveReward;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public GiveRewardBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item; 
            SetItems = new ConcurrentDictionary<int, Item>();

            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }
        }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            var Unknown = Packet.PopInt();
            var Time = Packet.PopInt();
            var Message = Packet.PopString();

            //this.StringData = Time + ";" + Message;
        }

        public bool Execute(params object[] Params)
        {

            return true;
        }
    }
}