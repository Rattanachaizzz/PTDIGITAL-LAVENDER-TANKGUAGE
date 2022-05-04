using System;
using System.Collections.Generic;
using System.Text;

namespace DispenserManagement.Model
{
    public class PumpsRealtime
    {
        public int pump_id { get; set; }
        public int? active_hose_number { get; set; }
        public double? volume { get; set; }
        public double? value { get; set; }
        public double? sell_price { get; set; }
        public string status { get; set; }
        public bool? notification { get; set; }
        public DateTime last_update { get; set; }
        public string pending_gauge { get; set; }
        public int alarm_id { get; set; }
        public string alarm { get; set; }
    }
}
