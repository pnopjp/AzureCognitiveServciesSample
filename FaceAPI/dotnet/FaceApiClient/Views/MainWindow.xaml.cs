using FaceApiClient.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace FaceApiClient.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddFaceButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var dl = new OpenFileDialog();
            dl.FilterIndex = 1;
            dl.Multiselect = false;
            dl.Filter = "画像ファイル(*.jpg, *.png)|*.jpg;*.png";
            bool? result = dl.ShowDialog();
            if (result == true)
            {
                var vm = this.DataContext as MainWindowViewModel;
                vm.AddPersonFace.Execute(dl.FileName);
            }
        }

        private void DetectButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var dl = new OpenFileDialog();
            dl.FilterIndex = 1;
            dl.Multiselect = false;
            dl.Filter = "画像ファイル(*.jpg, *.png)|*.jpg;*.png";
            bool? result = dl.ShowDialog();
            if (result == true)
            {
                var vm = this.DataContext as MainWindowViewModel;
                vm.DetectFaces.Execute(dl.FileName);
            }
        }
    }
}
