using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DispenserManagement.Model;
using TankGaugeManagement.Model;

namespace DispenserManagement.Model
{
    class PostgresContext : DbContext
    {
        public DbSet<Grades> grades { get; set; }
        public DbSet<Hoses> hoses { get; set; }
        public DbSet<Loops> loops { get; set; }
        public DbSet<Protocols> protocols { get; set; }
        public DbSet<Pumps> pumps { get; set; }
        public DbSet<PumpsRealtime> pumps_real_time { get; set; }
        public DbSet<Transactions> transactions { get; set; }
        public DbSet<Tanks> tanks { get; set; }
        public DbSet<TankGaugeLogs> tank_gauge_logs { get; set; }
        public DbSet<TanksDelivery> tanks_delivery { get; set; }
        public DbSet<TankGaugeFeatures> tank_gauge_features { get; set; }
        public DbSet<TanksRecentDelivery> tanks_recent_delivery { get; set; }
        public DbSet<TanksAdjustDelivery> tanks_adjust_delivery { get; set; }
        public DbSet<SiteConfigs> site_config { get; set; }
        public DbSet<TanksAlarmHistory> tanks_alarm_history { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("lavender");
            builder.Entity<Grades>().HasKey(m => m.grade_id);
            builder.Entity<Hoses>().HasKey(m => m.hose_id);
            builder.Entity<Loops>().HasKey(m => m.loop_id);
            builder.Entity<Protocols>().HasKey(m => m.protocol_id);
            builder.Entity<Pumps>().HasKey(m => m.pump_id);
            builder.Entity<PumpsRealtime>().HasKey(m => m.pump_id);
            builder.Entity<Transactions>().HasKey(m => m.transaction_id);
            builder.Entity<Tanks>().HasKey(m => m.tank_id);
            builder.Entity<TankGaugeLogs>().HasKey(m => m.log_id);
            builder.Entity<TanksDelivery>().HasKey(m => m.tank_delivery_id);
            builder.Entity<TankGaugeFeatures>().HasKey(m => m.feature_id);
            builder.Entity<TanksRecentDelivery>().HasKey(m => m.delivery_id);
            builder.Entity<TanksAdjustDelivery>().HasKey(m => m.delivery_id);
            builder.Entity<SiteConfigs>().HasKey(m => m.config_id);
            builder.Entity<TanksAlarmHistory>().HasKey(m => m.history_id);

            //for map increment primary key 
            builder.Entity<Grades>().Property(m => m.grade_id).HasDefaultValueSql("nextval('\"grade_id\"')");
            builder.Entity<Hoses>().Property(m => m.hose_id).HasDefaultValueSql("nextval('\"hose_id\"')");
            builder.Entity<Loops>().Property(m => m.loop_id).HasDefaultValueSql("nextval('\"loop_id\"')");
            builder.Entity<Protocols>().Property(m => m.protocol_id).HasDefaultValueSql("nextval('\"protocol_id\"')");
            //builder.Entity<Pumps>().Property(m => m.pump_id).HasDefaultValueSql("nextval('\"pump_id\"')");
            builder.Entity<Transactions>().Property(m => m.transaction_id).HasDefaultValueSql("nextval('\"transaction_id\"')");
            builder.Entity<Tanks>().Property(m => m.tank_id).HasDefaultValueSql("nextval('\"tank_id\"')");
            builder.Entity<TankGaugeLogs>().Property(m => m.log_id).HasDefaultValueSql("nextval('\"log_id\"')");
            builder.Entity<TanksDelivery>().Property(m => m.tank_delivery_id).HasDefaultValueSql("nextval('\"tank_delivery_id\"')");
            builder.Entity<TankGaugeFeatures>().Property(m => m.feature_id).HasDefaultValueSql("nextval('\"feature_id\"')");
            builder.Entity<TanksRecentDelivery>().Property(m => m.delivery_id).HasDefaultValueSql("nextval('\"delivery_id\"')");
            builder.Entity<TanksAdjustDelivery>().Property(m => m.delivery_id).HasDefaultValueSql("nextval('\"delivery_id\"')");
            builder.Entity<SiteConfigs>().Property(m => m.config_id).HasDefaultValueSql("nextval('\"config_id\"')");
            builder.Entity<TanksAlarmHistory>().Property(m => m.history_id).HasDefaultValueSql("nextval('\"history_id\"')");
            base.OnModelCreating(builder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Lavender : DefaultConnection
            //Localhost : LocalConnection
            //Localhost : TestConnection
            //string constr = PostgresUtility.GetConnectionString("ConnectionStrings:RemoteConnection");
#if DEBUG
            string constr = PostgresUtility.GetConnectionString("ConnectionStrings:DebugConnection");
#else
            string constr = PostgresUtility.GetConnectionString("ConnectionStrings:ReleaseConnection");
#endif
            optionsBuilder.UseNpgsql(constr);
        }
    }
}
