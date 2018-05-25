namespace Plus.Communication.Packets.Incoming.Catalog
{
    using System.Data;
    using HabboHotel.Catalog.Vouchers;
    using HabboHotel.GameClients;
    using Outgoing.Catalog;
    using Outgoing.Inventory.Purse;

    public class RedeemVoucherEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var code = packet.PopString().Replace("\r", "");

            if (!Program.GameContext.GetCatalog().GetVoucherManager().TryGetVoucher(code, out var voucher))
            {
                session.SendPacket(new VoucherRedeemErrorComposer(0));
                return;
            }

            if (voucher.CurrentUses >= voucher.MaxUses)
            {
                session.SendNotification("Oops, this voucher has reached the maximum usage limit!");
                return;
            }

            DataRow row;
            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_vouchers` WHERE `user_id` = @userId AND `voucher` = @Voucher LIMIT 1");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("Voucher", code);
                row = dbClient.GetRow();
            }

            if (row != null)
            {
                session.SendNotification("You've already used this voucher code, one per each user, sorry!");
                return;
            }

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_vouchers` (`user_id`,`voucher`) VALUES (@userId, @Voucher)");
                dbClient.AddParameter("userId", session.GetHabbo().Id);
                dbClient.AddParameter("Voucher", code);
                dbClient.RunQuery();
            }

            voucher.UpdateUses();

            if (voucher.Type == VoucherType.Credit)
            {
                session.GetHabbo().Credits += voucher.Value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }
            else if (voucher.Type == VoucherType.Ducket)
            {
                session.GetHabbo().Duckets += voucher.Value;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, voucher.Value));
            }

            session.SendPacket(new VoucherRedeemOkComposer());
        }
    }
}