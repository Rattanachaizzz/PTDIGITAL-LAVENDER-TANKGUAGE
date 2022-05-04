using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class Tanks
    {
        public int tank_id { get; set; }
        public int grade_id { get; set; }
        public string tank_name { get; set; }
        public int tank_number { get; set; }
        public string tank_description { get; set; }
        public double capacity { get; set; }
        public double? gauge_level { get; set; }
        public double? gauge_volume { get; set; }
        public double? gauge_tc_volume { get; set; }
        public double? temperature { get; set; }
        public double? water_level { get; set; }
        public double? water_volume { get; set; }
        public double? ullage { get; set; }
        public double? theoretical_volume { get; set; }
        public string tank_alarm_category { get; set; }
        public string tank_alarm_type { get; set; }
        public string tank_alarm_description { get; set; }
        public int tank_type { get; set; }
        public int? tank_gauge_id { get; set; }
        public int? tank_type_id { get; set; }
        public int? tank_connection_type_id { get; set; }
        public int? tank_probe_status_id { get; set; }
        public DateTime? tank_reading_dt { get; set; }
        public double low_volume_warning { get; set; }
        public double low_volume_alarm { get; set; }
        public double high_volume_warning { get; set; }
        public double high_volume_alarm { get; set; }
    }
}
