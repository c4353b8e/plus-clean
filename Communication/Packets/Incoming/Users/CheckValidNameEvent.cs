namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Players;
    using Outgoing.Users;

    internal class CheckValidNameEvent : IPacketEvent
    {
        public void Parse(Player session, ClientPacket packet)
        {
            bool inUse;
            var name = packet.PopString();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                inUse = dbClient.GetInteger() == 1;
            }

            var letters = name.ToLower().ToCharArray();
            const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            if (letters.Any(chr => !allowedCharacters.Contains(chr)))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
                return;
            }

            if (Program.GameContext.GetChatManager().GetFilter().IsFiltered(name))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && name.ToLower().Contains("mod") || name.ToLower().Contains("adm") || name.ToLower().Contains("admin") || name.ToLower().Contains("m0d"))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
                return;
            }

            if (!name.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 4));
                return;
            }

            if (name.Length > 15)
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 3));
                return;
            }

            if (name.Length < 3)
            {
                session.SendPacket(new NameChangeUpdateComposer(name, 2));
                return;
            }

            if (inUse)
            {
                ICollection<string> suggestions = new List<string>();
                for (var i = 100; i < 103; i++)
                {
                    suggestions.Add(i.ToString());
                }

                session.SendPacket(new NameChangeUpdateComposer(name, 5, suggestions));
                return;
            }

            session.SendPacket(new NameChangeUpdateComposer(name, 0));
        }
    }
}
