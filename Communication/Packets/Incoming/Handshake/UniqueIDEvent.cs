namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Game.Players;
    using Outgoing.Handshake;

    public class UniqueIdEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            packet.PopString();
            var machineId = packet.PopString();

            session.MachineId = machineId;

            session.SendPacket(new SetUniqueIdComposer(machineId));
        }
    }
}