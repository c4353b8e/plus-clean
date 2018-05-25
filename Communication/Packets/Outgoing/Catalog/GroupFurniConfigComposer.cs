namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using Game.Groups;

    internal class GroupFurniConfigComposer : ServerPacket
    {
        public GroupFurniConfigComposer(ICollection<Group> groups)
            : base(ServerPacketHeader.GroupFurniConfigMessageComposer)
        {
            WriteInteger(groups.Count);
            foreach (var group in groups)
            {
                WriteInteger(group.Id);
                WriteString(group.Name);
                WriteString(group.Badge);
                WriteString(Program.GameContext.GetGroupManager().GetColourCode(group.Colour1, true));
                WriteString(Program.GameContext.GetGroupManager().GetColourCode(group.Colour2, false));
                WriteBoolean(false);
                WriteInteger(group.CreatorId);
                WriteBoolean(group.ForumEnabled);
            }
        }
    }
}
