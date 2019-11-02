using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware
{
    public interface IPLC : IModbusCommunication
    {
        #region 控制接口

        /// <summary>
        /// 使能输出脉冲
        /// </summary>
        bool IsEnablePulse { get; set; }

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// 占空比(1-100)
        /// </summary>
        int DutyRatio { get; set; }

        /// <summary>
        /// 设置输出状态
        /// </summary>
        /// <param name="number">输出引脚编号</param>
        /// <param name="isEnable">输出状态</param>
        void SetOutput(int number, bool isEnable);

        /// <summary>
        /// 获取输出状态
        /// </summary>
        /// <param name="number">输出引脚编号</param>
        /// <returns>输出状态</returns>
        bool GetOutput(int number);

        #endregion

    }
}
