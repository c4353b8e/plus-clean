namespace Plus.HabboHotel.GameClients
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Communication.ConnectionManager;
    using Communication.Packets.Outgoing;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Notifications;
    using Core.Logging;
    using Users.Messenger;

    public class GameClientManager
    {
        private static readonly ILogger Logger = new Logger<GameClientManager>();

        private readonly ConcurrentDictionary<int, GameClient> _clients;
        private readonly ConcurrentDictionary<int, GameClient> _userIdRegister;
        private readonly ConcurrentDictionary<string, GameClient> _usernameRegister;

        private readonly Queue _timedOutConnections;

        private readonly Stopwatch _clientPingStopwatch;

        public GameClientManager()
        {
            _clients = new ConcurrentDictionary<int, GameClient>();
            _userIdRegister = new ConcurrentDictionary<int, GameClient>();
            _usernameRegister = new ConcurrentDictionary<string, GameClient>();

            _timedOutConnections = new Queue();

            _clientPingStopwatch = new Stopwatch();
            _clientPingStopwatch.Start();
        }

        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
        }

        public GameClient GetClientByUserId(int userId)
        {
            return _userIdRegister.ContainsKey(userId) ? _userIdRegister[userId] : null;
        }

        public GameClient GetClientByUsername(string username)
        {
            return _usernameRegister.ContainsKey(username.ToLower()) ? _usernameRegister[username.ToLower()] : null;
        }

        public bool TryGetClient(int clientId, out GameClient client)
        {
            return _clients.TryGetValue(clientId, out client);
        }

        public bool UpdateClientUsername(GameClient client, string oldUsername, string newUsername)
        {
            if (client == null || !_usernameRegister.ContainsKey(oldUsername.ToLower()))
            {
                return false;
            }

            _usernameRegister.TryRemove(oldUsername.ToLower(), out client);
            _usernameRegister.TryAdd(newUsername.ToLower(), client);
            return true;
        }

        public string GetNameById(int id)
        {
            var client = GetClientByUserId(id);

            if (client != null)
            {
                return client.GetHabbo().Username;
            }

            string username;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT username FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", id);
                username = dbClient.GetString();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (var id in users)
            {
                var client = GetClientByUserId(id);
                if (client != null)
                {
                    yield return client;
                }
            }
        }

        public void StaffAlert(ServerPacket message, int exclude = 0)
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }

                if (client.GetHabbo().Rank < 2 || client.GetHabbo().Id == exclude)
                {
                    continue;
                }

                client.SendPacket(message);
            }
        }

        public void ModAlert(string message)
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert"))
                {
                    try { client.SendWhisper(message, 5); }
                    catch { }
                }
            }
        }

        public void DoAdvertisingReport(GameClient reporter, GameClient target)
        {
            if (reporter == null || target == null || reporter.GetHabbo() == null || target.GetHabbo() == null)
            {
                return;
            }

            var builder = new StringBuilder();
            builder.Append("New report submitted!\r\r");
            builder.Append("Reporter: " + reporter.GetHabbo().Username + "\r");
            builder.Append("Reported User: " + target.GetHabbo().Username + "\r\r");
            builder.Append(target.GetHabbo().Username + "s last 10 messages:\r\r");

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + target.GetHabbo().Id + "' ORDER BY `id` DESC LIMIT 10");
                var logs = dbClient.GetTable();

                if (logs != null)
                {
                    var number = 11;
                    foreach (DataRow log in logs.Rows)
                    {
                        number -= 1;
                        builder.Append(number + ": " + Convert.ToString(log["message"]) + "\r");
                    }
                }
            }

            foreach (var client in GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions().HasRight("staff_ignore_advertisement_reports"))
                {
                    client.SendPacket(new MotdNotificationComposer(builder.ToString()));
                }
            }
        }


        public void SendPacket(ServerPacket packet, string fuse = "")
        {
            foreach (var client in _clients.Values.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(fuse))
                {
                    if (!client.GetHabbo().GetPermissions().HasRight(fuse))
                    {
                        continue;
                    }
                }

                client.SendPacket(packet);
            }
        }

        public void CreateAndStartClient(int clientId, ConnectionInformation connection)
        {
            var client = new GameClient(clientId, connection);
            if (_clients.TryAdd(client.ConnectionId, client))
            {
                client.StartConnection();
            }
            else
            {
                connection.Dispose();
            }
        }

        public void DisposeConnection(int clientId)
        {
            if (!TryGetClient(clientId, out var client))
            {
                return;
            }

            if (client != null)
            {
                client.Dispose();
            }

            _clients.TryRemove(clientId, out client);
        }

        public void LogClonesOut(int userId)
        {
            var client = GetClientByUserId(userId);
            if (client != null)
            {
                client.Disconnect();
            }
        }

        public void RegisterClient(GameClient client, int userId, string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
            {
                _usernameRegister[username.ToLower()] = client;
            }
            else
            {
                _usernameRegister.TryAdd(username.ToLower(), client);
            }

            if (_userIdRegister.ContainsKey(userId))
            {
                _userIdRegister[userId] = client;
            }
            else
            {
                _userIdRegister.TryAdd(userId, client);
            }
        }

        public void UnregisterClient(int userid, string username)
        {
            _userIdRegister.TryRemove(userid, out _);
            _usernameRegister.TryRemove(username.ToLower(), out _);
        }

        public void CloseAll()
        {
            foreach (var client in GetClients.ToList())
            {
                if (client == null)
                {
                    continue;
                }

                if (client.GetHabbo() != null)
                {
                    try
                    {
                        using (var dbClient = Program.DatabaseManager.GetQueryReactor())
                        {
                            dbClient.RunQuery(client.GetHabbo().GetQueryString);
                        }
                        Console.Clear();
                        Logger.Trace("<<- SERVER SHUTDOWN ->> IVNENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            Logger.Trace("Done saving users inventory!");
            Logger.Trace("Closing server connections...");
            try
            {
                foreach (var client in GetClients.ToList())
                {
                    if (client == null || client.GetConnection() == null)
                    {
                        continue;
                    }

                    try
                    {
                        client.GetConnection().Dispose();
                    }
                    catch { }

                    Console.Clear();
                    Logger.Trace("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");

                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            if (_clients.Count > 0)
            {
                _clients.Clear();
            }

            Logger.Trace("Connections closed!");
        }

        private void TestClientConnections()
        {
            if (_clientPingStopwatch.ElapsedMilliseconds >= 30000)
            {
                _clientPingStopwatch.Restart();

                try
                {
                    var toPing = new List<GameClient>();

                    foreach (var client in _clients.Values.ToList())
                    {
                        if (client.PingCount < 6)
                        {
                            client.PingCount++;

                            toPing.Add(client);
                        }
                        else
                        {
                            lock (_timedOutConnections.SyncRoot)
                            {
                                _timedOutConnections.Enqueue(client);
                            }
                        }
                    }

                    var start = DateTime.Now;

                    foreach (var client in toPing.ToList())
                    {
                        try
                        {
                            client.SendPacket(new PongComposer());
                        }
                        catch
                        {
                            lock (_timedOutConnections.SyncRoot)
                            {
                                _timedOutConnections.Enqueue(client);
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    //ignored
                }
            }
        }

        private void HandleTimeouts()
        {
            if (_timedOutConnections.Count > 0)
            {
                lock (_timedOutConnections.SyncRoot)
                {
                    while (_timedOutConnections.Count > 0)
                    {
                        GameClient client = null;

                        if (_timedOutConnections.Count > 0)
                        {
                            client = (GameClient)_timedOutConnections.Dequeue();
                        }

                        if (client != null)
                        {
                            client.Disconnect();
                        }
                    }
                }
            }
        }

        public int Count => _clients.Count;

        public ICollection<GameClient> GetClients => _clients.Values;
    }
}