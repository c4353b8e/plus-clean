namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    using Game.Users.Effects;

    internal class AvatarEffectExpiredComposer : ServerPacket
    {
        public AvatarEffectExpiredComposer(AvatarEffect Effect)
            : base(ServerPacketHeader.AvatarEffectExpiredMessageComposer)
        {
            WriteInteger(Effect.SpriteId);
        }
    }
}
