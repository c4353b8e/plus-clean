namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using System;
    using System.Linq;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Players;

    internal class MassDanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_massdance";

        public string Parameters => "%DanceId%";

        public string Description => "Force everyone in the room to dance to a dance of your choice.";

        public void Execute(Player session, Room room, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            var danceId = Convert.ToInt32(Params[1]);
            if (danceId < 0 || danceId > 4)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            var users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
            {
                foreach (var u in users.ToList())
                {
                    if (u == null)
                    {
                        continue;
                    }

                    if (u.CarryItemId > 0)
                    {
                        u.CarryItemId = 0;
                    }

                    u.DanceId = danceId;
                    room.SendPacket(new DanceComposer(u, danceId));
                }
            }
        }
    }
}
