namespace Plus.Game.Items.Interactor
{
    using Communication.Packets.Outgoing.Rooms.Furni.Wired;
    using Players;
    using Wired;

    public class InteractorWired : IFurniInteractor
    {
        public void OnPlace(Player Session, Item Item)
        {
        }

        public void OnRemove(Player Session, Item Item)
        {
            //Room Room = Item.GetRoom();
            //Room.GetWiredHandler().RemoveWired(Item);
        }

        public void OnTrigger(Player Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null || Item == null)
            {
                return;
            }

            if (!HasRights)
            {
                return;
            }

            IWiredItem Box = null;
            if (!Item.GetRoom().GetWired().TryGet(Item.Id, out Box))
            {
                return;
            }

            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Item.RequestUpdate(2, true);

            if (Item.GetBaseItem().WiredType == WiredBoxType.AddonRandomEffect)
            {
                return;
            }

            if (Item.GetRoom().GetWired().IsTrigger(Item))
            {
                var BlockedItems = WiredBoxTypeUtility.ContainsBlockedEffect(Box, Item.GetRoom().GetWired().GetEffects(Box));
                Session.SendPacket(new WiredTriggeRconfigComposer(Box, BlockedItems));
            }
            else if (Item.GetRoom().GetWired().IsEffect(Item))
            {
                var BlockedItems = WiredBoxTypeUtility.ContainsBlockedTrigger(Box, Item.GetRoom().GetWired().GetTriggers(Box));
                Session.SendPacket(new WiredEffectConfigComposer(Box, BlockedItems));
            }
            else if (Item.GetRoom().GetWired().IsCondition(Item))
            {
                Session.SendPacket(new WiredConditionConfigComposer(Box));
            }
        }


        public void OnWiredTrigger(Item Item)
        {
        }
    }
}