using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    public interface IPLCCommunication: IDisposable
    {
        /// <summary>
        /// 设备连接标志
        /// </summary>
        bool IsConnect { get; set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="baudrate">波特率</param>
        /// <returns></returns>
        bool Connect(int baudrate);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        bool Write(int register, byte value);

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        bool Write(int register, byte[] values);

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        bool Read(int register, out byte value);

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        bool Read(int register, out byte[] values);

    }
}
