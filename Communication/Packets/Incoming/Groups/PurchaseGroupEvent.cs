namespace Plus.Communication.Packets.Incoming.Groups
{
    using System;
    using Game.Groups;
    using Game.Players;
    using Game.Rooms;
    using Outgoing.Catalog;
    using Outgoing.Groups;
    using Outgoing.Inventory.Purse;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Session;

    internal class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var name = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var description = Program.GameContext.GetChatManager().GetFilter().CheckMessage(packet.PopString());
            var roomId = packet.PopInt();
            var mainColour = packet.PopInt();
            var secondaryColour = packet.PopInt();
            packet.PopInt(); //unknown

            var groupCost = Convert.ToInt32(Program.SettingsManager.TryGetValue("catalog.group.purchase.cost"));

            if (session.GetHabbo().Credits < groupCost)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("A group costs " + groupCost + " credits! You only have " + session.GetHabbo().Credits + "!"));
                return;
            }

            session.GetHabbo().Credits -= groupCost;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

            if (!RoomFactory.TryGetData(roomId, out var room))
            {
                return;
            }

            if (room == null || room.OwnerId != session.GetHabbo().Id || room.Group != null)
            {
                return;
            }

            var badge = string.Empty;

            for (var i = 0; i < 5; i++)
            {
                badge += BadgePartUtility.WorkBadgeParts(i == 0, packet.PopInt().ToString(), packet.PopInt().ToString(), packet.PopInt().ToString());
            }

            if (!Program.GameContext.GetGroupManager().TryCreateGroup(session.GetHabbo(), name, description, roomId, badge, mainColour, secondaryColour, out var group))
            {
                session.SendNotification("An error occured whilst trying to create this group.\n\nTry again. If you get this message more than once, report it at the link below.\r\rhttp://boonboards.com");
                return;
            }

            session.SendPacket(new PurchaseOKComposer());

            room.Group = group;

            if (session.GetHabbo().CurrentRoomId != room.Id)
            {
                session.SendPacket(new RoomForwardComposer(room.Id));
            }

            session.SendPacket(new NewGroupInfoComposer(roomId, group.Id));
        }
    }
}