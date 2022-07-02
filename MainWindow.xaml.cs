using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace FastbootEnhance
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow THIS;
        const string version = "1.3.1";
        public MainWindow()
        {
            InitializeComponent();
            THIS = this;

            string mutexName = "FastbootEnhance";
            bool createdNew;
            Mutex singleInstanceWatcher = new Mutex(false, mutexName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show(Properties.Resources.program_already_running, Properties.Resources.error, MessageBoxButton.OK, MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }

            try
            {
                new DirectoryInfo(PayloadUI.PAYLOAD_TMP).Delete(true);
            }
            catch (DirectoryNotFoundException) { }

            try
            {
                new DirectoryInfo(FastbootUI.PAYLOAD_TMP).Delete(true);
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
                    new DirectoryInfo(PayloadUI.PAYLOAD_TMP).Delete(true);
                }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }

                try
                {
                    new DirectoryInfo(FastbootUI.PAYLOAD_TMP).Delete(true);
                }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }

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
