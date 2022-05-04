using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DispenserManagement.Model;
using Microsoft.EntityFrameworkCore.Design;
using TankGaugeManagement.Model;

namespace TankGaugeManagement
{
    public class TankManagement
    {
        PostgresContext db = new PostgresContext();
        public static string KeepFile;
        public static FileInfo LastFile = null;
        public void PumpStatus(object sender, EventArgs e)
        {
            db = new PostgresContext();
            Tank tank = (Tank)sender;
            try
            {
                foreach (Hose hose in tank.Hoses)
                {
                    var getStatus = db.pumps_real_time.FirstOrDefault(w => w.pump_id == hose.PumpId && w.active_hose_number == hose.HoseNumber);
                    if (getStatus == null) continue;
                    if (string.IsNullOrEmpty(getStatus.pending_gauge)) continue;
                    hose.Status = getStatus.status;
                    hose.PendingCommand = getStatus.pending_gauge;
                    if (getStatus.pending_gauge == "C")
                    {
                        var getTrans = db.transactions.Where(w => w.pump_id == hose.PumpId && w.hose_id == hose.HoseId && w.completed_ts >= DateTime.Now.AddDays(-1)).OrderByDescending(w => w.transaction_id).FirstOrDefault();
                        hose.TotalVolume = getTrans.total_meter_volume;
                        hose.TransactionVolume = getTrans.delivery_volume;
                    }
                    getStatus.pending_gauge = "";
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                TankGaugeLogs log = new TankGaugeLogs();
                log.tank_id = tank.Id;
                log.create_date = DateTime.Now;
                log.catagory = "Error";
                log.message = "PumpStatus Cannot Get Pending Command with error : " + ex.Message;
                db.tank_gauge_logs.Add(log);
                db.SaveChanges();
            }
        }
        public void UpdateTank(object sender, EventArgs e, string cmdType)
        {
            db = new PostgresContext();
            Tank tank = (Tank)sender;
            TankGaugeLogs log = new TankGaugeLogs();
            try
            {
                var getTank = db.tanks.SingleOrDefault(w => w.tank_id == tank.Id);
                if (getTank != null)
                {
                    switch (cmdType)
                    {
                        case "TankInventory":
                            getTank.gauge_volume = tank.GaugeVolume;
                            getTank.gauge_tc_volume = tank.GaugeTcVolume;
                            getTank.gauge_level = tank.Height;
                            getTank.ullage = tank.Ullage;
                            getTank.water_level = tank.WaterLevel;
                            getTank.water_volume = tank.WaterVolume;
                            getTank.temperature = tank.Temperature;
                            getTank.tank_reading_dt = tank.DateStamp;
                            getTank.tank_probe_status_id = tank.TankProbeStatus;
                            break;
                        case "SystemStatus":
                            if (tank.TankAlarmCategory != "00" && tank.TankAlarmType != "00" && !string.IsNullOrEmpty(tank.TankAlarmCategory) && !string.IsNullOrEmpty(tank.TankAlarmType))
                            {
                                TanksAlarmHistory alarm = new TanksAlarmHistory();
                                alarm.tank_id = tank.Id;
                                alarm.history_dt = DateTime.Now;
                                alarm.tank_alarm = tank.TankAlarmDescription;
                                db.tanks_alarm_history.Add(alarm);
                                db.SaveChanges();
                            }
                            getTank.tank_alarm_category = tank.TankAlarmCategory;
                            getTank.tank_alarm_type = tank.TankAlarmType;
                            getTank.tank_alarm_description = tank.TankAlarmDescription;

                            break;
                        case "TankDelivery":
                            TanksDelivery t = new TanksDelivery();
                            for (int i = 0; i < tank.StartGaugeVolume.Count; i++)
                            {
                                var checkDelivery = db.tanks_delivery.Where(w => w.start_date_time == tank.StartDate[i] && w.start_volume == Math.Round(tank.StartGaugeVolume[i], 3)).OrderByDescending(w => w.tank_delivery_id).FirstOrDefault();
                                if (checkDelivery != null) continue;

                                t = new TanksDelivery();
                                t.tank_id = tank.Id;
                                t.product_code = tank.ProductCode;
                                t.start_date_time = tank.StartDate[i];
                                t.start_volume = tank.StartGaugeVolume[i];
                                t.start_tc_volume = tank.StartGaugeTcVolume[i];
                                t.start_height = tank.StartHeight[i];
                                t.start_water = tank.StartWaterVolume[i];
                                t.start_temperature = tank.StartTemperature[i];
                                t.end_date_time = tank.EndDate[i];
                                t.end_volume = tank.EndGaugeVolume[i];
                                t.end_tc_volume = tank.EndGaugeTcVolume[i];
                                t.end_height = tank.EndHeight[i];
                                t.end_water = tank.EndWaterVolume[i];
                                t.end_temperature = tank.EndTemperature[i];
                                t.date_time_update = tank.DateStampDeliver;
                                db.tanks_delivery.Add(t);
                            }
                            break;
                        case "Gaia_B_Response":
                            TanksAdjustDelivery a = new TanksAdjustDelivery();
                            for (int i = 0; i < tank.B_StartVolume.Count; i++)
                            {
                                a = new TanksAdjustDelivery();
                                a.tank_id = tank.Id;
                                a.starting_dt = tank.B_StartDate[i];
                                a.starting_vol = tank.B_StartVolume[i];
                                a.adj_delivery_vol = tank.B_AdjustDeliveryVolume[i];
                                a.adj_temperature_delivery_vol = tank.B_AdjustTempVolume[i];
                                a.starting_temp1 = tank.B_StartFuelTemp1[i];
                                a.starting_temp2 = tank.B_StartFuelTemp2[i];
                                a.starting_temp3 = tank.B_StartFuelTemp3[i];
                                a.starting_temp4 = tank.B_StartFuelTemp4[i];
                                a.starting_temp5 = tank.B_StartFuelTemp5[i];
                                a.starting_temp6 = tank.B_StartFuelTemp6[i];
                                a.starting_height = tank.B_StartFuelHeight[i];
                                a.starting_temp_avg = tank.B_StartFuelTempAvg[i];
                                a.ending_dt = tank.B_EndDate[i];
                                a.ending_vol = tank.B_EndVolume[i];
                                a.ending_temp1 = tank.B_EndFuelTemp1[i];
                                a.ending_temp2 = tank.B_EndFuelTemp2[i];
                                a.ending_temp3 = tank.B_EndFuelTemp3[i];
                                a.ending_temp4 = tank.B_EndFuelTemp4[i];
                                a.ending_temp5 = tank.B_EndFuelTemp5[i];
                                a.ending_temp6 = tank.B_EndFuelTemp6[i];
                                a.ending_height = tank.B_EndFuelHeight[i];
                                a.ending_temp_avg = tank.B_EndFuelTempAvg[i];
                                a.reading_dt = tank.B_DateStamp;
                                db.tanks_adjust_delivery.Add(a);
                            }
                            break;
                        case "Gaia_C_Response":
                            TanksRecentDelivery r = new TanksRecentDelivery();
                            for (int i = 0; i < tank.C_StartGaugeVolume.Count; i++)
                            {
                                r = new TanksRecentDelivery();
                                r.tank_id = tank.Id;
                                r.product_code = tank.C_ProductCode;
                                r.starting_dt = tank.C_StartDate[i];
                                r.starting_vol = tank.C_StartGaugeVolume[i];
                                r.starting_tc_vol = tank.C_StartGaugeTcVolume[i];
                                r.starting_water = tank.C_StartWaterVolume[i];
                                r.starting_temp = tank.C_StartTemperature[i];
                                r.starting_height = tank.C_StartHeight[i];
                                r.ending_dt = tank.C_EndDate[i];
                                r.ending_vol = tank.C_EndGaugeVolume[i];
                                r.ending_tc_vol = tank.C_EndGaugeTcVolume[i];
                                r.ending_water = tank.C_EndWaterVolume[i];
                                r.ending_temp = tank.C_EndTemperature[i];
                                r.ending_height = tank.C_EndHeight[i];
                                r.reading_dt = tank.C_DateStamp;
                                db.tanks_recent_delivery.Add(r);
                            }
                            break;
                        case "Disconnect":
                            getTank.tank_probe_status_id = tank.TankProbeStatus;
                            break;
                    }
                    db.SaveChanges();
                    ResetParam(tank, cmdType);
                }
                else
                {
                    log.tank_id = tank.Id;
                    log.create_date = DateTime.Now;
                    log.catagory = "Error";
                    log.type = "Database";
                    log.message = "Cannot get tank in database.";
                    db.tank_gauge_logs.Add(log);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.tank_id = tank.Id;
                log.create_date = DateTime.Now;
                log.catagory = "Error";
                log.type = "Database";
                log.message = cmdType + "Tank ID : " + tank.Id + " Cannot Update with error : " + ex.Message;
                db.tank_gauge_logs.Add(log);
                db.SaveChanges();
            }

        }
        public void TimeoutResponse(object sender, EventArgs e, string timeMessage)
        {
            Tank tank = (Tank)sender;
            db = new PostgresContext();
            TankGaugeLogs log = new TankGaugeLogs();
            string message = string.IsNullOrEmpty(timeMessage) ? "Receive data Time out." : "Tank " + tank.Id + " receive error data : [ " + timeMessage + " ]";
            log.tank_id = tank.Id;
            log.create_date = DateTime.Now;
            log.catagory = "Error";
            log.message = message;
            db.tank_gauge_logs.Add(log);
            db.SaveChanges();
            Console.WriteLine("TimeoutResponse Event : " + tank.Id);
        }
        public void LogManagement(object sender, EventArgs e, string logMessage, string logCatagory, string logType, bool isAll)
        {
            db = new PostgresContext();
            Tank tank = (Tank)sender;
            TankGaugeLogs log = new TankGaugeLogs();
            log.tank_id = isAll ? 0 : tank.Id;
            log.create_date = DateTime.Now;
            log.catagory = logCatagory;
            log.type = logType;
            log.message = logMessage;
            db.tank_gauge_logs.Add(log);
            db.SaveChanges();
        }
        public void ClearCommand(object sender, EventArgs e)
        {
            db = new PostgresContext();
            Tank tank = (Tank)sender;
            TankGaugeLogs log = new TankGaugeLogs();
            try
            {
                var getActive = tank.Hoses.Where(w => w.IsActive).ToList();
                //var getPump = db.pumps_real_time.ToList();
                if (getActive.Count() != 0 /*&& getPump.Count() != 0*/)
                {
                    foreach (Hose h in getActive)
                    {
                        //getPump.First(w => w.pump_id == h.PumpId && w.active_hose_number == h.HoseNumber).pending_gauge = h.PendingCommand;
                        h.IsActive = false;
                    }
                    //db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.tank_id = tank.Id;
                log.create_date = DateTime.Now;
                log.catagory = "Error";
                log.message = "ClearCommand Tank ID : " + tank.Id + " Cannot Update with error : " + ex.Message;
                db.tank_gauge_logs.Add(log);
                db.SaveChanges();
            }
        }
        public void ResetParam(Tank tank, string cmdType)
        {
            switch (cmdType)
            {
                case "TankDelivery":
                    tank.StartDate.Clear();
                    tank.StartGaugeVolume.Clear();
                    tank.StartGaugeTcVolume.Clear();
                    tank.StartHeight.Clear();
                    tank.StartWaterVolume.Clear();
                    tank.StartTemperature.Clear();
                    tank.EndDate.Clear();
                    tank.EndGaugeVolume.Clear();
                    tank.EndGaugeTcVolume.Clear();
                    tank.EndHeight.Clear();
                    tank.EndWaterVolume.Clear();
                    tank.EndTemperature.Clear();
                    break;
                case "Gaia_B_Response":
                    tank.B_StartDate.Clear();
                    tank.B_StartVolume.Clear();
                    tank.B_AdjustDeliveryVolume.Clear();
                    tank.B_AdjustTempVolume.Clear();
                    tank.B_StartFuelTemp1.Clear();
                    tank.B_StartFuelTemp2.Clear();
                    tank.B_StartFuelTemp3.Clear();
                    tank.B_StartFuelTemp4.Clear();
                    tank.B_StartFuelTemp5.Clear();
                    tank.B_StartFuelTemp6.Clear();
                    tank.B_StartFuelHeight.Clear();
                    tank.B_StartFuelTempAvg.Clear();
                    tank.B_EndDate.Clear();
                    tank.B_EndVolume.Clear();
                    tank.B_EndFuelTemp1.Clear();
                    tank.B_EndFuelTemp2.Clear();
                    tank.B_EndFuelTemp3.Clear();
                    tank.B_EndFuelTemp4.Clear();
                    tank.B_EndFuelTemp5.Clear();
                    tank.B_EndFuelTemp6.Clear();
                    tank.B_EndFuelHeight.Clear();
                    tank.B_EndFuelTempAvg.Clear();
                    break;
                case "Gaia_C_Response":
                    tank.C_StartDate.Clear();
                    tank.C_StartGaugeVolume.Clear();
                    tank.C_StartGaugeTcVolume.Clear();
                    tank.C_StartWaterVolume.Clear();
                    tank.C_StartTemperature.Clear();
                    tank.C_StartHeight.Clear();
                    tank.C_EndDate.Clear();
                    tank.C_EndGaugeVolume.Clear();
                    tank.C_EndGaugeTcVolume.Clear();
                    tank.C_EndWaterVolume.Clear();
                    tank.C_EndTemperature.Clear();
                    tank.C_EndHeight.Clear();
                    break;
            }
        }
        public void LogFileGD(object sender, EventArgs e, string message, string type)
        {
            db = new PostgresContext();
            Tank tank = (Tank)sender;
            TankGaugeLogs log = new TankGaugeLogs();
            try
            {
                int countFile = 1;
                int len = 18;
                DateTime dateNow = DateTime.Now;
                string path = "";
                string datetime = dateNow.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
                if (OS.os)
                {
                    path = @AppDomain.CurrentDomain.BaseDirectory + "logs\\raw\\" + tank.LoopName + "_" +
                                 dateNow.ToString("yyyy-MM-dd_HHmm", new CultureInfo("en-US")) + ".txt";
                }
                else
                {
                    path = @"/var/log/lavender/atg/" + tank.LoopName + "_" +
                                 dateNow.ToString("yyyy-MM-dd_HHmm", new CultureInfo("en-US")) + ".txt";
                }

                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (string.IsNullOrEmpty(KeepFile))
                {
                    var getFile = new DirectoryInfo(Path.GetDirectoryName(path)).GetFiles()
                        .Where(w => w.Name.Contains(tank.LoopName) && w.Extension == ".txt" &&
                                    w.Name.Contains(datetime)).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    if (getFile != null)
                    {
                        KeepFile = getFile.FullName;
                    }
                }
                if (!string.IsNullOrEmpty(KeepFile))
                {
                    //long lastDate = long.Parse((LastFile.Name.Split("_")[1].Replace("-", "") + LastFile.Name.Split("_")[2]).Replace(".txt", ""));
                    //long currentDate = long.Parse(dateNow.ToString("yyyyMMddHHmm", new CultureInfo("en-US")));
                    LastFile = new FileInfo(KeepFile);
                    if (LastFile.Length < 2048000)
                    {
                        path = LastFile.FullName;
                        countFile = 1;
                    }
                    else if (LastFile.FullName.Contains(path.Replace(".txt", "")))
                    {
                        path = path.Replace(".txt", "_" + countFile + ".txt");
                        countFile++;
                    }

                }
                KeepFile = path;
                if (!File.Exists(path))
                    File.Create(path).Close();

                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    for (int i = 0; i < message.Length; i += 18)
                    {
                        len = i + 18 > message.Length ? message.Length - i : len;
                        sw.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " [" + type + "] Tank : " + tank.Id + " : " + message.Substring(i, len));
                        sw.Flush();
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                KeepFile = "";
                log.tank_id = tank.Id;
                log.create_date = DateTime.Now;
                log.catagory = "Error";
                log.type = "LogGD";
                log.message = "Write file Log GD Tank ID : " + tank.Id + " is failed with error : " + ex.Message;
                db.tank_gauge_logs.Add(log);
                db.SaveChanges();
            }
        }
    }
}
