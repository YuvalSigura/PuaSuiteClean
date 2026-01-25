using System.Windows.Controls;
using System.Windows;

namespace PuaSuiteClean.Pages
{
    public partial class ScannerPage : Page
    {
        public ScannerPage()
        {
            InitializeComponent();
        }

        private void StartScan_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = "Starting scan...\n";
            OutputBox.AppendText("Scanner engine not implemented yet.\n");
        }
    }
}
