using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    /// <summary>
    /// 示波器接口
    /// </summary>
    public interface IScope: IDisposable
    {
        /// <summary>
        /// 设备连接标志
        /// </summary>
        bool IsConnect { get; }

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
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        void ReadData(int channelIndex, out double[] channelData);

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        void ReadData(out double[] channelData1, out double[] channelData2);

        #region 设备参数设置

        /// <summary>
        /// 通道1电压档位
        /// </summary>
        EVoltageDIV CH1VoltageDIV { get; set; }

        /// <summary>
        /// 通道2电压档位
        /// </summary>
        EVoltageDIV CH2VoltageDIV { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 扫频模式
        /// </summary>
        ETriggerSweep TriggerSweep { get; set; }

        /// <summary>
        /// 触发源
        /// </summary>
        ETriggerSource TriggerSource { get; set; }

        /// <summary>
        /// 触发电平
        /// </summary>
        int TriggerLevel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        ETriggerSlope TriggerSlope { get; set; }

        /// <summary>
        /// 差值方式
        /// </summary>
        EInsertMode InsertMode { get; set; }

        /// <summary>
        /// 采集时长(MS)
        /// </summary>
        int SampleTime { get; set; }

        #endregion

    }

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
    /// 采样频率
    /// </summary>
    public enum ESampleRate : int
    {
        [Description("48MSa/s")]
        DIV_48MSaS = 48 * 1000 * 1000,
        [Description("16MSa/s")]
        DIV_16MSaS = 16 * 1000 * 1000,
        [Description("8MSa/s")]
        DIV_8MSaS = 8 * 1000 * 1000,
        [Description("4MSa/s")]
        DIV_4MSaS = 4 * 1000 * 1000,
        [Description("1MSa/s")]
        DIV_1MSaS = 1 * 1000 * 1000,
        [Description("500KSa/s")]
        DIV_500KSaS = 500 * 1000,
        [Description("200KSa/s")]
        DIV_200KSaS = 200 * 1000,
        [Description("100KSa/s")]
        DIV_100KSaS = 100 * 1000,
        [Description("0Sa/s")]
        DIV_0Sas = 0

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
        Normal,

        /// <summary>
        /// 单次
        /// </summary>
        [Description("单次")]
        Signle,
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
        CH2,
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
        Fall,
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
        Line,

        /// <summary>
        /// SinX/X差值
        /// </summary>
        [Description("SinX/X差值")]
        SinX_X,
    }



}
