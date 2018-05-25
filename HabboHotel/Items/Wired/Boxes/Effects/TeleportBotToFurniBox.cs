namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;

    internal class TeleportBotToFurniBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectTeleportBotToFurniBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public TeleportBotToFurniBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var BotName = Packet.PopString();

            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                {
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
                }
            }

            StringData = BotName;
        }

        public bool Execute(params object[] Params)
        {
            if (string.IsNullOrEmpty(StringData))
            {
                return false;
            }

            var User = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (User == null)
            {
                return false;
            }

            var rand = new Random();
            var Items = SetItems.Values.ToList();
            Items = Items.OrderBy(x => rand.Next()).ToList();

            if (Items.Count == 0)
            {
                return false;
            }

            var Item = Items.First();
            if (Item == null)
            {
                return false;
            }

            if (!Instance.GetRoomItemHandler().GetFloor.Contains(Item))
            {
                SetItems.TryRemove(Item.Id, out Item);

                if (Items.Contains(Item))
                {
                    Items.Remove(Item);
                }

                if (SetItems.Count == 0 || Items.Count == 0)
                {
                    return false;
                }

                Item = Items.First();
                if (Item == null)
                {
                    return false;
                }
            }

            if (Instance.GetGameMap() == null)
            {
                return false;
            }

            Instance.GetGameMap().TeleportToItem(User, Item);
            Instance.GetRoomUserManager().UpdateUserStatusses();


            return true;
        }
    }
}