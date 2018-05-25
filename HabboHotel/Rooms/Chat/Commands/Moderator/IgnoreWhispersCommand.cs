namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class IgnoreWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_ignore_whispers";

        public string Parameters => "";

        public string Description => "Allows you to ignore all of the whispers in the room, except from your own.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().IgnorePublicWhispers = !Session.GetHabbo().IgnorePublicWhispers;
            Session.SendWhisper("You're " + (Session.GetHabbo().IgnorePublicWhispers ? "now" : "no longer") + " ignoring public whispers!");
        }
    }
}
