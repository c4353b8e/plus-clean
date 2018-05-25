namespace Plus.Communication.ConnectionManager
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using Core.Logging;
    using Socket_Exceptions;

    public class SocketManager
    {
        private static readonly ILogger Logger = new Logger<SocketManager>();

        #region declares

        public delegate void ConnectionEvent(ConnectionInformation connection);

        private bool _acceptConnections;

        private int _acceptedConnections;

        private Socket _connectionListener;

        private bool _disableNagleAlgorithm;

        private int _maxIpConnectionCount;

        private int _maximumConnections;

        private IDataParser _parser;

        private int _portInformation;

        public event ConnectionEvent OnConnectionEvent;

        //private Dictionary<string, int> ipConnectionCount;
        private ConcurrentDictionary<string, int> _ipConnectionsCount;
        #endregion

        #region initializer

        public void Init(int portId, int maxConnections, int connectionsPerIp, IDataParser parser,  bool disableNaglesAlgorithm)
        {
            _ipConnectionsCount = new ConcurrentDictionary<string, int>();

            _parser = parser;
            _disableNagleAlgorithm = disableNaglesAlgorithm;
            _maximumConnections = maxConnections;
            _portInformation = portId;
            _maxIpConnectionCount = connectionsPerIp;
            PrepareConnectionDetails();
            _acceptedConnections = 0;
            Logger.Trace("Listening for connections on port: " + portId + "");
        }

        private void PrepareConnectionDetails()
        {
            _connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = _disableNagleAlgorithm
            };

            try
            {
                _connectionListener.Bind(new IPEndPoint(IPAddress.Any, _portInformation));
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        public void InitializeConnectionRequests()
        {
            //Out.writeLine("Starting to listen to connection requests", Out.logFlags.ImportantLogLevel);
            _connectionListener.Listen(100);
            _acceptConnections = true;

            try
            {
                _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
            }
            catch
            {
                Destroy();
            }
        }

        #endregion

        #region destructor

        public void Destroy()
        {
            _acceptConnections = false;
            try { _connectionListener.Close(); }
            catch {
                //ignored
            }
            _connectionListener = null;
        }

        #endregion

        #region connection request

        private void NewConnectionRequest(IAsyncResult iAr)
        {
            if (_connectionListener != null)
            {
                if (_acceptConnections)
                {
                    try
                    {
                        var replyFromComputer = ((Socket)iAr.AsyncState).EndAccept(iAr);
                        replyFromComputer.NoDelay = _disableNagleAlgorithm;

                        var ip = replyFromComputer.RemoteEndPoint.ToString().Split(':')[0];

                        var connectionCount = GetAmountOfConnectionFromIp(ip);
                        if (connectionCount < _maxIpConnectionCount)
                        {
                            _acceptedConnections++;
                            var c = new ConnectionInformation(_acceptedConnections, replyFromComputer, _parser.Clone() as IDataParser, ip);
                            ReportUserLogin(ip);
                            c.ConnectionChanged += OnConnectionChanged;

                            if (OnConnectionEvent != null)
                            {
                                OnConnectionEvent(c);
                            }
                        }
                        else
                        {
                            Logger.Trace("Connection denied from [" + replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] + "]. Too many connections (" + connectionCount + ").");
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
                    }
                }
            }
        }

        private void OnConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
            {
                ReportDisconnect(information);
            }
        }

        #endregion

        #region connection disconnected

        public void ReportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.ConnectionChanged -= OnConnectionChanged;
            ReportUserLogout(gameConnection.GetIp());
            //activeConnections.Remove(gameConnection.getConnectionID());
        }

        #endregion

        #region ip connection management

        private void ReportUserLogin(string ip)
        {
            AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) + 1);
        }

        private void ReportUserLogout(string ip)
        {
            AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) - 1);
        }

        private void AlterIpConnectionCount(string ip, int amount)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                _ipConnectionsCount.TryRemove(ip, out int _);
            }
            _ipConnectionsCount.TryAdd(ip, amount);
        }

        private int GetAmountOfConnectionFromIp(string ip)
        {
            if (_ipConnectionsCount.ContainsKey(ip))
            {
                return _ipConnectionsCount[ip];
            }

            return 0;
        }

        #endregion
    }
}