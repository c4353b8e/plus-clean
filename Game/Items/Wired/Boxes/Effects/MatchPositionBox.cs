namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Core.Logging;
    using Rooms;

    internal class MatchPositionBox : IWiredItem, IWiredCycle
    {
        private static readonly ILogger Logger = new Logger<MatchPositionBox>();

        private int _delay;
        public Room Instance { get; set; }

        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectMatchPosition;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public int Delay { get => _delay;
            set { _delay = value; TickCount = value + 1; } }

        public int TickCount { get; set; }

        private bool Requested;
        public string ItemsData { get; set; }

        public MatchPositionBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            Requested = false;
        }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            var Unknown = Packet.PopInt();
            var State = Packet.PopInt();
            var Direction = Packet.PopInt();
            var Placement = Packet.PopInt();
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

            StringData = State + ";" + Direction + ";" + Placement;

            var Delay = Packet.PopInt();
            this.Delay = Delay;
        }

        public bool Execute(params object[] Params)
        {
            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }
            return true;
        }

        public bool OnCycle()
        {
            if (!Requested || string.IsNullOrEmpty(StringData) || StringData == "0;0;0" || SetItems.Count == 0)
            {
                return false;
            }

            foreach (var item in SetItems.Values.ToList())
            {
                if (Instance.GetRoomItemHandler().GetFloor == null && !Instance.GetRoomItemHandler().GetFloor.Contains(item))
                {
                    continue;
                }

                foreach (var I in ItemsData.Split(';'))
                {
                    if (string.IsNullOrEmpty(I))
                    {
                        continue;
                    }

                    var itemId = Convert.ToInt32(I.Split(':')[0]);
                    var II = Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(itemId));
                    if (II == null)
                    {
                        continue;
                    }

                    var partsString = I.Split(':');
                    try
                    {
                        if (string.IsNullOrEmpty(partsString[0]) || string.IsNullOrEmpty(partsString[1]))
                        {
                            continue;
                        }
                    }
                    catch { continue; }

                    var part = partsString[1].Split(',');

                    try
                    {
                        if (int.Parse(StringData.Split(';')[0]) == 1)//State
                        {
                            if (part.Count() >= 4)
                            {
                                SetState(II, part[4]);
                            }
                            else
                            {
                                SetState(II, "1");
                            }
                        }
                    }
                    catch (Exception e) { Logger.Error(e); }

                    try
                    {
                        if (int.Parse(StringData.Split(';')[1]) == 1)//Direction
                        {
                            SetRotation(II, Convert.ToInt32(part[3]));
                        }
                    }
                    catch (Exception e) { Logger.Error(e); }

                    try
                    {
                        if (int.Parse(StringData.Split(';')[2]) == 1)//Position
                        {
                            SetPosition(II, Convert.ToInt32(part[0]), Convert.ToInt32(part[1]), Convert.ToDouble(part[2]));
                        }
                    }
                    catch (Exception e) { Logger.Error(e); }
                }
            }
            Requested = false;
            return true;
        }

        private void SetState(Item Item, string Extradata)
        {
            if (Item.ExtraData == Extradata)
            {
                return;
            }

            if (Item.GetBaseItem().InteractionType == InteractionType.DICE)
            {
                return;
            }

            Item.ExtraData = Extradata;
            Item.UpdateState(false, true);
        }

        private void SetRotation(Item Item, int Rotation)
        {
            if (Item.Rotation == Rotation)
            {
                return;
            }

            Item.Rotation = Rotation;
            Item.UpdateState(false, true);
        }

        private void SetPosition(Item Item, int CoordX, int CoordY, double CoordZ)
        {
            Instance.SendPacket(new SlideObjectBundleComposer(Item.GetX, Item.GetY, Item.GetZ, CoordX, CoordY, CoordZ, 0, 0, Item.Id));

            Instance.GetRoomItemHandler().SetFloorItem(Item, CoordX, CoordY, CoordZ);
            //Instance.GetGameMap().GenerateMaps();
        }
    }
}
