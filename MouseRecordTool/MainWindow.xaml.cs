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

namespace MouseRecordTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 点集合
        /// </summary>
        List<Point> pointList = new List<Point>();


        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //startPoint = e.GetPosition(sender as Canvas);
            pointList.Clear();
            myCanvas.Children.Clear();
            (myCanvas as Canvas).Children.Add(coordLabel);

            Point point = e.GetPosition(myCanvas);
            coordLabel.SetValue(Canvas.LeftProperty, point.X + 5);
            coordLabel.SetValue(Canvas.TopProperty, point.Y - 15);
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 返回指针相对于Canvas的位置
                Point point = e.GetPosition(myCanvas);

                //获取控件的大小
                double xRatio = point.X / myCanvas.ActualWidth;
                double yRatio = point.Y / myCanvas.ActualHeight;

                //if (pointList.Count == 0)
                //{
                //    // 加入起始点
                //    pointList.Add(new Point(this.startPoint.X, this.startPoint.Y));
                //}
                //else
                //{
                //    // 加入移动过程中的point
                //    pointList.Add(point);
                //}

                pointList.Add(point);
                coordLabel.SetValue(Canvas.LeftProperty, point.X + 5);
                coordLabel.SetValue(Canvas.TopProperty, point.Y - 15);

                // 去重复点
                var disList = pointList.Distinct().ToList();
                var count = disList.Count(); // 总点数

                //if (point != this.startPoint && this.startPoint != null)
                {
                    var l = new Line();
                    //string color = (cboColor.SelectedItem as ComboBoxItem).Content as string;
                    string color = "红色";

                    if (color == "默认")
                    {
                        l.Stroke = Brushes.Black;
                    }
                    if (color == "红色")
                    {
                        l.Stroke = Brushes.Red;
                    }
                    if (color == "绿色")
                    {
                        l.Stroke = Brushes.Green;
                    }
                    l.StrokeThickness = 1;
                    if (count < 2)
                        return;
                    l.X1 = disList[count - 2].X;  // count-2  保证 line的起始点为点集合中的倒数第二个点。
                    l.Y1 = disList[count - 2].Y;
                    // 终点X,Y 为当前point的X,Y
                    l.X2 = point.X;
                    l.Y2 = point.Y;
                    myCanvas.Children.Add(l);
                }

                coordLabel.Text = $"{xRatio * 100:F2}%,{yRatio * 100:F2}%";
                //coordLabel.Foreground = Brushes.Green; ;
            }

        }

        private void myCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //pointList.Clear();
            var disList = pointList.Distinct().ToList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 电压
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public double Time { get; set; }

        //X轴改变
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Y轴改变
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
