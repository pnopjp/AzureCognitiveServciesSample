using FaceApiClient.Views;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System.Windows;

namespace FaceApiClient
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private IUnityContainer Container { get; } = new UnityContainer();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(x => this.Container.Resolve(x));
            this.Container.Resolve<MainWindow>().Show();
        }
    }
}
