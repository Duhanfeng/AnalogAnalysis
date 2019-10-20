using AnalogSignalAnalysisWpf.Hardware.PWM;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    public class FrequencyMeasurementInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 输入频率
        /// </summary>
        public int InputFrequency { get; set; }

        /// <summary>
        /// 采样时间
        /// </summary>
        public int SampleTime { get; set; }

        /// <summary>
        /// 当前频率
        /// </summary>
        public string CurrentFrequency { get; set; }

        /// <summary>
        /// 创建FrequencyMeasurementInfo新实例
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sampleTime"></param>
        /// <param name="currentFrequency"></param>
        public FrequencyMeasurementInfo(int input, int sampleTime, List<double> pulseFrequencies)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            InputFrequency = input;
            SampleTime = sampleTime;

            string pulseMessage = "";

            if (pulseFrequencies != null)
            {
                foreach (var item in pulseFrequencies)
                {
                    pulseMessage += item.ToString("F2") + ", ";
                }
            }

            CurrentFrequency = pulseMessage;
        }

    }


    public class FrequencyMeasurementViewModel : Screen
    {
        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        public FrequencyMeasurementViewModel()
        {
            //获取配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            ScopeScale = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EScale>());
            ScopeSampleRateCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ESampleRate>());
            ScopeVoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());

            MinVoltageThreshold = SystemParamManager.SystemParam.FrequencyMeasureParams.MinVoltageThreshold;
            MaxVoltageThreshold = SystemParamManager.SystemParam.FrequencyMeasureParams.MaxVoltageThreshold;
            FrequencyErrLimit = SystemParamManager.SystemParam.FrequencyMeasureParams.FrequencyErrLimit;
            ComDelay = SystemParamManager.SystemParam.FrequencyMeasureParams.ComDelay;

            CHAScale = SystemParamManager.SystemParam.FrequencyMeasureParams.CHAScale;
            CHAVoltageDIV = SystemParamManager.SystemParam.FrequencyMeasureParams.CHAVoltageDIV;
            SampleRate = SystemParamManager.SystemParam.FrequencyMeasureParams.SampleRate;
            NotifyOfPropertyChange(() => ScopeCHAScale);
            NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            NotifyOfPropertyChange(() => ScopeSampleRate);

            MeasurementInfos = new ObservableCollection<FrequencyMeasurementInfo>();

            UpdateHardware();

        }

        public FrequencyMeasurementViewModel(IScopeBase scope, IPWM pwm) : this()
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (pwm == null)
            {
                throw new ArgumentException("plc invalid");
            }

            Scope = scope;
            PWM = pwm;
        }

        #region 测量信息

        private ObservableCollection<FrequencyMeasurementInfo> measurementInfos;

        /// <summary>
        /// 测量信息
        /// </summary>
        public ObservableCollection<FrequencyMeasurementInfo> MeasurementInfos
        {
            get
            {
                return measurementInfos;
            }
            set
            {
                measurementInfos = value;
                NotifyOfPropertyChange(() => MeasurementInfos);
            }
        }

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

        private int currentSampleTime;

        /// <summary>
        /// 当前采样时间
        /// </summary>
        public int CurrentSampleTime
        {
            get
            {
                return currentSampleTime;
            }
            set
            {
                currentSampleTime = value;
                NotifyOfPropertyChange(() => CurrentSampleTime);
            }
        }


        private int currentInputFrequency;

        /// <summary>
        /// 当前输入频率
        /// </summary>
        public int CurrentInputFrequency
        {
            get
            {
                return currentInputFrequency;
            }
            set
            {
                currentInputFrequency = value;
                NotifyOfPropertyChange(() => CurrentInputFrequency);
            }
        }


        private int maxFrequency;

        /// <summary>
        /// 极限频率
        /// </summary>
        public int MaxFrequency
        {
            get
            {
                return maxFrequency;
            }
            set
            {
                maxFrequency = value;
                NotifyOfPropertyChange(() => MaxFrequency);
            }
        }

        #endregion

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPWM PWM { get; set; }

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        { 
            get
            {
                if ((Scope?.IsConnect == true) && (PWM?.IsConnect == true))
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
        /// 放大倍数
        /// </summary>
        public ObservableCollection<string> ScopeScale { get; set; }

        /// <summary>
        /// 电压档位
        /// </summary>
        public ObservableCollection<string> ScopeVoltageDIVCollection { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ObservableCollection<string> ScopeSampleRateCollection { get; set; }

        private EScale CHAScale = EScale.x10;

        /// <summary>
        /// CHA探头衰变
        /// </summary>
        public string ScopeCHAScale
        {
            get
            {
                return EnumHelper.GetDescription(CHAScale);
            }
            set
            {
                CHAScale = EnumHelper.GetEnum<EScale>(value);
                NotifyOfPropertyChange(() => ScopeCHAScale);

                SystemParamManager.SystemParam.FrequencyMeasureParams.CHAScale = CHAScale;
                SystemParamManager.SaveParams();
            }
        }

        private EVoltageDIV CHAVoltageDIV = EVoltageDIV.DIV_2V5;

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public string ScopeCHAVoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(CHAVoltageDIV);
            }
            set
            {
                CHAVoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);

                SystemParamManager.SystemParam.FrequencyMeasureParams.CHAVoltageDIV = CHAVoltageDIV;
                SystemParamManager.SaveParams();
            }
        }

        private ESampleRate SampleRate = ESampleRate.Sps_49K;

        /// <summary>
        /// 采样率
        /// </summary>
        public string ScopeSampleRate
        {
            get
            {
                return EnumHelper.GetDescription(SampleRate);
            }
            set
            {
                SampleRate = EnumHelper.GetEnum<ESampleRate>(value);
                NotifyOfPropertyChange(() => ScopeSampleRate);

                SystemParamManager.SystemParam.FrequencyMeasureParams.SampleRate = SampleRate;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 配置参数

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

                //保存参数
                SystemParamManager.SystemParam.FrequencyMeasureParams.MinVoltageThreshold = value;
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

                //保存参数
                SystemParamManager.SystemParam.FrequencyMeasureParams.MaxVoltageThreshold = value;
                SystemParamManager.SaveParams();
            }
        }

        private double frequencyErrLimit = 0.2;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit
        {
            get
            {
                return frequencyErrLimit;
            }
            set
            {
                frequencyErrLimit = value;
                NotifyOfPropertyChange(() => FrequencyErrLimit);

                //保存参数
                SystemParamManager.SystemParam.FrequencyMeasureParams.FrequencyErrLimit = value;
                SystemParamManager.SaveParams();
            }
        }

        private int comDelay = 200;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay
        {
            get
            {
                return comDelay;
            }
            set
            {
                comDelay = value;
                NotifyOfPropertyChange(() => ComDelay);

                //保存参数
                SystemParamManager.SystemParam.FrequencyMeasureParams.ComDelay = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<FrequencyMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(FrequencyMeasurementCompletedEventArgs e)
        {
            if (e.IsSuccess == true)
            {
                MaxFrequency = e.MaxLimitFrequency;
            }
            else
            {
                MaxFrequency = -1;
            }

            IsMeasuring = false;
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
        public ObservableCollection<Data> ScopeChACollection
        {
            get
            {
                return scopeChACollection;
            }
            set
            {
                scopeChACollection = value;
                NotifyOfPropertyChange(() => ScopeChACollection);
            }
        }

        private ObservableCollection<Data> scopeChAEdgeCollection;

        /// <summary>
        /// 通道A边沿数据
        /// </summary>
        public ObservableCollection<Data> ScopeChAEdgeCollection
        {
            get
            {
                return scopeChAEdgeCollection;
            }
            set
            {
                scopeChAEdgeCollection = value;
                NotifyOfPropertyChange(() => ScopeChAEdgeCollection);
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
            ScopeChACollection = collection;

        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="data">数据</param>
        private void ShowEdgeData(double[] data, List<int> edgeIndexs, DigitEdgeType digitEdgeType)
        {
            int SampleInterval = 1;

            var collection = new ObservableCollection<Data>();
            for (int i = 0; i < data.Length / SampleInterval; i++)
            {
                collection.Add(new Data() { Value1 = data[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
            }
            var collection2 = new ObservableCollection<Data>();
            if ((digitEdgeType == DigitEdgeType.FirstFillingEdge) || (digitEdgeType == DigitEdgeType.FirstRisingEdge))
            {
                foreach (var item in edgeIndexs)
                {
                    //collection.Add(new Data() { Value1 = ScopeChACollection[item / SampleInterval].Value1, Value = item * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    collection2.Add(new Data() { Value1 = collection[item / SampleInterval].Value1, Value = collection[item / SampleInterval].Value });
                }
            }

            ScopeChACollection = collection;
            ScopeChAEdgeCollection = collection2;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (Scope?.IsConnect != true)
            {
                throw new Exception("scope invalid");
            }

            //频率列表(单位:Hz)
            int[] frequencies1 = new int[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };
            int[] sampleTime = new int[] { 1000, 500, 500, 500, 500, 300, 300, 300, 200, 200, 200, 200, 200, 200, 200, 100, 100, 100, 100, 100 };

            measureThread = new Thread(() =>
            {
                if (frequencies1.Length != sampleTime.Length)
                {
                    return;
                }
                IsMeasuring = true;
                MaxFrequency = -1;

                int lastFrequency = -1;

                for (int i = 0; i < frequencies1.Length; i++)
                {
                    //设置PLC频率
                    PWM.Frequency = frequencies1[i];
                    OnMessageRaised(MessageLevel.Message, $"F: [Frequency- {PWM.Frequency}]");
                    Thread.Sleep(ComDelay);

                    //设置Scope采集时长
                    Scope.SampleTime = sampleTime[i];
                    OnMessageRaised(MessageLevel.Message, $"F: [SampleTime- {Scope.SampleTime}]");

                    //读取Scope数据
                    double[] originalData;
                    Scope.ReadDataBlock(0, out originalData);

                    if ((originalData == null) || (originalData.Length == 0))
                    {
                        //测试失败
                        OnMessageRaised(MessageLevel.Warning, $"F: ReadDataBlock Fail");
                        OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                        return;
                    }

                    //数据滤波
                    double[] filterData;
                    Analysis.MeanFilter(originalData, 11, out filterData);

                    //阈值查找边沿
                    List<int> edgeIndexs;
                    DigitEdgeType digitEdgeType;
                    Analysis.FindEdgeByThreshold(filterData, MinVoltageThreshold, MaxVoltageThreshold, out edgeIndexs, out digitEdgeType);

                    //显示波形
                    ShowEdgeData(filterData, edgeIndexs, digitEdgeType);

                    //分析脉冲数据
                    List<double> pulseFrequencies;
                    List<double> dutyRatios;
                    Analysis.AnalysePulseData(edgeIndexs, digitEdgeType, (int)Scope.SampleRate, out pulseFrequencies, out dutyRatios);

                    //显示分析结果
                    CurrentInputFrequency = frequencies1[i];
                    CurrentSampleTime = sampleTime[i];
                    new Thread(delegate ()
                    {
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                            System.Threading.SynchronizationContext.Current.Send(pl =>
                            {
                                MeasurementInfos.Add(new FrequencyMeasurementInfo(CurrentInputFrequency, CurrentSampleTime, pulseFrequencies));
                            }, null);
                        });
                    }).Start();

                    {
                        string pulseMessage = "";
                        foreach (var item in pulseFrequencies)
                        {
                            pulseMessage += item.ToString("F2") + " ";
                        }
                        OnMessageRaised(MessageLevel.Message, $"F: Frequency-{pulseMessage}");
                    }
                    
                    if (pulseFrequencies.Count > 0)
                    {
                        //检测脉冲是否异常
                        double minFrequency = frequencies1[i] * (1 - FrequencyErrLimit);
                        double maxFrequency = frequencies1[i] * (1 + FrequencyErrLimit);
                        if (!Analysis.CheckFrequency(pulseFrequencies, minFrequency, maxFrequency, 1))
                        {
                            if (lastFrequency != -1)
                            {
                                //测试完成
                                OnMessageRaised(MessageLevel.Message, $"F: Success, max = {lastFrequency}");
                                OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                            }
                            else
                            {
                                //测试失败
                                OnMessageRaised(MessageLevel.Warning, $"F: Fail");
                                OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                            }
                            return;
                        }
                        else
                        {
                            lastFrequency = frequencies1[i];
                        }
                    }
                    else
                    {
                        if (lastFrequency != -1)
                        {
                            //测试完成
                            OnMessageRaised(MessageLevel.Message, $"F: Success, max = {lastFrequency}");
                            OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                        }
                        else
                        {
                            //测试失败
                            OnMessageRaised(MessageLevel.Warning, $"F: Fail");
                            OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                        }
                        return;
                    }

                }

                if (lastFrequency != -1)
                {
                    //测试完成
                    OnMessageRaised(MessageLevel.Message, $"F: Success, max = {lastFrequency}");
                    OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(true, lastFrequency));
                }
                else
                {
                    //测试失败
                    OnMessageRaised(MessageLevel.Warning, $"F: Fail");
                    OnMeasurementCompleted(new FrequencyMeasurementCompletedEventArgs(false));
                }

            });

            measureThread.Start();
        }

        /// <summary>
        /// 更新硬件
        /// </summary>
        public void UpdateHardware()
        {
            NotifyOfPropertyChange(() => IsHardwareValid);
        }

        #endregion
    }
}
