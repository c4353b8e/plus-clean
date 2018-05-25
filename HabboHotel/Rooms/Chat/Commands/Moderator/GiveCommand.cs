namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    using Communication.Packets.Outgoing.Inventory.Purse;
    using GameClients;

    internal class GiveCommand : IChatCommand
    {
        public string PermissionRequired => "command_give";

        public string Parameters => "%username% %type% %amount%";

        public string Description => "";

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a currency type! (coins, duckets, diamonds, gotw)");
                return;
            }

            var Target = Program.GameContext.GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("Oops, couldn't find that user!");
                return;
            }

            var UpdateVal = Params[2];
            switch (UpdateVal.ToLower())
            {
                case "coins":
                case "credits":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_coins"))
                        {
                            Session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            Target.GetHabbo().Credits = Target.GetHabbo().Credits += Amount;
                            Target.SendPacket(new CreditBalanceComposer(Target.GetHabbo().Credits));

                            if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Target.SendNotification(Session.GetHabbo().Username + " has given you " + Amount + " Credit(s)!");
                            }

                            Session.SendWhisper("Successfully given " + Amount + " Credit(s) to " + Target.GetHabbo().Username + "!");
                            break;
                        }

                        Session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }

                case "pixels":
                case "duckets":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_pixels"))
                        {
                            Session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            Target.GetHabbo().Duckets += Amount;
                            Target.SendPacket(new HabboActivityPointNotificationComposer(Target.GetHabbo().Duckets, Amount));

                            if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Target.SendNotification(Session.GetHabbo().Username + " has given you " + Amount + " Ducket(s)!");
                            }

                            Session.SendWhisper("Successfully given " + Amount + " Ducket(s) to " + Target.GetHabbo().Username + "!");
                            break;
                        }

                        Session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }

                case "diamonds":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds"))
                        {
                            Session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            Target.GetHabbo().Diamonds += Amount;
                            Target.SendPacket(new HabboActivityPointNotificationComposer(Target.GetHabbo().Diamonds, Amount, 5));

                            if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Target.SendNotification(Session.GetHabbo().Username + " has given you " + Amount + " Diamond(s)!");
                            }

                            Session.SendWhisper("Successfully given " + Amount + " Diamond(s) to " + Target.GetHabbo().Username + "!");
                            break;
                        }

                        Session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }

                case "gotw":
                case "gotwpoints":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_give_gotw"))
                        {
                            Session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            Target.GetHabbo().GOTWPoints = Target.GetHabbo().GOTWPoints + Amount;
                            Target.SendPacket(new HabboActivityPointNotificationComposer(Target.GetHabbo().GOTWPoints, Amount, 103));

                            if (Target.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                Target.SendNotification(Session.GetHabbo().Username + " has given you " + Amount + " GOTW Point(s)!");
                            }

                            Session.SendWhisper("Successfully given " + Amount + " GOTW point(s) to " + Target.GetHabbo().Username + "!");
                            break;
                        }

                        Session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }
                default:
                    Session.SendWhisper("'" + UpdateVal + "' is not a valid currency!");
                    break;
            }
        }
    }
}