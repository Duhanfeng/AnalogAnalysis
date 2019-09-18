using AnalogDataAnalysisWpf.Hantek66022BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalogDataAnalysisWpf
{
    /// <summary>
    /// DeviceConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceConfigView : UserControl
    {
        public DeviceConfigView(VirtualOscilloscope virtualOscilloscope)
        {
            InitializeComponent();

            var viewModel = new DeviceConfigViewModel(virtualOscilloscope);
            viewModel.MessageRaised += ViewModel_MessageRaised;
            DataContext = viewModel;
        }

        private void ViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            MessageBox.Show(e.Message);
        }
    }
}
