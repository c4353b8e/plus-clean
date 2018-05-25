namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Cache.Type;
    using Game.Players;
    using Outgoing.Groups;

    internal class GetGroupMembersEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var groupId = packet.PopInt();
            var page = packet.PopInt();
            var searchVal = packet.PopString();
            var requestType = packet.PopInt();

            if (!Program.GameContext.GetGroupManager().TryGetGroup(groupId, out var group))
            {
                return;
            }

            var members = new List<UserCache>();

            switch (requestType)
            {
                case 0:
                    {
                        var memberIds = group.GetAllMembers;
                        foreach (var id in memberIds.ToList())
                        {
                            var groupMember = Program.GameContext.GetCacheManager().GenerateUser(id);
                            if (groupMember == null)
                            {
                                continue;
                            }

                            if (!members.Contains(groupMember))
                            {
                                members.Add(groupMember);
                            }
                        }
                        break;
                    }

                case 1:
                    {
                        var adminIds = group.GetAdministrators;
                        foreach (var id in adminIds.ToList())
                        {
                            var groupMember = Program.GameContext.GetCacheManager().GenerateUser(id);
                            if (groupMember == null)
                            {
                                continue;
                            }

                            if (!members.Contains(groupMember))
                            {
                                members.Add(groupMember);
                            }
                        }
                        break;
                    }

                case 2:
                    {
                        var requestIds = group.GetRequests;
                        foreach (var id in requestIds.ToList())
                        {
                            var groupMember = Program.GameContext.GetCacheManager().GenerateUser(id);
                            if (groupMember == null)
                            {
                                continue;
                            }

                            if (!members.Contains(groupMember))
                            {
                                members.Add(groupMember);
                            }
                        }
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(searchVal))
            {
                members = members.Where(x => x.Username.StartsWith(searchVal)).ToList();
            }

            var startIndex = (page - 1) * 14 + 14;
            var finishIndex = members.Count;

            session.SendPacket(new GroupMembersComposer(group, members.Skip(startIndex).Take(finishIndex - startIndex).ToList(), members.Count, page, group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id), requestType, searchVal));
        }
    }
}