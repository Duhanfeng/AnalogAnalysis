using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    public interface IPLCCommunication: IDisposable
    {
        #region Modbus配置参数

        /// <summary>
        /// 串口号
        /// </summary>
        string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 串口波特率
        /// </summary>
        int SerialPortBaudRate { get; set; }

        /// <summary>
        /// 从站地址
        /// </summary>
        byte SlaveAddress { get; set; }

        /// <summary>
        /// 写超时
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// 读超时
        /// </summary>
        int ReadTimeout { get; set; }

        #endregion

        #region Modbus控制接口

        /// <summary>
        /// 设备连接标志
        /// </summary>
        bool IsConnect { get; set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="devIndex"></param>
        /// <returns></returns>
        bool Connect(int devIndex);

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
        bool Write(ushort register, ushort value);

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        bool Write(ushort register, ushort[] values);

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        bool Read(ushort register, out ushort value);

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        bool Read(ushort register, ushort count, out ushort[] values);

        #endregion

    }
}
