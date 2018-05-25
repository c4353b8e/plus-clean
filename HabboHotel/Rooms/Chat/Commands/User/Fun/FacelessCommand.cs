namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using Communication.Packets.Outgoing.Rooms.Engine;

    internal class FacelessCommand :IChatCommand
    {
        public string PermissionRequired => "command_faceless";

        public string Parameters => "";

        public string Description => "Allows you to go faceless!";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
    
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || User.GetClient() == null)
            {
                return;
            }

            string[] headParts;
            var figureParts = Session.GetHabbo().Look.Split('.');
            foreach (var Part in figureParts)
            {
                if (Part.StartsWith("hd"))
                {
                    headParts = Part.Split('-');
                    if (!headParts[1].Equals("99999"))
                    {
                        headParts[1] = "99999";
                    }
                    else
                    {
                        return;
                    }

                    Session.GetHabbo().Look = Session.GetHabbo().Look.Replace(Part, "hd-" + headParts[1] + "-" + headParts[2]);
                    break;
                }
            }

            Session.GetHabbo().Look = Program.FigureManager.ProcessFigure(Session.GetHabbo().Look, Session.GetHabbo().Gender, Session.GetHabbo().GetClothing().GetClothingParts, true);

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `look` = '" + Session.GetHabbo().Look + "' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
            }

            Session.SendPacket(new UserChangeComposer(User, true));
            Session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(User, false));
        }
    }
}
