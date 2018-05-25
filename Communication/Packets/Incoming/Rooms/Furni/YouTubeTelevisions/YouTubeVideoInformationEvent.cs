namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using System.Linq;
    using Game.Players;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class YouTubeVideoInformationEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            var itemId = packet.PopInt();
            var videoId = packet.PopString();

            foreach (var tele in Program.GameContext.GetItemManager().GetTelevisionManager().TelevisionList.ToList())
            {
                if (tele.YouTubeId != videoId)
                {
                    continue;
                }

                session.SendPacket(new GetYouTubeVideoComposer(itemId, tele.YouTubeId));
            }
        }
    }
}