using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace AnalogSignalAnalysisWpf.Hardware
{
    public class SerialPortPWM : IPWM
    {
        /// <summary>
        /// 创建SerialPortPWM新实例
        /// </summary>
        public SerialPortPWM()
        {

        }

        /// <summary>
        /// 创建SerialPortPWM新实例
        /// </summary>
        /// <param name="portName">串口号</param>
        public SerialPortPWM(string portName)
        {
            PrimarySerialPortName = portName;
        }

        /// <summary>
        /// 串口号
        /// </summary>
        public string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; private set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>执行结果</returns>
        public bool Connect()
        {
            int frequency;
            int dutyRatio;

            if (ReadFrequencyAndDutyRatio(out frequency, out dutyRatio))
            {
                IsConnect = true;
                Frequency = 0;
                DutyRatio = 50;
            }
            else
            {
                IsConnect = false;
            }

            return IsConnect;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;

        }

        private int frequency = -1;

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency
        {
            get
            {
                return IsConnect ? frequency : -1;
            }
            set
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return;
                }

                if (IsConnect)
                {
                    string configData = "";

                    if (value < 1000)
                    {
                        configData = $"F{value:D3}";
                    }
                    else if (value < 10 * 1000)
                    {
                        double tempValue = value / 1000.0;
                        configData = $"F{tempValue:0.00}";
                    }
                    else if (value < 100 * 1000)
                    {
                        double tempValue = value / 1000.0;
                        configData = $"F{tempValue:00.0}";
                    }
                    else
                    {
                        return;
                    }

                    using (SerialPort port = new SerialPort(PrimarySerialPortName))
                    {
                        //配置串口
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;
                        port.Open();

                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(configData);
                        port.Write(byteArray, 0, byteArray.Length);
                        frequency = value;
                    }

                }

            }
        }

        private int dutyRatio = -1;

        public int DutyRatio
        {
            get
            {
                return IsConnect ? dutyRatio : -1;
            }
            set
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return;
                }

                if (IsConnect)
                {
                    value = (value > 100) ? 100 : value;
                    value = (value < 1) ? 1 : value;

                    string configData = $"D{(int)value:D3}";

                    using (SerialPort port = new SerialPort(PrimarySerialPortName))
                    {
                        //配置串口
                        port.BaudRate = 9600;
                        port.DataBits = 8;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;
                        port.Open();

                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(configData);
                        port.Write(byteArray, 0, byteArray.Length);
                        dutyRatio = value;
                    }
                }

            }
        }

        /// <summary>
        /// 读取频率和占空比
        /// </summary>
        /// <returns></returns>
        private bool ReadFrequencyAndDutyRatio(out int frequency, out int dutyRatio)
        {
            frequency = -1;
            dutyRatio = -1;

            if (string.IsNullOrEmpty(PrimarySerialPortName))
            {
                return false;
            }

            try
            {
                using (SerialPort port = new SerialPort(PrimarySerialPortName))
                {
                    //配置串口
                    port.BaudRate = 9600;
                    port.DataBits = 8;
                    port.Parity = Parity.None;
                    port.StopBits = StopBits.One;
                    port.Open();

                    //读取数据
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes("read");
                    port.Write(byteArray, 0, byteArray.Length);
                    Thread.Sleep(100);
                    var recvCmd = port.ReadExisting();

                    if (string.IsNullOrEmpty(recvCmd))
                    {
                        return false;
                    }

                    //解析数据
                    var r1 = recvCmd.Split('\n').ToList();
                    var r2 = (from val in r1
                              where val.Contains("F")
                              select val).ToList();
                    if (r2?.Count == 1)
                    {
                        var r3 = r2[0].TrimStart('F');

                        if (r3.Contains("KHz"))
                        {
                            frequency = (int)(double.Parse(r3.Replace("KHz", "")) * 1000);
                        }
                        else if (r3.Contains("Hz"))
                        {
                            frequency = (int)(double.Parse(r3.Replace("Hz", "")));
                        }
                    }

                    var r4 = (from val in r1
                              where val.Contains("D")
                              select val).ToList();
                    if (r4?.Count == 1)
                    {
                        var r5 = r4[0].TrimStart('D');
                        dutyRatio = int.Parse(r5);
                    }
                }

                if ((frequency != -1) && (dutyRatio != -1))
                {
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }
    }
}
