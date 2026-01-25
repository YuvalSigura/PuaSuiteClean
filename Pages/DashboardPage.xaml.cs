using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using PuaSuiteClean.Logic;
using PuaSuiteClean.Logic.PE;
using PuaSuiteClean.Logic.Patching;
using PuaSuiteClean.Logic.AutoPatch;

namespace PuaSuiteClean.Pages
{
    public partial class DashboardPage : UserControl
    {
        private readonly PatchManagerV2 patchManager = new PatchManagerV2();
        private readonly LoaderBuilder loaderBuilder = new LoaderBuilder();
        private readonly UpdaterEngine updater = new UpdaterEngine();

        private AutoPatchEngineV2? autoPatchV2;

        private string? selectedFile = null;
        private LanguageType detectedType = LanguageType.Unknown;
        private string? lastLoaderPath = null;

        public DashboardPage()
        {
            InitializeComponent();
        }

        // 1) BROWSE
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                selectedFile = dlg.FileName;
                TxtTargetFile.Text = selectedFile;

                autoPatchV2 = new AutoPatchEngineV2(selectedFile);

                DetectFileType();
                LoadAvailablePatches();
            }
        }

        // 2) TYPE DETECTION
        private void DetectFileType()
        {
            if (string.IsNullOrWhiteSpace(selectedFile))
            {
                detectedType = LanguageType.Unknown;
                LblFileType.Text = "No file selected.";
                return;
            }

            detectedType = TargetInspector.Inspect(selectedFile);
            LblFileType.Text = $"Detected type: {detectedType}";
        }

        // 3) PATCH LIST (ONLY V2)
        private void LoadAvailablePatches()
        {
            PatchList.Items.Clear();

            string[] files = patchManager.GetAvailablePatches();

            if (files.Length == 0)
            {
                PatchList.Items.Add("No V2 patches found.");
                return;
            }

            foreach (string p in files)
                PatchList.Items.Add(p);
        }

        // 4) LOADER
        private void BtnBuildLoader_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFile == null)
            {
                MessageBox.Show("Select file first.");
                return;
            }

            lastLoaderPath = loaderBuilder.BuildLoader(selectedFile, detectedType);

            LblLoaderStatus.Text =
                string.IsNullOrWhiteSpace(lastLoaderPath)
                ? "Failed to build loader."
                : $"Loader built → {lastLoaderPath}";
        }

        private void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lastLoaderPath == null)
            {
                MessageBox.Show("Build loader first.");
                return;
            }

            if (updater.NeedsUpdate(lastLoaderPath))
            {
                updater.Update(lastLoaderPath);
                LblLoaderStatus.Text = "Loader updated.";
            }
            else
            {
                LblLoaderStatus.Text = "Loader already up-to-date.";
            }
        }

        private void BtnOpenLoaderFolder_Click(object sender, RoutedEventArgs e)
        {
            if (lastLoaderPath == null)
            {
                MessageBox.Show("Build loader first.");
                return;
            }

            FileUtils.OpenFolder(lastLoaderPath);
        }

        // 5) AUTOPATCH V2
        private void BtnAutoPatchV2_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFile == null)
            {
                MessageBox.Show("Select a file first.");
                return;
            }

            if (autoPatchV2 == null)
                autoPatchV2 = new AutoPatchEngineV2(selectedFile);

            var report = autoPatchV2.Run();

            Directory.CreateDirectory("logs");
            string path = Path.Combine("logs",
                "AutoPatchV2-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt");

            File.WriteAllText(path, report.BuildText());

            MessageBox.Show($"AutoPatch V2 finished.\nSaved:\n{path}");
        }

        // 6) APPLY PATCH V2
        private void BtnApplyPatchV2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("No file selected.");
                return;
            }

            PatchBatchRunnerV2.ApplyPatches(selectedFile);

            MessageBox.Show("Patch V2 applied.");
        }

        // Compatibility tools
        private void BtnScanPE_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFile == null)
            {
                MessageBox.Show("Select a PE file.");
                return;
            }

            var scan = new PEScanner().ScanFile(selectedFile);

            MessageBox.Show(
                $"Imports: {scan.Imports.Count}\n" +
                $"AntiDebug: {scan.AntiDebug.Count}\n" +
                $"License Hits: {scan.LicenseHits.Count}");
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFile == null)
            {
                MessageBox.Show("Select a file.");
                return;
            }

            string p = new BackupManager().CreateBackup(selectedFile);
            MessageBox.Show("Backup created:\n" + p);
        }

        private void BtnAnalyzeCrash_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFile == null)
            {
                MessageBox.Show("Select file first.");
                return;
            }

            string result = new CrashAnalyzer().AnalyzeCrash(selectedFile, "crash.log");
            MessageBox.Show(result);
        }
    }
}
