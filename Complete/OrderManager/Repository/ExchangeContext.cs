using System.Linq;
using Almirex.Contracts.Messages;
using Microsoft.EntityFrameworkCore;

namespace OrderManager.Repository
{
    public class OrderManagerContext : DbContext
    {   
        public OrderManagerContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ExecutionReport> ExecutionReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionReport>().ToTable("execution_report");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrderId).HasColumnName("order_id");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.AvgPx).HasColumnName("avg_px");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.ClOrdID).HasColumnName("cl_ordid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.ClientID).HasColumnName("clientid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.CumQty).HasColumnName("cum_qty");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.CummCommission).HasColumnName("cumm_commission");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.CxlRejReason).HasColumnName("cxl_rej_reason");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.EscrowRestricted).HasColumnName("escrow_restricted");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.ExecId).HasColumnName("exec_id");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.ExecRefID).HasColumnName("exec_refid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.ExecType).HasColumnName("exec_type");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.Fee).HasColumnName("fee");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.LastCommission).HasColumnName("last_commission");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.LastLiquidityInd).HasColumnName("last_liquidity_ind");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.LastPx).HasColumnName("last_px");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.LastQty).HasColumnName("last_qty");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.LeavesEscrow).HasColumnName("leaves_escrow");
//            modelBuilder.Entity<ExecutionReport>().Property(x => x.LeavesQty).HasColumnName("leaves_qty");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.MassStatusReqID).HasColumnName("mass_status_reqid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrdRejReason).HasColumnName("ord_rej_reason");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrdStatus).HasColumnName("ord_status");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrdStatusReqID).HasColumnName("ord_status_reqid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrdType).HasColumnName("ord_type");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrderQty).HasColumnName("order_qty");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrigClOrdID).HasColumnName("orig_cl_ordid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.OrigEscrow).HasColumnName("orig_escrow");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.PegOffset).HasColumnName("peg_offset");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.PegPriceType).HasColumnName("peg_price_type");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.PegScope).HasColumnName("peg_scope");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.PeggedPrice).HasColumnName("pegged_price");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.Price).HasColumnName("price");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.SecondaryOrderId).HasColumnName("secondary_order_id");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.SeqNum).HasColumnName("seq_num");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.Side).HasColumnName("side");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.StopPx).HasColumnName("stop_px");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.SubmitTime).HasColumnName("submit_time");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.Symbol).HasColumnName("symbol");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TimeInForce).HasColumnName("time_in_force");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TotalNumReports).HasColumnName("total_num_reports");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TradeId).HasColumnName("trade_id");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TransactTime).HasColumnName("transact_time");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TrdMatchID).HasColumnName("trd_matchid");
            modelBuilder.Entity<ExecutionReport>().Property(x => x.TriggerPriceType).HasColumnName("trigger_price_type");

            modelBuilder.Entity<ExecutionReport>().HasKey(x => x.OrderId);

        }
    }
}
