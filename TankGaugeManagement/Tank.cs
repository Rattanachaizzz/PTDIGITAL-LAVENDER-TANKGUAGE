using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace TankGaugeManagement
{
    public class Tank
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public int EvenId { get; set; }
        public DateTime DateStamp { get; set; }
        public double GaugeLevel { get; set; }
        public double GaugeVolume { get; set; }
        public double GaugeTcVolume { get; set; }
        public double Temperature { get; set; }
        public double Height { get; set; }
        public double WaterLevel { get; set; }
        public double WaterVolume { get; set; }
        public double Ullage { get; set; }
        public int ProductCode { get; set; }

        public DateTime DateStampDeliver { get; set; }
        public List<DateTime> StartDate { get; set; } = new List<DateTime>();
        public List<double> StartGaugeVolume { get; set; } = new List<double>();
        public List<double> StartGaugeTcVolume { get; set; } = new List<double>();
        public List<double> StartWaterVolume { get; set; } = new List<double>();
        public List<double> StartTemperature { get; set; } = new List<double>();
        public List<double> StartHeight { get; set; } = new List<double>();
        public List<double> EndGaugeVolume { get; set; } = new List<double>();
        public List<double> EndGaugeTcVolume { get; set; } = new List<double>();
        public List<double> EndWaterVolume { get; set; } = new List<double>();
        public List<double> EndTemperature { get; set; } = new List<double>();
        public List<double> EndHeight { get; set; } = new List<double>();
        public List<DateTime> EndDate { get; set; } = new List<DateTime>();

        public DateTime C_DateStamp { get; set; }
        public int C_ProductCode { get; set; }
        public List<DateTime> C_StartDate { get; set; } = new List<DateTime>();
        public List<double> C_StartGaugeVolume { get; set; } = new List<double>();
        public List<double> C_StartGaugeTcVolume { get; set; } = new List<double>();
        public List<double> C_StartWaterVolume { get; set; } = new List<double>();
        public List<double> C_StartTemperature { get; set; } = new List<double>();
        public List<double> C_StartHeight { get; set; } = new List<double>();
        public List<double> C_EndGaugeVolume { get; set; } = new List<double>();
        public List<double> C_EndGaugeTcVolume { get; set; } = new List<double>();
        public List<double> C_EndWaterVolume { get; set; } = new List<double>();
        public List<double> C_EndTemperature { get; set; } = new List<double>();
        public List<double> C_EndHeight { get; set; } = new List<double>();
        public List<DateTime> C_EndDate { get; set; } = new List<DateTime>();
        public DateTime B_DateStamp { get; set; }
        public List<DateTime> B_StartDate { get; set; } = new List<DateTime>();
        public List<double> B_StartVolume { get; set; } = new List<double>();
        public List<double> B_AdjustDeliveryVolume { get; set; } = new List<double>();
        public List<double> B_AdjustTempVolume { get; set; } = new List<double>();
        public List<double> B_StartFuelHeight { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp1 { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp2 { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp3 { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp4 { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp5 { get; set; } = new List<double>();
        public List<double> B_StartFuelTemp6 { get; set; } = new List<double>();
        public List<double> B_StartFuelTempAvg { get; set; } = new List<double>();
        public List<double> B_EndVolume { get; set; } = new List<double>();
        public List<double> B_EndFuelHeight { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp1 { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp2 { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp3 { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp4 { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp5 { get; set; } = new List<double>();
        public List<double> B_EndFuelTemp6 { get; set; } = new List<double>();
        public List<double> B_EndFuelTempAvg { get; set; } = new List<double>();
        public List<double> B_TotalDuringDelivery { get; set; } = new List<double>();
        public List<DateTime> B_EndDate { get; set; } = new List<DateTime>();
        public string TankAlarmCategory { get; set; }
        public string TankAlarmType { get; set; }
        public string TankAlarmDescription { get; set; }
        public int TankProbeStatus { get; set; }
        public DateTime TankReadingDt { get; set; }
        public int LoopId { get; set; }
        public int LoopType { get; set; }
        public string LoopName { get; set; }
        public SerialPort SerialPort { get; set; }
        public int ProtocolId { get; set; }
        public string ProtocolName { get; set; }
        public int CommandNumber { get; set; }
        public List<Hose> Hoses { get; set; }
        public int CountDisconn { get; set; } = 0;
        public int ReadDataTimeout { get; set; } = 5000;
        public int TimeDelay { get; set; } = 1000;
    }
    public class Hose
    {
        public int HoseId { get; set; }
        public int PumpId { get; set; }
        public int LogicalAddress { get; set; }
        public int? HoseNumber { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public double TotalValue { get; set; }
        public double TotalVolume { get; set; }
        public double TransactionVolume { get; set; }
        public string PendingCommand { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
