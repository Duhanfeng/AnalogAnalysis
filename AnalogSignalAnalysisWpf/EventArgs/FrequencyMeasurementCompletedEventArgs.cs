using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// 频率测量完成事件参数
    /// </summary>
    public class FrequencyMeasurementCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 创建FrequencyMeasurementCompletedEventArgs新实例
        /// </summary>
        /// <param name="isSuccess">成功标志</param>
        /// <param name="maxLimitFrequency">极限频率</param>
        public FrequencyMeasurementCompletedEventArgs(bool isSuccess, int maxLimitFrequency = -1)
        {
            IsSuccess = isSuccess;
            MaxLimitFrequency = maxLimitFrequency;
        }

        /// <summary>
        /// 成功标志
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 极限频率
        /// </summary>
        public int MaxLimitFrequency { get; }

    }
}
