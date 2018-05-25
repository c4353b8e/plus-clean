namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    using Communication.Packets.Outgoing.Rooms.Chat;

    internal class MakeSayCommand : IChatCommand
    {
        public string PermissionRequired => "command_makesay";

        public string Parameters => "%username% %message%";

        public string Description => "Forces the specified user to say the specified message.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
            {
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("You must enter a username and the message you wish to force them to say.");
            }
            else
            {
                var Message = CommandManager.MergeParams(Params, 2);
                var TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                if (TargetUser != null)
                {
                    if (TargetUser.GetClient() != null && TargetUser.GetClient().GetHabbo() != null)
                    {
                        if (!TargetUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_make_say_any"))
                        {
                            Room.SendPacket(new ChatComposer(TargetUser.VirtualId, Message, 0, TargetUser.LastBubble));
                        }
                        else
                        {
                            Session.SendWhisper("You cannot use makesay on this user.");
                        }
                    }
                }
                else
                {
                    Session.SendWhisper("This user could not be found in the room");
                }
            }
        }
    }
}
