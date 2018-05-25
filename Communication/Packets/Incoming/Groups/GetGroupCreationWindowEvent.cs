namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Linq;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Groups;

    internal class GetGroupCreationWindowEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null)
            {
                return;
            }

            var rooms = RoomFactory.GetRoomsDataByOwnerSortByName(session.GetHabbo().Id).Where(x => x.Group == null).ToList();

            session.SendPacket(new GroupCreationWindowComposer(rooms));
        }
    }
}
