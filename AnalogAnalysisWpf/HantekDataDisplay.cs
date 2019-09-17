using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalogAnalysis
{
    public static class HantekDataDisplay
    {
        [DllImport("HTDisplayDll.dll")]
        public static extern void HTDrawGrid(
                IntPtr hDC, //绘图对象的句柄
                int nLeft,//绘图范围的 left 坐标
                int nTop, //绘图范围的 top 坐标
                int nRight, //绘图范围的 right 坐标
                int nBottom, //绘图范围的 bottom 坐标
                ushort nHoriGridNum, //水平绘制大格的个数，
                ushort nVertGridNum,//垂直绘制大格的个数
                ushort nBright,//绘制图象的亮度
                ushort IsGrid //是否绘制网格刻度
                );


        [DllImport("HTDisplayDll.dll")]
        public static extern void HTDrawWaveInYTVB(IntPtr hDC, int left, int top, int right, int bottom, ushort R, ushort G,
                                      ushort B, ushort nDisType, IntPtr pSrcData, uint nSrcDataLen, uint nDisDataLen, uint nCenterData,
                                      ushort nDisLeverPos, double dbHorizontal, double dbVertical, ushort nYTFormat, uint nScanLen);
    }
}
