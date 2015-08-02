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
    /// FavoriteAddDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FavoriteAddDialog : Window
    {
        public string FavTitle
        {
            get { return TitleBox.Text; }
            private set
            {
                TitleBox.Text = value;
            }
        }

        public string FavURL
        {
            get { return URLBox.Text; }
            private set
            {
                URLBox.Text = value;
            }
        }

        public FavoriteAddDialog()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(string title, string url)
        {
            FavTitle = title;
            FavURL = url;

            return ShowDialog();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult =
                (!string.IsNullOrWhiteSpace(TitleBox.Text)) && (!string.IsNullOrWhiteSpace(URLBox.Text));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
