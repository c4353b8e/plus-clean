namespace Plus.Game.Rooms.Chat.Commands.Administrator
{
    using System.Linq;
    using Communication.Packets.Outgoing.Catalog;
    using Players;

    internal class UpdateCommand : IChatCommand
    {
        public string PermissionRequired => "command_update";

        public string Parameters => "%variable%";

        public string Description => "Reload a specific part of the hotel.";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("You must inculde a thing to update, e.g. :update catalog");
                return;
            }

            var UpdateVariable = Params[1];
            switch (UpdateVariable.ToLower())
            {
                case "cata":
                case "catalog":
                case "catalogue":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_catalog' permission.");
                            break;
                        }

                        Program.GameContext.GetCatalog().Init(Program.GameContext.GetItemManager());
                        Program.GameContext.PlayerController.SendPacket(new CatalogUpdatedComposer());
                        Session.SendWhisper("Catalogue successfully updated.");
                        break;
                    }

                case "items":
                case "furni":
                case "furniture":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_furni"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_furni' permission.");
                            break;
                        }

                        Program.GameContext.GetItemManager().Init();
                        Session.SendWhisper("Items successfully updated.");
                        break;
                    }

                case "models":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_models"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_models' permission.");
                            break;
                        }

                        Program.GameContext.GetRoomManager().LoadModels();
                        Session.SendWhisper("Room models successfully updated.");
                        break;
                    }

                case "promotions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_promotions"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_promotions' permission.");
                            break;
                        }

                        Program.GameContext.GetLandingManager().Init();
                        Session.SendWhisper("Landing view promotions successfully updated.");
                        break;
                    }

                case "youtube":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_youtube"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_youtube' permission.");
                            break;
                        }

                        Program.GameContext.GetItemManager().GetTelevisionManager().Init();
                        Session.SendWhisper("Youtube televisions playlist successfully updated.");
                        break;
                    }

                case "filter":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_filter"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_filter' permission.");
                            break;
                        }

                        Program.GameContext.GetChatManager().GetFilter().Init();
                        Session.SendWhisper("Filter definitions successfully updated.");
                        break;
                    }

                case "navigator":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_navigator"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_navigator' permission.");
                            break;
                        }

                        Program.GameContext.GetNavigator().Init();
                        Session.SendWhisper("Navigator items successfully updated.");
                        break;
                    }

                case "ranks":
                case "rights":
                case "permissions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_rights"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_rights' permission.");
                            break;
                        }

                        Program.GameContext.GetPermissionManager().Init();

                        foreach (var Client in Program.GameContext.PlayerController.GetClients.ToList())
                        {
                            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetPermissions() == null)
                            {
                                continue;
                            }

                            Client.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }

                        Session.SendWhisper("Rank definitions successfully updated.");
                        break;
                    }

                case "config":
                case "settings":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_configuration"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_configuration' permission.");
                            break;
                        }

                        Program.SettingsManager.Init();
                        Session.SendWhisper("Server configuration successfully updated.");
                        break;
                    }

                case "bans":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bans"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_bans' permission.");
                            break;
                        }

                        Program.GameContext.GetModerationManager().ReCacheBans();
                        Session.SendWhisper("Ban cache re-loaded.");
                        break;
                    }

                case "quests":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_quests"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_quests' permission.");
                            break;
                        }

                        Program.GameContext.GetQuestManager().Init();
                        Session.SendWhisper("Quest definitions successfully updated.");
                        break;
                    }

                case "achievements":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_achievements"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_achievements' permission.");
                            break;
                        }

                        Program.GameContext.GetAchievementManager().Init();
                        Session.SendWhisper("Achievement definitions bans successfully updated.");
                        break;
                    }

                case "moderation":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_moderation"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_moderation' permission.");
                            break;
                        }

                        Program.GameContext.GetModerationManager().Init();
                        Program.GameContext.PlayerController.ModAlert("Moderation presets have been updated. Please reload the client to view the new presets.");

                        Session.SendWhisper("Moderation configuration successfully updated.");
                        break;
                    }

                case "vouchers":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_vouchers"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_vouchers' permission.");
                            break;
                        }

                        Program.GameContext.GetCatalog().GetVoucherManager().Init();
                        Session.SendWhisper("Catalogue vouche cache successfully updated.");
                        break;
                    }

                case "gc":
                case "games":
                case "gamecenter":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_game_center"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_game_center' permission.");
                            break;
                        }

                        Program.GameContext.GetGameDataManager().Init();
                        Session.SendWhisper("Game Center cache successfully updated.");
                        break;
                    }

                case "pet_locale":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_pet_locale"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_pet_locale' permission.");
                            break;
                        }

                        Program.GameContext.GetChatManager().GetPetLocale().Init();
                        Session.SendWhisper("Pet locale cache successfully updated.");
                        break;
                    }

                case "locale":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_locale"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_locale' permission.");
                            break;
                        }

                        Program.LanguageManager.Init();
                        Session.SendWhisper("Locale cache successfully updated.");
                        break;
                    }

                case "mutant":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_anti_mutant"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_anti_mutant' permission.");
                            break;
                        }

                        Program.FigureManager.Load();
                        Session.SendWhisper("FigureData manager successfully reloaded.");
                        break;
                    }

                case "bots":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bots"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_bots' permission.");
                            break;
                        }

                        Program.GameContext.GetBotManager().Init();
                        Session.SendWhisper("Bot managaer successfully reloaded.");
                        break;
                    }

                case "rewards":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_rewards"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_rewards' permission.");
                            break;
                        }

                        Program.GameContext.GetRewardManager().Init();
                        Session.SendWhisper("Rewards managaer successfully reloaded.");
                        break;
                    }

                case "chat_styles":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_chat_styles"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_chat_styles' permission.");
                            break;
                        }

                        Program.GameContext.GetChatManager().GetChatStyles().Init();
                        Session.SendWhisper("Chat Styles successfully reloaded.");
                        break;
                    }

                case "badge_definitions":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_badge_definitions"))
                        {
                            Session.SendWhisper("Oops, you do not have the 'command_update_badge_definitions' permission.");
                            break;
                        }

                        Program.GameContext.GetBadgeManager().Init();
                        Session.SendWhisper("Badge definitions successfully reloaded.");
                        break;
                    }

                default:
                    Session.SendWhisper("'" + UpdateVariable + "' is not a valid thing to reload.");
                    break;
            }
        }
    }
}
