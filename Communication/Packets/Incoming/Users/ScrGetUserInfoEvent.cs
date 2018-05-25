namespace Plus.Communication.Packets.Incoming.Users
{
    using Game.Players;
    using Outgoing.Users;

    internal class ScrGetUserInfoEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new ScrSendUserInfoComposer());
        }
    }
}
