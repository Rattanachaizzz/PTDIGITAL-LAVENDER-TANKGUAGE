using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TanksDelivery
    {
        public int tank_delivery_id { get; set; }
        public int tank_id { get; set; }
        public int product_code { get; set; }
        public DateTime? start_date_time { get; set; }
        public DateTime? end_date_time { get; set; }
        public double? start_volume { get; set; }
        public double? end_volume { get; set; }
        public double? start_temperature { get; set; }
        public double? end_temperature { get; set; }
        public double? start_tc_volume { get; set; }
        public double? end_tc_volume { get; set; }
        public double? start_water { get; set; }
        public double? end_water { get; set; }
        public double? start_height { get; set; }
        public double? end_height { get; set; }
        public DateTime date_time_update { get; set; }
        public bool? sync_gaia { get; set; } = false;
    }
}
