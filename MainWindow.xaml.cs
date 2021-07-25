using System.Diagnostics;
using System.Windows;

namespace FastbootEnhance
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow THIS;
        const string version = "1.1.1";
        public MainWindow()
        {
            InitializeComponent();
            THIS = this;
            PayloadUI.init();
            FastbootUI.init();

            Title += " v" + version;

            Closed += delegate
            {
                Process.GetCurrentProcess().Kill();
            };
        }

        private void Thread_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.akr-developers.com/d/506");
        }

        private void OSS_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/xzr467706992/FastbootEnhance");
        }
    }
}
