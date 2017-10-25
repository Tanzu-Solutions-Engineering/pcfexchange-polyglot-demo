using Almirex.Contracts.Messages;
using Microsoft.EntityFrameworkCore;

namespace Exchange.Repository
{
    public class ExchangeContext : DbContext
    {
        public ExchangeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ExecutionReport> ExecutionReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionReport>().ToTable("ExecutionReport");
            modelBuilder.Entity<ExecutionReport>().HasKey(x => x.ExecId);
            modelBuilder.Entity<ExecutionReport>().HasIndex(x => new { x.OrdStatus });

        }
    }
}
