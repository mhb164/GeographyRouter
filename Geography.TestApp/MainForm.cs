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

namespace System.Windows.Forms
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
        private void ExtractRoutesButton_Click(object sender, EventArgs e) => ExtractRoutes();
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
            using (var dialog = new FolderSelectDialog() { Title = "Select Geography Data Root Directory" })
                if (dialog.ShowDialog())
                    LoadRepo(dialog.FileName);
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
                Repository = GeographyRepository.Load<GeographyRepository>(directory, Log);
                ResetTitle("Ready");


                InvokeIfNecessary(() =>
                {
                    splitContainer1.Panel1.Enabled = true;
                });
            })
            { IsBackground = true }.Start();
        }

        private void ExtractRoutes()
        {
            if (Repository == null) return;
            splitContainer1.Panel1.Enabled = false;
            Routings = null;
            new Thread(() =>
            {
                ResetTitle("Extracting Routes...");
                Routings = new GeographyRouter.GeoRouter(Repository, Log);
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
