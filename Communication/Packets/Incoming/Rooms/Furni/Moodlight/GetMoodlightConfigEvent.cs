namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Items;
    using HabboHotel.Items.Data.Moodlight;
    using Outgoing.Rooms.Furni.Moodlight;

    internal class GetMoodlightConfigEvent : IPacketEvent
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

            if (!room.CheckRights(session, true))
            {
                return;
            }

            if (room.MoodlightData == null)
            {
                foreach (var item in room.GetRoomItemHandler().GetWall.ToList())
                {
                    if (item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                    {
                        room.MoodlightData = new MoodlightData(item.Id);
                    }
                }
            }

            if (room.MoodlightData == null)
            {
                return;
            }

            session.SendPacket(new MoodlightConfigComposer(room.MoodlightData));
        }
    }
}