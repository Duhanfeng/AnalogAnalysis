using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware
{
    public interface IPWM
    {
        /// <summary>
        /// 串口号
        /// </summary>
        string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 设备连接标志
        /// </summary>
        bool IsConnect { get; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>执行结果</returns>
        bool Connect();

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// 占空比(1-100)
        /// </summary>
        int DutyRatio { get; set; }
    }
}
