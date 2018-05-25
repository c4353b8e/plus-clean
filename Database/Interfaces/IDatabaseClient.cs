namespace Plus.Database.Interfaces
{
    using System;
    using MySql.Data.MySqlClient;

    public interface IDatabaseClient : IDisposable
    {
        void Connect();
        void Disconnect();
        IQueryAdapter GetQueryReactor();
        MySqlCommand CreateNewCommand();
    }
}