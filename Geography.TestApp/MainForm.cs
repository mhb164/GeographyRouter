using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Geography.TestApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ResetTitle();
        }

        string RepositoryDirectory;
        GeographyRepository Repository;
        GeographyRouter.GeoRouter Routings;

        public void InvokeIfNecessary(MethodInvoker action) { if (InvokeRequired) BeginInvoke(action); else { action(); } }
        private void Log(string message) => InvokeIfNecessary(() => LogBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}> {message}{Environment.NewLine}"));

        #region UI Event handlers
        private void ClearLogsButton_Click(object sender, EventArgs e) => InvokeIfNecessary(() => LogBox.Text = "");
        private void SaveLogsAsTextButon_Click(object sender, EventArgs e) => SaveAsText(LogBox.Text);
        private void CopyLogsToClipboardButon_Click(object sender, EventArgs e) => CopyLogsToClipboard(LogBox.Text);
        private void LoadButton_Click(object sender, EventArgs e) => LoadRepo();
        private void ExtractRoutesButton_Click(object sender, EventArgs e) => ExtractRoutes(SaveRoutingSelector.Checked);
        #endregion

        private void SaveAsText(string text)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt";
                sfd.OverwritePrompt = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(sfd.FileName, text);
            }
        }
        private void CopyLogsToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                MessageBox.Show(this, "Copy to clipboard failed");
            }
        }
        private void ResetTitle(string suffix = "") => InvokeIfNecessary(() =>
        {
            if (string.IsNullOrWhiteSpace(RepositoryDirectory))
                Text = $"Geography {suffix}".Trim();
            else
                Text = $"Geography {suffix} {RepositoryDirectory}".Trim();
            //Geography TestApp
        });
        private void LoadRepo()
        {
            using (var dialog = new FolderBrowserDialog() { Description = "Select Root Directory" })
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    LoadRepo(dialog.SelectedPath);
        }

        private void LoadRepo(string directory)
        {
            splitContainer1.Panel1.Enabled = false;
            RepositoryDirectory = directory;
            Repository = null;
            Routings = null;
            new Thread(() =>
            {
                ResetTitle("Loading...");
                Repository = GeographyRepository.Load<GeographyRepository>(RepositoryDirectory, Log);
                ResetTitle("Ready");

                InvokeIfNecessary(() =>
                {
                    splitContainer1.Panel1.Enabled = true;
                });
            })
            { IsBackground = true }.Start();
        }

        private void ExtractRoutes(bool saveRoutings)
        {
            if (Repository == null) return;
            splitContainer1.Panel1.Enabled = false;
            Routings = null;
            new Thread(() =>
            {
                ResetTitle("Extracting Routes...");
                var config = new GeographyRouter.Config(true);

                Routings = new GeographyRouter.GeoRouter(config, Repository, Log);

                if (saveRoutings)
                {
                    ResetTitle("Save Routes...");
                    var reportDirectory = $"{RepositoryDirectory}RoutingReports({DateTime.Now:yyyy-MM-dd-HH-mm-ss})";
                    Directory.CreateDirectory(reportDirectory);

                    var routings = Routings.Routings.ToList();
                    foreach (var routing in routings)
                    {
                        Log($"Save Route {routing.Source.Code} [{routings.IndexOf(routing) + 1} of {routings.Count}]");

                        var reportFilename = Path.Combine(reportDirectory, $"{routing.Source.Code}-Routing.log");
                        using (var reportFile = new StreamWriter(reportFilename))
                        {
                            reportFile.WriteLine($"{routing.Source.Code}-Routing");

                            var source = routing.Items.FirstOrDefault();
                            var precedences = source.FillDowngoing();

                            foreach (var precedence in precedences.OrderBy(x => x))
                            {
                                var item = routing.ItemsByPrecedence[precedence];

                                var precedenceText = $"{precedence} (pre:{item.PrePrecedence}, next: {string.Join(",", item.NextPrecedences)}";
                                if (item is GeographyRouter.Route route)
                                {
                                    reportFile.WriteLine($"<Route {precedenceText}> {string.Join(", ", route.Elements.Select(x => x.Code))}");
                                }
                                else if (item is GeographyRouter.Node node)
                                {
                                    reportFile.WriteLine($"<Node {precedenceText}> {string.Join(", ", node.Elements.Select(x => x.Code))}");
                                }
                                else if (item is GeographyRouter.Branch branch)
                                {
                                    reportFile.WriteLine($"<Branch {precedenceText}>");
                                }
                            }

                            reportFile.Flush();
                        }
                    }

                }
                ResetTitle("Ready");

                InvokeIfNecessary(() =>
                {
                    splitContainer1.Panel1.Enabled = true;
                });
            })
            { IsBackground = true }.Start();
        }
    }
}
