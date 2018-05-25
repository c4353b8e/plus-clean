namespace Plus.Game.Rooms.Chat.Commands.Events
{
    using System;
    using Communication.Packets.Outgoing.Moderation;
    using Players;

    internal class EventAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";

        public string Parameters => "";

        public string Description => "Send a hotel alert for your event!";

        public void Execute(Player Session, Room Room, string[] Params)
        {
            if (Session == null)
            {
                return;
            }

            if (Room != null)
            {
                if (Params.Length != 1)
                {
                    Session.SendWhisper("Invalid command! :eventalert", 0);
                }
                else if (Program.GameContext.lastEvent == DateTime.MinValue)
                {
                    Program.GameContext.PlayerController.SendPacket(new BroadcastMessageAlertComposer(":follow " + Session.GetHabbo().Username + " for events! win prizes!\r\n- " + Session.GetHabbo().Username, ""), "");
                    Program.GameContext.lastEvent = DateTime.Now;
                }
                else
                {
                    var timeSpan = DateTime.Now - Program.GameContext.lastEvent;
                    if (timeSpan.Hours >= 1)
                    {
                        Program.GameContext.PlayerController.SendPacket(new BroadcastMessageAlertComposer(":follow " + Session.GetHabbo().Username + " for events! win prizes!\r\n- " + Session.GetHabbo().Username, ""), "");
                        Program.GameContext.lastEvent = DateTime.Now;
                    }
                    else
                    {
                        var num = checked(60 - timeSpan.Minutes);
                        Session.SendWhisper("Event Cooldown! " + num + " minutes left until another event can be hosted.", 0);
                    }
                }
            }
        }
    }
}
