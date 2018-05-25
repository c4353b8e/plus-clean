namespace Plus.Communication.Packets.Incoming.Inventory.AvatarEffects
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.AvatarEffects;

    internal class AvatarEffectActivatedEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var effectId = packet.PopInt();

            var effect = session.GetHabbo().Effects().GetEffectNullable(effectId, false, true);

            if (effect == null || session.GetHabbo().Effects().HasEffect(effectId, true))
            {
                return;
            }

            if (effect.Activate())
            {
                session.SendPacket(new AvatarEffectActivatedComposer(effect));
            }
        }
    }
}
