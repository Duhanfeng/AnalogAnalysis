using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private int frequency;

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency
        {
            get
            {
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

                frequency = value;

                using (SerialPort port = new SerialPort(PrimarySerialPortName))
                {
                    //配置串口
                    port.BaudRate = 9600;
                    port.DataBits = 8;
                    port.Parity = Parity.None;
                    port.StopBits = StopBits.One;
                    port.Open();

                    //创建Modbus主机
                    var adapter = new SerialPortAdapter(port);
                    adapter.WriteTimeout = 500;
                    adapter.ReadTimeout = 500;

                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(configData);
                    adapter.Write(byteArray, 0, byteArray.Length);
                }

            }
        }

        private double dutyRatio;

        /// <summary>
        /// 占空比(0.01-1)
        /// </summary>
        public double DutyRatio
        {
            get 
            { 
                return dutyRatio; 
            }
            set 
            {
                if (string.IsNullOrEmpty(PrimarySerialPortName))
                {
                    return;
                }

                value = (value > 1) ? 1 : value;
                value = (value < 0.01) ? 0.01 : value;

                string configData = $"D{(int)(value * 100):D3}";

                dutyRatio = value;
                using (SerialPort port = new SerialPort(PrimarySerialPortName))
                {
                    //配置串口
                    port.BaudRate = 9600;
                    port.DataBits = 8;
                    port.Parity = Parity.None;
                    port.StopBits = StopBits.One;
                    port.Open();

                    //创建Modbus主机
                    var adapter = new SerialPortAdapter(port);
                    adapter.WriteTimeout = 500;
                    adapter.ReadTimeout = 500;

                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(configData);
                    adapter.Write(byteArray, 0, byteArray.Length);
                }

            }
        }

    }
}
