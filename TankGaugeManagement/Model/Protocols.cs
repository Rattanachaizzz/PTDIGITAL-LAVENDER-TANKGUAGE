using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace DispenserManagement.Model
{
    public class Protocols
    {
        public int protocol_id { get; set; }
        public string protocol_name { get; set; }
        public int protocol_type { get; set; }
        public string master_pts_parameter { get; set; }
    }
}
