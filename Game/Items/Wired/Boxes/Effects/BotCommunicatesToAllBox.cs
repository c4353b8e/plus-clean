namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class BotCommunicatesToAllBox: IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotCommunicatesToAllBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotCommunicatesToAllBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var ChatMode = Packet.PopInt();
            var ChatConfig = Packet.PopString();

            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            //this.StringData = ChatConfig.Replace('\t', ';') + ";" + ChatMode;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var User = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (User == null)
            {
                return false;
            }

            //TODO: This needs finishing.


            return true;
        }
    }
}