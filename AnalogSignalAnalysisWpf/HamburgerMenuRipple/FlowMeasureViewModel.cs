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

namespace AnalogSignalAnalysisWpf
{

    public class FlowMeasureViewModel : Screen
    {
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

        public FlowMeasureViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            SampleTime = SystemParamManager.SystemParam.FlowMeasureParams.SampleTime;
            UpdateHardware();

            IsAdmin = false;
        }

        public FlowMeasureViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
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
        }

        #endregion

        #region 测量信息

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

        private double flow;

        /// <summary>
        /// 流量
        /// </summary>
        public double Flow
        {
            get 
            { 
                return flow; 
            }
            set 
            { 
                flow = value;
                NotifyOfPropertyChange(() => Flow);
            }
        }


        #endregion

        #region 配置参数

        private int sampleTime;

        /// <summary>
        /// 测试频率(Hz)
        /// </summary>
        public int SampleTime
        {
            get
            {
                return sampleTime;
            }
            set
            {
                sampleTime = value;
                NotifyOfPropertyChange(() => SampleTime);
                SystemParamManager.SystemParam.FlowMeasureParams.SampleTime = value;
                SystemParamManager.SaveParams();
            }
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
            MeasurementStarted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<ThroughputMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(ThroughputMeasurementCompletedEventArgs e)
        {
            Power.IsEnableOutput = false;

            PWM.Frequency = 0;
            if (e.IsSuccess == true)
            {
                RunningStatus = "成功";
            }
            else
            {
                RunningStatus = "失败";
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

        #region 折线图

        private ObservableCollection<Data> scopeChACollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHACollection
        {
            get
            {
                return scopeChACollection;
            }
            set
            {
                scopeChACollection = value;
                NotifyOfPropertyChange(() => ScopeCHACollection);
            }
        }

        private ObservableCollection<Data> scopeChAEdgeCollection;

        /// <summary>
        /// 通道A边沿数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHAEdgeCollection
        {
            get
            {
                return scopeChAEdgeCollection;
            }
            set
            {
                scopeChAEdgeCollection = value;
                NotifyOfPropertyChange(() => ScopeCHAEdgeCollection);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowEdgeData(double[] data)
        {
            int SampleInterval = 1;

            var collection = new ObservableCollection<Data>();
            for (int i = 0; i < data.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = data[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
            }
            ScopeCHACollection = collection;
        }

        #endregion

        #region 方法

        private object lockObject = new object();

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 流量转电压
        /// </summary>
        /// <param name="pressure"></param>
        /// <returns></returns>
        private double FlowToVoltage(double pressure)
        {

            return pressure / SystemParamManager.SystemParam.GlobalParam.FlowK;
        }

        /// <summary>
        /// 电压转流量
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        private double VoltageToFlow(double voltage)
        {

            return voltage * SystemParamManager.SystemParam.GlobalParam.FlowK;
        }

        /// <summary>
        /// 显示测量信息
        /// </summary>
        /// <param name="currentInputFrequency"></param>
        /// <param name="currentSampleTime"></param>
        /// <param name="pulseFrequencies"></param>
        /// <param name="scopeCHACollection"></param>
        /// <param name="scopeCHAEdgeCollection"></param>
        private void ShowMeasureInfos(int currentInputFrequency, int currentSampleTime, List<double> pulseFrequencies)
        {
            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    SynchronizationContext.Current.Send(pl =>
                    {
                        
                    }, null);
                });
            }).Start();
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

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            //设置电源模块直通
            PLC.Switch = false;

            OnMeasurementStarted();

            measureThread = new Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                //设置频率
                PWM.Frequency = 0;
                PWM.DutyRatio = SystemParamManager.SystemParam.FrequencyMeasureParams.DutyRatio;

                //使能Power输出
                Power.Voltage = SystemParamManager.SystemParam.FrequencyMeasureParams.OutputVoltage;
                Power.IsEnableOutput = true;
                Thread.Sleep(SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay);

                //设置Scope采集时长
                Scope.SampleTime = SampleTime;

                //读取Scope数据
                double[] originalData;
                Scope.ReadDataBlock(0, out originalData);

                if ((originalData == null) || (originalData.Length == 0))
                {
                    //测试失败
                    OnMessageRaised(MessageLevel.Warning, $"F: ReadDataBlock Fail");
                    OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs());
                    return;
                }

                //数据滤波
                double[] filterData;
                Analysis.MeanFilter(originalData, 7, out filterData);

                //电压转流量
                double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToFlow(x)).ToArray();

                //获取平均值
                Flow = Analysis.Mean(pressureData);

                //显示结果
                ShowEdgeData(pressureData);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //测试成功
                OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs(true, Flow));

                stopwatch.Stop();
            });

            measureThread.Start();
        }

        #endregion
    }
}
