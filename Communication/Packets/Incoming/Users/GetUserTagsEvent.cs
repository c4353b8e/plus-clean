namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class GetUserTagsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var userId = packet.PopInt();

            session.SendPacket(new UserTagsComposer(userId));
        }
    }
}
