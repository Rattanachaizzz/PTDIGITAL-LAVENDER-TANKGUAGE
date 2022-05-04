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
    public class VeederRoot
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
            this.TankUpdated += (sender2, e2) => tankManagement.UpdateTank(sender2, e2, cmdType);
            this.TimeoutResponse += (sender2, e2) => tankManagement.TimeoutResponse(sender2, e2, timemessage);
            this.LogManageResponse += (sender2, e2) => tankManagement.LogManagement(sender2, e2, logMessage, logCatagory, logType, isAll);
            this.CommandManageResponse += tankManagement.ClearCommand;
            this.LogFileResponse += (sender2, e2) => tankManagement.LogFileGD(sender2, e2, gdMessage, gdType);
            this.allTanks = tanks;
            isReconcil = features.SingleOrDefault(w => w.feature_id == 5) != null;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            features = features.OrderBy(w => w.feature_id).ToList();
            while (true)
            {
                if (id == features.Count)
                {
                    id = 0;
                    watch.Restart();
                }

                if (isReconcil && CheckPumpStatus(tanks))
                {
                    foreach (Tank t in tanks)
                    {
                        foreach (Hose hose in t.Hoses)
                        {
                            if (t.EvenId == 10) t.EvenId = 0;
                            if (hose.PendingCommand == "B")
                                B_Command(t, hose);
                            else if (hose.PendingCommand == "C")
                                C_Command(t, hose);

                            t.EvenId++;
                        }
                    }
                }
                if (isReconcil && watch2.ElapsedMilliseconds > 20000)
                {
                    D_Command(tanks.OrderBy(w => w.Id).First());
                    watch2.Restart();
                }

                if (watch.ElapsedMilliseconds > tanks[0].TimeDelay)
                {
                    switch (features[id].feature_id)
                    {
                        case 1:
                            foreach (Tank tank in tanks)
                            {
                                TankInventoryCommand(tank);
                            }
                            break;
                        case 2:
                            Thread.Sleep(500);
                            SystemStatusCommand(tanks.OrderBy(w => w.Id).First());
                            break;
                        case 3:
                            Thread.Sleep(500);
                            TankDeliveryCommand(tanks.OrderBy(w => w.Id).First());
                            break;
                        case 4:
                            Thread.Sleep(500);
                            TankDeliveryCleanCommand(tanks.OrderBy(w => w.Id).First());
                            break;
                        case 6:
                            foreach (Tank tank in tanks)
                            {
                                Gaia_B_Command(tank);
                                Thread.Sleep(200);
                                Gaia_C_Command(tank);
                                Thread.Sleep(200);
                            }
                            break;
                    }
                    id++;
                }
                if (isReconcil && watch2.ElapsedMilliseconds > 20000)
                {
                    D_Command(tanks.OrderBy(w => w.Id).First());
                    watch2.Restart();
                }

            }
        }

        public bool CheckPumpStatus(List<Tank> tanks)
        {
            bool result = false;
            foreach (Tank tank in tanks)
            {
                OnStatusChanged(tank);
                if (tank.Hoses.Count(w => !string.IsNullOrEmpty(w.PendingCommand)) != 0)
                    result = true;
            }
            return result;
        }

        public void TankInventoryCommand(Tank tank)
        {
            try
            {
                //i201TT - Inventory Check for Tank Number TT
                List<byte> messageList = new List<byte>();
                string command = "01693230313" + tank.Number / 10 + "3" + tank.Number % 10 + "03";
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

        public void SystemStatusCommand(Tank tank)
        {
            try
            {
                //i10100 - Alarm check for all tanks
                List<byte> messageList = new List<byte>();
                string command = "01693130313030";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Request System Status for all Tanks.", "Info", "Request", true);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "SystemStatusCommand : " + e.Message, "Error", "Request", true);
            }
        }

        public void TankDeliveryCommand(Tank tank)
        {
            try
            {
                //i20200 - Tank Delivery check for all tanks
                List<byte> messageList = new List<byte>();
                string command = "01693230323030";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Request Tank Delivery for all Tanks.", "Info", "Request", true);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankDeliveryCommand : " + e.Message, "Error", "Request", true);
            }
        }

        public void TankDeliveryCleanCommand(Tank tank)
        {
            try
            {
                //s05100 - Tank Delivery clean for all tanks
                List<byte> messageList = new List<byte>();
                string command = "01733035313030";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Request all tanks Clear In-Tank Delivery.", "Info", "Request", true);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankDeliveryCleanCommand : " + e.Message, "Error", "Request", true);
            }
        }

        public void D_Command(Tank tank)
        {
            try
            {
                //<SOH>D<EOT> - BIR EDIM {D} Command
                List<byte> messageList = new List<byte>();
                string command = "014404";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "BIR heartbeat check [D] Command sent.", "Info", "Request", true);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "D_Command : " + e.Message, "Info", "Request", true);
            }
        }

        public void B_Command(Tank tank, Hose hose)
        {
            try
            {
                //<SOH>B<EOT> - BIR EDIM {B} Command
                hose.PendingCommand = "";
                hose.IsActive = true;
                List<byte> messageList = new List<byte>();
                string command = "01423" + tank.EvenId + "3030303030303" + hose.PumpId / 10 + "3" + hose.PumpId % 10;
                command += BitConverter.ToString(Encoding.ASCII.GetBytes(CheckCRC(command))).Replace("-", "") + "04";

                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Event start command is sent for Pump ID : " + hose.PumpId, "Info", "Request", false);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "B_Command : " + e.Message, "Error", "Request", false);
            }
        }

        public void C_Command(Tank tank, Hose hose)
        {
            try
            {
                //<SOH>C<EOT> - BIR EDIM {C} Command
                hose.PendingCommand = "";
                hose.IsActive = true;
                List<byte> messageList = new List<byte>();
                string totalVolume = hose.TotalVolume.ToString("000000.00");
                string transVolume = hose.TransactionVolume.ToString("0000.000");
                totalVolume = totalVolume.Substring(totalVolume.Length - 9);
                transVolume = transVolume.Substring(transVolume.Length - 8);
                string command = "01433" + tank.EvenId + "3030303030303" + hose.PumpId / 10 + "3" + hose.PumpId % 10 + "313" + (hose.HoseNumber - 1);
                foreach (char letter in (totalVolume + transVolume))
                {
                    int value = Convert.ToInt32(letter);
                    command += String.Format("{0:X2}", value);
                }
                command += BitConverter.ToString(Encoding.ASCII.GetBytes(CheckCRC(command))).Replace("-", "") + "04";

                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "Transaction command is sent for Pump ID : " + hose.PumpId + " Hose Number : " + hose.HoseNumber + " Volume : " + transVolume + " Meter : " + totalVolume, "Info", "Request", false);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "C_Command : " + e.Message, "Error", "Request", false);
            }
        }
        public void Gaia_B_Command(Tank tank)
        {
            try
            {
                //<SOH>i20B<ETX> - BIR EDIM {i20B} Command
                List<byte> messageList = new List<byte>();
                string command = "01693230423" + tank.Number / 10 + "3" + tank.Number % 10 + "03";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "BIR Adjusted Delivery Report Command sent.", "Info", "Request", false);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "Gaia_B_Command : " + e.Message, "Error", "Request", false);
            }
        }
        public void Gaia_C_Command(Tank tank)
        {
            try
            {
                //<SOH>i20C<ETX> - BIR EDIM {i20C} Command
                List<byte> messageList = new List<byte>();
                string command = "01693230433" + tank.Number / 10 + "3" + tank.Number % 10 + "03";
                for (int i = 0; i < command.Length; i += 2)
                {
                    string hs = command.Substring(i, 2);
                    messageList.Add((byte)Convert.ToChar(Convert.ToUInt32(hs, 16)));
                }

                OnLogResponse(tank, "In-Tank Most Recent Delivery Report Command sent.", "Info", "Request", false);
                SendMessage(tank, messageList.ToArray());
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "Gaia_C_Command : " + e.Message, "Error", "Request", false);
            }
        }
        public void SendMessage(Tank tank, byte[] message)
        {
            byte[] buffer;
            int count = 0;
            string receive = "";
            string data = Encoding.ASCII.GetString(message);
            SerialPort serialPort = tank.SerialPort;
            try
            {
            again:
                Console.WriteLine("Message Command : " + data);
                //Thread t1 = new Thread(() => LogFileGD(tank, data, "SEND   "));
                //t1.Start();
                //t1.Join();
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
                    //Thread t2 = new Thread(() => LogFileGD(tank, System.Text.Encoding.ASCII.GetString(buffer), "RECEIVE"));
                    //t2.Start();
                    OnLogFileResponse(tank, System.Text.Encoding.ASCII.GetString(buffer), "RECEIVE");
                    receive = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine("Response Command : " + receive);
                    tank.CountDisconn = 0;

                    if (data.Substring(1, 1) != "B" && data.Substring(1, 1) != "C" && data.Substring(1, 1) != "D")
                    {
                        string check = receive.Substring(receive.Length - 5, 4);
                        if (CheckCRC(BitConverter.ToString(buffer).Replace("-", "").Remove(BitConverter.ToString(buffer).Replace("-", "").Length - 10)) != check) goto again;
                    }

                    ProcessResponse(tank, buffer, message);
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
            string data = BitConverter.ToString(message).Replace("-", ""), receive = "";
            DateTime timeOut = DateTime.Now;
            try
            {
                while (!isSuccess)
                {
                    if (isFirst && DateTimeOffset.Now.Subtract(timeOut).TotalMilliseconds > tank.ReadDataTimeout)
                    {
                        OnTimeoutResponse(tank, receive);
                        return;
                    }

                    responseList.Add((byte)serialPort.ReadByte());

                    if (responseList.Count > 0 && !isFirst)
                    {
                        isFirst = true;
                        timeOut = DateTime.Now;
                    }

                    receive = BitConverter.ToString(responseList.ToArray()).Replace("-", "");
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

            buffer = responseList.ToArray();
        }
        public void ProcessResponse(Tank tank, byte[] buffer, byte[] message)
        {
            string s = Encoding.ASCII.GetString(buffer);
            string m = Encoding.ASCII.GetString(message);
            if (m.Substring(1, 1) == "B" && BitConverter.ToString(buffer) == "06")
                B_Response(tank);
            else if (m.Substring(1, 1) == "C" && BitConverter.ToString(buffer) == "06")
                C_Response(tank);
            else if (m.Substring(1, 1) == "D" && BitConverter.ToString(buffer) == "06")
                D_Response(tank);
            else if (s.Substring(1, 4) == "i201")
                TankInventoryResponse(tank, s);
            else if (s.Substring(1, 4) == "i101")
                SystemStatusResponse(tank, s);
            else if (s.Substring(1, 4) == "i202")
                TankDeliveryResponse(tank, s);
            else if (s.Substring(1, 4) == "s051")
                TankDeliveryCleanResponse(tank);
            else if (s.Substring(1, 4) == "i20B")
                Gaia_B_Response(tank, s);
            else if (s.Substring(1, 4) == "i20C")
                Gaia_C_Response(tank, s);
            else if (s.Substring(1, 4) == "9999")
                OnLogResponse(tank, " Command " + m + " not support in this ATG.", "Error", "Response", true);
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

        public void TankInventoryResponse(Tank tank, string receive)
        {
            try
            {
                int numTank = int.Parse(receive.Substring(5, 2));
                if (numTank == tank.Number && receive.Length >= 82)
                {
                    int numRound = int.Parse(receive.Substring(24, 2));
                    tank.DateStamp = DateTime.Now;
                    for (int i = 0; i < numRound; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                tank.GaugeVolume = Calculate(receive.Substring(26, 8));
                                break;
                            case 1:
                                tank.GaugeTcVolume = Calculate(receive.Substring(34, 8));
                                break;
                            case 2:
                                tank.Ullage = Calculate(receive.Substring(42, 8));
                                break;
                            case 3:
                                tank.Height = Calculate(receive.Substring(50, 8));
                                break;
                            case 4:
                                tank.WaterLevel = Calculate(receive.Substring(58, 8));
                                break;
                            case 5:
                                tank.Temperature = Calculate(receive.Substring(66, 8));
                                break;
                            case 6:
                                tank.WaterVolume = Calculate(receive.Substring(74, 8));
                                break;
                        }
                    }

                    tank.TankProbeStatus = 1;
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
                    else if (receive.Length < 82)
                        OnLogResponse(tank, "Response Tank ID " + tank.Id + " not found data.", "Error", "Response", false);
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankInventoryResponse : " + e.Message, "Error", "Response", false);
            }
        }
        public void SystemStatusResponse(Tank tank, string receive)
        {
            try
            {
                string data = receive.Remove(0, 17);
                data = data.Remove(data.Length - 7);
                string A = "", N = "";
                List<int> numTank = new List<int>();
                if (data == "00")
                {
                    foreach (Tank t in this.allTanks)
                    {
                        t.TankAlarmCategory = "00";
                        t.TankAlarmType = "00";
                        t.TankAlarmDescription = "All Function Normal";
                        OnTankUpdated(t, "SystemStatus");
                        OnLogResponse(t, "Tank alarm updated for Tank ID :  " + t.Id + " [00]", "Info", "Response", false);
                        OnLogResponse(t, "Tank ID : " + t.Id + " is All function normal.", "Info", "Response", false);
                    }
                }
                else
                {
                    for (int i = 0; i < data.Length / 6; i++)
                    {
                        A = A_Checker(data.Substring(0 + (i * 6), 2));
                        N = N_Checker(data.Substring(2 + (i * 6), 2), data.Substring(0 + (i * 6), 2));
                        numTank.Add(int.Parse(data.Substring(4 + (i * 6), 2)));

                        var getTank = this.allTanks.FirstOrDefault(w => w.Number == int.Parse(data.Substring(4 + (i * 6), 2)));
                        if (getTank != null)
                        {
                            getTank.TankAlarmCategory = data.Substring(0 + (i * 6), 2);
                            getTank.TankAlarmType = data.Substring(2 + (i * 6), 2);
                            getTank.TankAlarmDescription = A + " " + N;
                            OnTankUpdated(getTank, "SystemStatus");
                            OnLogResponse(getTank, "Tank alarm updated for Tank ID :  " + getTank.Id + " [" + data.Substring(0 + (i * 6), 6) + "]", "Alarm", "Response", false);
                            OnLogResponse(getTank, "Tank ID : " + getTank.Id + " is " + A + " " + N, "Alarm", "Response", false);
                        }
                    }
                    foreach (Tank gauge in this.allTanks.Where(w => !numTank.Contains(w.Number)).ToList())
                    {
                        gauge.TankAlarmCategory = "00";
                        gauge.TankAlarmType = "00";
                        gauge.TankAlarmDescription = "All Function Normal";
                        OnTankUpdated(gauge, "SystemStatus");
                        OnLogResponse(gauge, "Tank alarm updated for Tank ID :  " + gauge.Id + " [00]", "Info", "Response", false);
                        OnLogResponse(gauge, "Tank ID : " + gauge.Id + " is All function normal.", "Info", "Response", false);
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
                string data = receive.Remove(0, 17);
                data = data.Remove(data.Length - 7);
                List<int> numTank = new List<int>();
                int cData = 0;
            nextTank:
                int nTank = int.Parse(data.Substring(0, 2));
                int rDelivery = int.Parse(data.Substring(3, 2), NumberStyles.HexNumber);
                var getTank = this.allTanks.FirstOrDefault(w => w.Number == nTank);
                getTank.ProductCode = (int)Convert.ToByte(Convert.ToChar(data.Substring(2, 1)));
                data = data.Remove(0, 5);

                if (getTank != null && rDelivery != 0)
                {
                    getTank.DateStampDeliver = DateTime.Now;
                    for (int i = 0; i < rDelivery; i++)
                    {
                        getTank.StartDate.Add(DateTime.ParseExact(data.Substring(0, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        getTank.EndDate.Add(DateTime.ParseExact(data.Substring(10, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        int rData = int.Parse(data.Substring(20, 2), NumberStyles.HexNumber);
                        data = data.Remove(0, 22);
                        for (int j = 0; j < rData; j++)
                        {
                            switch (j)
                            {
                                case 0:
                                    getTank.StartGaugeVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 1:
                                    getTank.StartGaugeTcVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 2:
                                    getTank.StartWaterVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 3:
                                    getTank.StartTemperature.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 4:
                                    getTank.EndGaugeVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 5:
                                    getTank.EndGaugeTcVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 6:
                                    getTank.EndWaterVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 7:
                                    getTank.EndTemperature.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 8:
                                    getTank.StartHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 9:
                                    getTank.EndHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                            }
                            data = data.Remove(0, 8);
                        }

                        if (getTank != null)
                        {
                            OnLogResponse(getTank, "In-Tank Delivery Report by Tank ID : " + getTank.Id + " [start_time: " + getTank.StartDate[i] + "  end_time: " + getTank.EndDate[i] + " ]", "Info", "Response", false);
                            OnLogResponse(getTank, "In-Tank Delivery Report by Tank ID : " + getTank.Id + " [start_volume: " + getTank.StartGaugeVolume[i] + "  end_volume: " + getTank.EndGaugeVolume[i] + " ]", "Info", "Response", false);
                        }

                    }
                    OnTankUpdated(getTank, "TankDelivery");
                    if (data.Length != 0)
                        goto nextTank;
                }
                else
                {
                    if (getTank != null)
                        OnLogResponse(getTank, "Tank ID : " + getTank.Id + " no data delivery report.", "Info", "Response", false);
                    if (data.Length != 0)
                        goto nextTank;
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "TankDeliveryResponse : " + e.Message, "Error", "Response", true);
            }
        }

        public void TankDeliveryCleanResponse(Tank tank)
        {
            OnLogResponse(tank, "TankDeliveryClean Command is success.", "Info", "Response", true);
        }

        public void D_Response(Tank tank)
        {
            OnLogResponse(tank, "D Command is success.", "Info", "Response", true);
        }

        public void B_Response(Tank tank)
        {
            OnCommandResponse(tank);
            OnLogResponse(tank, "B Command is success.", "Info", "Response", false);
        }

        public void C_Response(Tank tank)
        {
            OnCommandResponse(tank);
            OnLogResponse(tank, "C Command is success.", "Info", "Response", false);
        }

        public void Gaia_B_Response(Tank tank, string receive)
        {
            try
            {
                string data = receive.Remove(0, 17);
                data = data.Remove(data.Length - 7);
                List<int> numTank = new List<int>();
                int cData = 0;
                int nTank = int.Parse(data.Substring(0, 2));
                int rDelivery = int.Parse(data.Substring(2, 2), NumberStyles.HexNumber);
                var getTank = this.allTanks.FirstOrDefault(w => w.Number == nTank);
                data = data.Remove(0, 4);
                tank.B_DateStamp = DateTime.Now;
                if (rDelivery != 0 && getTank != null && getTank.Id == tank.Id)
                {
                    for (int i = 0; i < rDelivery; i++)
                    {
                        tank.B_StartDate.Add(DateTime.ParseExact(data.Substring(0, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        tank.B_EndDate.Add(DateTime.ParseExact(data.Substring(10, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        int rData = int.Parse(data.Substring(20, 2), NumberStyles.HexNumber);
                        data = data.Remove(0, 22);
                        for (int j = 0; j < rData; j++)
                        {
                            switch (j)
                            {
                                case 0:
                                    tank.B_StartVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 1:
                                    tank.B_EndVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 2:
                                    tank.B_AdjustDeliveryVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 3:
                                    tank.B_AdjustTempVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 4:
                                    tank.B_StartFuelHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 5:
                                    tank.B_StartFuelTemp1.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 6:
                                    tank.B_StartFuelTemp2.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 7:
                                    tank.B_StartFuelTemp3.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 8:
                                    tank.B_StartFuelTemp4.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 9:
                                    tank.B_StartFuelTemp5.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 10:
                                    tank.B_StartFuelTemp6.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 11:
                                    tank.B_EndFuelHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 12:
                                    tank.B_EndFuelTemp1.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 13:
                                    tank.B_EndFuelTemp2.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 14:
                                    tank.B_EndFuelTemp3.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 15:
                                    tank.B_EndFuelTemp4.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 16:
                                    tank.B_EndFuelTemp5.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 17:
                                    tank.B_EndFuelTemp6.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 18:
                                    tank.B_TotalDuringDelivery.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 19:
                                    tank.B_StartFuelTempAvg.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 20:
                                    tank.B_EndFuelTempAvg.Add(Calculate(data.Substring(0, 8)));
                                    break;
                            }
                            data = data.Remove(0, 8);
                        }
                    }
                    if (getTank != null)
                        OnLogResponse(tank, "BIR Adjusted Delivery Report by Tank ID : " + tank.Id + " is success.", "Info", "Response", false);
                    OnTankUpdated(tank, "Gaia_B_Response");
                }
                else
                {
                    if (getTank != null)
                        OnLogResponse(getTank, "Tank ID : " + getTank.Id + " no data delivery report.", "Info", "Response", false);
                    if (getTank != null && getTank.Id != tank.Id)
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " not match with Response.", "Error", "Response", false);
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "Gaia_B_Response : " + e.Message, "Error", "Response", false);
            }
        }
        public void Gaia_C_Response(Tank tank, string receive)
        {
            try
            {
                string data = receive.Remove(0, 17);
                data = data.Remove(data.Length - 7);
                List<int> numTank = new List<int>();
                int cData = 0;
                int nTank = int.Parse(data.Substring(0, 2));
                tank.C_ProductCode = (int)Convert.ToByte(Convert.ToChar(data.Substring(2, 1)));
                int rDelivery = int.Parse(data.Substring(3, 2), NumberStyles.HexNumber);
                var getTank = this.allTanks.FirstOrDefault(w => w.Number == nTank);
                data = data.Remove(0, 5);
                tank.C_DateStamp = DateTime.Now;
                if (rDelivery != 0 && getTank != null && getTank.Id == tank.Id)
                {
                    for (int i = 0; i < rDelivery; i++)
                    {
                        tank.C_StartDate.Add(DateTime.ParseExact(data.Substring(0, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        tank.C_EndDate.Add(DateTime.ParseExact(data.Substring(10, 10), "yyMMddHHmm", new CultureInfo("en-US")));
                        int rData = int.Parse(data.Substring(20, 2), NumberStyles.HexNumber);
                        data = data.Remove(0, 22);
                        for (int j = 0; j < rData; j++)
                        {
                            switch (j)
                            {
                                case 0:
                                    tank.C_StartGaugeVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 1:
                                    tank.C_StartGaugeTcVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 2:
                                    tank.C_StartWaterVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 3:
                                    tank.C_StartTemperature.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 4:
                                    tank.C_EndGaugeVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 5:
                                    tank.C_EndGaugeTcVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 6:
                                    tank.C_EndWaterVolume.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 7:
                                    tank.C_EndTemperature.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 8:
                                    tank.C_StartHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                                case 9:
                                    tank.C_EndHeight.Add(Calculate(data.Substring(0, 8)));
                                    break;
                            }
                            data = data.Remove(0, 8);
                        }
                    }
                    if (getTank != null)
                        OnLogResponse(tank, "In-Tank Most Recent Delivery Report by Tank ID : " + tank.Id + " is success.", "Info", "Response", false);
                    OnTankUpdated(tank, "Gaia_C_Response");
                }
                else
                {
                    if (getTank != null)
                        OnLogResponse(getTank, "Tank ID : " + getTank.Id + " no data delivery report.", "Info", "Response", false);
                    if (getTank != null && getTank.Id != tank.Id)
                        OnLogResponse(tank, "Tank ID : " + tank.Id + " not match with Response.", "Error", "Response", false);
                }
            }
            catch (Exception e)
            {
                OnLogResponse(tank, "Gaia_C_Response : " + e.Message, "Error", "Response", false);
            }
        }

        public string A_Checker(string a)
        {
            switch (a)
            {
                case "01":
                    return "System Alarm";
                case "02":
                    return "Tank Alarm";
                case "03":
                    return "Liquid Sensor Alarm";
                case "04":
                    return "Vapor Sensor Alarm";
                case "05":
                    return "Input Alarm";
                case "06":
                    return "Volumetric Line Leak Alarm";
                case "07":
                    return "Groundwater Sensor Alarm";
                case "08":
                    return "Type A Sensor Alarm";
                case "12":
                    return "Type B Sensor Alarm";
                case "13":
                    return "Universal Sensor Alarm";
                case "14":
                    return "Auto-Dial Fax Alarm";
                case "18":
                    return "Mechanical Dispenser Interface Alarm";
                case "19":
                    return "Electronic Dispenser Interface Alarm";
                case "20":
                    return "Product Alarm";
                case "21":
                    return "Pressure Line Leak Alarm";
                case "26":
                    return "Wireless PLLD Alarm";
                case "28":
                    return "Smart Sensor Alarm";
                case "29":
                    return "Modbus Alarm";
                case "30":
                    return "ISD Site Alarm";
                case "31":
                    return "ISD Hose Alarm";
                case "32":
                    return "ISD Vapor Flow Meter Alarm";
                case "33":
                    return "PMC Alarm";
                case "34":
                    return "Pump Relay Monitor Alarm";
                case "35":
                    return "VMCI Dispenser Interface Alarm";
                case "36":
                    return "VMC Alarm";
                case "99":
                    return "Externally Detected Alarm (not reported by Console)";
                default:
                    return "Response 'A' not match in case.";
            }
        }
        public string N_Checker(string n, string a)
        {
            if (a == "01")
            {
                switch (n)
                {
                    case "01":
                        return "Printer out of Paper";
                    case "02":
                        return "Printer Error";
                    case "03":
                        return "EEPROM Configuration Error";
                    case "04":
                        return "Battery Off";
                    case "05":
                        return "Too Many Tanks";
                    case "06":
                        return "System Security Warning";
                    case "07":
                        return "ROM Revision Warning";
                    case "08":
                        return "Remote Display Communications Error";
                    case "09":
                        return "Autodial Error";
                    case "10":
                        return "Software Module Warning";
                    case "11":
                        return "Tank Test Shutdown Warning";
                    case "12":
                        return "Protective Cover Alarm";
                    case "13":
                        return "BIR Shift Close Pending";
                    case "14":
                        return "BIR Daily Close Pending";
                    case "15":
                        return "PC(H8) Revision Warning";
                    case "16":
                        return "System Self Test Error";
                    case "17":
                        return "System Clock Incorrect Warning";
                    case "18":
                        return "System Device Poll Timeout";
                    case "19":
                        return "Maintenance Tracker NVMem Removed";
                    case "20":
                        return "Maintenance Tracker Communication Module Removed";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "02")
            {
                switch (n)
                {
                    case "01":
                        return "Tank Setup Data Warning";
                    case "02":
                        return "Tank Leak Alarm";
                    case "03":
                        return "Tank High Water Alarm";
                    case "04":
                        return "Tank Overfill Alarm";
                    case "05":
                        return "Tank Low Product Alarm";
                    case "06":
                        return "Tank Sudden Loss Alarm";
                    case "07":
                        return "Tank High Product Alarm";
                    case "08":
                        return "Tank Invalid Fuel Level Alarm";
                    case "09":
                        return "Tank Probe Out Alarm";
                    case "10":
                        return "Tank High Water Warning";
                    case "11":
                        return "Tank Delivery Needed Warning";
                    case "12":
                        return "Tank Maximum Product Alarm";
                    case "13":
                        return "Tank Gross Leak Test Fail Alarm";
                    case "14":
                        return "Tank Periodic Leak Test Fail Alarm";
                    case "15":
                        return "Tank Annual Leak Test Fail Alarm";
                    case "16":
                        return "Tank Periodic Test Needed Warning";
                    case "17":
                        return "Tank Annual Test Needed Warning";
                    case "18":
                        return "Tank Periodic Test Needed Alarm";
                    case "19":
                        return "Tank Annual Test Needed Alarm";
                    case "20":
                        return "Tank Leak Test Active";
                    case "21":
                        return "Tank No CSLD Idle Time Warning";
                    case "22":
                        return "Tank Siphon Break Active Warning";
                    case "23":
                        return "Tank CSLD Rate Increase Warning";
                    case "24":
                        return "Tank AccuChart Calibration Warning";
                    case "25":
                        return "Tank HRM Reconciliation Warning";
                    case "26":
                        return "Tank HRM Reconciliation Alarm";
                    case "27":
                        return "Tank Cold Temperature Warning";
                    case "28":
                        return "Tank Missing Delivery Ticket Warning";
                    case "29":
                        return "Tank/Line Gross Leak Alarm";
                    case "30":
                        return "Delivery Density Warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "03" || a == "04" || a == "07" || a == "08" || a == "12" || a == "13")
            {
                switch (n)
                {
                    case "02":
                        return "Sensor Setup Data Warning";
                    case "03":
                        return "Sensor Fuel Alarm";
                    case "04":
                        return "Sensor Out Alarm";
                    case "05":
                        return "Sensor Short Alarm";
                    case "06":
                        return "Sensor Water Alarm";
                    case "07":
                        return "Sensor Water Out Alarm";
                    case "08":
                        return "Sensor High Liquid Alarm";
                    case "09":
                        return "Sensor Low Liquid Alarm";
                    case "10":
                        return "Sensor Liquid Warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "05")
            {
                switch (n)
                {
                    case "01":
                        return "Input Setup Data Warning";
                    case "02":
                        return "Input Normal";
                    case "03":
                        return "Input Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "06")
            {
                switch (n)
                {
                    case "01":
                        return "VLLD Setup Data Warning";
                    case "02":
                        return "VLLD Self Test Alarm";
                    case "03":
                        return "VLLD Shutdown Alarm";
                    case "04":
                        return "VLLD Leak Test Fail Alarm";
                    case "05":
                        return "VLLD Selftest Invalid Warning";
                    case "06":
                        return "VLLD Continuous Handle On Warning";
                    case "07":
                        return "VLLD Gross Line Test Fail Alarm";
                    case "08":
                        return "VLLD Gross Line Selftest Fail Alarm";
                    case "09":
                        return "VLLD Gross Pump Test Fail Alarm";
                    case "10":
                        return "VLLD Gross Pump Selftest Fail Alarm";
                    case "11":
                        return "VLLD Periodic Test Needed Warning";
                    case "12":
                        return "VLLD Annual Test Needed Warning";
                    case "13":
                        return "VLLD Periodic Test Needed Alarm";
                    case "14":
                        return "VLLD Annual Test Needed Alarm";
                    case "15":
                        return "VLLD Periodic Line Test Fail Alarm";
                    case "16":
                        return "VLLD Periodic Line Selftest Fail Alarm";
                    case "17":
                        return "VLLD Periodic Pump Test Fail Alarm";
                    case "18":
                        return "VLLD Periodic Pump Selftest Fail Alarm";
                    case "19":
                        return "VLLD Annual Line Test Fail Alarm";
                    case "20":
                        return "VLLD Annual Line Selftest Fail Alarm";
                    case "21":
                        return "VLLD Annual Pump Test Fail Alarm";
                    case "22":
                        return "VLLD Annual Pump Selftest Fail Alarm";
                    case "23":
                        return "VLLD Pressure Warning";
                    case "24":
                        return "VLLD Pressure Alarm";
                    case "25":
                        return "VLLD Gross Test Fault Alarm";
                    case "26":
                        return "VLLD Periodic Test Fault Alarm";
                    case "27":
                        return "VLLD Annual Test Fault Alarm";
                    case "28":
                        return "VLLD Fuel Out Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "14")
            {
                switch (n)
                {
                    case "01":
                        return "Autodial Setup Data Warning";
                    case "02":
                        return "Autodial Failed Alarm";
                    case "03":
                        return "Autodial Service Report Warning";
                    case "04":
                        return "Autodial Alarm Clear Warning";
                    case "05":
                        return "Autodial Delivery Report Warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "18" || a == "19")
            {
                switch (n)
                {
                    case "02":
                        return "DIM Disabled Alarm";
                    case "03":
                        return "DIM Communication Failure Alarm";
                    case "04":
                        return "DIM Transaction Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "20")
            {
                switch (n)
                {
                    case "01":
                        return "BIR Setup Data Warning";
                    case "02":
                        return "BIR Threshold Alarm";
                    case "03":
                        return "BIR Close Shift Warning";
                    case "04":
                        return "BIR Close Daily Warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "21")
            {
                switch (n)
                {
                    case "01":
                        return "PLLD Setup Data Warning";
                    case "02":
                        return "PLLD Gross Test Fail Alarm";
                    case "03":
                        return "PLLD Annual Test Fail Alarm";
                    case "04":
                        return "PLLD Periodic Test Needed Warning";
                    case "05":
                        return "PLLD Periodic Test Needed Alarm";
                    case "06":
                        return "PLLD Sensor Open Alarm";
                    case "07":
                        return "PLLD High Pressure Alarm";
                    case "08":
                        return "PLLD Shutdown Alarm";
                    case "09":
                        return "PLLD High Pressure Warning";
                    case "10":
                        return "PLLD Continuous Handle On Warning";
                    case "11":
                        return "PLLD Periodic Test Fail Alarm";
                    case "12":
                        return "PLLD Annual Test Needed Warning";
                    case "13":
                        return "PLLD Annual Test Needed Alarm";
                    case "14":
                        return "PLLD Low Pressure Alarm";
                    case "15":
                        return "PLLD Sensor Short Alarm";
                    case "16":
                        return "PLLD Continuous Handle On Alarm";
                    case "17":
                        return "PLLD Fuel Out Alarm";
                    case "18":
                        return "PLLD Line Equipment Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "26")
            {
                switch (n)
                {
                    case "01":
                        return "WPLLD Setup Data Warning";
                    case "02":
                        return "WPLLD Gross Test Fail Alarm";
                    case "03":
                        return "WPLLD Periodic Test Fail Alarm";
                    case "04":
                        return "WPLLD Periodic Test Needed Warning";
                    case "05":
                        return "WPLLD Periodic Test Needed Alarm";
                    case "06":
                        return "WPLLD Sensor Open Alarm";
                    case "07":
                        return "WPLLD Communications Alarm";
                    case "08":
                        return "WPLLD Shutdown Alarm";
                    case "09":
                        return "WPLLD Continuous Handle On Warning";
                    case "10":
                        return "WPLLD Annual Test Fail Alarm";
                    case "11":
                        return "WPLLD Annual Test Needed Warning";
                    case "12":
                        return "WPLLD Annual Test Needed Alarm";
                    case "13":
                        return "WPLLD High Pressure Warning";
                    case "14":
                        return "WPLLD High Pressure Alarm";
                    case "15":
                        return "WPLLD Sensor Short Alarm";
                    case "16":
                        return "WPLLD Continuous Handle On Alarm";
                    case "17":
                        return "WPLLD Fuel Out Alarm";
                    case "18":
                        return "WPLLD Line Equipment Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "28")
            {
                switch (n)
                {
                    case "01":
                        return "Smart Sensor Setup Data Warning";
                    case "02":
                        return "Smart Sensor Communication Alarm";
                    case "03":
                        return "Smart Sensor Fault Alarm";
                    case "04":
                        return "Smart Sensor Fuel Warning";
                    case "05":
                        return "Smart Sensor Fuel Alarm";
                    case "06":
                        return "Smart Sensor Water Warning";
                    case "07":
                        return "Smart Sensor Water Alarm";
                    case "08":
                        return "Smart Sensor High Liquid Warning";
                    case "09":
                        return "Smart Sensor High Liquid Alarm";
                    case "10":
                        return "Smart Sensor Low Liquid Warning";
                    case "11":
                        return "Smart Sensor Low Liquid Alarm";
                    case "12":
                        return "Smart Sensor Temperature Warning";
                    case "13":
                        return "Smart Sensor Relay Active";
                    case "14":
                        return "Smart Sensor Install Alarm";
                    case "15":
                        return "Smart Sensor Sensor Fault Warning";
                    case "16":
                        return "Smart Sensor Vacuum Warning";
                    case "17":
                        return "Smart Sensor No Vacuum Warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "29")
            {
                switch (n)
                {
                    case "01":
                        return "Improper Setup alarm";
                    case "02":
                        return "Communication Loss alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "30")
            {
                switch (n)
                {
                    case "01":
                        return "Stage 1 Transfer Monitoring Failure warning";
                    case "02":
                        return "Containment Monitoring Gross Failure warning";
                    case "03":
                        return "Containment Monitoring Gross Failure alarm";
                    case "04":
                        return "Containment Monitoring Degradation Failure warning";
                    case "05":
                        return "Containment Monitoring Degradation Failure alarm";
                    case "06":
                        return "Containment Monitoring CVLD Failure warning";
                    case "07":
                        return "Containment Monitoring CVLD Failure alarm";
                    case "08":
                        return "Vapor Processor Over Pressure Failure warning";
                    case "09":
                        return "Vapor Processor Over Pressure Failure alarm";
                    case "10":
                        return "Vapor Processor Status Test warning";
                    case "11":
                        return "Vapor Processor Status Test alarm";
                    case "12":
                        return "Missing Relay Setup alarm";
                    case "13":
                        return "Missing Hose Setup alarm";
                    case "14":
                        return "Missing Tank Setup alarm";
                    case "15":
                        return "Missing Vapor Flow Meter alarm";
                    case "16":
                        return "Missing Vapor Pressure Sensor alarm";
                    case "17":
                        return "Missing Vapor Pressure Input alarm";
                    case "18":
                        return "Setup Fail warning";
                    case "19":
                        return "Setup Fail alarm";
                    case "20":
                        return "Sensor Out warning";
                    case "21":
                        return "Sensor Out alarm";
                    case "22":
                        return "PC-ISD Offline";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "31")
            {
                switch (n)
                {
                    case "01":
                        return "Collection Monitoring Gross Failure warning";
                    case "02":
                        return "Collection Monitoring Gross Failure alarm";
                    case "03":
                        return "Collection Monitoring Degradation Failure warning";
                    case "04":
                        return "Collection Monitoring Degradation Failure alarm";
                    case "05":
                        return "Flow Performance Hose Blockage Failure warning";
                    case "06":
                        return "Flow Performance Hose Blockage Failure alarm";
                    case "07":
                        return "Vapor Flow Meter Setup alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "32")
            {
                switch (n)
                {
                    case "01":
                        return "Locked rotor alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "33")
            {
                switch (n)
                {
                    case "01":
                        return "Vapor Processor Run Time Fault warning";
                    case "02":
                        return "Processor Monitoring Effluent Emissions Failure warning";
                    case "03":
                        return "Processor Monitoring Effluent Emissions Failure alarm";
                    case "04":
                        return "Processor Monitoring Over Pressure Failure warning";
                    case "05":
                        return "Processor Monitoring Over Pressure Failure alarm";
                    case "06":
                        return "Processor Monitoring Duty Cycle Failure warning";
                    case "07":
                        return "Processor Monitoring Duty Cycle Failure alarm";
                    case "08":
                        return "PMC (stand alone mode only) Setup warning";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "34")
            {
                switch (n)
                {
                    case "01":
                        return "Setup Data Warning";
                    case "02":
                        return "Pump Relay Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "35")
            {
                switch (n)
                {
                    case "01":
                        return "Setup Data Warning";
                    case "02":
                        return "Disabled VMCI Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "36")
            {
                switch (n)
                {
                    case "01":
                        return "VMC Comm timeout";
                    case "02":
                        return "Meter Not Connected";
                    case "03":
                        return "FP Shutdown Warning";
                    case "04":
                        return "FP Shutdown Alarm";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else if (a == "99")
            {
                switch (n)
                {
                    case "01":
                        return "Externally Dectected Communication Alarm";
                    case "02":
                        return "Communications - Data Reception Timeout";
                    case "03":
                        return "Communications - Failed Checksum";
                    case "04":
                        return "Communications - Parity Error";
                    case "05":
                        return "Modem - Line Busy";
                    case "06":
                        return "Modem - No Answer";
                    case "07":
                        return "Modem - No Carrier";
                    case "08":
                        return "Modem - No Dial Tone";
                    case "09":
                        return "Modem - Modem Error";
                    case "10":
                        return "Modem - Modem Not Responding";
                    case "11":
                        return "Modem - Port Not Available";
                    case "12":
                        return "Polling - Could Not Update Queue";
                    case "13":
                        return "Polling - Invalid Data Type Requested";
                    default:
                        return "Response 'N' not match in case.";
                }
            }
            else return "";
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
        public event EventHandler CommandManageResponse;
        protected internal void OnCommandResponse(Tank tank)
        {
            if (CommandManageResponse != null)
                CommandManageResponse(tank, EventArgs.Empty);
        }
        public event EventHandler LogFileResponse;
        protected internal void OnLogFileResponse(Tank tank, string message, string type)
        {
            this.gdMessage = message;
            this.gdType = type;
            if (LogFileResponse != null)
                LogFileResponse(tank, EventArgs.Empty);
        }
    }
}
