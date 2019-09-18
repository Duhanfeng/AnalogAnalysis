using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf.Hantek66022BE
{
    /// <summary>
    /// 电压档位
    /// </summary>
    public enum EVoltageDIV : int
    {
        [Description("20mV/DIV")]
        DIV_20MV = 0,
        [Description("50mV/DIV")]
        DIV_50MV,
        [Description("100mV/DIV")]
        DIV_100MV,
        [Description("200mV/DIV")]
        DIV_200MV,
        [Description("500mV/DIV")]
        DIV_500MV,
        [Description("1V/DIV")]
        DIV_1V,
        [Description("2V/DIV")]
        DIV_2V,
        [Description("5V/DIV")]
        DIV_5V,
    }

    /// <summary>
    /// 采样档位
    /// </summary>
    public enum ETimeDIV : int
    {
        [Description("48MSa/s")]
        DIV_48MSaS = 0,
        [Description("16MSa/s")]
        DIV_16MSaS = 11,
        [Description("8MSa/s")]
        DIV_8MSaS = 12,
        [Description("4MSa/s")]
        DIV_4MSaS = 13,
        [Description("1MSa/s")]
        DIV_1MSaS = 14,
        [Description("500KSa/s")]
        DIV_500KSaS = 25,
        [Description("200KSa/s")]
        DIV_200KSaS = 26,
        [Description("100KSa/s")]
        DIV_100KSaS = 27,
    }

    /// <summary>
    /// 扫频模式
    /// </summary>
    public enum ETriggerSweep : short
    {
        /// <summary>
        /// 自动模式
        /// </summary>
        [Description("自动模式")]
        Auto = 0,

        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 1,

        /// <summary>
        /// 单次
        /// </summary>
        [Description("单次")]
        Signle = 2,
    }

    /// <summary>
    /// 触发源
    /// </summary>
    public enum ETriggerSource : short
    {
        /// <summary>
        /// 通道1
        /// </summary>
        [Description("通道1")]
        CH1 = 0,

        /// <summary>
        /// 通道2
        /// </summary>
        [Description("通道2")]
        CH2 = 1,
    }

    /// <summary>
    /// 触发沿方式
    /// </summary>
    public enum ETriggerSlope : short
    {
        /// <summary>
        /// 上升沿
        /// </summary>
        [Description("上升沿")]
        Rise = 0,

        /// <summary>
        /// 下降沿
        /// </summary>
        [Description("下降沿")]
        Fall = 1,
    }

    /// <summary>
    /// 差值方式
    /// </summary>
    public enum EInsertMode : short
    {
        /// <summary>
        /// Step差值
        /// </summary>
        [Description("Step差值")]
        Step = 0,

        /// <summary>
        /// Line差值
        /// </summary>
        [Description("Line差值")]
        Line = 1,

        /// <summary>
        /// SinX/X差值
        /// </summary>
        [Description("SinX/X差值")]
        SinX_X = 2,
    }

}
