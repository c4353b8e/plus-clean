namespace Plus.Communication.Packets.Incoming.Catalog
{
    using HabboHotel.GameClients;
    using Outgoing.Catalog;

    public class GetSellablePetBreedsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var type = packet.PopString();

            var item = Program.GameContext.GetItemManager().GetItemByName(type);
            if (item == null)
            {
                return;
            }

            var petId = item.BehaviourData;

            session.SendPacket(new SellablePetBreedsComposer(type, petId, Program.GameContext.GetCatalog().GetPetRaceManager().GetRacesForRaceId(petId)));
        }
    }
}