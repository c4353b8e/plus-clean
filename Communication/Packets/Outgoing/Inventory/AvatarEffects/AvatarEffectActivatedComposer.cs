namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using HabboHotel.Users.Effects;

    internal class AvatarEffectActivatedComposer : ServerPacket
    {
        public AvatarEffectActivatedComposer(AvatarEffect Effect)
            : base(ServerPacketHeader.AvatarEffectActivatedMessageComposer)
        {
            WriteInteger(Effect.SpriteId);
            WriteInteger((int)Effect.Duration);
            WriteBoolean(false);//Permanent
        }
    }
}