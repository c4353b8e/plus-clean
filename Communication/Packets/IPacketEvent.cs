namespace Plus.Communication.Packets
{
    using HabboHotel.GameClients;
    using Incoming;

    public interface IPacketEvent
    {
        void Parse(GameClient session, ClientPacket packet);
    }
}