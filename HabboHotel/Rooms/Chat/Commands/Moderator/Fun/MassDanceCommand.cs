namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using System;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Avatar;

    internal class MassDanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_massdance";

        public string Parameters => "%DanceId%";

        public string Description => "Force everyone in the room to dance to a dance of your choice.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            var DanceId = Convert.ToInt32(Params[1]);
            if (DanceId < 0 || DanceId > 4)
            {
                Session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            var Users = Room.GetRoomUserManager().GetRoomUsers();
            if (Users.Count > 0)
            {
                foreach (var U in Users.ToList())
                {
                    if (U == null)
                    {
                        continue;
                    }

                    if (U.CarryItemId > 0)
                    {
                        U.CarryItemId = 0;
                    }

                    U.DanceId = DanceId;
                    Room.SendPacket(new DanceComposer(U, DanceId));
                }
            }
        }
    }
}
