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
using System.Windows.Shapes;

namespace NZNZWPF
{
    /// <summary>
    /// SizeDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SizeDialog : Window
    {
        public double InputWidth
        {
            get
            {
                try
                {
                    return Convert.ToDouble(WidthBox.Text);
                }
                catch(Exception e)
                {
                    return 500;
                }
            }
        }

        public double InputHeight
        {
            get
            {
                try
                {
                    return Convert.ToDouble(HeightBox.Text);
                }
                catch(Exception e)
                {
                    return 500;
                }
            }
        }

        public bool IsAnd
        {
            get
            {
                return JoinBox.SelectedIndex == 0;
            }
        }

        public SizeDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
