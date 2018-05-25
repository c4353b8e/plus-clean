namespace Plus.Game.Rooms.Chat.Commands.User
{
    using Players;

    internal class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_setspeed";

        public string Parameters => "%value%";

        public string Description => "Set the speed of the rollers in the current room.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a value for the roller speed.");
                return;
            }

            int Speed;
            if (int.TryParse(Params[1], out Speed))
            {
                Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(Speed);
            }
            else
            {
                Session.SendWhisper("Invalid amount, please enter a valid number.");
            }
        }
    }
}