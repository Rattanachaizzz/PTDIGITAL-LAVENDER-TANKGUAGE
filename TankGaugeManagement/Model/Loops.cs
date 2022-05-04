using System;
using System.Collections.Generic;
using System.Text;

namespace DispenserManagement.Model
{
    public class Loops
    {
        public int loop_id { get; set; }
        public string loop_name { get; set; }
        public int loop_type { get; set; }
        public int protocol_id { get; set; }
        public int baudrate { get; set; }
        public int read_timeout { get; set; }
        public int readdata_timeout { get; set; }
        public int time_delay { get; set; }
    }
}
