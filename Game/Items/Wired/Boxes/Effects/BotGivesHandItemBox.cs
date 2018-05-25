namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class BotGivesHandItemBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotGivesHanditemBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotGivesHandItemBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var DrinkID = Packet.PopInt();
            var BotName = Packet.PopString();

            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            StringData = BotName + ";" + DrinkID;
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

            var Player = (Habbo)Params[0];

            if(Player == null)
            {
                return false;
            }

            var Actor = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);

            if(Actor == null)
            {
                return false;
            }

            var User = Instance.GetRoomUserManager().GetBotByName(StringData.Split(';')[0]);

            if (User == null)
            {
                return false;
            }

            if (User.BotData.TargetUser == 0)
            {
                if (!Instance.GetGameMap().CanWalk(Actor.SquareBehind.X, Actor.SquareBehind.Y, false))
                {
                    return false;
                }

                var Data = StringData.Split(';');

                int DrinkId;

                if (!int.TryParse(Data[1], out DrinkId))
                {
                    return false;
                }

                User.CarryItem(DrinkId);
                User.BotData.TargetUser = Actor.HabboId;

                User.MoveTo(Actor.SquareBehind.X, Actor.SquareBehind.Y);
            }

            return true;
        }
    }
}
