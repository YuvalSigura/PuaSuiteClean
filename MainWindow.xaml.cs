using System.Windows;
using PuaSuiteClean.Pages;

namespace PuaSuiteClean
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load Dashboard once
            RootHost.Content = new DashboardPage();
        }
    }
}
