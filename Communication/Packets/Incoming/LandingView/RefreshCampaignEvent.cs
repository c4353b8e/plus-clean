namespace Plus.Communication.Packets.Incoming.LandingView
{
    using Game.Players;
    using Outgoing.LandingView;

    internal class RefreshCampaignEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            try
            {
                var parseCampaings = packet.PopString();
                if (parseCampaings.Contains("gamesmaker"))
                {
                    return;
                }

                var campaingName = "";
                var parser = parseCampaings.Split(';');

                foreach (var value in parser)
                {
                    if (string.IsNullOrEmpty(value) || value.EndsWith(","))
                    {
                        continue;
                    }

                    var data = value.Split(',');
                    campaingName = data[1];
                }

                session.SendPacket(new CampaignComposer(parseCampaings, campaingName));
            }
            catch
            {
                //ignored
            }
        }
    }
}