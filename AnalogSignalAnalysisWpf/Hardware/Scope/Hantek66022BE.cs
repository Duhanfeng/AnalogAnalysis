using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    public class Hantek66022BE : IScope
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
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        public void ReadData(int channelIndex, out double[] channelData)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadData(out double[] channelData1, out double channelData2)
        {
            throw new InvalidOperationException();
        }

        #region 设备参数设置

        /// <summary>
        /// 通道1电压档位
        /// </summary>
        public EVoltageDIV CH1VoltageDIV { get; set; }

        /// <summary>
        /// 通道2电压档位
        /// </summary>
        public EVoltageDIV CH2VoltageDIV { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 扫频模式
        /// </summary>
        public ETriggerSweep TriggerSweep { get; set; }

        /// <summary>
        /// 触发源
        /// </summary>
        public ETriggerSource TriggerSource { get; set; }

        /// <summary>
        /// 触发电平
        /// </summary>
        public int TriggerLevel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerSlope TriggerSlope { get; set; }

        /// <summary>
        /// 差值方式
        /// </summary>
        public EInsertMode InsertMode { get; set; }

        /// <summary>
        /// 采集时长
        /// </summary>
        public int SampleTime { get; set; }

        #endregion

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
        // ~Hantek66022BE()
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
