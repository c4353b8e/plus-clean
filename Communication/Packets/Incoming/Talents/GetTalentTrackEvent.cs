namespace Plus.Communication.Packets.Incoming.Talents
{
    using Game.Players;
    using Outgoing.Talents;

    internal class GetTalentTrackEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var type = packet.PopString();

            var levels = Program.GameContext.GetTalentTrackManager().GetLevels();

            session.SendPacket(new TalentTrackComposer(levels, type));
        }
    }
}
