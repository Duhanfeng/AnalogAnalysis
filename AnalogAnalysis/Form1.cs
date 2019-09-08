using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalogAnalysis
{
    public partial class Form1 : Form
    {
        private IntPtr calData = Marshal.AllocHGlobal(2 * 32);

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Hantek66022BE.dsoOpenDevice(0) != 0)
            {
                unsafe
                {
                    Hantek66022BE.dsoGetCalLevel(0, calData, 32);
                    Hantek66022BE.dsoSetVoltDIV(0, 0, 5);
                    Hantek66022BE.dsoSetVoltDIV(0, 1, 5);
                    Hantek66022BE.dsoSetTimeDIV(0, 14);

                    HantekDataDisplay.HTDrawGrid(DisplayPanel.Handle, 0, 0, DisplayPanel.Width, DisplayPanel.Height, 10, 10, 128, 1);
                }
            }
            else
            {
                MessageBox.Show("打开设备失败");
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {

        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            unsafe
            {
                uint dataLength = 10240;
                IntPtr channel1Data = Marshal.AllocHGlobal(2 * (int)dataLength);
                IntPtr channel2Data = Marshal.AllocHGlobal(2 * (int)dataLength);
                uint trigPointIndex = 0;

                if (Hantek66022BE.dsoReadHardData(0, channel1Data, channel2Data, dataLength, calData, 5, 5, 0, 0, 64, 0, 14, 50, (uint)dataLength, ref trigPointIndex, 0) != -1)
                {
                    //显示数据
                    ushort* channel1 = (ushort*)channel1Data.ToPointer();
                    ushort* channel2 = (ushort*)channel2Data.ToPointer();

                    HantekDataDisplay.HTDrawWaveInYTVB(DisplayPanel.Handle, 0, 0, DisplayPanel.Width, DisplayPanel.Height, 255, 0, 0, 1, channel1Data, dataLength, dataLength, dataLength / 2, 64, 1, 1, 0, 0);

                    DataChart.Series[0].Points.Clear();

                    for (int i = 0; i < dataLength; i++)
                    {
                        DataChart.Series[0].Points.AddY(channel1[i]*(2.0/65));
                        Console.WriteLine(channel1[i]);
                    }
                }
                else
                {
                    MessageBox.Show("读取数据失败!");
                }

                Marshal.FreeHGlobal(channel1Data);
                Marshal.FreeHGlobal(channel2Data);
            }

        }
    }
}
