using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FastbootEnhance
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow THIS;
        const string version = "1.2.0";
        public MainWindow()
        {
            InitializeComponent();
            THIS = this;

            try
            {
                new DirectoryInfo(Payload.PAYLOAD_TMP).Delete(true);
            }
            catch (DirectoryNotFoundException) { }

            PayloadUI.init();
            FastbootUI.init();

            Title += " v" + version;

            Closed += delegate
            {
                if (PayloadUI.payload != null)
                    PayloadUI.payload.Dispose();
                try
                {
                    new DirectoryInfo(Payload.PAYLOAD_TMP).Delete(true);
                }
                catch (DirectoryNotFoundException) { }
                Process.GetCurrentProcess().Kill();
            };
        }

        private void Thread_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.akr-developers.com/d/506");
        }

        private void OSS_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/libxzr/FastbootEnhance");
        }
    }
}
