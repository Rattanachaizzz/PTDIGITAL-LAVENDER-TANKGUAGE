using System;
using System.Collections.Generic;
using System.Text;

namespace DispenserManagement.Model
{
    public class Transactions
    {
        public int transaction_id { get; set; }
        public int pump_id { get; set; }
        public int hose_id { get; set; }
        public int price_level { get; set; }
        public DateTime completed_ts { get; set; }
        public DateTime? cleared_ts { get; set; }
        public int delivery_type { get; set; }
        public double delivery_volume { get; set; }
        public double delivery_value { get; set; }
        public double sell_price { get; set; }
        public int ? cleared_by { get; set; }
        public int ? reserved_by { get; set; }
        public double total_meter_volume { get; set; }
        public double total_meter_value { get; set; }
        public bool sync_gaia { get; set; }
        public bool sync_backoffice { get; set; }
    }
}
