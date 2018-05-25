namespace Plus.Game.Rooms.Chat.Commands
{
    using Players;

    public interface IChatCommand
    {
        string PermissionRequired { get; }
        string Parameters { get; }
        string Description { get; }
        void Execute(Player Session, Room Room, string[] Params);
    }
}
