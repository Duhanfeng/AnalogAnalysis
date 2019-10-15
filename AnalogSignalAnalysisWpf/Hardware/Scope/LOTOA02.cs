using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    public class LOTOA02
    {
        #region C++接口

        //-------------------声明动态库USBInterFace.dll的一些接口函数--------------------------------------------------------------------
        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        public static extern void SpecifyDevIdx(Int32 index);  //设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 DeviceOpen();     //打开设备，并准备资源

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 DeviceClose();     //关闭设备并释放资源

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        public static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize);//USB传输控制命令

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        public static extern Int32 USBCtrlTransSimple(Int32 Request);//USB传输控制命令



        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        public static extern IntPtr GetBuffer4Wr(Int32 index);//获取原始数据缓冲区首指针

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 AiReadBulkData(Int32 SampleCount, uint num, Int32 ulTimeout, IntPtr PBuffer);//批量读取原始数据块

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        public static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);

        //声明需要的变量
        public static byte g_CtrlByte0 = 0;//记录IO控制位
        public static byte g_CtrlByte1 = 0;//记录IO控制位

        public static IntPtr g_pBuffer = (IntPtr)0;
        public static byte[] g_chADataArray = new byte[RecvDataLenght]; //用来放通道A原始数据的数组。
        public static byte[] g_chBDataArray = new byte[RecvDataLenght]; //用来放通道B原始数据的数组。

        #endregion

        #region 参数配置


        enum Channel
        {
            CHA,
            CHB
        }

        private static void SetCoupling(Channel channel, int type)
        {
            if (channel == Channel.CHA)
            {

                if (type == 0)
                {
                    //DC耦合
                    g_CtrlByte0 &= 0xef;
                    g_CtrlByte0 |= 0x10;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                }
                else
                {
                    //AC耦合
                    g_CtrlByte0 &= 0xef;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                }
            }
            else
            {
                if (type == 0)
                {
                    //DC耦合
                    g_CtrlByte0 &= 0xef;
                    g_CtrlByte0 |= 0x10;
                    USBCtrlTrans(0x24, g_CtrlByte0, 1);
                }
                else
                {
                    //AC耦合
                    g_CtrlByte0 &= 0xef;
                    USBCtrlTrans(0x24, g_CtrlByte0, 1);
                }
            }

        }

        enum SampleRate
        {
            Sps_49K,
            Sps_781K,
            Sps_12M5,
            Sps_100M,
        }

        private static void SetSampleRate(SampleRate sampleRate)
        {
            switch (sampleRate)
            {
                case SampleRate.Sps_49K:
                    //设置49K Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x0e;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case SampleRate.Sps_781K:
                    //设置781K Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x0c;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case SampleRate.Sps_12M5:
                    // 设置12.5M Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x08;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case SampleRate.Sps_100M:
                    //设置100M Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x00;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                default:
                    break;
            }
        }

        enum TriggerModel
        {
            CHA,
            Ext,
            No,
        }

        private static void SetTriggerModel(TriggerModel triggerModel)
        {
            switch (triggerModel)
            {
                case TriggerModel.CHA:
                    USBCtrlTrans(0xE7, 0x01, 1);
                    break;
                case TriggerModel.Ext:
                    //g_TrigSourceChan = 2;
                    //通道EXT触发
                    USBCtrlTrans(0xE7, 0x01, 1);
                    break;
                case TriggerModel.No:
                    USBCtrlTrans(0xE7, 0x00, 1);
                    break;
                default:
                    break;
            }
        }

        enum TriggerEdge
        {
            Rising,
            Filling,
        }

        private static void SetTriggerEdge(TriggerEdge triggerEdge)
        {
            switch (triggerEdge)
            {
                case TriggerEdge.Rising:
                    USBCtrlTrans(0xC5, 0x00, 1);
                    break;
                case TriggerEdge.Filling:
                    USBCtrlTrans(0xC5, 0x01, 1);
                    break;
                default:
                    break;
            }
        }

        private static void SetTriggerLevel(byte level)
        {
            //设置触发数据
            USBCtrlTrans(0x16, level, 1);
        }

        /// <summary>
        /// 电压档位
        /// </summary>
        enum VoltageDIV
        {
            [Description("250mV/DIV")]
            DIV_250MV,
            [Description("500mV/DIV")]
            DIV_500MV,
            [Description("1V/DIV")]
            DIV_1V,
            [Description("2.5V/DIV")]
            DIV_2V5,
            [Description("5V/DIV")]
            DIV_5V,
        }

        private static void SetVoltageDIV(int channel, VoltageDIV voltageDIV)
        {
            if (channel == 0)
            {
                switch (voltageDIV)
                {
                    case VoltageDIV.DIV_250MV:
                        g_CtrlByte1 &= 0xF7;
                        USBCtrlTrans(0x22, 0x04, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_500MV:
                        g_CtrlByte1 &= 0xF7;
                        USBCtrlTrans(0x22, 0x02, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_1V:
                        g_CtrlByte1 &= 0xF7;
                        g_CtrlByte1 |= 0x08;
                        USBCtrlTrans(0x22, 0x04, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_2V5:
                        g_CtrlByte1 &= 0xF7;
                        g_CtrlByte1 |= 0x08;
                        USBCtrlTrans(0x22, 0x02, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_5V:
                        g_CtrlByte1 &= 0xF7;
                        g_CtrlByte1 |= 0x08;
                        USBCtrlTrans(0x22, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (voltageDIV)
                {
                    case VoltageDIV.DIV_250MV:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x04;//放大4倍               
                        USBCtrlTrans(0x23, 0x40, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_500MV:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x02;//放大两倍
                        USBCtrlTrans(0x23, 0x40, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_1V:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x04;
                        USBCtrlTrans(0x23, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_2V5:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x02;
                        USBCtrlTrans(0x23, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case VoltageDIV.DIV_5V:
                        g_CtrlByte1 &= 0xF9;
                        USBCtrlTrans(0x23, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    default:
                        break;
                }

            }

        }

        /// <summary>
        /// 使能通道B
        /// </summary>
        /// <param name="isEnable"></param>
        private static void EnableCHB(bool isEnable)
        {
            if (isEnable)
            {
                g_CtrlByte1 &= 0xfe;
                g_CtrlByte1 |= 0x01;
                USBCtrlTrans(0x24, g_CtrlByte1, 1);
            }
            else
            {
                g_CtrlByte1 &= 0xfe;
                g_CtrlByte1 |= 0x00;
                USBCtrlTrans(0x24, g_CtrlByte1, 1);
            }
        }

        #endregion

        #region 示波器接口

        public static uint RecvDataLenght = 64 * 1024;

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
            try
            {
                IsConnect = false;

                //设置产品编号
                SpecifyDevIdx(6);

                //打开设备
                Int32 res = DeviceOpen();
                if (res != 0)
                {
                    return false;
                }

                //获取数据缓冲区首指针
                g_pBuffer = GetBuffer4Wr(-1);
                if (g_pBuffer == IntPtr.Zero)
                {
                    return false;
                }

                //设置数据缓冲区
                SetInfo(1, 0, 0x11, 0, 0, RecvDataLenght * 2);//设置使用的缓冲区为128K字节,即每个通道64K字节
                IsConnect = true;
            }
            catch (Exception)
            {
                IsConnect = false;
            }

            return IsConnect;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            DeviceClose();
            IsConnect = false;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        public void ReadDataBlock(int channelIndex, out double[] channelData)
        {
            throw new Exception();
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadDataBlock(out double[] channelData1, out double[] channelData2)
        {
            channelData1 = new double[0];
            channelData2 = new double[0];

            //开始AD采集
            USBCtrlTransSimple((Int32)0x33);

            //等待采集完成
            while (USBCtrlTransSimple((Int32)0x50) != 33)
            {
                Thread.Sleep(10);
            }

            //获取数据
            int res = AiReadBulkData((int)RecvDataLenght * 2, 1, 2000, g_pBuffer);
            if (res == 0)
            {
                unsafe
                {
                    byte* pData = (byte*)g_pBuffer;
                    for (int i = 0; i < RecvDataLenght; i++)
                    {
                        g_chADataArray[i] = *(pData + i * 2);     //通道A的数据在原始缓冲区里的标号是0，2，4，6，8...
                        g_chBDataArray[i] = *(pData + i * 2 + 1);  //通道B的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                }
            }

        }

        #endregion
    }
}
