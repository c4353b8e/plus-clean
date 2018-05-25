namespace Plus.Communication.Packets.Incoming.Inventory.AvatarEffects
{
    using HabboHotel.GameClients;

    internal class AvatarEffectSelectedEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var effectId = packet.PopInt();
            if (effectId < 0)
            {
                effectId = 0;
            }

            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (effectId != 0 && session.GetHabbo().Effects().HasEffect(effectId, true))
            {
                user.ApplyEffect(effectId);
            }
        }
    }
}
