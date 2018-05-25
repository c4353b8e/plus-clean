namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using HabboHotel.GameClients;
    using HabboHotel.Items;

    internal class ToggleMoodlightEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            if (!Program.GameContext.GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            {
                return;
            }

            if (!room.CheckRights(session, true) || room.MoodlightData == null)
            {
                return;
            }

            var item = room.GetRoomItemHandler().GetItem(room.MoodlightData.ItemId);
            if (item == null || item.GetBaseItem().InteractionType != InteractionType.MOODLIGHT)
            {
                return;
            }

            if (room.MoodlightData.Enabled)
            {
                room.MoodlightData.Disable();
            }
            else
            {
                room.MoodlightData.Enable();
            }

            item.ExtraData = room.MoodlightData.GenerateExtraData();
            item.UpdateState();
        }
    }
}