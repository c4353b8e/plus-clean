namespace Plus.Game.Rooms.Chat.Logs
{
    using System;
    using Users;

    public sealed class ChatlogEntry
    {
        private readonly WeakReference _playerReference;
        private readonly WeakReference _roomReference;

        public ChatlogEntry(int PlayerId, int RoomId, string Message, double Timestamp, Habbo Player = null, RoomData Instance = null)
        {
            this.PlayerId = PlayerId;
            this.RoomId = RoomId;
            this.Message = Message;
            this.Timestamp = Timestamp;

            if (Player != null)
            {
                _playerReference = new WeakReference(Player);
            }

            if (Instance != null)
            {
                _roomReference = new WeakReference(Instance);
            }
        }

        public int PlayerId { get; }

        public int RoomId { get; }

        public string Message { get; }

        public double Timestamp { get; }

        public Habbo PlayerNullable()
        {
            if (_playerReference.IsAlive)
            {
                var PlayerObj = (Habbo)_playerReference.Target;

                return PlayerObj;
            }

            return null;
        }

        public Room RoomNullable()
        {
            if (_roomReference.IsAlive)
            {
                var RoomObj = (Room)_roomReference.Target;
                if (RoomObj.mDisposed)
                {
                    return null;
                }

                return RoomObj;
            }
            return null;
        }
    }
}
