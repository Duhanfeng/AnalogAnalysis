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

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// NewIOMeasurementView.xaml 的交互逻辑
    /// </summary>
    public partial class NewIOMeasurementView : UserControl
    {
        public NewIOMeasurementView()
        {
            InitializeComponent();
        }

        private void SelcetImportConfigFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "json file|*.json",
            };

            if (ofd.ShowDialog() == true)
            {
                var model = DataContext as NewIOMeasurementViewModel;
                model.ImportConfigFile(ofd.FileName);
            }
        }
    }
}
