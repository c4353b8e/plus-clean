namespace Plus.Game.Items.Interactor
{
    using Players;
    using Quests;
    using Rooms;

    internal class InteractorSwitch : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {

        }

        public void OnRemove(Player Session, Item Item)
        {

        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
            {
                return;
            }

            var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                var Modes = Item.GetBaseItem().Modes - 1;

                if (Modes <= 0)
                {
                    return;
                }

                Program.GameContext.GetQuestManager().ProgressUserQuest(Session, QuestType.FurniSwitch);

                var CurrentMode = 0;
                var NewMode = 0;

                if (!int.TryParse(Item.ExtraData, out CurrentMode))
                {
                }

                if (CurrentMode <= 0)
                {
                    NewMode = 1;
                }
                else if (CurrentMode >= Modes)
                {
                    NewMode = 0;
                }
                else
                {
                    NewMode = CurrentMode + 1;
                }

                Item.ExtraData = NewMode.ToString();
                Item.UpdateState();
            }
            else
            {
                User.MoveTo(Item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
          
        }
    }
}