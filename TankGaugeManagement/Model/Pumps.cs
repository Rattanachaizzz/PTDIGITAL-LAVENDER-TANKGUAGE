using System;
using System.Collections.Generic;
using System.Text;

namespace DispenserManagement.Model
{
    public class Pumps
    {
        public int pump_id { get; set; }
        public int loop_id { get; set; }
        public int ? pump_channel_id { get; set; }
        public int pump_type { get; set; }
        public int pump_display_id { get; set; }
        public string pump_name { get; set; }
        public string pump_description { get; set; }
        public int logical_address { get; set; }
        public int physical_address { get; set; }
        public int stack_limit { get; set; }
        public string firmware_version { get; set; }
        public string firmware_detail { get; set; }
        public bool auto_authorize { get; set; }
        public bool self_mode { get; set; }
    }
}
