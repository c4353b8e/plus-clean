namespace Plus.Game.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Core.Logging;
    using Users;

    public sealed class PermissionManager
    {
        private static readonly ILogger Logger = new Logger<PermissionManager>();

        private readonly Dictionary<int, Permission> Permissions = new Dictionary<int, Permission>();

        private readonly Dictionary<string, PermissionCommand> _commands = new Dictionary<string, PermissionCommand>();

        private readonly Dictionary<int, PermissionGroup> PermissionGroups = new Dictionary<int, PermissionGroup>();

        private readonly Dictionary<int, List<string>> PermissionGroupRights = new Dictionary<int, List<string>>();

        private readonly Dictionary<int, List<string>> PermissionSubscriptionRights = new Dictionary<int, List<string>>();

        public PermissionManager()
        {
            Init();
        }

        public void Init()
        {
            Permissions.Clear();
            _commands.Clear();
            PermissionGroups.Clear();
            PermissionGroupRights.Clear();

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions`");
                var GetPermissions = dbClient.GetTable();

                if (GetPermissions != null)
                {
                    foreach (DataRow Row in GetPermissions.Rows)
                    {
                        Permissions.Add(Convert.ToInt32(Row["id"]), new Permission(Convert.ToInt32(Row["id"]), Convert.ToString(Row["permission"]), Convert.ToString(Row["description"])));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_commands`");
                var GetCommands = dbClient.GetTable();

                if (GetCommands != null)
                {
                    foreach (DataRow Row in GetCommands.Rows)
                    {
                         _commands.Add(Convert.ToString(Row["command"]), new PermissionCommand(Convert.ToString(Row["command"]), Convert.ToInt32(Row["group_id"]), Convert.ToInt32(Row["subscription_id"])));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_groups`");
                var GetPermissionGroups = dbClient.GetTable();

                if (GetPermissionGroups != null)
                {
                    foreach (DataRow Row in GetPermissionGroups.Rows)
                    {
                        PermissionGroups.Add(Convert.ToInt32(Row["id"]), new PermissionGroup(Convert.ToString("name"), Convert.ToString("description"), Convert.ToString("badge")));
                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_rights`");
                var GetPermissionRights = dbClient.GetTable();

                if (GetPermissionRights != null)
                {
                    foreach (DataRow Row in GetPermissionRights.Rows)
                    {
                        var GroupId = Convert.ToInt32(Row["group_id"]);
                        var PermissionId = Convert.ToInt32(Row["permission_id"]);

                        if (!PermissionGroups.ContainsKey(GroupId))
                        {
                            continue; // permission group does not exist
                        }

                        Permission Permission = null;

                        if (!Permissions.TryGetValue(PermissionId, out Permission))
                        {
                            continue; // permission does not exist
                        }

                        if (PermissionGroupRights.ContainsKey(GroupId))
                        {
                            PermissionGroupRights[GroupId].Add(Permission.PermissionName);
                        }
                        else
                        {
                            var RightsSet = new List<string>
                            {
                                    Permission.PermissionName
                                };

                            PermissionGroupRights.Add(GroupId, RightsSet);
                        }

                    }
                }
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_subscriptions`");
                var GetPermissionSubscriptions = dbClient.GetTable();

                if (GetPermissionSubscriptions != null)
                {
                    foreach (DataRow Row in GetPermissionSubscriptions.Rows)
                    {
                        var PermissionId = Convert.ToInt32(Row["permission_id"]);
                        var SubscriptionId = Convert.ToInt32(Row["subscription_id"]);

                        Permission Permission = null;
                        if (!Permissions.TryGetValue(PermissionId, out Permission))
                        {
                            continue; // permission does not exist
                        }

                        if (PermissionSubscriptionRights.ContainsKey(SubscriptionId))
                        {
                            PermissionSubscriptionRights[SubscriptionId].Add(Permission.PermissionName);
                        }
                        else
                        {
                            var RightsSet = new List<string>
                            {
                                    Permission.PermissionName
                                };

                            PermissionSubscriptionRights.Add(SubscriptionId, RightsSet);
                        }
                    }
                }
            }

            Logger.Trace("Loaded permission manager");
        }

        public bool TryGetGroup(int Id, out PermissionGroup Group)
        {
            return PermissionGroups.TryGetValue(Id, out Group);
        }

        public List<string> GetPermissionsForPlayer(Habbo Player)
        {
            var PermissionSet = new List<string>();

            List<string> PermRights = null;
            if (PermissionGroupRights.TryGetValue(Player.Rank, out PermRights))
            {
                PermissionSet.AddRange(PermRights);
            }

            List<string> SubscriptionRights = null;
            if (PermissionSubscriptionRights.TryGetValue(Player.VIPRank, out SubscriptionRights))
            {
                PermissionSet.AddRange(SubscriptionRights);
            }

            return PermissionSet;
        }

        public List<string> GetCommandsForPlayer(Habbo Player)
        {
            return _commands.Where(x => Player.Rank >= x.Value.GroupId && Player.VIPRank >= x.Value.SubscriptionId).Select(x => x.Key).ToList();
        }
    }
}