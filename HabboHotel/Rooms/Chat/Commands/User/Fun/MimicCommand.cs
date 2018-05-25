namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using GameClients;

    internal class MimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_mimic";

        public string Parameters => "%username%";

        public string Description => "Liking someone elses swag? Copy it!";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter the username of the user you wish to mimic.");
                return;
            }

            var TargetClient = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (!TargetClient.GetHabbo().AllowMimic)
            {
                Session.SendWhisper("Oops, you cannot mimic this user - sorry!");
                return;
            }

            var TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            Session.GetHabbo().Gender = TargetUser.GetClient().GetHabbo().Gender;
            Session.GetHabbo().Look = TargetUser.GetClient().GetHabbo().Look;

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `gender` = @gender, `look` = @look WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gender", Session.GetHabbo().Gender);
                dbClient.AddParameter("look", Session.GetHabbo().Look);
                dbClient.AddParameter("id", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User != null)
            {
                Session.SendPacket(new AvatarAspectUpdateComposer(Session.GetHabbo().Look, Session.GetHabbo().Gender));
                Session.SendPacket(new UserChangeComposer(User, true));
                Room.SendPacket(new UserChangeComposer(User, false));
            }
        }
    }
}