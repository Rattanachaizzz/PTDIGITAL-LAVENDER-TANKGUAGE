using DispenserManagement.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Transactions;
using TankGaugeManagement;
using TankGaugeManagement.Model;
using Timer = System.Timers.Timer;

namespace TankGaugeProtocol
{
    public class Visy
    {
        private string timemessage;
        private string logMessage = "";
        private string logCatagory = "";
        private string logType = "";
        private int countFile = 1;
        private List<Tank> allTanks;
        private string cmdType;
        private int maxTimeout = 0;
        private bool isAll;
        private bool isReconcil;
        private string gdMessage = "";
        private string gdType = "";
        private DateTime D_Timer = DateTime.Now;
        private DateTime dateDelay = DateTime.Now;
        private int id = 0;
        public void Connect(List<Tank> tanks, List<TankGaugeFeatures> features)
        {
            TankManagement tankManagement = new TankManagement();
            this.StatusChanged += tankManagement.PumpStatus;
            this.LogManageResponse += (sender2, e2) => tankManagement.LogManagement(sender2, e2, logMessage, logCatagory, logType, isAll);
            this.LogFileResponse += (sender2, e2) => tankManagement.LogFileGD(sender2, e2, gdMessage, gdType);
            this.TankUpdated += (sender2, e2) => tankManagement.UpdateTank(sender2, e2, cmdType);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            features = features.OrderBy(w => w.feature_id).ToList();
            this.allTanks = tanks;

            while (true)
            {
                if (id == features.Count)
                {
                    id = 0;
                    watch.Restart();
                }

                if (watch.ElapsedMilliseconds > tanks[0].TimeDelay)
                {
                    switch (features[id].feature_id)
                    {
                        case 7:
                            foreach (Tank tank in tanks)
                            {
                                //TankInventoryCommand(tank);
                            }
                            break;
                        case 8:
                            Thread.Sleep(500);
                              SystemStatusCommand(tanks);
                            break;
                        case 9:
                            Thread.Sleep(500);
                            foreach (Tank tank in tanks)
                            {
                                //TankDeliveryCommand(tank);
                            }
                            break;
                    }
                    id++;
                }
            }
        }

        public void TankInventoryCommand(Tank tank)
        {
            try
            {
                //Q A X TT & sp 8 4 4 6 cr lf  - Inventory Check for Tank Number TT
                List<byte> messageList = new List<byte>();
                string command = "5141583" + tank.Number / 10 + "3" + tank.Number % 10 + "2620383434360D0A";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Request Inventory for Tank ID : " + tank.Id, "Info", "Request", false);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankInventoryCommand : " + e.Message, "Error", "Request", false);
            }
        }

        public void SystemStatusCommand(List<Tank> tanks)
        {

            for (int j = 1; j <= 6; j++)
            {

                switch (j)
                {
                    //Alarm_command
                    case 1:
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q P S t t cr lf - Status of probe
                                List<byte> messageList = new List<byte>();
                                string command = "5150533" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status of Probe for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());

                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                    case 2:
                        Thread.Sleep(500);
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q A P t t cr lf  - Alarm product
                                List<byte> messageList = new List<byte>();
                                string command = "5141503" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status Alarm Product for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());

                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                    case 3:
                        Thread.Sleep(500);
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q A W t t cr lf   - Alarm water
                                List<byte> messageList = new List<byte>();
                                string command = "5141573" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status Alarm Water for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                    case 4:
                        Thread.Sleep(500);
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q T S t t cr lf  - Tank status
                                List<byte> messageList = new List<byte>();
                                string command = "5154533" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status Tank for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                    case 5:
                        Thread.Sleep(500);
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q A D S t t cr lf   - Alarm sump density 
                                List<byte> messageList = new List<byte>();
                                string command = "514144533" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status Alarm Sump Density for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                    case 6:
                        Thread.Sleep(500);
                        foreach (Tank tank in tanks)
                        {
                            try
                            {
                                //Q A D P t t cr lf   -  Alarm product density 
                                List<byte> messageList = new List<byte>();
                                string command = "514144503" + tank.Number / 10 + "3" + tank.Number % 10 + "0D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Status Alarm Product Density for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                        }
                        break;
                }
            }
        }

        public void TankDeliveryCommand(Tank tank)
        {
            try
            {
                for (int j = 1; j <= 5; j++)
                {
                    switch (j)
                    {
                        case 1:
                            try
                            {
                                //Q H  sp 1 X 0 1 & sp sp  1 2 6 cr lf - delivery Check for Tank Number TT
                                List<byte> messageList = new List<byte>();
                                string command = "51483" + tank.Number / 10 + "3" + tank.Number % 10 + "58303" + j + "2620203132360D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                OnLogResponse(tank, "Request Tank Delivery for Tank ID : " + tank.Id, "Info", "Request", false);
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                            break;
                        case 2:
                            try
                            {
                                //Q H  sp 1 X 0 1 & sp sp  1 2 6 cr lf - delivery Check for Tank Number TT
                                List<byte> messageList = new List<byte>();
                                string command = "51483" + tank.Number / 10 + "3" + tank.Number % 10 + "58303" + j + "2620203132360D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                            break;
                        case 3:
                            try
                            {
                                //Q H  sp 1 X 0 1 & sp sp  1 2 6 cr lf - delivery Check for Tank Number TT
                                List<byte> messageList = new List<byte>();
                                string command = "51483" + tank.Number / 10 + "3" + tank.Number % 10 + "58303" + j + "2620203132360D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                            break;
                        case 4:
                            try
                            {
                                //Q H  sp 1 X 0 1 & sp sp  1 2 6 cr lf - delivery Check for Tank Number TT
                                List<byte> messageList = new List<byte>();
                                string command = "51483" + tank.Number / 10 + "3" + tank.Number % 10 + "58303" + j + "2620203132360D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                            break;
                        case 5:
                            try
                            {
                                //Q H  sp 1 X 0 1 & sp sp  1 2 6 cr lf - delivery Check for Tank Number TT
                                List<byte> messageList = new List<byte>();
                                string command = "51483" + tank.Number / 10 + "3" + tank.Number % 10 + "58303" + j + "2620203132360D0A";
                                for (int i = 0; i < command.Length; i += 2)
                                {
                                    string hs = command.Substring(i, 2);
                                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                                }
                                SendMessage(tank, messageList.ToArray());
                            }
                            catch (Exception e)
                            {
                                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
                            }
                            break;
                    }

                }

            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankDeliveryCommand : " + e.Message, "Error", "Request", true);
            }
        }

        public void SendMessage(Tank tank, byte[] message)
        {
            byte[] buffer;
            int count = 0;
            string receive = "";
            string data = Encoding.ASCII.GetString(message).Replace("\r\n", ""); //0D=\r,0A=\n    \r\n
            SerialPort serialPort = tank.SerialPort;
            try
            {
            again:
                Console.WriteLine("Message Command  : " + data.Replace(" ", ""));
                OnLogFileResponse(tank, data, "SEND   ");
                serialPort.DiscardOutBuffer();
                serialPort.DiscardInBuffer();
                Monitor.Enter(serialPort);
                serialPort.Write(message, 0, message.Length);
                PrepareResponse(serialPort, tank, message, out buffer);
                Monitor.Exit(serialPort);

                count++;
                if (count >= 3) return;
                if (buffer != null && buffer.Length > 0)
                {
                    try
                    {
                        string s = System.Text.Encoding.ASCII.GetString(buffer).Replace("\r\n", "");
                        if (s.Substring(10, 6) == "     0" || s.Substring(17, 6) == "     0" || s.Substring(24, 6) == "     0" || s.Substring(31, 6) == "     0")
                        {
                            OnLogFileResponse(tank, s.Replace("     ", ""), "RECEIVE");
                            receive = Encoding.ASCII.GetString(buffer);
                            Console.WriteLine("Response Command : " + receive.Replace(" ", "").Replace("\r\n", ""));
                            tank.CountDisconn = 0;
                            ProcessResponse(tank, buffer, message);
                        }
                        else
                        {
                            OnLogFileResponse(tank, s, "RECEIVE");
                            receive = Encoding.ASCII.GetString(buffer);
                            Console.WriteLine("Response Command : " + receive.Replace(" ", "").Replace("\r\n", ""));
                            tank.CountDisconn = 0;
                            ProcessResponse(tank, buffer, message);
                        }
                    }
                    catch
                    {
                        OnLogFileResponse(tank, System.Text.Encoding.ASCII.GetString(buffer).Replace("\r\n", ""), "RECEIVE");
                        receive = Encoding.ASCII.GetString(buffer);
                        Console.WriteLine("Response Command : " + receive.Replace(" ", "").Replace("\r\n", ""));
                        tank.CountDisconn = 0;
                        ProcessResponse(tank, buffer, message);
                    }

                }
                else tank.CountDisconn++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public void PrepareResponse(SerialPort serialPort, Tank tank, byte[] message, out byte[] buffer)
        {
            bool isSuccess = false;
            List<byte> responseList = new List<byte>();
            byte getByte = 0;
            buffer = null;
            int receiveLength = 0;
            bool isFirst = false;
            string data = BitConverter.ToString(message).Replace("-", ""), receive = ""; //เเปลง Dec(1,105,50,48,49,48,49,3) ==> hex(0169323031303103)  
            DateTime timeOut = DateTime.Now;
            try
            {
                while (!isSuccess) //!isSuccess = True
                {
                    if (isFirst && DateTimeOffset.Now.Subtract(timeOut).TotalMilliseconds > tank.ReadDataTimeout)
                    {
                        OnTimeoutResponse(tank, receive);
                        return;
                    }
                    responseList.Add((byte)serialPort.ReadByte());
                    if (responseList.Count > 0 && !isFirst) //!isFirst=true
                    {
                        isFirst = true;
                        timeOut = DateTime.Now;
                        continue;
                    }

                    receive = BitConverter.ToString(responseList.ToArray()).Replace("-", "");  //เเปลง Dec ==> ascii
                    if (receive.Length > 4 && data.Substring(0, 4) == receive.Substring(0, 4) && receive.Substring(receive.Length - 2) == "03")
                        isSuccess = true;
                    else if (receive.Length == 1 && receive.Substring(receive.Length - 2) == "06")
                        isSuccess = true;
                    else if (receive.Length > 4 && receive.Substring(1, 4) == "9999" && receive.Substring(receive.Length - 2) == "03")
                        isSuccess = true;
                    else isSuccess = false;
                }
            }
            catch (TimeoutException ex)
            {
                if (tank.CountDisconn % 3 == 2)
                {
                    maxTimeout = 0;
                    buffer = responseList.ToArray();
                    tank.TankProbeStatus = 2;
                    OnTankUpdated(tank, "Disconnect");
                    OnLogResponse(tank, "Tank " + tank.Id + " Non Response.", "Warning", "Response", false);
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "PrepareResponse : " + e.Message, "Warning", "Response", false);
                return;
            }


            //Inventory_command -----------------------------------------------------------------------------------------------------------------------------------------------------
            //QSX 1&1$ 0&2$4632FDED&7$4277523E&8$0:145 
            byte[] Inventory_result = { 0x51, 0x41, 0x58, 0x20, 0x31, 0x26, 0x31, 0x24, 0x20, 0x30, 0x26, 0x32, 0x24, 0x34, 0x35, 0x31, 0x36, 0x33, 0x37, 0x46, 0x32, 0x26, 0x33,
                            0x24, 0x34, 0x36, 0x32, 0x44, 0x36, 0x32, 0x30, 0x34, 0x26, 0x34, 0x24, 0x34, 0x35, 0x31, 0x37, 0x32, 0x44, 0x46, 0x32, 0x26, 0x35, 0x24, 0x34, 0x31, 0x43, 0x35, 0x36, 0x33, 0x43, 0x43, 0x26, 0x36,
                            0x24, 0x34, 0x33, 0x45, 0x32, 0x42, 0x45, 0x41, 0x39, 0x26, 0x37, 0x24, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x26, 0x44, 0x24, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x3A, 0x31,
                            0x35, 0x38, 0x0D, 0x0A };
            //Delivery_command -----------------------------------------------------------------------------------------------------------------------------------------------------
            //QH 1X 1&1$220309$ 74510$220309$ 74624&2$4506634B$451EB011$43C26630&3$4506FA3C$451F6196$43C33AD0&4$41D2C668$41D2E46C$3C702000&5$43D1238E$43EC3AF4$4258BB30:103
            byte[] Delivery_result = { 0x51, 0x48, 0x20, 0x31, 0x58, 0x20, 0x31, 0x26, 0x31, 0x24, 0x32, 0x32, 0x30, 0x33, 0x30, 0x39, 0x24, 0x20, 0x37, 0x34, 0x35, 0x31, 0x30, 0x24, 0x32, 0x32, 0x30, 0x33, 0x30, 0x39,
                                  0x24, 0x20, 0x37, 0x34, 0x36, 0x32, 0x34, 0x26, 0x32, 0x24, 0x34, 0x35, 0x30, 0x36, 0x36, 0x33, 0x34, 0x42, 0x24, 0x34, 0x35, 0x31, 0x45, 0x42, 0x30, 0x31, 0x31, 0x24, 0x34, 0x33,
                                  0x43, 0x32, 0x36, 0x36, 0x33, 0x30, 0x26, 0x33, 0x24, 0x34, 0x35, 0x30, 0x36, 0x46, 0x41, 0x33, 0x43, 0x24, 0x34, 0x35, 0x31, 0x46, 0x36, 0x31, 0x39, 0x36, 0x24, 0x34, 0x33, 0x43,
                                  0x33, 0x33, 0x41, 0x44, 0x30, 0x26, 0x34, 0x24, 0x34, 0x31, 0x44, 0x32, 0x43, 0x36, 0x36, 0x38, 0x24, 0x34, 0x31, 0x44, 0x32, 0x45, 0x34, 0x36, 0x43, 0x24, 0x33, 0x43, 0x37, 0x30,
                                  0x32, 0x30, 0x30, 0x30, 0x26, 0x35, 0x24, 0x34, 0x33, 0x44, 0x31, 0x32, 0x33, 0x38, 0x45, 0x24, 0x34, 0x33, 0x45, 0x43, 0x33, 0x41, 0x46, 0x34, 0x24, 0x34, 0x32, 0x35, 0x38, 0x42,
                                  0x42, 0x33, 0x30, 0x3A, 0x31, 0x30, 0x33, 0x0D, 0x0A };
            //Alarm_command -----------------------------------------------------------------------------------------------------------------------------------------------------
            //Status of probe >>> QPS 1$ 0:244
            byte[] Status_of_probe_result = { 0x51, 0x50, 0x53, 0x20, 0x31, 0x24, 0x20, 0x30, 0x3A, 0x32, 0x34, 0x34, 0x0D, 0x0A };
            //----------------------------------------------------------------------------------------------------------------------
            //Alarm product >>> QAP 1$2:196
            byte[] Alarm_product_result = { 0x51, 0x41, 0x50, 0x20, 0x31, 0x24, 0x32, 0x3A, 0x31, 0x39, 0x36, 0x0D, 0x0A };
            //----------------------------------------------------------------------------------------------------------------------
            //Alarm water >>> QAW 1$0:201
            byte[] Alarm_water_result = { 0x51, 0x41, 0x57, 0x20, 0x31, 0x24, 0x30, 0x3A, 0x32, 0x30, 0x31, 0x0D, 0x0A };
            //----------------------------------------------------------------------------------------------------------------------
            //Tank status  >>> QTS 1$0:216
            byte[] Tank_status_result = { 0x51, 0x54, 0x53, 0x20, 0x31, 0x24, 0x30, 0x3A, 0x32, 0x31, 0x36, 0x0D, 0x0A };
            //----------------------------------------------------------------------------------------------------------------------
            //Alarm sump density  >>> QADS 1$0: 10
            byte[] Alarm_sump_density_result = { 0x51, 0x41, 0x44, 0x53, 0x20, 0x31, 0x24, 0x30, 0x3A, 0x20, 0x31, 0x30, 0x0D, 0x0A };
            //Alarm product density   >>> QADP 1$2:  9
            byte[] Alarm_product_density_result = { 0x51, 0x41, 0x44, 0x50, 0x20, 0x31, 0x24, 0x32, 0x3A, 0x20, 0x20, 0x39, 0x0D, 0x0A };
            //Error_command -----------------------------------------------------------------------------------------------------------------------------------------------------
            //ERROR 
            byte[] ERROR_result = { 0x45, 0x52, 0x52, 0x4F, 0x52, 0x0D, 0x0A };

            string Str_ascii = Encoding.ASCII.GetString(message);
            //Str_ascii
            //Inventory => QAX01& 8446\r\n
            //Delivery  => QH01X 1&   62\r\n
            /*Alarms 
                Status of probe => Q P S t t cr lf 
                Alarm product => Q A P t t cr lf 
                Alarm water => Q A W t t cr lf 
                Tank status => Q T S t t cr lf
                Alarm sump density => Q A D S t t cr lf
                Alarm product density => Q A D P t t cr lf*/

            /*#region Sim_Result
            //Inventory
            if (Str_ascii.Substring(0, 3) == "QAX")
            {
                if (Str_ascii.Substring(3, 2) == "01")
                {
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "02")
                {
                    Inventory_result[4] = 0X32;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "03")
                {
                    Inventory_result[4] = 0X33;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "04")
                {
                    Inventory_result[4] = 0X34;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "05")
                {
                    Inventory_result[4] = 0X35;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "06")
                {
                    Inventory_result[4] = 0X36;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "07")
                {
                    Inventory_result[4] = 0X37;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "08")
                {
                    Inventory_result[4] = 0X38;
                    buffer = Inventory_result;
                }
                else if (Str_ascii.Substring(3, 2) == "09")
                {
                    Inventory_result[4] = 0X39;
                    buffer = Inventory_result;
                }
            }
            //Delivery
            else if (Str_ascii.Substring(0, 2) == "QH")
            {
                if (Str_ascii.Substring(2, 2) == "01")
                {
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "02")
                {
                    Delivery_result[3] = 0X32;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "03")
                {
                    Delivery_result[3] = 0X33;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "04")
                {
                    Delivery_result[3] = 0X34;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "05")
                {
                    Delivery_result[3] = 0X35;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "06")
                {
                    Delivery_result[3] = 0X36;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "07")
                {
                    Delivery_result[3] = 0X37;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "08")
                {
                    Delivery_result[3] = 0X38;
                    buffer = Delivery_result;
                }
                else if (Str_ascii.Substring(2, 2) == "09")
                {
                    Delivery_result[3] = 0X39;
                    buffer = Delivery_result;
                }
            }
            //Alarms
            else if (Str_ascii.Substring(0, 3) == "QPS") //Status of probe  [4] == tt ; [6],[7] == Alarm
            {
                if (Str_ascii.Substring(3, 2) == "01")
                {
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "02")
                {
                    Status_of_probe_result[4] = 0X32;
                    Status_of_probe_result[7] = 0X30;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "03")
                {
                    Status_of_probe_result[4] = 0X33;
                    Status_of_probe_result[7] = 0X30;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "04")
                {
                    Status_of_probe_result[4] = 0X34;
                    Status_of_probe_result[7] = 0X30;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "05")
                {
                    Status_of_probe_result[4] = 0X35;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "06")
                {
                    Status_of_probe_result[4] = 0X36;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "07")
                {
                    Status_of_probe_result[4] = 0X37;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "08")
                {
                    Status_of_probe_result[4] = 0X38;
                    buffer = Status_of_probe_result;
                }
                if (Str_ascii.Substring(3, 2) == "09")
                {
                    Status_of_probe_result[4] = 0X39;
                    buffer = Status_of_probe_result;
                }
            }
            else if (Str_ascii.Substring(0, 3) == "QAP") //Alarm product [4] == tt ; [6] == Alarm
            {
                if (Str_ascii.Substring(3, 2) == "01")
                {
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "02")
                {
                    Alarm_product_result[4] = 0X32;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "03")
                {
                    Alarm_product_result[4] = 0X33;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "04")
                {
                    Alarm_product_result[4] = 0X34;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "05")
                {
                    Alarm_product_result[4] = 0X35;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "06")
                {
                    Alarm_product_result[4] = 0X36;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "07")
                {
                    Alarm_product_result[4] = 0X37;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "08")
                {
                    Alarm_product_result[4] = 0X38;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
                if (Str_ascii.Substring(3, 2) == "09")
                {
                    Alarm_product_result[4] = 0X39;
                    Alarm_product_result[6] = 0X30;
                    buffer = Alarm_product_result;
                }
            }
            else if (Str_ascii.Substring(0, 3) == "QAW") //Alarm water [4] == tt ; [6] == Alarm
            {
                if (Str_ascii.Substring(3, 2) == "01")
                {
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "02")
                {
                    Alarm_water_result[4] = 0X32;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "03")
                {
                    Alarm_water_result[4] = 0X33;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "04")
                {
                    Alarm_water_result[4] = 0X34;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "05")
                {
                    Alarm_water_result[4] = 0X35;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "06")
                {
                    Alarm_water_result[4] = 0X36;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "07")
                {
                    Alarm_water_result[4] = 0X37;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "08")
                {
                    Alarm_water_result[4] = 0X38;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
                if (Str_ascii.Substring(3, 2) == "09")
                {
                    Alarm_water_result[4] = 0X39;
                    Alarm_water_result[6] = 0X30;
                    buffer = Alarm_water_result;
                }
            }
            else if (Str_ascii.Substring(0, 3) == "QTS") //Tank status [4] == tt ; [6] == Alarm
            {
                if (Str_ascii.Substring(3, 2) == "01")
                {
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "02")
                {
                    Tank_status_result[4] = 0X32;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "03")
                {
                    Tank_status_result[4] = 0X33;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "04")
                {
                    Tank_status_result[4] = 0X34;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "05")
                {
                    Tank_status_result[4] = 0X35;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "06")
                {
                    Tank_status_result[4] = 0X36;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "07")
                {
                    Tank_status_result[4] = 0X37;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "08")
                {
                    Tank_status_result[4] = 0X38;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
                if (Str_ascii.Substring(3, 2) == "09")
                {
                    Tank_status_result[4] = 0X39;
                    Tank_status_result[6] = 0X30;
                    buffer = Tank_status_result;
                }
            }
            else if (Str_ascii.Substring(0, 4) == "QADS") //Alarm sump density [5] == tt ; [7] == Alarm
            {
                if (Str_ascii.Substring(4, 2) == "01")
                {
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "02")
                {
                    Alarm_sump_density_result[5] = 0X32;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "03")
                {
                    Alarm_sump_density_result[5] = 0X33;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "04")
                {
                    Alarm_sump_density_result[5] = 0X34;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "05")
                {
                    Alarm_sump_density_result[5] = 0X35;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "06")
                {
                    Alarm_sump_density_result[5] = 0X36;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "07")
                {
                    Alarm_sump_density_result[5] = 0X37;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "08")
                {
                    Alarm_sump_density_result[5] = 0X38;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "09")
                {
                    Alarm_sump_density_result[5] = 0X39;
                    Alarm_sump_density_result[7] = 0X30;
                    buffer = Alarm_sump_density_result;
                }
            }
            else if (Str_ascii.Substring(0, 4) == "QADP") //Alarm product density [5] == tt ; [7] == Alarm
            {
                if (Str_ascii.Substring(4, 2) == "01")
                {
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "02")
                {
                    Alarm_product_density_result[5] = 0X32;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "03")
                {
                    Alarm_product_density_result[5] = 0X33;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "04")
                {
                    Alarm_product_density_result[5] = 0X34;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "05")
                {
                    Alarm_product_density_result[5] = 0X35;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "06")
                {
                    Alarm_product_density_result[5] = 0X36;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "07")
                {
                    Alarm_product_density_result[5] = 0X37;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "08")
                {
                    Alarm_product_density_result[5] = 0X38;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
                if (Str_ascii.Substring(4, 2) == "09")
                {
                    Alarm_product_density_result[5] = 0X39;
                    Alarm_product_density_result[7] = 0X30;
                    buffer = Alarm_product_density_result;
                }
            }
            else
            {
                buffer = ERROR_result;
            }
            #endregion*/

            //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
            buffer = responseList.ToArray();
        }

        public void ProcessResponse(Tank tank, byte[] buffer, byte[] message)
        {
            //เเปลง Dec ==> ASCII
            string s = Encoding.ASCII.GetString(buffer);  // Q S X sp 1 & 1 $ sp 0 & 2 $ 4 6 3 2 F D E D & 7 $ 4 2 7 7 5 2 3 E & 8 $ 0 : 1 4 5 cr lf
            string m = Encoding.ASCII.GetString(message); //Q A X sp 1 & sp sp 3 9 0 cr lf   15-4 =11
            string x = s.Substring(1, 3);
            if (s.Substring(0, 3) == "QAX")
                TankInventoryResponse(tank, s);
            else if (s.Substring(0, 3) == "QPS")
                StatuProbeResponse(tank, s);
            else if (s.Substring(0, 3) == "QAP")
                StatusAlarmproductResponse(tank, s);
            else if (s.Substring(0, 3) == "QAW")
                StatusAlarmwaterResponse(tank, s);
            else if (s.Substring(0, 3) == "QTS")
                StatusTankResponse(tank, s);
            else if (s.Substring(0, 4) == "QADS")
                StatusAlarmSumpDensityResponse(tank, s);
            else if (s.Substring(0, 4) == "QADP")
                StatusAlarmProductDensityResponse(tank, s);
            else if (s.Substring(0, 2) == "QH")
                TankDeliveryResponse(tank, s);
            else if (s == "ERROR\r\n")
                OnLogResponse(tank, "Command " + m.Substring(0, m.Length - 2) + " not support in this ATG.", "Error", "Response", true);
        }

        public string CheckCRC(string buffer)
        {
            var sum = 0;
            List<byte> messageList = new List<byte>();
            for (int i = 0; i < buffer.Length; i += 2)
            {
                string hs = buffer.Substring(i, 2);
                messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
            }
            foreach (var t in messageList)
            {
                sum += t;
            }
            sum = ~sum + 1;
            string hCRC = Convert.ToString(sum, 16);
            hCRC = hCRC.Substring(hCRC.Length - 4).ToUpper();
            return hCRC;
        }


        public void TankInventoryResponse(Tank tank, string receive) // receive : Q A X sp 1 & 1 $ sp 0 & 2 $ 4 6 3 2 F D E D & 7 $ 4 2 7 7 5 2 3 E & 8 $ 0 : 1 4 5 cr lf
        {
            try
            {
                int numTank = int.Parse(receive.Substring(3, 2));
                if (numTank == tank.Number && receive.Length >= 38)
                {
                    tank.DateStamp = DateTime.Now;
                    tank.TankProbeStatus = 1;
                    tank.GaugeVolume = Calculate(receive.Substring(13, 8));
                    tank.Ullage = Calculate(receive.Substring(24, 8));
                    tank.GaugeTcVolume = Calculate(receive.Substring(35, 8));
                    tank.Temperature = Calculate(receive.Substring(46, 8));
                    tank.Height = Calculate(receive.Substring(57, 8));
                    tank.WaterLevel = Calculate(receive.Substring(68, 8));
                    tank.WaterVolume = Calculate(receive.Substring(79, 7));
                    OnTankUpdated(tank, "TankInventory");
                    OnLogResponse(tank, "Tank inventory update for Gauge ID : " + tank.Id + " for TankID : " + tank.Id
                                        + " [vol : " + tank.GaugeVolume.ToString("F5") + " tc_vol : " + tank.GaugeTcVolume.ToString("F5") + " ullage : " + tank.Ullage.ToString("F5")
                                        + " height : " + tank.Height.ToString("F5") + " water : " + tank.WaterLevel.ToString("F5") + " water_vol : " + tank.WaterVolume.ToString("F5")
                                        + " temperature : " + tank.Temperature.ToString("F5"), "Info", "Response", false);
                }
                else
                {
                    tank.TankProbeStatus = 2;
                    if (numTank != tank.Number)
                        OnLogResponse(tank, "Response Tank ID not match.", "Error", "Response", false);
                    else if (receive.Length < 38)
                        OnLogResponse(tank, "Response Tank ID " + tank.Id + " not found data.", "Error", "Response", false);
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankInventoryResponse : " + e.Message, "Error", "Response", false);
            }
        }

        public void StatuProbeResponse(Tank tank, string receive) //QPS 1$ 0:244\r\n
        {
            try
            {
                string data = receive.Remove(0, 6);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == " 0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status of Probe Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status of probe  normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatuProbe_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void StatusAlarmproductResponse(Tank tank, string receive) //QAP 1$0:196\r\n
        {
            try
            {
                string data = receive.Remove(0, 6);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == "0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status Alarm Product Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status Alarm product  normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatusAlarmproductCommand_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }
        public void StatusAlarmwaterResponse(Tank tank, string receive)
        {
            try
            {
                string data = receive.Remove(0, 6);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == "0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status Alarm water Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status Alarm water  normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatusAlarmwaterCommand_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void StatusTankResponse(Tank tank, string receive)
        {
            try
            {
                string data = receive.Remove(0, 6);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == "0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status Tank Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status Tank normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatusTankCommand_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void StatusAlarmSumpDensityResponse(Tank tank, string receive) //"QADS 1$0: 10\r\n"
        {
            try
            {
                string data = receive.Remove(0, 7);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == "0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status Alarm Sump Density Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status Alarm Sump Density normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatusAlarmSumpDensityCommand_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void StatusAlarmProductDensityResponse(Tank tank, string receive) //"QADS 1$0: 10\r\n"
        {
            try
            {
                string data = receive.Remove(0, 7);
                data = data.Remove(data.Length - 6);
                List<int> numTank = new List<int>();
                if (data == "0")
                {
                    tank.TankAlarmCategory = "00";
                    tank.TankAlarmType = "00";
                    tank.TankAlarmDescription = "Status Alarm Product Density Normal";
                    OnTankUpdated(tank, "SystemStatus");
                    OnLogResponse(tank, "Tank ID : " + tank.Id + " is Status Alarm Product Density  normal.", "Info", "Response", false);
                }
                else
                {
                    string A = StatusAlarmProductDensityCommand_Checker(data);
                    if (tank != null)
                    {
                        tank.TankAlarmCategory = data;
                        tank.TankAlarmType = data;
                        tank.TankAlarmDescription = A;
                        OnTankUpdated(tank, "SystemStatus");
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " is " + A, "Alarm", "Response", false);
                    }
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void TankDeliveryResponse(Tank tank, string receive)
        {
            try
            {
                //"QH 1X 1&1$220309$ 74510$220309$ 74624 &2$4506634B$451EB011$43C26630 &3$4506FA3C$451F6196$43C33AD0 &4$41D2C668$41D2E46C$3C702000 &5$43D1238E$43EC3AF4$4258BB30:103\r\n"
                //QH 1X 1|&1$220309$ 74510$220309$ 74624|&2$4506634B$451EB011$43C26630|&3$4506FA3C$451F6196$43C33AD0|&4$41D2C668$41D2E46C$3C702000|&5$43D1238E$43EC3AF4$4258BB30:103     
                //bit 1 : &1$220309$ 74510$220309$ 74624  ==> start_date | start_time | stop_date | stop_time 
                //bit 2 : &2$4506634B$451EB011$43C26630   ==> start volume | stop volume
                //bit 3 : &3$4506FA3C$451F6196$43C33AD0   ==> tc_start_volume | tc_stop_volume
                //bit 4 : &4$41D2C668$41D2E46C$3C702000   ==> start_temp | stop_temp 
                //bit 5 : &5$43D1238E$43EC3AF4$4258BB30   ==> start_product_level | stop_product_level 

                //bit 1 
                String StartDate = receive.Substring(10, 6);        //220309
                String EndDate = receive.Substring(24, 6);          //220309
                String StartTime = receive.Substring(17, 6);        //sp74510 
                String EndTime = receive.Substring(31, 6);          //sp74624
                //bit 2 
                String StartGaugeVolume = receive.Substring(40, 8); //"4506634B"
                String EndGaugeVolume = receive.Substring(49, 8);   //"451EB011"
                //bit 3 
                String StartGaugeTcVolume = receive.Substring(69, 8); //"4506FA3C"
                String EndGaugeTcVolume = receive.Substring(78, 8);   //"451F6196"
                //bit 4 
                String StartTemperature = receive.Substring(98, 8); //"41D2C668"
                String EndTemperature = receive.Substring(107, 8);   //"41D2E46C"
                //bit 5 
                String StartHeight = receive.Substring(127, 8);   //"43D1238E" 
                String EndHeight = receive.Substring(136, 8); //"43EC3AF4"
                //bit 6 
                String StartWaterVolume = receive.Substring(156, 8); //"00000000" 
                String EndWaterVolume = receive.Substring(165, 8);   //"00000000"

                int numTank = int.Parse(receive.Substring(2, 2));
                int tank_Number = tank.Number;
                int receive_Length = receive.Length;

                if (numTank == tank.Number && receive.Length >= 72)
                {
                    tank.DateStampDeliver = DateTime.Now;

                    for (int i = 0; i < 1; i++)
                    {
                        tank.StartDate.Add(DateTime.ParseExact(receive.Substring(10, 6) + receive.Substring(17, 6).Replace(' ', '0'), "yyMMddHHmmss", new CultureInfo("en-US"))); //bit1 220309
                        tank.EndDate.Add(DateTime.ParseExact(receive.Substring(24, 6) + receive.Substring(31, 6).Replace(' ', '0'), "yyMMddHHmmss", new CultureInfo("en-US")));   //bit1 220309
                        tank.StartGaugeVolume.Add(Calculate(receive.Substring(40, 8)));   //bit2
                        tank.StartGaugeTcVolume.Add(Calculate(receive.Substring(69, 8)));        //bit3
                        tank.StartWaterVolume.Add(Calculate(receive.Substring(156, 8)));                                 //bit6
                        tank.StartTemperature.Add(Calculate(receive.Substring(98, 8)));                   //bit4
                        tank.EndGaugeVolume.Add(Calculate(receive.Substring(49, 8)));     //bit2
                        tank.EndGaugeTcVolume.Add(Calculate(receive.Substring(78, 8)));          //bit3
                        tank.EndWaterVolume.Add(Calculate(receive.Substring(165, 8)));                                   //bit6
                        tank.EndTemperature.Add(Calculate(receive.Substring(107, 8)));                     //bit4
                        tank.StartHeight.Add(Calculate(receive.Substring(127, 8)));                                //bit5
                        tank.EndHeight.Add(Calculate(receive.Substring(136, 8)));                                  //bit5

                        OnLogResponse(tank, "In-Tank Delivery Report by Tank ID : " + tank.Id + " [start_time: " + tank.StartDate[i] + "  end_time: " + tank.EndDate[i] + " ]", "Info", "Response", false);
                        OnLogResponse(tank, "In-Tank Delivery Report by Tank ID : " + tank.Id + " [start_volume: " + tank.StartGaugeVolume[i] + "  end_volume: " + tank.EndGaugeVolume[i] + " ]", "Info", "Response", false);

                    }
                    OnTankUpdated(tank, "TankDelivery");
                }
                else
                {
                    if (numTank != tank.Number)
                        OnLogResponse(tank, "Response Tank ID not match.", "Error", "Response", false);
                    else if (receive.Length < 38)
                        OnLogResponse(tank, "Response Tank ID " + tank.Id + " not found data.", "Error", "Response", false);
                }
            }
            catch (Exception e)
            {
                if (receive.Substring(10, 6) == "     0")
                {
                    OnLogResponse(tank, "In-Tank Delivery Report by Tank ID : " + receive.Substring(2, 2).Replace(" ", "") + " No Data Delivery", "Error", "Response", true);
                }
                else
                {
                    OnLogResponse(tank, "TankDeliveryResponse : " + e.Message, "Error", "Response", false);
                }
            }
        }

        public double Calculate(string buffer)
        {
            UInt32 value = UInt32.Parse(buffer, System.Globalization.NumberStyles.HexNumber);
            bool isNegative = Convert.ToBoolean(value >> 31);
            value = value << 1;
            UInt16 E = (UInt16)(value >> 24);
            UInt32 M = (value & 0x00FFFFFF) >> 1;
            float Exp = (float)(Math.Pow(2, (E - 127)));
            float Man = (float)(1.0 + ((float)M / 8388608.0));
            float result = Exp * Man;
            if (isNegative)
                result = result * -1;
            result = (float)(Math.Round(result, 5));
            //fix for 2 decimal
            return result;
        }
        public string StatuProbe_Checker(string a)
        {
            switch (a)
            {
                case " 1":
                    return "Probe reports internal error.";
                case " 5":
                    return "Probe reports temperature measuring error.";
                case " 6":
                    return "Probe reports level measuring error.";
                case " 7":
                    return "Probe reports reduced measuring accuracy. ";
                case " 9":
                    return "Wireless transmitter reports missing probe response";
                case "10":
                    return "Probe communication error between VISY-Command and probe. ";
                case "11":
                    return "No response from probe or wireless transmitter.";
                case "12":
                    return "Incompatible probe data ";
                case "13":
                    return "Waiting for first incoming wireless data";
                case "99":
                    return "Probe not configured.";
                default:
                    return "Response 'A' not match in case.";
            }
        }
        public string StatusAlarmproductCommand_Checker(string a)
        {
            switch (a)
            {
                case "1":
                    return "Low Low alarm.";
                case "2":
                    return "Low alarm.";
                case "3":
                    return "High alarm.";
                case "4":
                    return "High High alarm.";
                default:
                    return "Response 'A' not match in case.";
            }
        }

        public string StatusAlarmwaterCommand_Checker(string a)
        {
            switch (a)
            {
                case "1":
                    return "High alarm.";
                case "2":
                    return "High High alarm.";
                default:
                    return "Response 'A' not match in case.";
            }
        }

        public string StatusTankCommand_Checker(string a)
        {
            switch (a)
            {
                case "1":
                    return "Delivery in progress or waves on product surface";
                default:
                    return "Response 'A' not match in case.";
            }
        }

        public string StatusAlarmSumpDensityCommand_Checker(string a)
        {
            switch (a)
            {
                case "2":
                    return "Low alarm.";
                case "3":
                    return "High Alarm.";
                default:
                    return "Response 'A' not match in case.";
            }
        }

        public string StatusAlarmProductDensityCommand_Checker(string a)
        {
            switch (a)
            {
                case "2":
                    return "Low alarm.";
                case "3":
                    return "High Alarm.";
                default:
                    return "Response 'A' not match in case.";
            }
        }


        public event EventHandler TimeoutResponse;
        protected internal void OnTimeoutResponse(Tank tank, string t)
        {
            this.timemessage = t;
            if (TimeoutResponse != null)
                TimeoutResponse(tank, EventArgs.Empty);
        }
        public event EventHandler LogManageResponse;
        protected internal void OnLogResponse(Tank tank, string s, string catagory, string type, bool isAllTank)
        {
            this.logMessage = s;
            this.logCatagory = catagory;
            this.logType = type;
            this.isAll = isAllTank;
            if (LogManageResponse != null)
                LogManageResponse(tank, EventArgs.Empty);
        }

        public event EventHandler LogFileResponse;
        protected internal void OnLogFileResponse(Tank tank, string message, string type)
        {
            this.gdMessage = message;
            this.gdType = type;
            if (LogFileResponse != null)
                LogFileResponse(tank, EventArgs.Empty);
        }

        public event EventHandler StatusChanged;
        protected internal void OnStatusChanged(Tank tank)
        {
            if (StatusChanged != null)
                StatusChanged(tank, EventArgs.Empty);
        }
        public event EventHandler TankUpdated;
        protected internal void OnTankUpdated(Tank tank, string cmd)
        {
            this.cmdType = cmd;
            if (StatusChanged != null)
                TankUpdated(tank, EventArgs.Empty);
        }

    }
}
