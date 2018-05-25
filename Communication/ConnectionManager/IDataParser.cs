namespace Plus.Communication.ConnectionManager
{
    using System;

    public interface IDataParser : IDisposable, ICloneable
    {
        void HandlePacketData(byte[] packet);
    }
}