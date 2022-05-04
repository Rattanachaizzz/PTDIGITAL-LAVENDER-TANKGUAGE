using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DispenserManagement.Model;
using TankGaugeManagement.Model;
using TankGaugeProtocol;

namespace TankGaugeManagement
{
    public class OS
    {
        public static readonly bool os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
    public class Connection
    {
        PostgresContext db = new PostgresContext();
        public void ConnectPort(int loopId, string loopName, int baudRate, int protocol, int readTimeout, int readDataTimeout)
        {
            try
            {
                db = new PostgresContext();
                SerialPort serialPort = new SerialPort();
                serialPort.PortName = OS.os ? loopName.ToUpper().Replace(" ", "") : "/dev/ttyS" + (int.Parse(loopName.Trim().Substring(loopName.Trim().Length - 1)) - 1);
                serialPort.BaudRate = baudRate;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = readTimeout;
                serialPort.Open();


                List<Tank> tanks = ConfigTank(loopId, protocol, serialPort, readDataTimeout);
                //tanks = tanks.Where(w => w.Id == 1).ToList();
                var features = db.tank_gauge_features.Where(w => w.protocol_id == protocol && w.enable).ToList();

                switch (tanks.FirstOrDefault().ProtocolName)
                {
                    case "Veeder Root":
                        VeederRoot veeder = new VeederRoot();
                        Thread veederThread = new Thread(() => veeder.Connect(tanks, features));
                        veederThread.Start();
                        break;
                    case "Visy":
                        Visy visy = new Visy();
                        Thread visyThread = new Thread(() => visy.Connect(tanks, features));
                        visyThread.Start();
                        break;
                    default:
                        Console.WriteLine("Cannot mapping protocol, please recheck config of protocol.");
                        break;
                }
            }
            catch (IOException io)
            {
                Console.WriteLine("Comport " + loopName + " cannot connect, please recheck and try again.");
                Console.WriteLine("Error message : " + io.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public List<Tank> ConfigTank(int loopId, int protocolId, SerialPort serialPort, int readDataTimeout)
        {
            try
            {
                List<Tank> tankList = new List<Tank>();
                //int tNumber = 0;
                var getTank = db.tanks.Where(w => w.tank_type == 2).ToList();
                //var getTank = (from t in db.tanks
                //    join h in db.hoses on t.grade_id equals h.grade_id
                //    join p in db.pumps on h.pump_id equals p.pump_id
                //    join g in db.grades on h.grade_id equals g.grade_id
                //    select new
                //        { t, h, p, g }).ToList();

                foreach (var tank in getTank)
                {
                    var getProtocol = db.protocols.FirstOrDefault(w => w.protocol_id == protocolId && w.protocol_type == 1);
                    var getLoop = db.loops.FirstOrDefault(w => w.loop_id == loopId);
                    var isRecon = db.tank_gauge_features.SingleOrDefault(w => w.protocol_id == protocolId && w.enable && w.feature_id == 5) != null;

                    Tank t = new Tank();
                    t.Id = tank.tank_id;
                    t.EvenId = 0;
                    t.Number = tank.tank_number;
                    t.Name = tank.tank_name;
                    t.LoopType = getLoop.loop_type;
                    t.LoopName = getLoop.loop_name.Replace(" ", "");
                    t.LoopId = loopId;
                    t.SerialPort = serialPort;
                    t.ProtocolId = getProtocol.protocol_id;
                    t.ProtocolName = getProtocol.protocol_name;
                    t.ReadDataTimeout = readDataTimeout;
                    t.TimeDelay = getLoop.time_delay;

                    t.Hoses = new List<Hose>();
                    if (isRecon)
                    {
                        var getHoseDetail = (from hose in db.hoses.Where(w => w.tank_id == tank.tank_id).ToList()
                                             join g in db.grades on hose.grade_id equals g.grade_id
                                             join p in db.pumps on hose.pump_id equals p.pump_id
                                             orderby hose.pump_id, hose.hose_id
                                             select new { hose, g, p }).ToList();


                        foreach (var hose in getHoseDetail)
                        {
                            var getStatus = db.pumps_real_time.FirstOrDefault(w => w.pump_id == hose.p.pump_id && w.active_hose_number == hose.hose.hose_number);
                            Hose h = new Hose();
                            h.HoseId = hose.hose.hose_id;
                            h.GradeId = hose.hose.grade_id;
                            h.GradeName = hose.g.grade_name;
                            h.LogicalAddress = hose.p.logical_address;
                            h.HoseNumber = hose.hose.hose_number;
                            h.PumpId = hose.p.pump_id;
                            h.TotalValue = hose.hose.total_meter_value;
                            h.TotalVolume = hose.hose.total_meter_volume;
                            h.Status = getStatus?.status ?? "Unknown";
                            t.Hoses.Add(h);
                        }
                    }

                    tankList.Add(t);
                }
                return tankList.OrderBy(w => w.Id).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
