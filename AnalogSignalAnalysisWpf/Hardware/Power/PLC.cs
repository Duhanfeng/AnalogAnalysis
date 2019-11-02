using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    class PLC : IPLC
    {
        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="devIndex"></param>
        /// <returns></returns>
        public bool Connect(int devIndex)
        {
            IsConnect = true;
            return true;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Write(int register, byte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Write(int register, byte[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Read(int register, out byte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Read(int register, out byte[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 比例系数
        /// </summary>
        public readonly int Scale = 1000;

        /// <summary>
        /// 电压值
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 电流值
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// 开关频率
        /// </summary>
        public int Frequency { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~PLC()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
