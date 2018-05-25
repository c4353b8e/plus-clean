namespace Plus.Communication.ConnectionManager
{
    using System;
    using Core.Logging;

    public class ConnectionHandling
    {
        private static readonly ILogger Logger = new Logger<ConnectionHandling>();

        private readonly SocketManager _manager;

        public ConnectionHandling(int port, int maxConnections, int connectionsPerIp, bool enabeNagles)
        {
            _manager = new SocketManager();
            _manager.Init(port, maxConnections, connectionsPerIp, new InitialPacketParser(), !enabeNagles);
            _manager.OnConnectionEvent += OnConnectionEvent;
            _manager.InitializeConnectionRequests();
        }
        
        private void OnConnectionEvent(ConnectionInformation connection)
        {
            connection.ConnectionChanged += OnConnectionChanged;
            Program.GameContext.GetClientManager().CreateAndStartClient(Convert.ToInt32(connection.GetConnectionId()), connection);
        }

        private void OnConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
            {
                CloseConnection(information);
            }
        }

        private void CloseConnection(ConnectionInformation connection)
        {
            try
            {
                connection.Dispose();
                Program.GameContext.GetClientManager().DisposeConnection(Convert.ToInt32( connection.GetConnectionId()));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Destroy()
        {
            _manager.Destroy();
        }
    }
}