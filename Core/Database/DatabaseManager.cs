namespace Plus.Core.Database
{
    using System;
    using Config;
    using Interfaces;
    using Logging;
    using MySql.Data.MySqlClient;

    public sealed class DatabaseManager
    {
        private static readonly ILogger Logger = new Logger<DatabaseManager>();

        private readonly string _connectionStr;

        public DatabaseManager(ConfigHandler _configuration)
        {
            _connectionStr = new MySqlConnectionStringBuilder
            {
                ConnectionTimeout = 10,
                Database = _configuration["db.name"],
                DefaultCommandTimeout = 30,
                Logging = false,
                MaximumPoolSize = uint.Parse(_configuration["db.pool.maxsize"]),
                MinimumPoolSize = uint.Parse(_configuration["db.pool.minsize"]),
                Password = _configuration["db.password"],
                Pooling = true,
                Port = uint.Parse(_configuration["db.port"]),
                Server = _configuration["db.hostname"],
                UserID = _configuration["db.username"],
                AllowZeroDateTime = true,
                ConvertZeroDateTime = true
            }.ToString();


            if (IsConnected())
            {
                Logger.Trace("Connected to Database!");
                ResetDatabase();
                return;
            }

            Logger.Error("Failed to Connect to the specified MySQL server.");
            Console.ReadKey(true);
            Environment.Exit(1);
        }

        private bool IsConnected()
        {
            try
            {
                var con = new MySqlConnection(_connectionStr);
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT 1+1";
                cmd.ExecuteNonQuery();

                cmd.Dispose();
                con.Close();
            }
            catch (MySqlException)
            {
                return false;
            }

            return true;
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient dbConnection = new DatabaseConnection(_connectionStr);
              
                dbConnection.Connect();

                return dbConnection.GetQueryReactor();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void ResetDatabase()
        {
            using (var dbClient = GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET `online` = '0', `auth_ticket` = NULL");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
            }
        }
    }
}