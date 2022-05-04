using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TanksAdjustDelivery
    {
        public int delivery_id { get; set; }
        public int tank_id { get; set; }
        public DateTime? starting_dt { get; set; }
        public DateTime? ending_dt { get; set; }
        public double? starting_vol { get; set; }
        public double? ending_vol { get; set; }
        public double? adj_delivery_vol { get; set; }
        public double? adj_temperature_delivery_vol { get; set; }
        public double? starting_temp1 { get; set; }
        public double? starting_temp2 { get; set; }
        public double? starting_temp3 { get; set; }
        public double? starting_temp4 { get; set; }
        public double? starting_temp5 { get; set; }
        public double? starting_temp6 { get; set; }
        public double? ending_temp1 { get; set; }
        public double? ending_temp2 { get; set; }
        public double? ending_temp3 { get; set; }
        public double? ending_temp4 { get; set; }
        public double? ending_temp5 { get; set; }
        public double? ending_temp6 { get; set; }
        public double? starting_height { get; set; }
        public double? ending_height { get; set; }
        public double? total_dispened_during_delivery { get; set; }
        public double? starting_temp_avg { get; set; }
        public double? ending_temp_avg { get; set; }
        public DateTime reading_dt { get; set; }
    }
}
