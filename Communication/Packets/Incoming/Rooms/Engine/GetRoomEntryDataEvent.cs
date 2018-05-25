namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using Outgoing.Rooms.Chat;
    using Outgoing.Rooms.Engine;
    using Utilities;

    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
            {
                return;
            }

            if (session.GetHabbo().InRoom)
            {
                if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var oldRoom))
                {
                    return;
                }

                if (oldRoom.GetRoomUserManager() != null)
                {
                    oldRoom.GetRoomUserManager().RemoveUserFromRoom(session, false);
                }
            }

            if (!room.GetRoomUserManager().AddAvatarToRoom(session))
            {
                room.GetRoomUserManager().RemoveUserFromRoom(session, false);
                return;//TODO: Remove?
            }

            room.SendObjects(session);

            if (session.GetHabbo().GetMessenger() != null)
            {
                session.GetHabbo().GetMessenger().OnStatusChanged(true);
            }

            if (session.GetHabbo().GetStats().QuestId > 0)
            {
                Program.GameContext.GetQuestManager().QuestReminder(session, session.GetHabbo().GetStats().QuestId);
            }

            session.SendPacket(new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)));
            session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, room.Hidewall.ToString() == "1"));

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user != null && session.GetHabbo().PetId == 0)
            {
                room.SendPacket(new UserChangeComposer(user, false));
            }

            session.SendPacket(new RoomEventComposer(room, room.Promotion));

            if (room.GetWired() != null)
            {
                room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());
            }

            if (UnixTimestamp.GetNow() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            {
                session.SendPacket(new FloodControlComposer((int)session.GetHabbo().FloodTime - (int)UnixTimestamp.GetNow()));
            }
        }
    }
}