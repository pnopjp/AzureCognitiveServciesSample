using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace FaceApiClient.Extensions
{
    public static class DialogService
    {
        public static async Task<MessageDialogResult> ShowMessage(
            string message, MessageDialogStyle dialogStyle = MessageDialogStyle.Affirmative)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            return await metroWindow.ShowMessageAsync(
                "Person Manager", message, dialogStyle, metroWindow.MetroDialogOptions);
        }
    }
}
