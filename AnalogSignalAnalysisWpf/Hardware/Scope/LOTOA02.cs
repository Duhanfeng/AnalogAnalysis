﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    public class LOTOA02 : IScopeBase
    {
        #region C++接口

        //-------------------声明动态库USBInterFace.dll的一些接口函数--------------------------------------------------------------------
        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SpecifyDevIdx")]
        private static extern void SpecifyDevIdx(Int32 index);  //设置产品编号，不同型号产品编号不同

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 DeviceOpen();     //打开设备，并准备资源

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 DeviceClose();     //关闭设备并释放资源

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTrans")]
        private static extern byte USBCtrlTrans(byte Request, UInt16 usValue, uint outBufSize);//USB传输控制命令

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "USBCtrlTransSimple")]
        private static extern Int32 USBCtrlTransSimple(Int32 Request);//USB传输控制命令


        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetBuffer4Wr")]
        private static extern IntPtr GetBuffer4Wr(Int32 index);//获取原始数据缓冲区首指针

        [DllImport("USBInterFace.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 AiReadBulkData(Int32 SampleCount, uint num, Int32 ulTimeout, IntPtr PBuffer);//批量读取原始数据块

        [DllImport("USBInterFace.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetInfo")]
        private static extern void SetInfo(double dataNumPerPixar, double currentSampleRate, byte ChannelMask, Int32 m_ZrroUniInt, uint BufferOffset, uint HWbufferSize);

        //声明需要的变量
        public static byte g_CtrlByte0 = 0;//记录IO控制位
        public static byte g_CtrlByte1 = 0;//记录IO控制位

        public static IntPtr g_pBuffer = (IntPtr)0;
        public static byte[] g_chADataArray = new byte[RecvDataLenght]; //用来放通道A原始数据的数组。
        public static byte[] g_chBDataArray = new byte[RecvDataLenght]; //用来放通道B原始数据的数组。

        #endregion

        #region 参数配置

        public enum EChannel
        {
            [Description("CHA")]
            CHA,
            [Description("CHB")]
            CHB
        }

        public enum ECoupling
        {
            [Description("直流耦合")]
            DC,
            [Description("交流耦合")]
            AC
        }

        public void SetCoupling(EChannel channel, ECoupling coupling)
        {
            if (channel == EChannel.CHA)
            {
                switch (coupling)
                {
                    case ECoupling.DC:
                        //DC耦合
                        g_CtrlByte0 &= 0xef;
                        g_CtrlByte0 |= 0x10;
                        USBCtrlTrans(0x94, g_CtrlByte0, 1);
                        break;
                    case ECoupling.AC:
                        //AC耦合
                        g_CtrlByte0 &= 0xef;
                        USBCtrlTrans(0x94, g_CtrlByte0, 1);
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
                        g_CtrlByte0 &= 0xef;
                        g_CtrlByte0 |= 0x10;
                        USBCtrlTrans(0x24, g_CtrlByte0, 1);
                        break;
                    case ECoupling.AC:
                        //AC耦合
                        g_CtrlByte0 &= 0xef;
                        USBCtrlTrans(0x24, g_CtrlByte0, 1);
                        break;
                    default:
                        break;
                }
            }

        }

        public enum ESampleRate
        {
            [Description("49K")]
            Sps_49K,
            [Description("781K")]
            Sps_781K,
            [Description("12.5M")]
            Sps_12M5,
            [Description("100M")]
            Sps_100M,
        }

        public void SetSampleRate(ESampleRate sampleRate)
        {
            switch (sampleRate)
            {
                case ESampleRate.Sps_49K:
                    //设置49K Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x0e;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case ESampleRate.Sps_781K:
                    //设置781K Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x0c;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case ESampleRate.Sps_12M5:
                    // 设置12.5M Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x08;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                case ESampleRate.Sps_100M:
                    //设置100M Hz 采样率
                    g_CtrlByte0 &= 0xf0;
                    g_CtrlByte0 |= 0x00;
                    USBCtrlTrans(0x94, g_CtrlByte0, 1);
                    break;
                default:
                    break;
            }
        }

        public enum ETriggerModel
        {
            [Description("CHA")]
            CHA,
            [Description("外触发")]
            Ext,
            [Description("无")]
            No,
        }

        public void SetTriggerModel(ETriggerModel triggerModel)
        {
            switch (triggerModel)
            {
                case ETriggerModel.CHA:
                    USBCtrlTrans(0xE7, 0x01, 1);
                    break;
                case ETriggerModel.Ext:
                    //g_TrigSourceChan = 2;
                    //通道EXT触发
                    USBCtrlTrans(0xE7, 0x01, 1);
                    break;
                case ETriggerModel.No:
                    USBCtrlTrans(0xE7, 0x00, 1);
                    break;
                default:
                    break;
            }
        }

        public enum ETriggerEdge
        {
            [Description("上升沿")]
            Rising,
            [Description("下降沿")]
            Filling,
        }

        public void SetTriggerEdge(ETriggerEdge triggerEdge)
        {
            switch (triggerEdge)
            {
                case ETriggerEdge.Rising:
                    USBCtrlTrans(0xC5, 0x00, 1);
                    break;
                case ETriggerEdge.Filling:
                    USBCtrlTrans(0xC5, 0x01, 1);
                    break;
                default:
                    break;
            }
        }

        public void SetTriggerLevel(byte level)
        {
            //设置触发数据
            USBCtrlTrans(0x16, level, 1);
        }

        /// <summary>
        /// 电压档位
        /// </summary>
        public enum EVoltageDIV
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

        public void SetVoltageDIV(EChannel channel, EVoltageDIV voltageDIV)
        {
            if (channel == EChannel.CHA)
            {
                switch (voltageDIV)
                {
                    case EVoltageDIV.DIV_250MV:
                        g_CtrlByte1 &= 0xF7;
                        USBCtrlTrans(0x22, 0x04, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_500MV:
                        g_CtrlByte1 &= 0xF7;
                        USBCtrlTrans(0x22, 0x02, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_1V:
                        g_CtrlByte1 &= 0xF7;
                        g_CtrlByte1 |= 0x08;
                        USBCtrlTrans(0x22, 0x04, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_2V5:
                        g_CtrlByte1 &= 0xF7;
                        g_CtrlByte1 |= 0x08;
                        USBCtrlTrans(0x22, 0x02, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_5V:
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
                    case EVoltageDIV.DIV_250MV:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x04;//放大4倍               
                        USBCtrlTrans(0x23, 0x40, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_500MV:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x02;//放大两倍
                        USBCtrlTrans(0x23, 0x40, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_1V:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x04;
                        USBCtrlTrans(0x23, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_2V5:
                        g_CtrlByte1 &= 0xF9;
                        g_CtrlByte1 |= 0x02;
                        USBCtrlTrans(0x23, 0x00, 1);
                        USBCtrlTrans(0x24, g_CtrlByte1, 1);
                        break;
                    case EVoltageDIV.DIV_5V:
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
        public void EnableCHB(bool isEnable)
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

        private static uint RecvDataLenght = 64 * 1024;

        /// <summary>
        /// 采集时长(MS)
        /// </summary>
        public int SampleTime { get; set; } = 200;

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
                        g_chADataArray[i] = *(pData + i * 2);       //通道A的数据在原始缓冲区里的标号是0，2，4，6，8...
                        g_chBDataArray[i] = *(pData + i * 2 + 1);   //通道B的数据在原始缓冲区里的标号是1，3，5，7，9...
                    }
                }
            }

        }

        #endregion

        #region 属性

        private ECoupling chaCoupling;

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

        private ECoupling chbCoupling;

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

        private ESampleRate sampleRate;

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
            }
        }

        private ETriggerModel triggerModel;

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

        private ETriggerEdge triggerEdge;

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

        private EVoltageDIV chaVoltageDIV;

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
            }
        }

        private EVoltageDIV chbVoltageDIV;

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
