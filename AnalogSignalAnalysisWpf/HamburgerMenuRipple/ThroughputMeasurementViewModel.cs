﻿using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AnalogSignalAnalysisWpf
{

    public class ThroughputMeasurementViewModel : Screen
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

        /// <summary>
        /// 创建ThroughputMeasurementViewModel新实例
        /// </summary>
        public ThroughputMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            MeasureType = SystemParamManager.SystemParam.ThroughputMeasureParams.MeasureType;

            PressureK = SystemParamManager.SystemParam.ThroughputMeasureParams.PressureK;
            MinVoltageThreshold = SystemParamManager.SystemParam.ThroughputMeasureParams.MinVoltageThreshold;
            MaxVoltageThreshold = SystemParamManager.SystemParam.ThroughputMeasureParams.MaxVoltageThreshold;

            DeadZone = SystemParamManager.SystemParam.ThroughputMeasureParams.DeadZone;
            CriticalValue = SystemParamManager.SystemParam.ThroughputMeasureParams.CriticalValue;

            OutputVoltage = SystemParamManager.SystemParam.ThroughputMeasureParams.OutputVoltage;
            OutputDelay = SystemParamManager.SystemParam.ThroughputMeasureParams.OutputDelay;
            SampleTime = SystemParamManager.SystemParam.ThroughputMeasureParams.SampleTime;

            VoltageFilterCount = SystemParamManager.SystemParam.ThroughputMeasureParams.VoltageFilterCount;
            DerivativeK = SystemParamManager.SystemParam.ThroughputMeasureParams.DerivativeK;
            IsEnableDerivativeFilter = SystemParamManager.SystemParam.ThroughputMeasureParams.IsEnableDerivativeFilter;
            DerivativeFilterCount = SystemParamManager.SystemParam.ThroughputMeasureParams.DerivativeFilterCount;


        }

        /// <summary>
        /// 创建ThroughputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器接口</param>
        /// <param name="power">Power接口</param>
        public ThroughputMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
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
            NotifyOfPropertyChange(() => CanMeasure);
        }

        #endregion

        #region 配置参数

        private int measureType = 1;

        /// <summary>
        /// 测量类型
        /// </summary>
        public int MeasureType
        {
            get
            {
                return measureType;
            }
            set
            {
                measureType = value;
                if (value == 0)
                {
                    IsEnableMeasureType0 = true;
                    IsEnableMeasureType1 = false;
                }
                else
                {
                    IsEnableMeasureType0 = false;
                    IsEnableMeasureType1 = true;
                }

                NotifyOfPropertyChange(() => MeasureType);
                NotifyOfPropertyChange(() => IsEnableMeasureType0);
                NotifyOfPropertyChange(() => IsEnableMeasureType1);

                SystemParamManager.SystemParam.ThroughputMeasureParams.MeasureType = value;
                SystemParamManager.SaveParams();
            }
        }

        private bool isEnableMeasureType0;

        public bool IsEnableMeasureType0
        {
            get
            {
                return isEnableMeasureType0;
            }
            set
            {
                isEnableMeasureType0 = value;
                NotifyOfPropertyChange(() => IsEnableMeasureType0);
            }
        }

        private bool isEnableMeasureType1;

        public bool IsEnableMeasureType1
        {
            get
            {
                return isEnableMeasureType1;
            }
            set
            {
                isEnableMeasureType1 = value;
                NotifyOfPropertyChange(() => IsEnableMeasureType1);
            }
        }

        #region 方式0

        private double pressureK = 1;

        /// <summary>
        /// 气压系数(K=P/V)
        /// </summary>
        public double PressureK
        {
            get
            {
                return pressureK;
            }
            set
            {
                pressureK = value;
                NotifyOfPropertyChange(() => PressureK);

                SystemParamManager.SystemParam.ThroughputMeasureParams.PressureK = value;
                SystemParamManager.SaveParams();
            }
        }

        private double minVoltageThreshold = 1.5;

        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold
        {
            get
            {
                return minVoltageThreshold;
            }
            set
            {
                minVoltageThreshold = value;
                NotifyOfPropertyChange(() => MinVoltageThreshold);

                SystemParamManager.SystemParam.ThroughputMeasureParams.MinVoltageThreshold = value;
                SystemParamManager.SaveParams();
            }
        }

        private double maxVoltageThreshold = 8.0;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold
        {
            get
            {
                return maxVoltageThreshold;
            }
            set
            {
                maxVoltageThreshold = value;
                NotifyOfPropertyChange(() => MaxVoltageThreshold);

                SystemParamManager.SystemParam.ThroughputMeasureParams.MaxVoltageThreshold = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 方式1

        private int deadZone = 15;

        /// <summary>
        /// 死区(0-30)
        /// </summary>
        public int DeadZone
        {
            get
            {
                return deadZone;
            }
            set
            {
                if (value > 30)
                {
                    value = 30;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                deadZone = value;
                NotifyOfPropertyChange(() => DeadZone);

                SystemParamManager.SystemParam.ThroughputMeasureParams.DeadZone = value;
                SystemParamManager.SaveParams();
            }
        }

        private double criticalValue = 0.8;

        /// <summary>
        /// 临界值
        /// </summary>
        public double CriticalValue
        {
            get
            {
                return criticalValue;
            }
            set
            {
                criticalValue = value;
                NotifyOfPropertyChange(() => CriticalValue);

                SystemParamManager.SystemParam.ThroughputMeasureParams.CriticalValue = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        private double outputVoltage = 12.0;

        /// <summary>
        /// 输出电压(V)
        /// </summary>
        public double OutputVoltage
        {
            get
            {
                return outputVoltage;
            }
            set
            {
                outputVoltage = value;
                NotifyOfPropertyChange(() => OutputVoltage);

                SystemParamManager.SystemParam.ThroughputMeasureParams.OutputVoltage = value;
                SystemParamManager.SaveParams();
            }
        }

        private int outputDelay = 300;

        /// <summary>
        /// 输出延时(MS)
        /// </summary>
        public int OutputDelay
        {
            get
            {
                return outputDelay;
            }
            set
            {
                outputDelay = value;
                NotifyOfPropertyChange(() => OutputDelay);

                SystemParamManager.SystemParam.ThroughputMeasureParams.OutputDelay = value;
                SystemParamManager.SaveParams();
            }
        }

        private int sampleTime = 500;

        /// <summary>
        /// 采样时间(MS)
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

                SystemParamManager.SystemParam.ThroughputMeasureParams.SampleTime = value;
                SystemParamManager.SaveParams();
            }
        }

        private int voltageFilterCount = 7;

        /// <summary>
        /// 电压滤波系数
        /// </summary>
        public int VoltageFilterCount
        {
            get
            {
                return voltageFilterCount;
            }
            set
            {
                if (value < 3)
                {
                    value = 3;
                }

                voltageFilterCount = value;
                NotifyOfPropertyChange(() => VoltageFilterCount);

                SystemParamManager.SystemParam.ThroughputMeasureParams.VoltageFilterCount = value;
                SystemParamManager.SaveParams();
            }
        }

        private int derivativeK = 500;

        /// <summary>
        /// 微分系数
        /// </summary>
        public int DerivativeK
        {
            get
            {
                return derivativeK;
            }
            set
            {
                derivativeK = value;
                NotifyOfPropertyChange(() => DerivativeK);

                SystemParamManager.SystemParam.ThroughputMeasureParams.DerivativeK = value;
                SystemParamManager.SaveParams();
            }
        }

        private bool isEnableDerivativeFilter;

        /// <summary>
        /// 使能微分滤波
        /// </summary>
        public bool IsEnableDerivativeFilter
        {
            get
            {
                return isEnableDerivativeFilter;
            }
            set
            {
                isEnableDerivativeFilter = value;
                NotifyOfPropertyChange(() => IsEnableDerivativeFilter);

                SystemParamManager.SystemParam.ThroughputMeasureParams.IsEnableDerivativeFilter = value;
                SystemParamManager.SaveParams();
            }
        }

        private int derivativeFilterCount = 7;

        /// <summary>
        /// 微分滤波系数
        /// </summary>
        public int DerivativeFilterCount
        {
            get
            {
                return derivativeFilterCount;
            }
            set
            {
                if (value < 3)
                {
                    value = 3;
                }

                derivativeFilterCount = value;
                NotifyOfPropertyChange(() => DerivativeFilterCount);

                SystemParamManager.SystemParam.ThroughputMeasureParams.DerivativeFilterCount = value;
                SystemParamManager.SaveParams();
            }
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

        private double currentVoltage;

        /// <summary>
        /// 当前电压(输出电压)
        /// </summary>
        public double CurrentVoltage
        {
            get
            {
                return currentVoltage;
            }
            set
            {
                currentVoltage = value;
                NotifyOfPropertyChange(() => CurrentVoltage);
            }
        }

        private double time;

        /// <summary>
        /// 测量时间
        /// </summary>
        public double Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                NotifyOfPropertyChange(() => Time);
            }
        }

        private double flow;

        /// <summary>
        /// 测量时间
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

        #region 折线图

        private ObservableCollection<Data> voltageCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> VoltageCollection
        {
            get
            {
                return voltageCollection;
            }
            set
            {
                voltageCollection = value;
                NotifyOfPropertyChange(() => VoltageCollection);
            }
        }

        private ObservableCollection<Data> edgeCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> EdgeCollection
        {
            get
            {
                return edgeCollection;
            }
            set
            {
                edgeCollection = value;
                NotifyOfPropertyChange(() => EdgeCollection);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowData(double[] data)
        {
            int SampleInterval = 1;

            var collection = new ObservableCollection<Data>();
            for (int i = 0; i < data.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = data[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
            }

            VoltageCollection = collection;
        }

        private ObservableCollection<Data> derivativeCollection;

        /// <summary>
        /// 微分数据
        /// </summary>
        public ObservableCollection<Data> DerivativeCollection
        {
            get
            {
                return derivativeCollection;
            }
            set
            {
                derivativeCollection = value;
                NotifyOfPropertyChange(() => DerivativeCollection);
            }
        }


        private ObservableCollection<Data> derivativeEdgeCollection;

        /// <summary>
        /// 微分边沿数据
        /// </summary>
        public ObservableCollection<Data> DerivativeEdgeCollection
        {
            get
            {
                return derivativeEdgeCollection;
            }
            set
            {
                derivativeEdgeCollection = value;
                NotifyOfPropertyChange(() => DerivativeEdgeCollection);
            }
        }

        /// <summary>
        /// 显示微分数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowDerivativeData(double[] data)
        {
            int SampleInterval = 1;

            var collection = new ObservableCollection<Data>();
            for (int i = 0; i < data.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = data[i * SampleInterval], Value = (i * 1000.0) / DerivativeK * SampleInterval });
            }

            DerivativeCollection = collection;
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
        public event EventHandler<ThroughputMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(ThroughputMeasurementCompletedEventArgs e)
        {
            CanMeasure = true;

            RunningStatus = e.IsSuccess ? "成功" : "失败";
            
            if (Power?.IsConnect == true)
            {
                Power.IsEnableOutput = false;
            }

            Thread.Sleep(100);
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = false;

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

        #region 方法

        private object lockObject = new object();

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

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

            if (MinVoltageThreshold >= MaxVoltageThreshold)
            {
                throw new ArgumentException("MinVoltageThreshold >= MaxVoltageThreshold");
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            RunningStatus = "运行中";

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            VoltageCollection = new ObservableCollection<Data>();
            EdgeCollection = new ObservableCollection<Data>();
            DerivativeEdgeCollection = new ObservableCollection<Data>();
            DerivativeCollection = new ObservableCollection<Data>();

            OnMeasurementStarted();

            measureThread = new Thread(() =>
            {

                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                RunningStatus = "开始测量";

                //设置采样时间
                Scope.SampleTime = SampleTime;

                PLC.PWMSwitch = false;
                
                //设置电压
                Power.IsEnableOutput = false;
                Power.Voltage = OutputVoltage;

                //启动线程
                new Thread(() =>
                {
                    //设置电压
                    Thread.Sleep(OutputDelay);
                    PLC.FlowSwitch = true;
                    Thread.Sleep(OutputDelay);
                    Power.IsEnableOutput = true;

                }).Start();

                try
                {
                    int start = -1;
                    int end = -1;

                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadDataBlock(0, out originalData);

                    //数据滤波
                    double[] filterData;
                    Analysis.MeanFilter(originalData, VoltageFilterCount, out filterData);

                    //电压转气压
                    double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                    //微分求导
                    double[] derivativeData;
                    Analysis.Derivative(pressureData, (int)Scope.SampleRate / DerivativeK, out derivativeData);

                    //数据滤波
                    double[] filterData2;
                    if (IsEnableDerivativeFilter)
                    {
                        Analysis.MeanFilter(derivativeData, DerivativeFilterCount, out filterData2);
                    }
                    else
                    {
                        filterData2 = derivativeData;
                    }

                    //显示当前曲线
                    ShowData(pressureData);
                    ShowDerivativeData(filterData2);

                    if (MeasureType == 0)
                    {
                        for (int i = (int)Scope.SampleRate * 50 / 1000; i < pressureData.Length; i++)
                        {
                            if ((pressureData[i] > MinVoltageThreshold) && (start == -1))
                            {
                                start = i;
                            }
                            else if ((pressureData[i] > MaxVoltageThreshold) && (start != -1) && (end == -1))
                            {
                                end = i;
                                break;
                            }
                        }

                        if ((start != -1) && (end != -1))
                        {
                            Time = ((end - start) * 1000.0) / (int)Scope.SampleRate;
                            Flow = SystemParamManager.SystemParam.GlobalParam.FlowK / Time * 1000 * 60;
                            var edge = new ObservableCollection<Data>();
                            edge.Add(new Data() { Value1 = VoltageCollection[start].Value1, Value = VoltageCollection[start].Value });
                            edge.Add(new Data() { Value1 = VoltageCollection[end].Value1, Value = VoltageCollection[end].Value });
                            EdgeCollection = edge;
                            OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs(true, Flow));

                        }
                        else
                        {
                            Time = -1;
                            OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs());
                        }
                    }
                    else
                    {
                        var deadZoneCount = (int)(filterData2.Length * ((double)DeadZone / 100));

                        for (int i = deadZoneCount; i < filterData2.Length - (deadZoneCount * 2); i++)
                        {
                            if ((filterData2[i] > CriticalValue) && (start == -1))
                            {
                                start = i;
                            }
                            else if ((filterData2[i - 1] >= CriticalValue) && (start != -1) && (filterData2[i] < CriticalValue))
                            {
                                end = i;
                            }
                        }

                        if ((start != -1) && (end != -1))
                        {
                            Time = ((end - start) * 1000.0) / DerivativeK;


                            OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs(true, Time));

                        }
                        else
                        {
                            Time = -1;
                            OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs());
                        }

                    }


                }
                catch (Exception)
                {
                    OnMeasurementCompleted(new ThroughputMeasurementCompletedEventArgs());
                }

                return;
            });

            measureThread.Start();
        }

        #endregion
    }
}
