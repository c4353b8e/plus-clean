namespace Plus
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;
    using Communication.ConnectionManager;
    using Communication.Packets;
    using Communication.Rcon;
    using Core;
    using Core.Config;
    using Core.Database;
    using Core.FigureData;
    using Core.Language;
    using Core.Logging;
    using Core.Settings;
    using Game;
    using Game.Players;

    public static class Program
    {
        private static readonly ILogger Logger = new Logger<ServerStatusUpdater>();

        private static ConnectionHandling _connectionManager;
        private static ConfigHandler _configuration;

        public static GameContext GameContext;
        public static RconSocket RconSocket;
        public static FigureDataManager FigureManager;
        public static DatabaseManager DatabaseManager;
        public static LanguageManager LanguageManager;
        public static SettingsManager SettingsManager;

        public static bool EncryptionEnabled;
        public static DateTime ServerStarted;

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;

            AppDomain.CurrentDomain.UnhandledException += Dispose;
            
            try
            {
                _configuration = new ConfigHandler(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())) + "\\Config\\config.ini");

                DatabaseManager = new DatabaseManager(_configuration);
                LanguageManager = new LanguageManager();
                SettingsManager = new SettingsManager();
                FigureManager = new FigureDataManager();

                RconSocket = new RconSocket(_configuration["rcon.tcp.bindip"], int.Parse(_configuration["rcon.tcp.port"]), _configuration["rcon.tcp.allowedaddr"].Split(Convert.ToChar(";")));

                GameContext = new GameContext
                {
                    PacketManager = new PacketManager(),
                    PlayerController = new PlayerController()
                };

                _connectionManager = new ConnectionHandling(int.Parse(_configuration["game.tcp.port"]), int.Parse(_configuration["game.tcp.conlimit"]), int.Parse(_configuration["game.tcp.conperip"]), _configuration["game.tcp.enablenagles"].ToLower() == "true");

                new ServerStatusUpdater();

                Console.WriteLine();

                ServerStarted = DateTime.Now;
                stopwatch.Stop();

                Logger.Debug("Emulator has finished loading. (took " + stopwatch.ElapsedMilliseconds / 1000 + " s, " + (stopwatch.ElapsedMilliseconds - stopwatch.ElapsedMilliseconds / 1000 * 1000) + " ms)");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }

            KeepAlive();
        }

        private static void KeepAlive()
        {
            while (true)
            {
                if (Console.ReadKey(true).Key != ConsoleKey.Enter)
                {
                    continue;
                }

                Console.Write("plus> ");
                var input = Console.ReadLine();

                if (input != null && input.Length <= 0 || input == null)
                {
                    continue;
                }

                ConsoleCommands.InvokeCommand(input.Split(' ')[0]);
            }
        }

        public static void Dispose(object sender, UnhandledExceptionEventArgs args)
        {
            Console.Clear();
            Console.Title = "PLUS EMULATOR: SHUTTING DOWN!";

            _connectionManager.Destroy();

            GameContext.Dispose();
            Thread.Sleep(5000);

            DatabaseManager.ResetDatabase();

            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}