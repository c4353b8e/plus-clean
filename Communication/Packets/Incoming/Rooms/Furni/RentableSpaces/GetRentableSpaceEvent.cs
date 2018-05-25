namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.RentableSpaces;

    internal class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            packet.PopInt(); //unknown

            session.SendPacket(new RentableSpaceComposer());
        }
    }
}
