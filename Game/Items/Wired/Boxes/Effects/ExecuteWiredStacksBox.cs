namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class ExecuteWiredStacksBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectExecuteWiredStacks;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public ExecuteWiredStacksBox(Room Instance, Item Item)
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
            if (Params.Length != 1)
            {
                return false;
            }

            var Player = (Habbo)Params[0];
            if (Player == null)
            {
                return false;
            }

            foreach (var Item in SetItems.Values.ToList())
            {
                if (Item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(Item) || !Item.IsWired)
                {
                    continue;
                }

                IWiredItem WiredItem;
                if(Instance.GetWired().TryGet(Item.Id, out WiredItem))
                {
                    if (WiredItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                    {
                    }
                    else
                    {
                       var Effects = Instance.GetWired().GetEffects(WiredItem);
                       if (Effects.Count > 0)
                       {
                           foreach (var EffectItem in Effects.ToList())
                           {
                               if (SetItems.ContainsKey(EffectItem.Item.Id) && EffectItem.Item.Id != Item.Id)
                               {
                               }
                               else if (EffectItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                               {
                               }
                               else
                               {
                                   EffectItem.Execute(Player);
                               }
                           }
                       }
                    }
                }
            }

            return true;
        }
    }
}