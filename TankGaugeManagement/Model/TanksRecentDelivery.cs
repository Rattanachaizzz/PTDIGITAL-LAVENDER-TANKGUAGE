using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TanksRecentDelivery
    {
        public int delivery_id { get; set; }
        public int tank_id { get; set; }
        public int product_code { get; set; }
        public DateTime? starting_dt { get; set; }
        public DateTime? ending_dt { get; set; }
        public double? starting_vol { get; set; }
        public double? starting_tc_vol { get; set; }
        public double? starting_water { get; set; }
        public double? starting_temp { get; set; }
        public double? ending_vol { get; set; }
        public double? ending_tc_vol { get; set; }
        public double? ending_water { get; set; }
        public double? ending_temp { get; set; }
        public double? starting_height { get; set; }
        public double? ending_height { get; set; }
        public DateTime reading_dt { get; set; }
    }
}
