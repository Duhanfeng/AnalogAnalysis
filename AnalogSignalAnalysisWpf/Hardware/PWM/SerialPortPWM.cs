using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace AnalogSignalAnalysisWpf.Hardware.PWM
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
            if (DutyRatio >= 0)
            {
                IsConnect = true;
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

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency
        {
            get
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return -1;
                }

                int frequency = -1;

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
                            return -1;
                        }

                        //解析数据
                        var r1 = recvCmd.Split('\n').ToList();
                        var r2 = (from val in r1
                                  where val.Contains("F")
                                  select val).ToList();
                        if (r2?.Count == 1)
                        {
                            var r3 = r2[0].TrimStart('F');

                            switch (r3.Length)
                            {
                                case 3:
                                    frequency = int.Parse(r3);
                                    break;
                                case 4:
                                    frequency = (int)(double.Parse(r3) * 1000);
                                    break;
                                case 5:
                                    frequency = int.Parse(r3.ToCharArray()[0].ToString()) * 100 * 1000 +
                                                int.Parse(r3.ToCharArray()[2].ToString()) * 10 * 1000 +
                                                int.Parse(r3.ToCharArray()[4].ToString()) * 10 * 1000;
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                }
                catch (Exception)
                {
                    frequency = -1;
                }

                return frequency;
            }
            set
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return;
                }

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
                }

            }
        }

        /// <summary>
        /// 占空比(0.01-1)
        /// </summary>
        public double DutyRatio
        {
            get
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return -1;
                }

                int dutyRatio = -1;

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
                            return -1;
                        }

                        //解析数据
                        var r1 = recvCmd.Split('\n').ToList();
                        var r2 = (from val in r1
                                  where val.Contains("D")
                                  select val).ToList();
                        if (r2?.Count == 1)
                        {
                            var r3 = r2[0].TrimStart('D');
                            dutyRatio = int.Parse(r3);
                        }
                    }
                }
                catch (Exception)
                {
                    dutyRatio = -1;
                }


                return dutyRatio;
            }
            set
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return;
                }

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
                }

            }
        }

    }
}
