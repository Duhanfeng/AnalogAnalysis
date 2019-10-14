using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PWM
{
    public interface IPWM
    {
        /// <summary>
        /// 频率(Hz)
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// 占空比
        /// </summary>
        double DutyRatio { get; set; }
    }
}
