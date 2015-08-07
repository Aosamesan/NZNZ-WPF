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
    /// ImageFile.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageFile : Window
    {
        public string OriginFileName { get; private set; }
        public string OriginURL { get; private set; }

        public ImageFile()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(ImageItem item)
        {
            OriginFileName = item.FileName;
            OriginURL = item.URL;

            Binding nameBinding = new Binding();
            nameBinding.Source = item;
            nameBinding.Path = new PropertyPath("FileName");
            nameBinding.Mode = BindingMode.TwoWay;
            nameBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(NameBox, TextBox.TextProperty, nameBinding);

            Binding urlBinding = new Binding();
            urlBinding.Source = item;
            urlBinding.Path = new PropertyPath("URL");
            urlBinding.Mode = BindingMode.TwoWay;
            urlBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(URLBox, TextBox.TextProperty, urlBinding);

            return ShowDialog();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Text = OriginFileName;
            URLBox.Text = OriginURL;
            DialogResult = false;
        }
    }
}
