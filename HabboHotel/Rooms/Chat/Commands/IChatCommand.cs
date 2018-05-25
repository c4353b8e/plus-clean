namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    using GameClients;

    public interface IChatCommand
    {
        string PermissionRequired { get; }
        string Parameters { get; }
        string Description { get; }
        void Execute(GameClient Session, Room Room, string[] Params);
    }
}
