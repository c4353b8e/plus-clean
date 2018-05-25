namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class TeleportUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectTeleportToFurni;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public int Delay { get => _delay;
            set { _delay = value; TickCount = value + 1; } }
        public int TickCount { get; set; }
        public string ItemsData { get; set; }

        private readonly Queue _queue;
        private int _delay;

        public TeleportUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();

            _queue = new Queue();
            TickCount = Delay;
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();

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

            Delay = Packet.PopInt();
        }

        public bool OnCycle()
        {
            if (_queue.Count == 0 || SetItems.Count == 0)
            {
                _queue.Clear();
                TickCount = Delay;
                return true;
            }

            while (_queue.Count > 0)
            {
                var Player = (Habbo)_queue.Dequeue();
                if (Player == null || Player.CurrentRoom != Instance)
                {
                    continue;
                }

                TeleportUser(Player);
            }

            TickCount = Delay;
            return true;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
            {
                return false;
            }

            var Player = (Habbo)Params[0];

            if (Player == null)
            {
                return false;
            }

            if (Player.Effects() != null)
            {
                Player.Effects().ApplyEffect(4);
            }

            _queue.Enqueue(Player);
            return true;
        }

        private void TeleportUser(Habbo Player)
        {
            if (Player == null)
            {
                return;
            }

            var Room = Player.CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
            {
                return;
            }

            if (Player.IsTeleporting || Player.IsHopping || Player.TeleporterId != 0)
            {
                return;
            }

            var rand = new Random();
            var Items = SetItems.Values.ToList();
            Items = Items.OrderBy(x => rand.Next()).ToList();

            if (Items.Count == 0)
            {
                return;
            }

            var Item = Items.First();
            if (Item == null)
            {
                return;
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
                    return;
                }

                Item = Items.First();
                if (Item == null)
                {
                    return;
                }
            }

            if (Room.GetGameMap() == null)
            {
                return;
            }

            Room.GetGameMap().TeleportToItem(User, Item);
            Room.GetRoomUserManager().UpdateUserStatusses();

            if (Player.Effects() != null)
            {
                Player.Effects().ApplyEffect(0);
            }
        }
    }
}
