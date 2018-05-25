namespace Plus.Communication.Packets.Incoming.Handshake
{
    using HabboHotel.GameClients;
    using Outgoing.Handshake;

    public class UniqueIdEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            packet.PopString();
            var machineId = packet.PopString();

            session.MachineId = machineId;

            session.SendPacket(new SetUniqueIdComposer(machineId));
        }
    }
}