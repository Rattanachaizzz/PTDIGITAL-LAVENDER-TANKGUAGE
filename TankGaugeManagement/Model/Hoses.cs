using System;
using System.Collections.Generic;
using System.Text;

namespace DispenserManagement.Model
{
    public class Hoses
    {
        public int hose_id { get; set; }
        public int pump_id { get; set; }
        public int hose_number { get; set; }
        public int grade_id { get; set; }
        public int price_profile_id { get; set; }
        public int active_price_level { get; set; }
        public int tank_id { get; set; }
        public double total_meter_volume { get; set; }
        public double total_meter_value { get; set; }
        public double? total_meter_machanical { get; set; }
    }
}
