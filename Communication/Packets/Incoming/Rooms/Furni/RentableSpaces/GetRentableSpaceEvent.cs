namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    using Game.Players;
    using Outgoing.Rooms.Furni.RentableSpaces;

    internal class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            packet.PopInt(); //unknown

            session.SendPacket(new RentableSpaceComposer());
        }
    }
}
