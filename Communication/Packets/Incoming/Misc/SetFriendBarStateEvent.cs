namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger.FriendBar;
    using Outgoing.Sound;

    internal class SetFriendBarStateEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
                return;
            }

            session.GetHabbo().FriendbarState = FriendBarStateUtility.GetEnum(packet.PopInt());
            session.SendPacket(new SoundSettingsComposer(session.GetHabbo().ClientVolume, session.GetHabbo().ChatPreference, session.GetHabbo().AllowMessengerInvites, session.GetHabbo().FocusPreference, FriendBarStateUtility.GetInt(session.GetHabbo().FriendbarState)));
        }
    }
}
