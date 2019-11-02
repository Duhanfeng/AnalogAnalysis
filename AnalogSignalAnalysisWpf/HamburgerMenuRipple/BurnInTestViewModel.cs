using AnalogSignalAnalysisWpf.Hardware.PLC;
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
   
    public class BurnInTestViewModel : Screen
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

        public BurnInTestViewModel()
        {
            
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();
            FrequencyErrLimit = SystemParamManager.SystemParam.FrequencyMeasureParams.FrequencyErrLimit;
 

            OutputVoltage = SystemParamManager.SystemParam.FrequencyMeasureParams.OutputVoltage;
            DutyRatio = SystemParamManager.SystemParam.FrequencyMeasureParams.DutyRatio;
            VoltageFilterCount = SystemParamManager.SystemParam.FrequencyMeasureParams.VoltageFilterCount;

            
            if ((SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas == null) || (SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas.Count == 0))
            {
                SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas = new ObservableCollection<FrequencyTestData>
                {
                    new FrequencyTestData(5, 1000),
                    new FrequencyTestData(10, 500),
                    new FrequencyTestData(15, 500),
                    new FrequencyTestData(20, 500),
                    new FrequencyTestData(25, 500),
                    new FrequencyTestData(30, 500),
                    new FrequencyTestData(35, 300),
                    new FrequencyTestData(40, 300),
                    new FrequencyTestData(45, 300),
                    new FrequencyTestData(50, 300),
                    new FrequencyTestData(55, 200),
                    new FrequencyTestData(60, 200),
                    new FrequencyTestData(65, 200),
                    new FrequencyTestData(70, 200),
                    new FrequencyTestData(75, 200),
                    new FrequencyTestData(80, 200),
                    new FrequencyTestData(85, 100),
                    new FrequencyTestData(90, 100),
                    new FrequencyTestData(95, 100),
                    new FrequencyTestData(100, 100),
                };
            }
            TestDatas = SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas;

            MeasurementInfos = new ObservableCollection<FrequencyMeasurementInfo>();

            UpdateHardware();

            IsAdmin = false;
        }

        public BurnInTestViewModel(IScopeBase scope, IPLC plc, IPWM pwm) : this()
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
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
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

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
                if ((Scope?.IsConnect == true) && (PLC?.IsConnect == true) && (PWM?.IsConnect == true))
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

        private ObservableCollection<FrequencyTestData> testDatas;

        /// <summary>
        /// 测试数据
        /// </summary>
        public ObservableCollection<FrequencyTestData> TestDatas
        {
            get
            {
                return testDatas;
            }
            set
            {
                testDatas = value;
                NotifyOfPropertyChange(() => TestDatas);

                SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas = value;
                SystemParamManager.SaveParams();
            }
        }

        private int testDatasIndex;

        /// <summary>
        /// 测试数据索引
        /// </summary>
        public int TestDatasIndex
        {
            get 
            { 
                return testDatasIndex; 
            }
            set 
            {
                testDatasIndex = value;
                NotifyOfPropertyChange(() => TestDatasIndex);
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

        private double outputVoltage = 24;

        /// <summary>
        /// 输出电压(吸合电压)
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

                SystemParamManager.SystemParam.FrequencyMeasureParams.OutputVoltage = value;
                SystemParamManager.SaveParams();
            }
        }

        private int dutyRatio = 50;

        /// <summary>
        /// 占空比
        /// </summary>
        public int DutyRatio
        {
            get
            {
                return dutyRatio;
            }
            set
            {
                if (value > 100)
                {
                    value = 100;
                }
                else if (value < 1)
                {
                    value = 1;
                }

                dutyRatio = value;
                NotifyOfPropertyChange(() => DutyRatio);

                SystemParamManager.SystemParam.FrequencyMeasureParams.DutyRatio = value;
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

                SystemParamManager.SystemParam.FrequencyMeasureParams.VoltageFilterCount = value;
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
        public event EventHandler<FrequencyMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(FrequencyMeasurementCompletedEventArgs e)
        {
            PLC.EnableOutput = false;

            PWM.Frequency = 0;
            if (e.IsSuccess == true)
            {
                RunningStatus = "成功";
                MaxFrequency = e.MaxLimitFrequency;
            }
            else
            {
                RunningStatus = "失败";
                MaxFrequency = -1;
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
                    //collection.Add(new Data() { Value1 = ScopeCHACollection[item / SampleInterval].Value1, Value = item * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    collection2.Add(new Data() { Value1 = collection[item / SampleInterval].Value1, Value = collection[item / SampleInterval].Value });
                }
            }

            ScopeCHACollection = collection;
            ScopeCHAEdgeCollection = collection2;
        }

        #endregion

        #region 方法

        private object lockObject = new object();

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

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

            if ((TestDatas == null) || (TestDatas.Count == 0))
            {
                RunningStatus = "运行数据无效";
                return;
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            RunningStatus = "运行中";

            SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas = TestDatas;
            SystemParamManager.SaveParams();

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();

            MeasurementInfos = new ObservableCollection<FrequencyMeasurementInfo>();

            OnMeasurementStarted();

            measureThread = new Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }
                
                MaxFrequency = -1;
                int lastFrequency = -1;

                //设置频率
                PWM.Frequency = 0;
                PWM.DutyRatio = DutyRatio;

                //使能PLC输出
                PLC.Voltage = OutputVoltage;
                PLC.EnableOutput = true;

                foreach (var item in TestDatas)
                {
                    //设置PLC频率
                    PWM.Frequency = item.Frequency;
                    OnMessageRaised(MessageLevel.Message, $"F: [Frequency- {PWM.Frequency}]");
                    Thread.Sleep(123);

                    //设置Scope采集时长
                    Scope.SampleTime = item.SampleTime;
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
                    Analysis.MeanFilter(originalData, VoltageFilterCount, out filterData);

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
                    CurrentInputFrequency = item.Frequency;
                    CurrentSampleTime = item.SampleTime;
                    new Thread(delegate ()
                    {
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                            System.Threading.SynchronizationContext.Current.Send(pl =>
                            {
                                MeasurementInfos.Insert(0, new FrequencyMeasurementInfo(CurrentInputFrequency, CurrentSampleTime, pulseFrequencies, ScopeCHACollection, ScopeCHAEdgeCollection));
                            }, null);
                        });
                    }).Start();

                    {
                        string pulseMessage = "";
                        foreach (var item2 in pulseFrequencies)
                        {
                            pulseMessage += item2.ToString("F2") + " ";
                        }
                        OnMessageRaised(MessageLevel.Message, $"F: Frequency-{pulseMessage}");
                    }

                    if (pulseFrequencies.Count > 0)
                    {
                        //检测脉冲是否异常
                        double minFrequency = item.Frequency * (1 - FrequencyErrLimit);
                        double maxFrequency = item.Frequency * (1 + FrequencyErrLimit);
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
                            lastFrequency = item.Frequency;
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
        /// 显示示波器数据
        /// </summary>
        /// <param name="index"></param>
        public void ShowScopeListView(int index)
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (index < 0)
            {
                return;
            }

            //显示数据
            ScopeCHACollection = new ObservableCollection<Data>(MeasurementInfos[index].ScopeCHACollection);
            ScopeCHAEdgeCollection = new ObservableCollection<Data>(MeasurementInfos[index].ScopeCHAEdgeCollection);
        }

        /// <summary>
        /// 插入测试数据
        /// </summary>
        public void InsertTestData()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (TestDatasIndex < 0)
            {
                return;
            }

            TestDatas.Insert(TestDatasIndex, new FrequencyTestData(0, 0));
        }

        /// <summary>
        /// 删除测试数据
        /// </summary>
        public void DeleteTestData()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (TestDatasIndex < 0)
            {
                return;
            }

            TestDatas.RemoveAt(TestDatasIndex);
        }

        /// <summary>
        /// 清空测试数据
        /// </summary>
        public void ClearTestDatas()
        {
            TestDatas = new ObservableCollection<FrequencyTestData>();
        }

        /// <summary>
        /// 保存测试数据
        /// </summary>
        public void SaveTestDatas()
        {
            SystemParamManager.SystemParam.FrequencyMeasureParams.TestDatas = TestDatas;
            SystemParamManager.SaveParams();
        }

        #endregion
    }
}
