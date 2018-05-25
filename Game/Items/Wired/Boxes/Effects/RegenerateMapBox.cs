namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class RegenerateMapsBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectRegenerateMaps;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public RegenerateMapsBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            StringData = "";
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();
        }

        public bool Execute(params object[] Params)
        {
            if (Instance == null)
            {
                return false;
            }

            var TimeSinceRegen = DateTime.Now - Instance.lastRegeneration;

            if (TimeSinceRegen.TotalMinutes > 1)
            {
                Instance.GetGameMap().GenerateMaps();
                Instance.lastRegeneration = DateTime.Now;
                return true;
            }
            
            return false;
        }
    }
}
