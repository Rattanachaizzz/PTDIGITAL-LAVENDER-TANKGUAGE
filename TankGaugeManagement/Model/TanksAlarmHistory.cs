using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TanksAlarmHistory
    {
        public int history_id { get; set; }
        public int tank_id { get; set; }
        public DateTime history_dt { get; set; }
        public string tank_alarm { get; set; }
        public bool is_cleared { get; set; }
        public bool? sync_gaia { get; set; } = false;
    }
}
