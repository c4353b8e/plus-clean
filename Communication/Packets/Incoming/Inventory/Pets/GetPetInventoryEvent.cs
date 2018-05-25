namespace Plus.Communication.Packets.Incoming.Inventory.Pets
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Pets;

    internal class GetPetInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            var pets = session.GetHabbo().GetInventoryComponent().GetPets();
            session.SendPacket(new PetInventoryComposer(pets));
        }
    }
}