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
    /// FavortiesDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FavortiesDialog : Window
    {
        static FavoriteCollection FavCollection;

        static FavortiesDialog()
        {
            FavCollection = Application.Current.Resources["FavoriteCollection"] as FavoriteCollection;
        }

        public FavoriteItem SelectedItem
        {
            get
            {
                return FavoriteListView.SelectedItem as FavoriteItem;
            }
        }

        public FavortiesDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = FavoriteListView.SelectedItem != null;
        }

        private void FavoriteListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = FavoriteListView.SelectedItem != null;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            FavoriteItem item = FavoriteListView.SelectedItem as FavoriteItem;

            if(item != null)
            {
                FavCollection.Remove(item);
            }
        }
    }
}
