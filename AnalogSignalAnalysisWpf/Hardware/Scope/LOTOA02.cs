using System;
using System.Threading;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{

    internal class CalibrationData
    {
        #region 零点数据

        public static double CHA_250mV_x1_Zero { get; set; } = 139.4402832;

        public static double CHA_500mV_x1_Zero { get; set; } = 139.3402344;

        public static double CHA_1000mV_x1_Zero { get; set; } = 142.3857422;

        public static double CHA_2500mV_x1_Zero { get; set; } = 140.6334717;

        public static double CHA_5000mV_x1_Zero { get; set; } = 140.2796224;


        public static double CHB_250mV_x1_Zero { get; set; } = 139.2799561;

        public static double CHB_500mV_x1_Zero { get; set; } = 137.8454346;

        public static double CHB_1000mV_x1_Zero { get; set; } = 143.0397461;

        public static double CHB_2500mV_x1_Zero { get; set; } = 140.017334;

        public static double CHB_5000mV_x1_Zero { get; set; } = 138.5435113;


        public static double CHA_250mV_x10_Zero { get; set; } = 142.5351318;

        public static double CHA_500mV_x10_Zero { get; set; } = 140.6565674;

        public static double CHA_1000mV_x10_Zero { get; set; } = 143.5648251;

        public static double CHA_2500mV_x10_Zero { get; set; } = 141.7614552;

        public static double CHA_5000mV_x10_Zero { get; set; } = 140.1818886;


        public static double CHB_250mV_x10_Zero { get; set; } = 143.0370605;

        public static double CHB_500mV_x10_Zero { get; set; } = 140.0056396;

        public static double CHB_1000mV_x10_Zero { get; set; } = 144.1826482;

        public static double CHB_2500mV_x10_Zero { get; set; } = 141.0506572;

        public static double CHB_5000mV_x10_Zero { get; set; } = 138.7887525;

        #endregion

        #region 比例系数

        public static double CHA_250mV_x1_Scale { get; set; } = 0;

        public static double CHA_500mV_x1_Scale { get; set; } = 0;

        public static double CHA_1000mV_x1_Scale { get; set; } = 0;

        public static double CHA_2500mV_x1_Scale { get; set; } = 0;

        public static double CHA_5000mV_x1_Scale { get; set; } = 0.045987393;


        public static double CHB_250mV_x1_Scale { get; set; } = 0;

        public static double CHB_500mV_x1_Scale { get; set; } = 0;

        public static double CHB_1000mV_x1_Scale { get; set; } = 0;

        public static double CHB_2500mV_x1_Scale { get; set; } = 0;

        public static double CHB_5000mV_x1_Scale { get; set; } = 0.046181419;


        public static double CHA_250mV_x10_Scale { get; set; } = 0;

        public static double CHA_500mV_x10_Scale { get; set; } = 0;

        public static double CHA_1000mV_x10_Scale { get; set; } = 0.111728836;

        public static double CHA_2500mV_x10_Scale { get; set; } = 0.192329772;

        public static double CHA_5000mV_x10_Scale { get; set; } = 0.462174852;


        public static double CHB_250mV_x10_Scale { get; set; } = 0;

        public static double CHB_500mV_x10_Scale { get; set; } = 0;

        public static double CHB_1000mV_x10_Scale { get; set; } = 0.108861738;

        public static double CHB_2500mV_x10_Scale { get; set; } = 0.191269944;

        public static double CHB_5000mV_x10_Scale { get; set; } = 0.467484475;

        #endregion

    }


    public class LOTOA02 : IScopeBase
    {
        //声明需要的变量
        public static byte g_CtrlByte0 = 0;//记录IO控制位
        public static byte g_CtrlByte1 = 0;//记录IO控制位

        public static IntPtr g_pBuffer = (IntPtr)0;

        private double currentCHAZero = 0;
        private double currentCHAScale = 0;

        private double currentCHBZero = 0;
        private double currentCHBScale = 0;

        /// <summary>
        /// 线程锁
        /// </summary>
        private object lockObject = new object();

        #region 参数配置

        public void SetCoupling(EChannel channel, ECoupling coupling)
        {
            lock (lockObject)
            {
                if (channel == EChannel.CHA)
                {
                    switch (coupling)
                    {
                        case ECoupling.DC:
                            //DC耦合
                            g_CtrlByte0 &= 0xef;
                            g_CtrlByte0 |= 0x10;
                            MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                            break;
                        case ECoupling.AC:
                            //AC耦合
                            g_CtrlByte0 &= 0xef;
                            MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (coupling)
                    {
                        case ECoupling.DC:
                            //DC耦合
                            g_CtrlByte1 &= 0xef;
                            g_CtrlByte1 |= 0x10;
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case ECoupling.AC:
                            //AC耦合
                            g_CtrlByte1 &= 0xef;
                            g_CtrlByte1 |= 0x00;
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        public void SetSampleRate(ESampleRate sampleRate)
        {
            lock (lockObject)
            {
                switch (sampleRate)
                {
                    case ESampleRate.Sps_49K:
                        //设置49K Hz 采样率
                        g_CtrlByte0 &= 0xf0;
                        g_CtrlByte0 |= 0x0e;
                        MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    case ESampleRate.Sps_96K:
                        //设置96K Hz 采样率
                        g_CtrlByte0 &= 0xf0;
                        g_CtrlByte0 |= 0x04;
                        MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    case ESampleRate.Sps_781K:
                        //设置781K Hz 采样率
                        g_CtrlByte0 &= 0xf0;
                        g_CtrlByte0 |= 0x0c;
                        MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    case ESampleRate.Sps_12M5:
                        // 设置12.5M Hz 采样率
                        g_CtrlByte0 &= 0xf0;
                        g_CtrlByte0 |= 0x08;
                        MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    case ESampleRate.Sps_100M:
                        //设置100M Hz 采样率
                        g_CtrlByte0 &= 0xf0;
                        g_CtrlByte0 |= 0x00;
                        MyDLLimport.USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    default:
                        break;
                }
            }

        }

        public void SetTriggerModel(ETriggerModel triggerModel)
        {
            lock (lockObject)
            {
                switch (triggerModel)
                {
                    case ETriggerModel.CHA:
                        MyDLLimport.USBCtrlTrans(0xE7, 0x01, 1);
                        break;
                    case ETriggerModel.Ext:
                        //g_TrigSourceChan = 2;
                        //通道EXT触发
                        MyDLLimport.USBCtrlTrans(0xE7, 0x01, 1);
                        break;
                    case ETriggerModel.No:
                        MyDLLimport.USBCtrlTrans(0xE7, 0x00, 1);
                        break;
                    default:
                        break;
                }
            }

        }

        public void SetTriggerEdge(ETriggerEdge triggerEdge)
        {
            lock (lockObject)
            {
                switch (triggerEdge)
                {
                    case ETriggerEdge.Rising:
                        MyDLLimport.USBCtrlTrans(0xC5, 0x00, 1);
                        break;
                    case ETriggerEdge.Filling:
                        MyDLLimport.USBCtrlTrans(0xC5, 0x01, 1);
                        break;
                    default:
                        break;
                }
            }

        }

        public void SetTriggerLevel(byte level)
        {
            lock (lockObject)
            {
                //设置触发数据
                MyDLLimport.USBCtrlTrans(0x16, level, 1);
            }

        }

        public void SetVoltageDIV(EChannel channel, EVoltageDIV voltageDIV)
        {
            lock (lockObject)
            {
                if (channel == EChannel.CHA)
                {
                    switch (voltageDIV)
                    {
                        case EVoltageDIV.DIV_250MV:
                            g_CtrlByte1 &= 0xF7;
                            MyDLLimport.USBCtrlTrans(0x22, 0x04, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_500MV:
                            g_CtrlByte1 &= 0xF7;
                            MyDLLimport.USBCtrlTrans(0x22, 0x02, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_1V:
                            g_CtrlByte1 &= 0xF7;
                            g_CtrlByte1 |= 0x08;
                            MyDLLimport.USBCtrlTrans(0x22, 0x04, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_2V5:
                            g_CtrlByte1 &= 0xF7;
                            g_CtrlByte1 |= 0x08;
                            MyDLLimport.USBCtrlTrans(0x22, 0x02, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_5V:
                            g_CtrlByte1 &= 0xF7;
                            g_CtrlByte1 |= 0x08;
                            MyDLLimport.USBCtrlTrans(0x22, 0x00, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (voltageDIV)
                    {
                        case EVoltageDIV.DIV_250MV:
                            g_CtrlByte1 &= 0xF9;
                            g_CtrlByte1 |= 0x04;//放大4倍               
                            MyDLLimport.USBCtrlTrans(0x23, 0x40, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_500MV:
                            g_CtrlByte1 &= 0xF9;
                            g_CtrlByte1 |= 0x02;//放大两倍
                            MyDLLimport.USBCtrlTrans(0x23, 0x40, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_1V:
                            g_CtrlByte1 &= 0xF9;
                            g_CtrlByte1 |= 0x04;
                            MyDLLimport.USBCtrlTrans(0x23, 0x00, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_2V5:
                            g_CtrlByte1 &= 0xF9;
                            g_CtrlByte1 |= 0x02;
                            MyDLLimport.USBCtrlTrans(0x23, 0x00, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        case EVoltageDIV.DIV_5V:
                            g_CtrlByte1 &= 0xF9;
                            MyDLLimport.USBCtrlTrans(0x23, 0x00, 1);
                            MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                            break;
                        default:
                            break;
                    }

                }
            }

        }

        /// <summary>
        /// 使能通道B
        /// </summary>
        /// <param name="isEnable"></param>
        public void EnableCHB(bool isEnable)
        {
            lock (lockObject)
            {
                if (isEnable)
                {
                    g_CtrlByte1 &= 0xfe;
                    g_CtrlByte1 |= 0x01;
                    MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
                else
                {
                    g_CtrlByte1 &= 0xfe;
                    g_CtrlByte1 |= 0x00;
                    MyDLLimport.USBCtrlTrans(0x24, g_CtrlByte1, 1);
                }
            }

        }

        ///// <summary>
        ///// 获取比例参数
        ///// </summary>
        //public void GetScaleParam()
        //{
        //    globleVariables.m_ZrroUniInt = MyDLLimport.USBCtrlTrans(144, 5, 1u) - 128;
        //    globleVariables.g_VbiasScale_1V_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 3, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_500mV_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 8, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_200mV_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 6, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_100mV_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 9, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_50mV_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 10, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_20mV_ch0 = (float)(int)MyDLLimport.USBCtrlTrans(144, 42, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_1V_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 4, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_500mV_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 11, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_200mV_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 7, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_100mV_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 12, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_50mV_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 13, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasScale_20mV_ch1 = (float)(int)MyDLLimport.USBCtrlTrans(144, 45, 1u) * 2f / 255f;
        //    globleVariables.g_VbiasZero020mv = MyDLLimport.USBCtrlTrans(144, 160, 1u);
        //    globleVariables.g_VbiasZero050mv = MyDLLimport.USBCtrlTrans(144, 16, 1u);
        //    globleVariables.g_VbiasZero0100mv = MyDLLimport.USBCtrlTrans(144, 18, 1u);
        //    globleVariables.g_VbiasZero0200mv = MyDLLimport.USBCtrlTrans(144, 20, 1u);
        //    globleVariables.g_VbiasZero0500mv = MyDLLimport.USBCtrlTrans(144, 14, 1u);
        //    globleVariables.g_VbiasZero01v = MyDLLimport.USBCtrlTrans(144, 1, 1u);
        //    globleVariables.g_VbiasZero120mv = MyDLLimport.USBCtrlTrans(144, 161, 1u);
        //    globleVariables.g_VbiasZero150mv = MyDLLimport.USBCtrlTrans(144, 17, 1u);
        //    globleVariables.g_VbiasZero1100mv = MyDLLimport.USBCtrlTrans(144, 19, 1u);
        //    globleVariables.g_VbiasZero1200mv = MyDLLimport.USBCtrlTrans(144, 21, 1u);
        //    globleVariables.g_VbiasZero1500mv = MyDLLimport.USBCtrlTrans(144, 15, 1u);
        //    globleVariables.g_VbiasZero11v = MyDLLimport.USBCtrlTrans(144, 2, 1u);
        //}

        #endregion

        #region 示波器接口

        /// <summary>
        /// 采集数量
        /// </summary>
        private uint sampleCount;

        /// <summary>
        /// 无效数据数量
        /// </summary>
        private uint invalidDataCount = 30;

        private int sampleTime = 200;

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
                sampleCount = (uint)((uint)SampleRate * (sampleTime + 50) / 1000);
                invalidDataCount = (uint)((uint)SampleRate * 50 / 1000);

                lock (lockObject)
                {
                    //设置数据缓冲区
                    MyDLLimport.SetInfo(1, 0, 0x11, 0, 0, sampleCount * 2);//设置使用的缓冲区字节数
                }

            }
        }

        /// <summary>
        /// 创建示波器实例
        /// </summary>
        public LOTOA02()
        {
            //获取数据缓冲区首指针(修改成创建实例时获取)
            g_pBuffer = MyDLLimport.GetBuffer4Wr(-1);
            if (g_pBuffer == IntPtr.Zero)
            {
                throw new Exception("创建示波器内部缓冲区无效");
            }
        }

        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; private set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>执行结果</returns>
        public bool Connect()
        {
            lock (lockObject)
            {
                try
                {
                    IsConnect = false;

                    //设置产品编号
                    MyDLLimport.SpecifyDevIdx(6);

                    //打开设备
                    Int32 res = MyDLLimport.DeviceOpen();
                    if (res != 0)
                    {
                        return false;
                    }

                    //设置默认参数
                    SampleRate = ESampleRate.Sps_49K;
                    SampleTime = 200;
                    TriggerModel = ETriggerModel.No;
                    CHAScale = EScale.x10;
                    CHBScale = EScale.x10;
                    CHAVoltageDIV = EVoltageDIV.DIV_2V5;
                    CHBVoltageDIV = EVoltageDIV.DIV_2V5;

                    IsConnect = true;

                }
                catch (Exception)
                {
                    IsConnect = false;
                }

            }

            return IsConnect;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            lock (lockObject)
            {
                MyDLLimport.DeviceClose();
                IsConnect = false;
            }

        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        public void ReadDataBlock(int channelIndex, out double[] channelData)
        {
            double[] channelData1;
            double[] channelData2;

            ReadDataBlock(out channelData1, out channelData2);
            channelData = (channelIndex == 0) ? channelData1 : channelData2;

        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadDataBlock(out double[] channelData1, out double[] channelData2)
        {
            lock (lockObject)
            {
                int retryCount = 0;
                channelData1 = new double[0];
                channelData2 = new double[0];

                //开始AD采集
                MyDLLimport.USBCtrlTransSimple((Int32)0x33);

                //等待采集完成
                while (MyDLLimport.USBCtrlTransSimple((Int32)0x50) != 33)
                {
                    Thread.Sleep(10);
                    retryCount++;

                    if (retryCount > 1000)
                    {
                        throw new TimeoutException("采集数据超时");
                    }
                }

                //获取数据
                int res = MyDLLimport.AiReadBulkData((int)sampleCount * 2, 1, 2000, g_pBuffer);
                if (res == 0)
                {
                    //等待事件
                    var g_CurrentEventID = MyDLLimport.EventCheck(2000);
                    if (g_CurrentEventID == 1365 || g_CurrentEventID == -1)
                    {
                        return;
                    }

                    channelData1 = new double[sampleCount - invalidDataCount];
                    channelData2 = new double[sampleCount - invalidDataCount];
                    unsafe
                    {
                        byte* pData = (byte*)g_pBuffer;
                        for (uint i = invalidDataCount; i < sampleCount; i++)
                        {
                            channelData1[i - invalidDataCount] = (*(pData + i * 2) - currentCHAZero) * currentCHAScale;
                            channelData2[i - invalidDataCount] = (*(pData + i * 2 + 1) - currentCHBZero) * currentCHBScale;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 开始记录数据
        /// </summary>
        public void StartSample()
        {
            lock (lockObject)
            {
                int retryCount = 0;

                //开始AD采集
                MyDLLimport.USBCtrlTransSimple((Int32)0x33);

                //等待采集完成
                while (MyDLLimport.USBCtrlTransSimple((Int32)0x50) != 33)
                {
                    Thread.Sleep(10);
                    retryCount++;

                    if (retryCount > 1000)
                    {
                        throw new TimeoutException("采集数据超时");
                    }
                }
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadData(int channelIndex, out double[] channelData)
        {
            double[] channelData1;
            double[] channelData2;

            ReadDataBlock(out channelData1, out channelData2);
            channelData = (channelIndex == 0) ? channelData1 : channelData2;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadData(out double[] channelData1, out double[] channelData2)
        {
            lock (lockObject)
            {
                channelData1 = new double[0];
                channelData2 = new double[0];

                //获取数据
                int res = MyDLLimport.AiReadBulkData((int)sampleCount * 2, 1, 2000, g_pBuffer);
                if (res == 0)
                {
                    //等待事件
                    var g_CurrentEventID = MyDLLimport.EventCheck(2000);
                    if (g_CurrentEventID == 1365 || g_CurrentEventID == -1)
                    {
                        return;
                    }

                    channelData1 = new double[sampleCount - invalidDataCount];
                    channelData2 = new double[sampleCount - invalidDataCount];
                    unsafe
                    {
                        byte* pData = (byte*)g_pBuffer;
                        for (uint i = invalidDataCount; i < sampleCount; i++)
                        {
                            channelData1[i - invalidDataCount] = (*(pData + i * 2) - currentCHAZero) * currentCHAScale;
                            channelData2[i - invalidDataCount] = (*(pData + i * 2 + 1) - currentCHBZero) * currentCHBScale;
                        }
                    }
                }
            }
        }

        #endregion

        #region 连续采集模式

        #region 事件

        public event EventHandler<ScopeReadDataCompletedEventArgs> ScopeReadDataCompleted;

        protected void OnScopeReadDataCompleted(double[] globalChannel1, double[] globalChannel2, double[] currentChannel1, double[] currentChannel2, int totalPacket, int currentPacket)
        {
            ScopeReadDataCompleted?.Invoke(this, new ScopeReadDataCompletedEventArgs(globalChannel1, globalChannel2, currentChannel1, currentChannel2, totalPacket, currentPacket));
            
        }

        #endregion

        public static uint CaculateEvtNum(int readCount)
        {
            if (readCount <= 131072)
            {
                return 1u;
            }
            else if (readCount <= 524288)
            {
                return 4u;
            }
            else if (readCount <= 1048576)
            {
                return 16u;
            }
            else if (readCount <= 2097152)
            {
                return 32u;
            }
            else
            {
                return 64u;
            }
        }

        private bool _shouldStop = false;

        /// <summary>
        /// 连续采集中标志
        /// </summary>
        public bool IsSerialSampling { get; set; } = false;

        /// <summary>
        /// 停止采集线程
        /// </summary>
        public void StopSampleThread()
        {
            _shouldStop = true;

        }

        /// <summary>
        /// 开始连续采集
        /// </summary>
        public void StartSerialSample(int serialSampleTime)
        {
            new Thread(() =>
            {
                lock (lockObject)
                {
                    _shouldStop = false;
                    SampleTime = serialSampleTime;
                    IsSerialSampling = true;

                    while (!_shouldStop)
                    {
                        //开始AD采集
                        MyDLLimport.USBCtrlTransSimple((Int32)0x33);

                        int eventTimeout = 2000;
                        double[] channelData1 = new double[sampleCount];
                        double[] channelData2 = new double[sampleCount];
                        uint eventNumber = CaculateEvtNum((int)sampleCount * 2);

                        //单次包的数据长度
                        uint packetDataLength = sampleCount / eventNumber;

                        //获取数据
                        int res = MyDLLimport.AiReadBulkData((int)sampleCount * 2, eventNumber, eventTimeout, g_pBuffer);

                        while (!_shouldStop)
                        {
                            //获取当前的事件
                            int currentEventID = MyDLLimport.EventCheck(eventTimeout);

                            if ((currentEventID == 1365) || (currentEventID == -1))
                            {
                                break;
                            }

                            if (packetDataLength < invalidDataCount)
                            {
                                throw new Exception("数据异常");
                            }

                            double[] currentChannel1 = new double[packetDataLength];
                            double[] currentChannel2 = new double[packetDataLength];

                            unsafe
                            {
                                byte* pData = (byte*)g_pBuffer;

                                if (currentEventID == 0)
                                {
                                    for (uint i = invalidDataCount; i < packetDataLength; i++)
                                    {
                                        currentChannel1[i] = (*(pData + i * 2) - currentCHAZero) * currentCHAScale;
                                        currentChannel2[i] = (*(pData + i * 2 + 1) - currentCHBZero) * currentCHBScale;

                                        channelData1[i] = currentChannel1[i];
                                        channelData2[i] = currentChannel2[i];
                                    }
                                }
                                else
                                {
                                    for (uint i = 0; i < packetDataLength; i++)
                                    {
                                        currentChannel1[i - 0] = (*(pData + (packetDataLength * currentEventID + i) * 2) - currentCHAZero) * currentCHAScale;
                                        currentChannel2[i - 0] = (*(pData + (packetDataLength * currentEventID + i) * 2 + 1) - currentCHBZero) * currentCHBScale;

                                        channelData1[packetDataLength * currentEventID + i] = currentChannel1[i];
                                        channelData2[packetDataLength * currentEventID + i] = currentChannel2[i];
                                    }
                                }
                            }

                            OnScopeReadDataCompleted(channelData1, channelData2, currentChannel1, currentChannel2, (int)eventNumber, currentEventID + 1);

                            if (currentEventID == (eventNumber - 1))
                            {
                                break;
                            }
                        }

                    }
                    IsSerialSampling = false;
                }
            }).Start();

        }

        #endregion

        #region 属性

        private EScale chaScale;

        /// <summary>
        /// CHA档位
        /// </summary>
        public EScale CHAScale
        {
            get
            {
                return chaScale;
            }
            set
            {
                chaScale = value;

                switch (CHAVoltageDIV)
                {
                    case EVoltageDIV.DIV_250MV:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_250mV_x1_Zero : CalibrationData.CHA_250mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_250mV_x1_Scale : CalibrationData.CHA_250mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_500MV:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_500mV_x1_Zero : CalibrationData.CHA_500mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_500mV_x1_Scale : CalibrationData.CHA_500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_1V:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_1000mV_x1_Zero : CalibrationData.CHA_1000mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_1000mV_x1_Scale : CalibrationData.CHA_1000mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_2V5:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_2500mV_x1_Zero : CalibrationData.CHA_2500mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_2500mV_x1_Scale : CalibrationData.CHA_2500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_5V:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_5000mV_x1_Zero : CalibrationData.CHA_5000mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_5000mV_x1_Scale : CalibrationData.CHA_5000mV_x10_Scale;
                        break;
                    default:
                        break;
                }
            }
        }

        private EScale chbScale = EScale.x1;

        /// <summary>
        /// CHB档位
        /// </summary>
        public EScale CHBScale
        {
            get
            {
                return chbScale;
            }
            set
            {
                chbScale = value;

                switch (CHBVoltageDIV)
                {
                    case EVoltageDIV.DIV_250MV:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_250mV_x1_Zero : CalibrationData.CHB_250mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_250mV_x1_Scale : CalibrationData.CHB_250mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_500MV:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_500mV_x1_Zero : CalibrationData.CHB_500mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_500mV_x1_Scale : CalibrationData.CHB_500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_1V:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_1000mV_x1_Zero : CalibrationData.CHB_1000mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_1000mV_x1_Scale : CalibrationData.CHB_1000mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_2V5:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_2500mV_x1_Zero : CalibrationData.CHB_2500mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_2500mV_x1_Scale : CalibrationData.CHB_2500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_5V:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_5000mV_x1_Zero : CalibrationData.CHB_5000mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_5000mV_x1_Scale : CalibrationData.CHB_5000mV_x10_Scale;
                        break;
                    default:
                        break;
                }
            }
        }

        private ECoupling chaCoupling = ECoupling.DC;

        /// <summary>
        /// CHA耦合
        /// </summary>
        public ECoupling CHACoupling
        {
            get
            {
                return chaCoupling;
            }
            set
            {
                chaCoupling = value;
                SetCoupling(EChannel.CHA, value);
            }
        }

        private ECoupling chbCoupling = ECoupling.DC;

        /// <summary>
        /// CHB耦合
        /// </summary>
        public ECoupling CHBCoupling
        {
            get
            {
                return chbCoupling;
            }
            set
            {
                chbCoupling = value;
                SetCoupling(EChannel.CHB, value);
            }
        }

        private ESampleRate sampleRate = ESampleRate.Sps_49K;

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate
        {
            get
            {
                return sampleRate;
            }
            set
            {
                sampleRate = value;
                SetSampleRate(value);

                sampleCount = (uint)((uint)SampleRate * (SampleTime + 50) / 1000);
                invalidDataCount = (uint)((uint)SampleRate * 50 / 1000);

                lock (lockObject)
                {
                    //设置数据缓冲区
                    MyDLLimport.SetInfo(1, 0, 0x11, 0, 0, sampleCount * 2);//设置使用的缓冲区字节数
                }

            }
        }

        private ETriggerModel triggerModel = ETriggerModel.No;

        /// <summary>
        /// 触发模式
        /// </summary>
        public ETriggerModel TriggerModel
        {
            get
            {
                return triggerModel;
            }
            set
            {
                triggerModel = value;
                SetTriggerModel(value);
            }
        }

        private ETriggerEdge triggerEdge = ETriggerEdge.Filling;

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerEdge TriggerEdge
        {
            get
            {
                return triggerEdge;
            }
            set
            {
                triggerEdge = value;
                SetTriggerEdge(value);
            }
        }

        private EVoltageDIV chaVoltageDIV = EVoltageDIV.DIV_5V;

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public EVoltageDIV CHAVoltageDIV
        {
            get
            {
                return chaVoltageDIV;
            }
            set
            {
                chaVoltageDIV = value;
                SetVoltageDIV(EChannel.CHA, value);

                switch (value)
                {
                    case EVoltageDIV.DIV_250MV:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_250mV_x1_Zero : CalibrationData.CHA_250mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_250mV_x1_Scale : CalibrationData.CHA_250mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_500MV:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_500mV_x1_Zero : CalibrationData.CHA_500mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_500mV_x1_Scale : CalibrationData.CHA_500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_1V:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_1000mV_x1_Zero : CalibrationData.CHA_1000mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_1000mV_x1_Scale : CalibrationData.CHA_1000mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_2V5:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_2500mV_x1_Zero : CalibrationData.CHA_2500mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_2500mV_x1_Scale : CalibrationData.CHA_2500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_5V:
                        currentCHAZero = (CHAScale == EScale.x1) ? CalibrationData.CHA_5000mV_x1_Zero : CalibrationData.CHA_5000mV_x10_Zero;
                        currentCHAScale = (CHAScale == EScale.x1) ? CalibrationData.CHA_5000mV_x1_Scale : CalibrationData.CHA_5000mV_x10_Scale;
                        break;
                    default:
                        break;
                }
            }
        }

        private EVoltageDIV chbVoltageDIV = EVoltageDIV.DIV_5V;

        /// <summary>
        /// CHB电压档位
        /// </summary>
        public EVoltageDIV CHBVoltageDIV
        {
            get
            {
                return chbVoltageDIV;
            }
            set
            {
                chbVoltageDIV = value;
                SetVoltageDIV(EChannel.CHB, value);

                switch (value)
                {
                    case EVoltageDIV.DIV_250MV:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_250mV_x1_Zero : CalibrationData.CHB_250mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_250mV_x1_Scale : CalibrationData.CHB_250mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_500MV:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_500mV_x1_Zero : CalibrationData.CHB_500mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_500mV_x1_Scale : CalibrationData.CHB_500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_1V:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_1000mV_x1_Zero : CalibrationData.CHB_1000mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_1000mV_x1_Scale : CalibrationData.CHB_1000mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_2V5:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_2500mV_x1_Zero : CalibrationData.CHB_2500mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_2500mV_x1_Scale : CalibrationData.CHB_2500mV_x10_Scale;
                        break;
                    case EVoltageDIV.DIV_5V:
                        currentCHBZero = (CHBScale == EScale.x1) ? CalibrationData.CHB_5000mV_x1_Zero : CalibrationData.CHB_5000mV_x10_Zero;
                        currentCHBScale = (CHBScale == EScale.x1) ? CalibrationData.CHB_5000mV_x1_Scale : CalibrationData.CHB_5000mV_x10_Scale;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 使能CHA通道
        /// </summary>
        public bool IsCHAEnable
        {
            get
            {
                return true;
            }
            set
            {

            }
        }

        private bool isCHBEnable;

        /// <summary>
        /// 使能CHB通道
        /// </summary>
        public bool IsCHBEnable
        {
            get
            {
                return isCHBEnable;
            }
            set
            {
                isCHBEnable = value;
                EnableCHB(value);
            }
        }

        #endregion
    }
}
