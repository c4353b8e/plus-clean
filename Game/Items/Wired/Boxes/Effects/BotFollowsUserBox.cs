namespace Plus.Game.Items.Wired.Boxes.Effects
{
    using System.Collections.Concurrent;
    using Communication.Packets.Incoming;
    using Rooms;
    using Users;

    internal class BotFollowsUserBox: IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotFollowsUserBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotFollowsUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var FollowMode = Packet.PopInt();//1 = follow, 0 = don't.
            var BotConfiguration = Packet.PopString();
       
            if (SetItems.Count > 0)
            {
                SetItems.Clear();
            }

            StringData = FollowMode + ";" +BotConfiguration;
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
            if (Player == null)
            {
                return false;
            }

            var Human = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (Human == null)
            {
                return false;
            }

            var Stuff = StringData.Split(';');
            if (Stuff.Length != 2)
            {
                return false;//This is important, incase a cunt scripts.
            }

            var Username = Stuff[1];

            var User = Instance.GetRoomUserManager().GetBotByName(Username);
            if (User == null)
            {
                return false;
            }

            var FollowMode = 0;
            if (!int.TryParse(Stuff[0], out FollowMode))
            {
                return false;
            }

            if (FollowMode == 0)
            {
                User.BotData.ForcedUserTargetMovement = 0;

                if (User.IsWalking)
                {
                    User.ClearMovement(true);
                }
            }
            else if (FollowMode == 1)
            {
                User.BotData.ForcedUserTargetMovement = Player.Id;

                if (User.IsWalking)
                {
                    User.ClearMovement(true);
                }

                User.MoveTo(Human.X, Human.Y);
            }

            return true;
        }
    }
}