namespace AnalogSignalAnalysisWpf.Hardware
{
    public interface IPLC : IModbusCommunication
    {
        #region 控制接口

        /// <summary>
        /// 开关
        /// </summary>
        bool Switch { get; set; }

        #endregion

    }
}
