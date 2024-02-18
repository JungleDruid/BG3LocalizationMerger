using BG3LocalizationMerger.Properties;
using BG3LocalizationMerger.Resources.Strings;
using ModernWpf.Controls.Primitives;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BG3LocalizationMerger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow? s_instance;

        public static MainWindow Instance => s_instance!;

        public MainWindow()
        {
            s_instance = this;
            UpgradeSettings();
            InitializeComponent();

            var props = Settings.Default;
            UnpackedDataTextBox.Text = props.UnpackedDataPath;
            LanguagePackTextBox.Text = props.LanguagePackPath;
            ReferencePackTextBox.Text = props.ReferencePackPath;
            ExportPathTextBox.Text = props.ExportPath;

            InitCulture();

            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version != null)
            {
                Title = $"{Title} v{version}";
            }
        }

        private static void UpgradeSettings()
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version != null)
            {
                if (Settings.Default.SettingsVersion != version.ToString())
                {
                    Settings.Default.Upgrade();
                    Settings.Default.SettingsVersion = version.ToString();
                    Settings.Default.Save();
                }
            }
        }

        private void InitCulture()
        {
            LanguageComboBox.ItemsSource = TranslationSource.GetAllCultures();

            string defaultCultureName = Settings.Default.CultureName;
            var defaultCulture = string.IsNullOrEmpty(defaultCultureName)
                ? CultureInfo.CurrentUICulture
                : new CultureInfo(defaultCultureName);

            foreach (CultureInfo culture in LanguageComboBox.ItemsSource)
            {
                if (culture.Equals(defaultCulture))
                {
                    LanguageComboBox.SelectedItem = culture;
                    return;
                }
            }

            foreach (CultureInfo culture in LanguageComboBox.ItemsSource)
            {
                if (culture.Equals(defaultCulture.Parent))
                {
                    LanguageComboBox.SelectedItem = culture;
                    return;
                }
            }

            foreach (CultureInfo culture in LanguageComboBox.ItemsSource)
            {
                if (culture.TwoLetterISOLanguageName == defaultCulture.TwoLetterISOLanguageName)
                {
                    LanguageComboBox.SelectedItem = culture;
                    return;
                }
            }

            LanguageComboBox.SelectedIndex = 0;
        }

        public static void Log(string text)
        {
            s_instance!.Dispatcher.Invoke(() =>
            {
                var textbox = s_instance.LogTextBox;
                text = $"[{DateTime.Now.ToLongTimeString()}] {text}";
                if (!string.IsNullOrEmpty(textbox.Text))
                    text = '\n' + text;
                textbox.Text += text;
                textbox.ScrollToEnd();
            });
        }

        private async void Merge_Click(object sender, RoutedEventArgs e)
        {
            if (!await VerifyTextBoxes(true))
                return;
            SaveSettings();

            await RunMerge(PackageManager.Instance.Merge);
        }

        private Task<bool> VerifyTextBoxes(bool showWarning, bool checkUnpackedData = true)
        {
            async Task<bool> ShowError(string text)
            {
                if (showWarning)
                    await Utils.ShowErrorMessage(this, text);
                return false;
            }

            if (checkUnpackedData)
            {
                var unpackData = UnpackedDataTextBox.Text;
                if (!Directory.Exists(unpackData))
                {
                    return ShowError(string.Format(Strings.NotExistMessage, Strings.UnpackedData));
                }
            }

            var languagePack = LanguagePackTextBox.Text;
            if (!File.Exists(languagePack))
            {
                return ShowError(string.Format(Strings.NotExistMessage, Strings.LanguagePack));
            }

            var referencePack = ReferencePackTextBox.Text;
            if (!File.Exists(referencePack))
            {
                return ShowError(string.Format(Strings.NotExistMessage, Strings.ReferencePack));
            }

            var exportPath = ExportPathTextBox.Text;
            if (
                !Path.IsPathFullyQualified(exportPath)
                || !Directory.Exists(Path.GetDirectoryName(exportPath))
            )
            {
                return ShowError(string.Format(Strings.InvalidMessage, Strings.ExportPath));
            }

            return Task.FromResult(true);
        }

        private void SetControlsVisible(
            DependencyObject parent,
            bool value,
            params Control[] exception
        )
        {
            var controls = Enumerable
                .Empty<Control>()
                .Concat(FindVisualChildren<Button>(parent))
                .Concat(FindVisualChildren<TextBox>(parent))
                .Concat(FindVisualChildren<CheckBox>(parent))
                .Where(x => x != LogTextBox && x is not TitleBarButton && !exception.Contains(x));

            foreach (var control in controls)
            {
                control.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SetControlsEnabled(
            DependencyObject parent,
            bool value,
            params Control[] exception
        )
        {
            var controls = Enumerable
                .Empty<Control>()
                .Concat(FindVisualChildren<Button>(parent))
                .Concat(FindVisualChildren<TextBox>(parent))
                .Concat(FindVisualChildren<CheckBox>(parent))
                .Where(x => x != LogTextBox && x is not TitleBarButton && !exception.Contains(x));

            foreach (var control in controls)
            {
                control.IsEnabled = value;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null)
                yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null)
                    continue;
                if (ithChild is T t)
                    yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild))
                    yield return childOfChild;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (await VerifyTextBoxes(false, false))
                SaveSettings();
        }

        private void SaveSettings()
        {
            var props = Settings.Default;
            props.UnpackedDataPath = UnpackedDataTextBox.Text;
            props.LanguagePackPath = LanguagePackTextBox.Text;
            props.ReferencePackPath = ReferencePackTextBox.Text;
            props.ExportPath = ExportPathTextBox.Text;
            props.Save();
        }

        private static readonly Lazy<Dictionary<Button, TextBox>> s_textBoxMap =
            new(
                () =>
                    new()
                    {
                        [s_instance!.LanguagePackBrowseButton] = s_instance.LanguagePackTextBox,
                        [s_instance!.ReferencePackBrowseButton] = s_instance.ReferencePackTextBox,
                        [s_instance!.ExportPathBrowseButton] = s_instance.ExportPathTextBox
                    }
            );

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var map = s_textBoxMap.Value;
            var button = (Button)sender;
            var textbox = map[button];
            VistaFileDialog dialog;

#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (button.Name)
            {
                case "LanguagePackBrowseButton":
                case "ReferencePackBrowseButton":
                    dialog = new VistaOpenFileDialog
                    {
                        DefaultExt = "pak",
                        Filter = "Package File|*.pak",
                    };
                    break;
                case "ExportPathBrowseButton":
                    dialog = new VistaSaveFileDialog
                    {
                        DefaultExt = "pak",
                        Filter = "Package File|*.pak",
                        OverwritePrompt = false
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }
#pragma warning restore IDE0066 // Convert switch statement to expression

            dialog.FileName = textbox.Text;
            if (dialog.ShowDialog() == true)
            {
                textbox.Text = dialog.FileName;
            }
        }

        private void BrowseUnpackedData(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = string.IsNullOrEmpty(UnpackedDataTextBox.Text)
                    ? ""
                    : UnpackedDataTextBox.Text + "\\"
            };
            if (dialog.ShowDialog() == true)
            {
                UnpackedDataTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SwapReference(object sender, RoutedEventArgs e)
        {
            (ReferencePackTextBox.Text, LanguagePackTextBox.Text) = (
                LanguagePackTextBox.Text,
                ReferencePackTextBox.Text
            );
        }

        private async void Merge_Unconditionally_Click(object sender, RoutedEventArgs e)
        {
            if (!await VerifyTextBoxes(true, false))
                return;
            if (
                !await Utils.ShowYesNoMessage(
                    this,
                    Strings.MergeUnconditionallyWarning,
                    Strings.MergeUnconditionallyTitle
                )
            )
                return;

            await RunMerge(PackageManager.Instance.MergeUnconditionally);
        }

        private async Task RunMerge(Func<CancellationToken, Task> action)
        {
            SaveSettings();
            CancelButton.Visibility = Visibility.Visible;
            SetControlsEnabled(this, false, CancelButton);
            SetControlsVisible(CancelButton.Parent, false, CancelButton);
            CancellationTokenSource source = new();
            void handler(object s, RoutedEventArgs e) => source.Cancel();
            CancelButton.Click += handler;
            try
            {
                var sw = Stopwatch.StartNew();
                await Task.Run(() => action.Invoke(source.Token));
                Log(string.Format(Strings.TotalElapsedMessage, sw.Elapsed.TotalSeconds));
            }
            catch (OperationCanceledException)
            {
                Log(Strings.CanceledMessage);
            }
            finally
            {
                GC.Collect(2, GCCollectionMode.Aggressive, true, true);
                CancelButton.Click -= handler;
                CancelButton.Visibility = Visibility.Collapsed;
                SetControlsVisible(CancelButton.Parent, true, CancelButton);
                SetControlsEnabled(this, true);
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is CultureInfo culture)
            {
                TranslationSource.Instance.Culture = culture;
                Settings.Default.CultureName = culture.Name;
                Settings.Default.Save();
            }
        }
    }
}
