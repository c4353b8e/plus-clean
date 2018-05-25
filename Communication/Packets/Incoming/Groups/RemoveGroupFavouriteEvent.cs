namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Outgoing.Groups;

    internal class RemoveGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.GetHabbo().GetStats().FavouriteGroupId = 0;

            if (session.GetHabbo().InRoom)
            {
                var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null)
                {
                    session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(null, user.VirtualId));
                }

                session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            }
            else
            {
                session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
            }
        }
    }
}
