using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Framework.Infrastructure.Serialization;
using System.IO;

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// 外部记录的数据
    /// </summary>
    public class ExternRecordData
    {
        //数据列表
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 最大电压
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public double MaxTime { get; set; }

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;
    }

    public class NewIOMeasurementViewModel : Screen
    {

        /// <summary>
        /// 内部锁
        /// </summary>
        private object lockObject = new object();

        #region 构造函数

        private bool isAdmin = false;

        /// <summary>
        /// 管理员权限
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                isAdmin = value;
                NotifyOfPropertyChange(() => IsAdmin);
            }
        }

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        public NewIOMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            //更新硬件状态
            UpdateHardware();
        }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器</param>
        /// <param name="power">Power</param>
        public NewIOMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (power == null)
            {
                throw new ArgumentException("power invalid");
            }

            if (plc == null)
            {
                throw new ArgumentException("plc invalid");
            }

            if (pwm == null)
            {
                throw new ArgumentException("pwm invalid");
            }

            Scope = scope;
            Power = power;
            PLC = plc;
            PWM = pwm;

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
            }
            else
            {
                RunningStatus = "就绪";
            }
        }

        #endregion

        #region 运行状态

        private bool isMeasuring;

        /// <summary>
        /// 测量标志
        /// </summary>
        public bool IsMeasuring
        {
            get
            {
                return isMeasuring;
            }
            set
            {
                isMeasuring = value;
                NotifyOfPropertyChange(() => IsMeasuring);
            }
        }

        private bool canMeasure = true;

        /// <summary>
        /// 允许测量
        /// </summary>
        public bool CanMeasure
        {
            get
            {
                if (IsHardwareValid)
                {
                    return canMeasure;
                }

                return false;
            }
            set
            {
                canMeasure = value;
                NotifyOfPropertyChange(() => CanMeasure);
            }
        }

        private string runningStatus = "就绪";

        /// <summary>
        /// 运行状态
        /// </summary>
        public string RunningStatus
        {
            get
            {
                return runningStatus;
            }
            set
            {
                runningStatus = value;
                NotifyOfPropertyChange(() => RunningStatus);
            }
        }

        private double currentInput;

        /// <summary>
        /// 当前输入电压(电源输出电压)
        /// </summary>
        public double CurrentInput
        {
            get
            {
                return currentInput;
            }
            set
            {
                currentInput = value;
                NotifyOfPropertyChange(() => CurrentInput);
            }
        }

        private double currentOutput;

        /// <summary>
        /// 当前输出电压(测量电压)
        /// </summary>
        public double CurrentOutput
        {
            get
            {
                return currentOutput;
            }
            set
            {
                currentOutput = value;
                NotifyOfPropertyChange(() => CurrentOutput);
            }
        }

        #endregion

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// Power接口
        /// </summary>
        public IPower Power { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// PWM接口
        /// </summary>
        public IPWM PWM { get; set; }

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        {
            get
            {
                if ((Scope?.IsConnect == true) &&
                    (Power?.IsConnect == true) &&
                    (PLC?.IsConnect == true) &&
                    (PWM?.IsConnect == true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 更新硬件
        /// </summary>
        public void UpdateHardware()
        {
            if (Scope?.IsConnect != true)
            {
                Scope?.Connect();
            }

            if (Power?.IsConnect != true)
            {
                Power?.Connect();
            }

            if (PLC?.IsConnect != true)
            {
                PLC?.Connect();
            }

            if (PWM?.IsConnect != true)
            {
                PWM?.Connect();
            }

            NotifyOfPropertyChange(() => IsHardwareValid);
            NotifyOfPropertyChange(() => CanMeasure);
        }

        #endregion

        #region 事件

        /// <summary>
        /// 测量开始事件
        /// </summary>
        public event EventHandler<EventArgs> MeasurementStarted;

        /// <summary>
        /// 测量开始事件
        /// </summary>
        protected void OnMeasurementStarted()
        {
            CanMeasure = false;
            MeasurementStarted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<EventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(EventArgs e)
        {
            CanMeasure = true;

            RunningStatus = "成功";

            PLC.PWMSwitch = false;
            PLC.FlowSwitch = false;

            if (Power?.IsConnect == true)
            {
                Power.IsEnableOutput = false;
            }

            lock (lockObject)
            {
                IsMeasuring = false;
            }
            MeasurementCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// 消息触发事件
        /// </summary>
        public event EventHandler<MessageRaisedEventArgs> MessageRaised;

        /// <summary>
        /// 触发消息触发事件
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected void OnMessageRaised(MessageLevel messageLevel, string message, Exception exception = null)
        {
            MessageRaised?.Invoke(this, new MessageRaisedEventArgs(messageLevel, message, exception));
        }

        #endregion

        #region 应用

        private string importFile;

        /// <summary>
        /// 导入的配置文件
        /// </summary>
        public string ImportFile
        {
            get { return importFile; }
            set { importFile = value; NotifyOfPropertyChange(() => ImportFile); }
        }

        /// <summary>
        /// 外部记录的数据
        /// </summary>
        public ExternRecordData ExternRecordData { get; set; }

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 示波器应该采集标志
        /// </summary>
        private bool shouldScopeSample = false;

        /// <summary>
        /// 电压转气压
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        private double VoltageToPressure(double voltage)
        {

            return (voltage - SystemParamManager.SystemParam.GlobalParam.PressureZeroVoltage) * SystemParamManager.SystemParam.GlobalParam.PressureK;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            //配置文件校验
            if (string.IsNullOrWhiteSpace(ImportFile) || !File.Exists(ImportFile))
            {
                OnMessageRaised(MessageLevel.Err, "配置文件无效");
                return;
            }

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            //设置电源模块直通
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = true;

            //开启测量线程
            measureThread = new System.Threading.Thread(() =>
            {
                //加载文件
                ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>("data.json");

                if (ExternRecordData == null)
                {
                    return;
                }
                ExternRecordData.TimeInterval = 1000;

                Stopwatch stopwatch = new Stopwatch();

                //设置示波器采集
                shouldScopeSample = true;
                Scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                Scope.ScopeReadDataCompleted += Scope_ScopeReadDataCompleted;

                //开始连续采集
                Scope.StartSerialSampple();

                int index = 0;
                double lastVol = 0;
                foreach (var item in ExternRecordData.RecordVoltages.Values)
                {
                    stopwatch.Start();
                    while (true)
                    {
                        if (stopwatch.Elapsed.TotalMilliseconds >= ExternRecordData.TimeInterval)
                        {
                            if (item > 0)
                            {
                                //设置当前电压
                                Power.Voltage = item;

                                Console.WriteLine($"[{stopwatch.Elapsed.TotalMilliseconds:F3}:{index}]: vol: {item}");
                                lastVol = item;
                            }
                            else
                            {
                                //设置当前电压
                                Power.Voltage = lastVol;

                                Console.WriteLine($"[{stopwatch.Elapsed.TotalMilliseconds:F3}:{index}]: vol: {lastVol}");
                            }

                            index++;
                            stopwatch.Restart();
                            break;
                        }
                    }

                }

                //输出完成电压后,设置相关标志位
                shouldScopeSample = false;

            });

            measureThread.Start();
        }

        private void Scope_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {
            double[] ch1, ch2;
            e.GetData(out ch1, out ch2);

            //显示结果

            if (!shouldScopeSample)
            {
                var scope = sender as IScopeBase;
                scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                scope.StopSampleThread();
            }

        }

        /// <summary>
        /// 导入配置信息文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportConfigFile(string file)
        {
            ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(file);
        }

        #endregion
    }
}
