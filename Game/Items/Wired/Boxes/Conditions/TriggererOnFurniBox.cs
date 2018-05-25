namespace Plus.Game.Items.Wired.Boxes.Conditions
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class TriggererOnFurniBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionTriggererOnFurni;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public TriggererOnFurniBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
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
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0)
            {
                return false;
            }

            var Player = (Habbo)Params[0];
            if (Player == null)
            {
                return false;
            }

            if (Player.CurrentRoom == null)
            {
                return false;
            }

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
            {
                return false;
            }

            var ItemsOnSquare = Instance.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
            foreach (var Item in ItemsOnSquare.ToList())
            {
                if (!SetItems.ContainsKey(Item.Id))
                {
                    continue;
                }

                return true;
            }
            return false;
        }
    }
}