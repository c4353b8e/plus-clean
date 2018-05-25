namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    using System.Linq;
    using Game.Items;
    using Game.Items.Data.Moodlight;
    using Game.Players;
    using Outgoing.Rooms.Furni.Moodlight;

    internal class GetMoodlightConfigEvent : IPacketEvent
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