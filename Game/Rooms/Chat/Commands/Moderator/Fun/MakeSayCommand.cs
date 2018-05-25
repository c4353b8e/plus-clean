namespace Plus.Game.Rooms.Chat.Commands.Moderator.Fun
{
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Players;

    internal class MakeSayCommand : IChatCommand
    {
        public string PermissionRequired => "command_makesay";
        public string Parameters => "%username% %message%";
        public string Description => "Forces the specified user to say the specified message.";

        public void Execute(Player session, Room room, string[] Params)
        {
            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (thisUser == null)
            {
                return;
            }

            if (Params.Length == 1)
            {
                session.SendWhisper("You must enter a username and the message you wish to force them to say.");
            }
            else
            {
                var message = CommandManager.MergeParams(Params, 2);
                var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);

                if (targetUser != null)
                {
                    if (targetUser.GetClient() != null && targetUser.GetClient().GetHabbo() != null)
                    {
                        if (!targetUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_make_say_any"))
                        {
                            room.SendPacket(new ChatComposer(targetUser.VirtualId, message, 0, targetUser.LastBubble));
                        }
                        else
                        {
                            session.SendWhisper("You cannot use makesay on this user.");
                        }
                    }
                }
                else
                {
                    session.SendWhisper("This user could not be found in the room");
                }
            }
        }
    }
}
