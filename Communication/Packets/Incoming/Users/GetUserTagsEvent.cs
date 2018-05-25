namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;
    using Outgoing.Users;

    internal class GetUserTagsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var userId = packet.PopInt();

            session.SendPacket(new UserTagsComposer(userId));
        }
    }
}
