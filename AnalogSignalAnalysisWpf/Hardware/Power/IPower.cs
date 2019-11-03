namespace AnalogSignalAnalysisWpf.Hardware
{
    public interface IPower : IModbusCommunication
    {
        #region 控制接口

        /// <summary>
        /// 电压值
        /// </summary>
        double Voltage { get; set; }

        /// <summary>
        /// 电流值
        /// </summary>
        double Current { get; set; }

        /// <summary>
        /// 使能输出
        /// </summary>
        bool IsEnableOutput { get; set; }

        /// <summary>
        /// 开关频率
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// 实际电压值
        /// </summary>
        double RealityVoltage { get; }

        /// <summary>
        /// 实际电流值
        /// </summary>
        double RealityCurrent { get; }

        /// <summary>
        /// 实际温度值
        /// </summary>
        double RealityTemperature { get; }

        #endregion
    }
}
