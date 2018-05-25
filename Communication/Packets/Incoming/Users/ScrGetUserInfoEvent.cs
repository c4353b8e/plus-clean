namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class ScrGetUserInfoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new ScrSendUserInfoComposer());
        }
    }
}
