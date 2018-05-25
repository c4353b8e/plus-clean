namespace Plus.Communication.Packets.Incoming.Sound
{
    using Game.Players;
    using Outgoing.Sound;

    internal class GetSongInfoEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new TraxSongInfoComposer());
        }
    }
}
