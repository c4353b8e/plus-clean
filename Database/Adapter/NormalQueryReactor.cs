namespace Plus.Database.Adapter
{
    using System;
    using Interfaces;

    public class NormalQueryReactor : QueryAdapter, IQueryAdapter
    {
        public NormalQueryReactor(IDatabaseClient client)
            : base(client)
        {
            Command = client.CreateNewCommand();
        }

        public void Dispose()
        {
            Command.Dispose();
            Client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}