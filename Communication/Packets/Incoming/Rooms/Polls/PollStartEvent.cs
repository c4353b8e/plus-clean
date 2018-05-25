namespace Plus.Communication.Packets.Incoming.Rooms.Polls
{
    using Game.Players;
    using Outgoing.Rooms.Polls;

    internal class PollStartEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new PollContentsComposer());
        }
    }
}
