namespace Plus.Communication.Packets.Incoming.Groups
{
    using Game.Players;
    using Outgoing.Groups;

    internal class GetBadgeEditorPartsEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            session.SendPacket(new BadgeEditorPartsComposer(
                Program.GameContext.GetGroupManager().BadgeBases,
                Program.GameContext.GetGroupManager().BadgeSymbols,
                Program.GameContext.GetGroupManager().BadgeBaseColours,
                Program.GameContext.GetGroupManager().BadgeSymbolColours,
                Program.GameContext.GetGroupManager().BadgeBackColours));
        }
    }
}
