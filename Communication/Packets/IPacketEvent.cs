namespace Plus.Communication.Packets
{
    using Game.Players;
    using Incoming;

    public interface IPacketEvent
    {
        void Parse(Player session, ClientPacket packet);
    }
}