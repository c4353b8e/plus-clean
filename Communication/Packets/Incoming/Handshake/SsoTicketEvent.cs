namespace Plus.Communication.Packets.Incoming.Handshake
{
    using Core.Logging;
    using HabboHotel.GameClients;

    public class SsoTicketEvent : IPacketEvent
    {
        private static readonly ILogger Logger = new Logger<SsoTicketEvent>();

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || Program.EncryptionEnabled && session.Rc4Client == null || session.GetHabbo() != null)
            {
                return;
            }

            var sso = packet.PopString();
            if (string.IsNullOrEmpty(sso) || sso.Length < 15)
            {
                return;
            }

            if (session.TryAuthenticate(sso))
            {
                Logger.Trace(session.GetHabbo().Username + " has joined the server.");
            }
        }
    }
}