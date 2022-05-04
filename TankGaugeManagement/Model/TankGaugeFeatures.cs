using System;
using System.Collections.Generic;
using System.Text;

namespace TankGaugeManagement.Model
{
    public class TankGaugeFeatures
    {
        public int feature_id { get; set; }
        public int protocol_id { get; set; }
        public string group { get; set; }
        public bool enable { get; set; }
    }
}
