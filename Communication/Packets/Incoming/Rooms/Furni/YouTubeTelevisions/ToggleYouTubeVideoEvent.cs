namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Furni.YouTubeTelevisions;

    internal class ToggleYouTubeVideoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var itemId = packet.PopInt();//Item Id
            var videoId = packet.PopString(); //Video ID

            session.SendPacket(new GetYouTubeVideoComposer(itemId, videoId));
        }
    }
}