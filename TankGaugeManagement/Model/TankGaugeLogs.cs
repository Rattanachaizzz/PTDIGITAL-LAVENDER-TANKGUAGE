using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TankGaugeLogs
    {
        public int log_id { get; set; }
        public int tank_id { get; set; }
        public DateTime create_date { get; set; }
        public string catagory { get; set; }
        public string type { get; set; }
        public string message { get; set; }
    }
}
