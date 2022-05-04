using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DispenserManagement.Model;
using Microsoft.EntityFrameworkCore;
using TankGaugeManagement.Model;

namespace TankGaugeManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello TankGauge!");
            try
            {
                using (PostgresContext connDb = new PostgresContext())
                {
                    var getSite = connDb.site_config.SingleOrDefault(w => w.key.ToLower() == "tank_gauge_version");
                    getSite.value = "1.0.5";         // Version
                    connDb.SaveChanges();
                }
                PostgresContext db = new PostgresContext();
                var loopPump = (from p in db.protocols.Where(w => w.protocol_type == 1).ToList()  
                                join l in db.loops.Where(w => w.loop_type == 2) on p.protocol_id equals l.protocol_id
                                group l by l.loop_id into loop
                                select new
                                {
                                    Loop_id = loop.FirstOrDefault().loop_id,
                                    Loop_name = loop.FirstOrDefault().loop_name,
                                    Loop_baudRate = loop.FirstOrDefault().baudrate,
                                    Loop_protocol = loop.FirstOrDefault().protocol_id,
                                    Loop_ReadTimeout = loop.FirstOrDefault().read_timeout,
                                    Loop_ReadDataTimeout = loop.FirstOrDefault().readdata_timeout
                                }).ToList();

                Connection connection = new Connection();
                foreach (var loop in loopPump)
                {
                    connection = new Connection();
                    connection.ConnectPort(loop.Loop_id, loop.Loop_name, loop.Loop_baudRate, loop.Loop_protocol, loop.Loop_ReadTimeout, loop.Loop_ReadDataTimeout);
                    //connection.ConnectPort(2,"COM 3",9600,2,500,800);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Process is stop, please recheck config and system with error : " + e.Message);
            }
        }
    }
}
