namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Utilities;

    internal class ToggleFurniBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectToggleFurniState;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public int TickCount { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public int Delay { get => _delay;
            set { _delay = value; TickCount = value; } }
        public string ItemsData { get; set; }

        private long _next;
        private int _delay;
        private bool Requested;

        public ToggleFurniBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            SetItems.Clear();
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                {
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
                }
            }

            var Delay = Packet.PopInt();
            this.Delay = Delay;
        }

        public bool Execute(params object[] Params)
        {
            if (_next == 0 || _next < UnixUtilities.GetNowMilliseconds())
            {
                _next = UnixUtilities.GetNowMilliseconds() + Delay;
            }


            Requested = true;
            TickCount = Delay;
            return true;
        }

        public bool OnCycle()
        {
            if (SetItems.Count == 0 || !Requested)
            {
                return false;
            }

            var Now = UnixUtilities.GetNowMilliseconds();
            if (_next < Now)
            {
                foreach (var Item in SetItems.Values.ToList())
                {
                    if (Item == null)
                    {
                        continue;
                    }

                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    {
                        Item n = null;
                        SetItems.TryRemove(Item.Id, out n);
                        continue;
                    }

                    Item.Interactor.OnWiredTrigger(Item);
                }

                Requested = false;

                _next = 0;
                TickCount = Delay;
        
            }
            return true;
        }
    }
}
