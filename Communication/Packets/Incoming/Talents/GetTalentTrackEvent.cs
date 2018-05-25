namespace Plus.Communication.Packets.Incoming.Talents
{
    using HabboHotel.GameClients;
    using Outgoing.Talents;

    internal class GetTalentTrackEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var type = packet.PopString();

            var levels = Program.GameContext.GetTalentTrackManager().GetLevels();

            session.SendPacket(new TalentTrackComposer(levels, type));
        }
    }
}
