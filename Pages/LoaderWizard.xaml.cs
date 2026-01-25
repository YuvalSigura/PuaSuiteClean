using System.Windows.Controls;

namespace PuaSuiteClean.Pages
{
    public partial class LoaderWizard : UserControl
    {
        public LoaderWizard()
        {
            InitializeComponent();
        }

        private void BtnRun_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TxtOutput.Text = "[SIMULATED] Loader execution start...\n";
            TxtOutput.Text += "Because this is a skeleton, no real execution performed.\n";
        }
    }
}

