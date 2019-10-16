using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    public interface IScopeBase
    {
        /// <summary>
        /// 采集时长(MS)
        /// </summary>
        int SampleTime { get; set; }

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
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        void ReadDataBlock(int channelIndex, out double[] channelData);

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        void ReadDataBlock(out double[] channelData1, out double[] channelData2);

    }
}
