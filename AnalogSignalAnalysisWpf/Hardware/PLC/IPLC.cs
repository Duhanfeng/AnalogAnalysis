namespace AnalogSignalAnalysisWpf.Hardware
{
    public interface IPLC : IModbusCommunication
    {
        #region 控制接口

        /// <summary>
        /// 开关
        /// </summary>
        bool PWMSwitch { get; set; }

        /// <summary>
        /// 流量开关
        /// </summary>
        bool FlowSwitch { get; set; }

        #endregion

    }
}
