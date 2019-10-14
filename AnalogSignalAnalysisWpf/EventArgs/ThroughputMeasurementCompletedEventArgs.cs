using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class ThroughputMeasurementCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 创建ThroughputMeasurementCompletedEventArgs新实例
        /// </summary>
        public ThroughputMeasurementCompletedEventArgs()
        {
            IsSuccess = false;
            Time = -1;
        }

        /// <summary>
        /// 创建ThroughputMeasurementCompletedEventArgs新实例
        /// </summary>
        /// <param name="isSuccess">成功标志</param>
        /// <param name="time">测量时间</param>
        public ThroughputMeasurementCompletedEventArgs(bool isSuccess, double time)
        {
            IsSuccess = isSuccess;
            Time = time;
        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 时间结果
        /// </summary>
        public double Time { get; }

    }
}
