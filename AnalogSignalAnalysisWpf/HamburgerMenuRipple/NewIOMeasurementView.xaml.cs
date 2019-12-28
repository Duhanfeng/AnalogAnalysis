using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using PixelFormat = System.Drawing.Imaging.PixelFormat;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((DataContext != null) && (DataContext is NewIOMeasurementViewModel))
            {
                var model = DataContext as NewIOMeasurementViewModel;
                model.Compared += Model_Compared;
            }
        }

        private void Model_Compared(object sender, EventArgs e)
        {
            if (!Directory.Exists("IORecord"))
            {
                Directory.CreateDirectory("IORecord");
            }

            Dispatcher.Invoke(new Action(() =>
            {
                var bmp = ToBitmapTool.ToBitmap(SparrowChart);
                bmp.Save($"IORecord/{DateTime.Now.ToString("yyyy-MM-dd HHmmss")}.bmp");
            }));

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        /// <summary>
        /// 导入模板文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.DefaultExt = ".json";
            ofd.Filter = "json file|*.json";

            if (ofd.ShowDialog() == true)
            {
                //此处做你想做的事 ...=ofd.SafeFileName; 
                var model = DataContext as NewIOMeasurementViewModel;
                model.ImportTemplate(ofd.FileName);
            }

        }

        /// <summary>
        /// 导出模板文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            //创建一个保存文件式的对话框  
            var sfd = new Microsoft.Win32.SaveFileDialog();

            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "json file|*.json";
            sfd.FileName = "";

            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == true)
            {
                //此处做你想做的事 ...=sfd.FileName; 

                var model = DataContext as NewIOMeasurementViewModel;
                model.ImportTemplate(sfd.FileName);
            }

        }
    }


    public static class ToBitmapTool
    {
        /// <summary>
        /// 截图转换成bitmap
        /// </summary>
        /// <param name="element"></param>
        /// <param name="width">默认控件宽度</param>
        /// <param name="height">默认控件高度</param>
        /// <param name="x">默认0</param>
        /// <param name="y">默认0</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this FrameworkElement element, int width = 0, int height = 0, int x = 0, int y = 0)
        {
            if (width == 0) width = (int)element.ActualWidth;
            if (height == 0) height = (int)element.ActualHeight;

            var rtb = new RenderTargetBitmap(width, height, x, y, System.Windows.Media.PixelFormats.Default);
            rtb.Render(element);
            var bit = BitmapSourceToBitmap(rtb);

            //测试代码
            DirectoryInfo d = new DirectoryInfo(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Cache"));
            if (!d.Exists) d.Create();
            bit.Save(System.IO.Path.Combine(d.FullName, "控件截图.png"));

            return bit;
        }

        /// <summary>
        /// BitmapSource转Bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap BitmapSourceToBitmap(this BitmapSource source)
        {
            return BitmapSourceToBitmap(source, source.PixelWidth, source.PixelHeight);
        }

        /// <summary>
        /// Convert BitmapSource to Bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap BitmapSourceToBitmap(this BitmapSource source, int width, int height)
        {
            Bitmap bmp = null;
            try
            {
                System.Drawing.Imaging.PixelFormat format = PixelFormat.Format24bppRgb;
                /*set the translate type according to the in param(source)*/
                switch (source.Format.ToString())
                {
                    case "Rgb24":
                    case "Bgr24": format = PixelFormat.Format24bppRgb; break;
                    case "Bgra32": format = System.Drawing.Imaging.PixelFormat.Format32bppPArgb; break;
                    case "Bgr32": format = PixelFormat.Format32bppRgb; break;
                    case "Pbgra32": format = PixelFormat.Format32bppArgb; break;
                }
                bmp = new Bitmap(width, height, format);
                BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                    ImageLockMode.WriteOnly,
                    format);
                source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                bmp.UnlockBits(data);
            }
            catch
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                    bmp = null;
                }
            }

            return bmp;
        }
    }
}
