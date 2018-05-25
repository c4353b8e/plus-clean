namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using Game.Items;
    using Game.Players;

    internal class MoodlightUpdateEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
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

            var preset = packet.PopInt();
            var backgroundMode = packet.PopInt();
            var colorCode = packet.PopString();
            var intensity = packet.PopInt();

            var backgroundOnly = backgroundMode >= 2;

            room.MoodlightData.Enabled = true;
            room.MoodlightData.CurrentPreset = preset;
            room.MoodlightData.UpdatePreset(preset, colorCode, intensity, backgroundOnly);

            item.ExtraData = room.MoodlightData.GenerateExtraData();
            item.UpdateState();
        }
    }
}
