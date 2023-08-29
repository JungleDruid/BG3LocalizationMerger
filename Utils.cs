using BG3LocalizationMerger.Resources.Strings;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;

namespace BG3LocalizationMerger
{
    internal static class Utils
    {
        public static async Task ShowErrorMessage(Window owner, string text, string? title = null)
        {
            await owner.Dispatcher.Invoke(() =>
            {
                ContentDialog dialog =
                    new()
                    {
                        Title = title ?? Strings.Error,
                        Content = text,
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = Strings.OKButton
                    };
                return dialog.ShowAsync();
            });
        }

        public static Task<bool> ShowYesNoMessage(Window owner, string text, string? title = null)
        {
            return owner.Dispatcher.Invoke(async () =>
            {
                ContentDialog dialog =
                    new()
                    {
                        Title = title ?? Strings.Confirmation,
                        Content = text,
                        PrimaryButtonText = Strings.YesButton,
                        CloseButtonText = Strings.NoButton,
                    };
                return await dialog.ShowAsync() == ContentDialogResult.Primary;
            });
        }
    }
}
