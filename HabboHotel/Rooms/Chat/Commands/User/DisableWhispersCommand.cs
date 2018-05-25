namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableWhispersCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_whispers";

        public string Parameters => "";

        public string Description => "Allows you to enable or disable the ability to receive whispers.";

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().ReceiveWhispers = !Session.GetHabbo().ReceiveWhispers;
            Session.SendWhisper("You're " + (Session.GetHabbo().ReceiveWhispers ? "now" : "no longer") + " receiving whispers!");
        }
    }
}
