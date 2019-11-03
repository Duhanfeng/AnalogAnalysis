using System.ComponentModel;

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


        /// <summary>
        /// 开始记录数据
        /// </summary>
        void StartSample();

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        void ReadData(int channelIndex, out double[] channelData);

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        void ReadData(out double[] channelData1, out double[] channelData2);

        #region 属性

        /// <summary>
        /// 使能CHA通道
        /// </summary>
        bool IsCHAEnable { get; set; }

        /// <summary>
        /// 使能CHB通道
        /// </summary>
        bool IsCHBEnable { get; set; }

        /// <summary>
        /// CHA电压档位
        /// </summary>
        EVoltageDIV CHAVoltageDIV { get; set; }

        /// <summary>
        /// CHB电压档位
        /// </summary>
        EVoltageDIV CHBVoltageDIV { get; set; }

        /// <summary>
        /// CHA档位
        /// </summary>
        EScale CHAScale { get; set; }

        /// <summary>
        /// CHB档位
        /// </summary>
        EScale CHBScale { get; set; }

        /// <summary>
        /// CHA耦合
        /// </summary>
        ECoupling CHACoupling { get; set; }

        /// <summary>
        /// CHB耦合
        /// </summary>
        ECoupling CHBCoupling { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 触发模式
        /// </summary>
        ETriggerModel TriggerModel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        ETriggerEdge TriggerEdge { get; set; }

        #endregion

    }

    public enum EChannel
    {
        [Description("CHA")]
        CHA,
        [Description("CHB")]
        CHB
    }

    public enum EScale
    {
        [Description("x1")]
        x1 = 1,
        [Description("x10")]
        x10 = 10,
        //[Description("x100")]
        //x100 = 100
    }

    public enum ECoupling
    {
        [Description("直流耦合")]
        DC,
        [Description("交流耦合")]
        AC
    }

    /// <summary>
    /// 电压档位
    /// </summary>
    public enum EVoltageDIV
    {
        [Description("250mV/DIV")]
        DIV_250MV,
        [Description("500mV/DIV")]
        DIV_500MV,
        [Description("1V/DIV")]
        DIV_1V,
        [Description("2.5V/DIV")]
        DIV_2V5,
        [Description("5V/DIV")]
        DIV_5V,
    }

    public enum ESampleRate
    {
        [Description("49K")]
        Sps_49K = 49 * 1000,
        [Description("96K(Stream)")]
        Sps_96K = 96 * 1000,
        [Description("781K")]
        Sps_781K = 781 * 1000,
        [Description("12.5M")]
        Sps_12M5 = 125 * 100 * 1000,
        [Description("100M")]
        Sps_100M = 100 * 1000 * 1000,
    }

    public enum ETriggerModel
    {
        [Description("CHA")]
        CHA,
        [Description("外触发")]
        Ext,
        [Description("无")]
        No,
    }

    public enum ETriggerEdge
    {
        [Description("上升沿")]
        Rising,
        [Description("下降沿")]
        Filling,
    }

}
