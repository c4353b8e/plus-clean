namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using System.Linq;
    using Game.Items;
    using Game.Players;
    using Outgoing.Groups;
    using Outgoing.Rooms.Engine;

    internal class UpdateGroupColoursEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var mainColour = packet.PopInt();
            var secondaryColour = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id)
            {
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = @groupId LIMIT 1");
                dbClient.AddParameter("colour1", mainColour);
                dbClient.AddParameter("colour2", secondaryColour);
                dbClient.AddParameter("groupId", group.Id);
                dbClient.RunQuery();
            }

            group.Colour1 = mainColour;
            group.Colour2 = secondaryColour;

            session.SendPacket(new GroupInfoComposer(group, session));
            if (session.GetHabbo().CurrentRoom != null)
            {
                foreach (var item in session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
                {
                    if (item == null || item.GetBaseItem() == null)
                    {
                        continue;
                    }

                    if (item.GetBaseItem().InteractionType != InteractionType.GUILD_ITEM && item.GetBaseItem().InteractionType != InteractionType.GUILD_GATE || item.GetBaseItem().InteractionType != InteractionType.GUILD_FORUM)
                    {
                        continue;
                    }

                    session.GetHabbo().CurrentRoom.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(item.UserID)));
                }
            }
        }
    }
}
