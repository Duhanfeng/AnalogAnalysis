using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf.Hantek66022BE
{
    /// <summary>
    /// Hantek66022BE 硬件控制接口
    /// </summary>
    static class HardwareControl
    {
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <returns>0-没有连接 1-连接成功</returns>
        [DllImport("HTMarch.dll")]
        public static extern Int16 dsoOpenDevice(UInt16 index);

        /// <summary>
        /// 设置设备的电压档位
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <param name="channel">通道索引,0-CH1,1-CH2</param>
        /// <param name="voltDIV">电压档位索引值
        /// 0-20mV/DIV
        /// 1-50mV/DIV
        /// 2-100mV/DIV
        /// 3-200mV/DIV
        /// 4-500mV/DIV
        /// 5-1V/DIV
        /// 6-2V/DIV
        /// 7-5V/DIV
        /// </param>
        /// <returns>0-失败 1-成功</returns>
        [DllImport("HTMarch.dll")]
        public static extern short dsoSetVoltDIV(ushort index, int channel, int voltDIV);

        /// <summary>
        /// 设置设备的采样档位
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <param name="timeDIV">时间档位索引值
        /// 0~10-40MSa/s
        /// 11-16MSa/s
        /// 12-8MSa/s
        /// 13-4MSa/s
        /// 14~24-1MSa/s
        /// 25-500KSa/s
        /// 26-200KSa/s
        /// 27-100KSa/s
        /// </param>
        /// <returns>0-失败 1-成功</returns>
        [DllImport("HTMarch.dll")]
        public static extern short dsoSetTimeDIV(ushort index, int timeDIV);

        /// <summary>
        /// 读取硬件数据
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <param name="channel1">CH1的数据缓冲区</param>
        /// <param name="channel2">CH2的数据缓冲区</param>
        /// <param name="readLength">要读取数据的长度</param>
        /// <param name="pCalLevel">校对电平</param>
        /// <param name="nCH1VoltDIV">CH1的电压档位</param>
        /// <param name="nCH2VoltDIV">CH2的电压档位</param>
        /// <param name="nTrigSweep">扫频模式 0-auto 1-正常 2-单次</param>
        /// <param name="nTrigSrc">触发源 0-CH1 1-CH2</param>
        /// <param name="nTrigLevel">触发电平 0~255</param>
        /// <param name="nSlope">触发沿方式 0-上升沿 1-下降沿</param>
        /// <param name="timeDIV">采样率档位</param>
        /// <param name="nHTrigPos">水平触发位置</param>
        /// <param name="nDisLen">显示的数据长度</param>
        /// <param name="trigPointIndex">返回的触发点的索引值</param>
        /// <param name="nInsertMode">差值方式 0-Step差值 1-Line差值 2-SinX/X差值</param>
        /// <returns>0-失败 1-成功</returns>
        [DllImport("HTMarch.dll")]
        public static extern short dsoReadHardData(ushort index, IntPtr channel1, IntPtr channel2, uint readLength, IntPtr pCalLevel, int nCH1VoltDIV, int nCH2VoltDIV, short nTrigSweep, short nTrigSrc, short nTrigLevel, short nSlope, int timeDIV, short nHTrigPos, uint nDisLen, ref UInt32 trigPointIndex, short nInsertMode);

        /// <summary>
        /// 获取校正电平
        /// </summary>
        /// <param name="index">设备索引</param>
        /// <param name="level">校正数据缓冲区</param>
        /// <param name="length">缓冲区长度(为32)</param>
        /// <returns>0-失败 非0-成功</returns>
        [DllImport("HTMarch.dll")]
        public static extern ushort dsoGetCalLevel(ushort index, IntPtr level, short length = 32);

        ///// <summary>
        ///// 设置校正电平
        ///// </summary>
        ///// <param name="index"></param>
        ///// <param name="level"></param>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //[DllImport("HTMarch.dll")]
        //public static extern ushort dsoSetCalLevel(ushort index, IntPtr level, short length);

        ///// <summary>
        ///// 校正
        ///// </summary>
        ///// <param name="index"></param>
        ///// <param name="timeDIV"></param>
        ///// <param name="nCH1VoltDIV"></param>
        ///// <param name="nCH2VoltDIV"></param>
        ///// <param name="pCalLevel"></param>
        ///// <returns></returns>
        //[DllImport("HTMarch.dll")]
        //public static extern short dsoCalibrate(ushort index, int timeDIV, int nCH1VoltDIV, int nCH2VoltDIV, IntPtr pCalLevel);



    }
}
