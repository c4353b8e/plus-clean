namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    using System.Collections;
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Rooms;
    using Users;

    internal class KickUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectKickUser;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public int TickCount { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public int Delay { get; set; }
        public string ItemsData { get; set; }
        private readonly Queue _toKick;

        public KickUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _toKick = new Queue();

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
            var Message = Packet.PopString();

            StringData = Message;
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

            if (TickCount <= 0)
            {
                TickCount = 3;
            }

            if (!_toKick.Contains(Player))
            {
                var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
                if (User == null)
                {
                    return false;
                }

                if (Player.GetPermissions().HasRight("mod_tool")  || Instance.OwnerId == Player.Id)
                {
                    Player.GetClient().SendPacket(new WhisperComposer(User.VirtualId, "Wired Kick Exception: Unkickable Player", 0, 0));
                    return false;
                }

                _toKick.Enqueue(Player);
                Player.GetClient().SendPacket(new WhisperComposer(User.VirtualId, StringData, 0, 0));
            }
            return true;
        }

        public bool OnCycle()
        {
            if (Instance == null)
            {
                return false;
            }

            if (_toKick.Count == 0)
            {
                TickCount = 3;
                return true;
            }

            lock (_toKick.SyncRoot)
            {
                while (_toKick.Count > 0)
                {
                    var Player = (Habbo)_toKick.Dequeue();
                    if (Player == null || !Player.InRoom || Player.CurrentRoom != Instance)
                    {
                        continue;
                    }

                    Instance.GetRoomUserManager().RemoveUserFromRoom(Player.GetClient(), true, false);
                }
            }
            TickCount = 3;
            return true;
        }
    }
}