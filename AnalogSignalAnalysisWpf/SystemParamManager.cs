using AnalogSignalAnalysisWpf.Hardware.Scope;
using Framework.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    #region 测量参数

    /// <summary>
    /// 频率测量参数
    /// </summary>
    public class FrequencyMeasureParams
    {
        /// <summary>
        /// 最小气压(MPa)
        /// </summary>
        public double MinPressure { get; set; } = 1.5;

        /// <summary>
        /// 最小气压(MPa)
        /// </summary>
        public double MaxPressure { get; set; } = 8.0;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit { get; set; } = 0.2;

        ///// <summary>
        ///// 通信延迟(MS)
        ///// </summary>
        //public int ComDelay { get; set; } = 200;

        /// <summary>
        /// 输出电压(吸合电压)
        /// </summary>
        public double OutputVoltage { get; set; } = 24;

        /// <summary>
        /// 占空比
        /// </summary>
        public int DutyRatio { get; set; } = 50;

        /// <summary>
        /// 电压滤波系数
        /// </summary>
        public int VoltageFilterCount { get; set; } = 11;

        ///// <summary>
        ///// CHA探头衰变
        ///// </summary>
        //public EScale CHAScale { get; set; } = EScale.x10;

        ///// <summary>
        ///// CHA电压档位
        ///// </summary>
        //public EVoltageDIV CHAVoltageDIV { get; set; } = EVoltageDIV.DIV_2V5;

        ///// <summary>
        ///// 采样率
        ///// </summary>
        //public ESampleRate SampleRate { get; set; } = ESampleRate.Sps_49K;

        /// <summary>
        /// 测试数据
        /// </summary>
        public ObservableCollection<FrequencyTestData> TestDatas { get; set; }

    }

    /// <summary>
    /// 输入输出测量参数
    /// </summary>
    public class InputOutputMeasureParams
    {
        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval { get; set; } = 0.25;

        /// <summary>
        /// 最小电压(V)
        /// </summary>
        public double MinVoltage { get; set; } = 1.0;

        /// <summary>
        /// 最大电压(V)
        /// </summary>
        public double MaxVoltage { get; set; } = 15.0;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 200;

        ///// <summary>
        ///// 通信延迟(MS)
        ///// </summary>
        //public int ComDelay { get; set; } = 200;

        ///// <summary>
        ///// CHA探头衰变
        ///// </summary>
        //public EScale CHAScale { get; set; } = EScale.x10;

        ///// <summary>
        ///// CHA电压档位
        ///// </summary>
        //public EVoltageDIV CHAVoltageDIV { get; set; } = EVoltageDIV.DIV_2V5;

        ///// <summary>
        ///// 采样率
        ///// </summary>
        //public ESampleRate SampleRate { get; set; } = ESampleRate.Sps_49K;

    }

    /// <summary>
    /// 吸合释放电压测量参数
    /// </summary>
    public class PNVoltageMeasureParams
    {
        ///// <summary>
        ///// 气压系数(K=P/V)
        ///// </summary>
        //public double PressureK { get; set; } = 1;

        /// <summary>
        /// 临界气压
        /// </summary>
        public double CriticalPressure { get; set; } = 3;

        ///// <summary>
        ///// 最小电压阈值(单位:V)
        ///// </summary>
        //public double MinVoltageThreshold { get; set; } = 1.5;

        ///// <summary>
        ///// 最大电压阈值(单位:V)
        ///// </summary>
        //public double MaxVoltageThreshold { get; set; } = 8.0;

        ///// <summary>
        ///// 频率误差
        ///// </summary>
        //public double FrequencyErrLimit { get; set; } = 0.2;

        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval { get; set; } = 0.25;

        /// <summary>
        /// 最小电压(V)
        /// </summary>
        public double MinVoltage { get; set; } = 1.0;

        /// <summary>
        /// 最大电压(V)
        /// </summary>
        public double MaxVoltage { get; set; } = 15.0;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 500;

        ///// <summary>
        ///// 通信延迟(MS)
        ///// </summary>
        //public int ComDelay { get; set; } = 200;

        /// <summary>
        /// CHA探头衰变
        /// </summary>
        //public EScale CHAScale { get; set; } = EScale.x10;

        ///// <summary>
        ///// CHA电压档位
        ///// </summary>
        //public EVoltageDIV CHAVoltageDIV { get; set; } = EVoltageDIV.DIV_2V5;

        ///// <summary>
        ///// 采样率
        ///// </summary>
        //public ESampleRate SampleRate { get; set; } = ESampleRate.Sps_49K;

    }

    /// <summary>
    /// 通气量测量参数
    /// </summary>
    public class ThroughputMeasureParams
    {
        /// <summary>
        /// 测量方式
        /// </summary>
        public int MeasureType { get; set; } = 1;

        #region 阈值方式

        /// <summary>
        /// 气压系数(K=P/V)
        /// </summary>
        public double PressureK { get; set; } = 1;

        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

        #endregion

        #region 微分方式

        /// <summary>
        /// 死区(0-30)
        /// </summary>
        public int DeadZone { get; set; } = 15;

        /// <summary>
        /// 临界值
        /// </summary>
        public double CriticalValue { get; set; } = 0.8;

        #endregion

        /// <summary>
        /// 输出电压(V)
        /// </summary>
        public double OutputVoltage { get; set; } = 12.0;

        /// <summary>
        /// 输出延时(MS)
        /// </summary>
        public int OutputDelay { get; set; } = 300;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 1000;

        /// <summary>
        /// CHA探头衰变
        /// </summary>
        //public EScale CHAScale { get; set; } = EScale.x10;

        ///// <summary>
        ///// CHA电压档位
        ///// </summary>
        //public EVoltageDIV CHAVoltageDIV { get; set; } = EVoltageDIV.DIV_2V5;

        ///// <summary>
        ///// 采样率
        ///// </summary>
        //public ESampleRate SampleRate { get; set; } = ESampleRate.Sps_49K;

        /// <summary>
        /// 电压滤波系数
        /// </summary>
        public int VoltageFilterCount { get; set; } = 7;

        /// <summary>
        /// 微分系数
        /// </summary>
        public int DerivativeK { get; set; } = 500;

        /// <summary>
        /// 使能微分滤波
        /// </summary>
        public bool IsEnableDerivativeFilter { get; set; } = false;

        /// <summary>
        /// 微分滤波系数
        /// </summary>
        public int DerivativeFilterCount { get; set; } = 7;

    }

    /// <summary>
    /// 老化测试参数
    /// </summary>
    public class BurnInTestParams
    {
        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// PWM次数
        /// </summary>
        public int PWMCount { get; set; }

    }

    #endregion

    #region 硬件参数

    /// <summary>
    /// 示波器参数
    /// </summary>
    public class ScopeParams
    {
        /// <summary>
        /// CHA档位
        /// </summary>
        public EScale CHAScale { get; set; }

        /// <summary>
        /// CHB档位
        /// </summary>
        public EScale CHBScale { get; set; }

        /// <summary>
        /// CHA耦合
        /// </summary>
        public ECoupling CHACoupling { get; set; }

        /// <summary>
        /// CHB耦合
        /// </summary>
        public ECoupling CHBCoupling { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 触发模式
        /// </summary>
        public ETriggerModel TriggerModel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerEdge TriggerEdge { get; set; }

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public EVoltageDIV CHAVoltageDIV { get; set; }

        /// <summary>
        /// CHB电压档位
        /// </summary>
        public EVoltageDIV CHBVoltageDIV { get; set; }

        /// <summary>
        /// 使能CHA通道
        /// </summary>
        public bool IsCHAEnable { get; set; }

        /// <summary>
        /// 使能CHB通道
        /// </summary>
        public bool IsCHBEnable { get; set; }

        /// <summary>
        /// 采样时间
        /// </summary>
        public int SampleTime { get; set; }

    }

    public class PWMParams
    {
        /// <summary>
        /// 串口
        /// </summary>
        public string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 占空比(1-100)
        /// </summary>
        public int DutyRatio { get; set; }
    }

    /// <summary>
    /// PWM参数
    /// </summary>
    public class PowerParams
    {
        #region Modbus配置参数

        /// <summary>
        /// 串口号
        /// </summary>
        public string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int SerialPortBaudRate { get; set; }

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        /// 写超时
        /// </summary>
        public int WriteTimeout { get; set; }

        /// <summary>
        /// 读超时
        /// </summary>
        public int ReadTimeout { get; set; }

        #endregion

        #region 控制接口

        /// <summary>
        /// 电压值
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 电流值
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// 使能输出
        /// </summary>
        public bool EnableOutput { get; set; }

        #endregion

    }

    #endregion

    /// <summary>
    /// 全局参数
    /// </summary>
    public class GlobalParam
    {
        /// <summary>
        /// 气压系数(P/V)
        /// </summary>
        public double PressureK { get; set; } = 1.0;

        /// <summary>
        /// 电源模块通信延迟(MS)
        /// </summary>
        public int PowerCommonDelay { get; set; } = 200;

        /// <summary>
        /// 衰减档位
        /// </summary>
        public EScale Scale { get; set; } = EScale.x10;

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public EVoltageDIV VoltageDIV { get; set; } = EVoltageDIV.DIV_2V5;

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate { get; set; } = ESampleRate.Sps_49K;

    }

    public class SystemParam
    {
        #region 硬件参数

        /// <summary>
        /// 示波器参数
        /// </summary>
        public ScopeParams ScopeParams { get; set; } = new ScopeParams();

        /// <summary>
        /// PWM参数
        /// </summary>
        public PWMParams PWMParams { get; set; } = new PWMParams();

        /// <summary>
        /// Power参数
        /// </summary>
        public PowerParams PowerParams { get; set; } = new PowerParams();

        #endregion

        #region 测量参数

        /// <summary>
        /// 频率测量参数
        /// </summary>
        public FrequencyMeasureParams FrequencyMeasureParams { get; set; } = new FrequencyMeasureParams();

        /// <summary>
        /// 输入输出测量参数
        /// </summary>
        public InputOutputMeasureParams InputOutputMeasureParams { get; set; } = new InputOutputMeasureParams();

        /// <summary>
        /// 吸合释放电压测量参数
        /// </summary>
        public PNVoltageMeasureParams PNVoltageMeasureParams { get; set; } = new PNVoltageMeasureParams();

        /// <summary>
        /// 通气量测量参数
        /// </summary>
        public ThroughputMeasureParams ThroughputMeasureParams { get; set; } = new ThroughputMeasureParams();

        /// <summary>
        /// 老化测试参数
        /// </summary>
        public BurnInTestParams BurnInTestParams { get; set; } = new BurnInTestParams();

        #endregion

        #region 全局参数

        public GlobalParam GlobalParam { get; set; } = new GlobalParam();

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class User
    {
        public string UserName { get; set; } = "Admin";

        public string Password { get; set; } = "123";

    }

    public class SystemParamManager
    {
        #region 单例模式

        /// <summary>
        /// 私有实例接口
        /// </summary>
        private static readonly SystemParamManager Instance = new SystemParamManager();

        /// <summary>
        /// 创建私有SceneManager新实例,保证外界无法通过new来创建新实例
        /// </summary>
        private SystemParamManager()
        {

        }

        /// <summary>
        /// 获取实例接口
        /// </summary>
        /// <returns></returns>
        public static SystemParamManager GetInstance()
        {

            return Instance;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 参数路径
        /// </summary>
        public readonly string ParamPath = "SystemParams/SystemConfig.json";

        /// <summary>
        /// 系统参数
        /// </summary>
        public SystemParam SystemParam { get; set; } = new SystemParam();

        #endregion

        #region 公共方法

        /// <summary>
        /// 加载参数
        /// </summary>
        /// <returns>执行结果</returns>
        public bool LoadParams()
        {
            return LoadParams(ParamPath);
        }

        /// <summary>
        /// 加载参数
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>执行结果</returns>
        public bool LoadParams(string file)
        {
            bool result = true;
            if (File.Exists(file))
            {
                SystemParam = JsonSerialization.DeserializeObjectFromFile<SystemParam>(file);
            }

            if (SystemParam == null)
            {
                SystemParam = new SystemParam();
                result = false;
            }

            //保存参数到默认配置文件
            JsonSerialization.SerializeObjectToFile(SystemParam, ParamPath);

            return result;
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        public void SaveParams()
        {
            SaveParams(ParamPath);
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        /// <param name="file">文件路径</param>
        public void SaveParams(string file)
        {
            JsonSerialization.SerializeObjectToFile(SystemParam, file);
        }

        #endregion

        #region 账户密码

        public User User { get; private set; }

        /// <summary>
        /// 加载用户名
        /// </summary>
        /// <returns></returns>
        public bool LoadUser()
        {
            var dataPath = System.Environment.GetEnvironmentVariable("appdata");
            var file = $"{dataPath}/.aa";

            bool result = true;
            if (File.Exists(file))
            {
                User = JsonSerialization.DeserializeObjectFromFile<User>(file);
            }

            if (User == null)
            {
                User = new User();
                result = false;
            }

            //保存参数到默认配置文件
            JsonSerialization.SerializeObjectToFile(User, file);

            return result;
        }

        /// <summary>
        /// 保存用户名
        /// </summary>
        public void SaveUser()
        {
            var dataPath = System.Environment.GetEnvironmentVariable("appdata");
            var file = $"{dataPath}/.aa";

            //保存参数到默认配置文件
            JsonSerialization.SerializeObjectToFile(User, file);
        }

        #endregion
    }
}
