namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using Game.Players;

    internal class SitEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
            {
                return;
            }

            var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.Statusses.ContainsKey("lie") || user.isLying || user.RidingHorse || user.IsWalking)
            {
                return;
            }

            if (!user.Statusses.ContainsKey("sit"))
            {
                if (user.RotBody % 2 == 0)
                {
                    try
                    {
                        user.Statusses.Add("sit", "1.0");
                        user.Z -= 0.35;
                        user.isSitting = true;
                        user.UpdateNeeded = true;
                    }
                    catch
                    {
                        //ignored
                    }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.isSitting = true;
                    user.UpdateNeeded = true;
                }
            }
            else if (user.isSitting)
            {
                user.Z += 0.35;
                user.Statusses.Remove("sit");
                user.Statusses.Remove("1.0");
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
        }
    }
}