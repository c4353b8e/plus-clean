namespace Plus.Communication.Rcon
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using Core.Logging;

    public class RconConnection
    {
        private Socket _socket;
        private byte[] _buffer = new byte[1024];

        private static readonly ILogger Logger = new Logger<RconConnection>();

        public RconConnection(Socket socket)
        {
            _socket = socket;

            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnCallBack, _socket);
            }
            catch { Dispose(); }
        }

        public void OnCallBack(IAsyncResult iAr)
        {
            try
            {
                if (!int.TryParse(_socket.EndReceive(iAr).ToString(), out var bytes))
                {
                    Dispose();
                    return;
                }

                var data = Encoding.Default.GetString(_buffer, 0, bytes);
                if (!Program.RconSocket.GetCommands().Parse(data))
                {
                    Logger.Error("Failed to execute a MUS command. Raw data: " + data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Dispose();
        }

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
            }
            
            _socket = null;
            _buffer = null;
        }
    }
}
