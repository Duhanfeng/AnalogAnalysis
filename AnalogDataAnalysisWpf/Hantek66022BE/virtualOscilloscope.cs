using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf.Hantek66022BE
{
    /// <summary>
    /// 设备应用
    /// </summary>
    public class VirtualOscilloscope
    {
        /// <summary>
        /// 设备校正数据
        /// </summary>
        private IntPtr calData = IntPtr.Zero;

        /// <summary>
        /// 创建DeviceApplication新实例
        /// </summary>
        public VirtualOscilloscope() : this(0)
        {

        }

        /// <summary>
        /// 创建DeviceApplication新实例
        /// </summary>
        /// <param name="deviceIndex">设备索引</param>
        public VirtualOscilloscope(int deviceIndex)
        {
            DeviceIndex = deviceIndex;

            //打开设备
            if (HardwareControl.dsoOpenDevice((ushort)deviceIndex) == 0)
            {
                throw new Exception("设备打开失败!");
            }

            IsOpen = true;

            //获取标定数据
            calData = Marshal.AllocHGlobal(2 * 32);

            if (HardwareControl.dsoGetCalLevel((ushort)deviceIndex, calData, 32) == 0)
            {
                throw new Exception("读取校正数据失败!");
            }

            //设置参数
            CH1VoltageDIV = EVoltageDIV.DIV_1V;
            CH2VoltageDIV = EVoltageDIV.DIV_1V;
            TimeDIV = ETimeDIV.DIV_1MSaS;
            SampleTime = 100;

        }

        /// <summary>
        /// 创建DeviceApplication新实例
        /// </summary>
        /// <param name="deviceIndex">设备索引</param>
        /// <param name="defaultSampleCount">默认采样数量</param>
        public VirtualOscilloscope(int deviceIndex, int sampleTime) : this(deviceIndex)
        {
            SampleTime = sampleTime;
        }

        /// <summary>
        /// 析构DeviceApplication实例
        /// </summary>
        ~VirtualOscilloscope()
        {
            //释放开辟的内存空间
            if (calData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(calData);
            }
        }

        /// <summary>
        /// 设备打开标志
        /// </summary>
        public bool IsOpen { get; private set; } = false;

        /// <summary>
        /// 设备索引
        /// </summary>
        public int DeviceIndex { get; private set; }

        #region 设备参数设置

        private EVoltageDIV ch1VoltageDIV;

        /// <summary>
        /// 通道1电压档位
        /// </summary>
        public EVoltageDIV CH1VoltageDIV
        {
            get
            {
                return ch1VoltageDIV;
            }
            set
            {
                if (HardwareControl.dsoSetVoltDIV((ushort)DeviceIndex, 0, (int)value) == 0)
                {
                    throw new InvalidOperationException($"设置设备{DeviceIndex}通道1电压档位失败!");
                }

                ch1VoltageDIV = value;
            }
        }

        private EVoltageDIV ch2VoltageDIV;

        /// <summary>
        /// 通道2电压档位
        /// </summary>
        public EVoltageDIV CH2VoltageDIV
        {
            get
            {
                return ch2VoltageDIV;
            }
            set
            {
                if (HardwareControl.dsoSetVoltDIV((ushort)DeviceIndex, 1, (int)value) == 0)
                {
                    throw new InvalidOperationException($"设置设备{DeviceIndex}通道2电压档位失败!");
                }

                ch2VoltageDIV = value;
            }
        }

        private ETimeDIV timeDIV;

        /// <summary>
        /// 采样频率档位
        /// </summary>
        public ETimeDIV TimeDIV
        {
            get
            {
                return timeDIV;
            }
            set
            {
                if (HardwareControl.dsoSetTimeDIV((ushort)DeviceIndex, (int)value) == 0)
                {
                    throw new InvalidOperationException($"设置设备{DeviceIndex}采样频率失败!");
                }

                timeDIV = value;
                SampleCount = TimeDIVToSampleRate(TimeDIV) / 1000 * sampleTime;
            }
        }

        /// <summary>
        /// 扫频模式
        /// </summary>
        public ETriggerSweep TriggerSweep { get; set; } = ETriggerSweep.Auto;

        /// <summary>
        /// 触发源
        /// </summary>
        public ETriggerSource TriggerSource { get; set; } = ETriggerSource.CH1;

        private short triggerLevel = 128;

        /// <summary>
        /// 触发电平
        /// </summary>
        public short TriggerLevel
        {
            get
            {
                return triggerLevel;
            }
            set
            {
                //数据防溢出处理
                if (value > 255)
                {
                    value = 255;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                triggerLevel = value;
            }
        }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerSlope TriggerSlope { get; set; } = ETriggerSlope.Rise;

        private short horizontalTriggerPosition = 50;

        /// <summary>
        /// 水平触发位置
        /// </summary>
        public short HorizontalTriggerPosition
        {
            get
            {
                return horizontalTriggerPosition;
            }
            set
            {
                //数据防溢出处理
                if (value > 100)
                {
                    value = 100;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                horizontalTriggerPosition = value;
            }
        }

        /// <summary>
        /// 差值方式
        /// </summary>
        public EInsertMode InsertMode { get; set; } = EInsertMode.Step;

        #endregion

        private int sampleTime;

        /// <summary>
        /// 采集时长(MS)
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
                SampleCount = TimeDIVToSampleRate(TimeDIV) / 1000 * sampleTime;
            }
        }

        /// <summary>
        /// 默认采样数量
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate
        {
            get
            {
                return TimeDIVToSampleRate(TimeDIV);
            }
        }

        /// <summary>
        /// 比例系数
        /// </summary>
        public double Scale = 2.0 / 67;

        /// <summary>
        /// 采样档位转采样率
        /// </summary>
        /// <param name="timeDIV"></param>
        /// <returns>采样率</returns>
        private static int TimeDIVToSampleRate(ETimeDIV timeDIV)
        {
            int sampleRate = 0;
            switch (timeDIV)
            {
                case ETimeDIV.DIV_48MSaS:
                    sampleRate = 48 * 1000 * 1000;
                    break;
                case ETimeDIV.DIV_16MSaS:
                    sampleRate = 16 * 1000 * 1000;
                    break;
                case ETimeDIV.DIV_8MSaS:
                    sampleRate = 8 * 1000 * 1000;
                    break;
                case ETimeDIV.DIV_4MSaS:
                    sampleRate = 4 * 1000 * 1000;
                    break;
                case ETimeDIV.DIV_1MSaS:
                    sampleRate = 1 * 1000 * 1000;
                    break;
                case ETimeDIV.DIV_500KSaS:
                    sampleRate = 500 * 1000;
                    break;
                case ETimeDIV.DIV_200KSaS:
                    sampleRate = 200 * 1000;
                    break;
                case ETimeDIV.DIV_100KSaS:
                    sampleRate = 100 * 1000;
                    break;
                default:
                    break;
            }

            return sampleRate;
        }

        /// <summary>
        /// 设备锁
        /// </summary>
        private object deviceLock = new object();

        /// <summary>
        /// 最大采样数据
        /// </summary>
        private readonly int maxSampleCount = 100*1000;

        /// <summary>
        /// 读取设备数据
        /// </summary>
        /// <param name="sampleCount">采样数据</param>
        /// <param name="channel1Array">通道1数据</param>
        /// <param name="channel2Array">通道2数据</param>
        /// <param name="triggerPointIndex">触发点位置</param>
        public void ReadDeviceData(int sampleCount, out short[] channel1Array, out short[] channel2Array, out uint triggerPointIndex)
        {
            //加线程锁
            lock (deviceLock)
            {
                //提取数据
                channel1Array = new short[sampleCount];
                channel2Array = new short[sampleCount];

                triggerPointIndex = 0;

                //创建内存空间
                IntPtr channel1Data = IntPtr.Zero;
                IntPtr channel2Data = IntPtr.Zero;

                try
                {
                    channel1Data = Marshal.AllocHGlobal(sizeof(short) * sampleCount);
                    channel2Data = Marshal.AllocHGlobal(sizeof(short) * sampleCount);

                    for (int i = 0; i < sampleCount / maxSampleCount; i++)
                    {
                        //获取硬件采样数据
                        if (HardwareControl.dsoReadHardData((ushort)DeviceIndex, 
                            IntPtr.Add(channel1Data, i * maxSampleCount * sizeof(short)),
                            IntPtr.Add(channel2Data, i * maxSampleCount * sizeof(short)),
                            (uint)maxSampleCount, calData,
                            (int)CH1VoltageDIV, (int)CH2VoltageDIV,
                            (short)TriggerSweep, (short)TriggerSource,
                            TriggerLevel, (short)TriggerSlope,
                            (int)TimeDIV, HorizontalTriggerPosition,
                            (uint)maxSampleCount, ref triggerPointIndex,
                            (short)InsertMode) != -1)
                        {

                        }
                    }
                    if ((sampleCount % maxSampleCount) != 0)
                    {
                        int surplusCount = sampleCount % maxSampleCount;

                        //获取硬件采样数据
                        if (HardwareControl.dsoReadHardData((ushort)DeviceIndex,
                            IntPtr.Add(channel1Data, (sampleCount - surplusCount) * sizeof(short)),
                            IntPtr.Add(channel2Data, (sampleCount - surplusCount) * sizeof(short)),
                            (uint)surplusCount, calData,
                            (int)CH1VoltageDIV, (int)CH2VoltageDIV,
                            (short)TriggerSweep, (short)TriggerSource,
                            TriggerLevel, (short)TriggerSlope,
                            (int)TimeDIV, HorizontalTriggerPosition,
                            (uint)surplusCount, ref triggerPointIndex,
                            (short)InsertMode) != -1)
                        {

                        }
                    }

                    //数据拷贝
                    unsafe
                    {
                        short* ch1 = (short*)channel1Data.ToPointer();
                        short* ch2 = (short*)channel2Data.ToPointer();

                        for (int i = 0; i < sampleCount; i++)
                        {
                            channel1Array[i] = ch1[i];
                            channel2Array[i] = ch2[i];
                        }
                    }
                    
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (channel1Data != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(channel1Data);
                        channel1Data = IntPtr.Zero;
                    }

                    if (channel2Data != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(channel2Data);
                        channel2Data = IntPtr.Zero;
                    }

                }
            }
        }

        /// <summary>
        /// 读取设备数据
        /// </summary>
        /// <param name="source">数据源</param>
        public void ReadDeviceData(out double[] source)
        {
            source = new double[SampleCount];

            short[] channel1Array;
            short[] channel2Array;
            uint triggerPointIndex;
            ReadDeviceData(SampleCount, out channel1Array, out channel2Array, out triggerPointIndex);

            for (int i = 0; i < SampleCount; i++)
            {
                source[i] = channel1Array[i] * Scale;
            }

            //for (int i = 0; i < SampleCount / maxSampleCount; i++)
            //{
            //    short[] channel1Array;
            //    short[] channel2Array;
            //    uint triggerPointIndex;
            //    ReadDeviceData(maxSampleCount, out channel1Array, out channel2Array, out triggerPointIndex);

            //    for (int j = 0; j < maxSampleCount; j++)
            //    {
            //        source[i * maxSampleCount + j] = channel1Array[j] * Scale;
            //    }
            //}
            //if ((SampleCount % maxSampleCount) != 0)
            //{
            //    short[] channel1Array;
            //    short[] channel2Array;
            //    uint triggerPointIndex;
            //    ReadDeviceData(SampleCount % maxSampleCount, out channel1Array, out channel2Array, out triggerPointIndex);

            //    for (int i = 0; i < SampleCount % maxSampleCount; i++)
            //    {
            //        source[(SampleCount /  maxSampleCount) * maxSampleCount + i] = channel1Array[i] * Scale;
            //    }
            //}
        }
    }
}
