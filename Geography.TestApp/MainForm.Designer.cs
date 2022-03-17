
namespace Geography.TestApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ExtractRoutesButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.LogsController = new System.Windows.Forms.Panel();
            this.CopyLogsToClipboardButon = new System.Windows.Forms.Button();
            this.SaveLogsAsTextButon = new System.Windows.Forms.Button();
            this.ClearLogsButton = new System.Windows.Forms.Button();
            this.SaveRoutingSelector = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.LogsController.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SaveRoutingSelector);
            this.splitContainer1.Panel1.Controls.Add(this.ExtractRoutesButton);
            this.splitContainer1.Panel1.Controls.Add(this.LoadButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LogBox);
            this.splitContainer1.Panel2.Controls.Add(this.LogsController);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 191;
            this.splitContainer1.TabIndex = 0;
            // 
            // ExtractRoutesButton
            // 
            this.ExtractRoutesButton.AutoSize = true;
            this.ExtractRoutesButton.Location = new System.Drawing.Point(87, 12);
            this.ExtractRoutesButton.Name = "ExtractRoutesButton";
            this.ExtractRoutesButton.Size = new System.Drawing.Size(115, 25);
            this.ExtractRoutesButton.TabIndex = 0;
            this.ExtractRoutesButton.Text = "Extract Routes";
            this.ExtractRoutesButton.UseVisualStyleBackColor = true;
            this.ExtractRoutesButton.Click += new System.EventHandler(this.ExtractRoutesButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.AutoSize = true;
            this.LoadButton.Location = new System.Drawing.Point(12, 12);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(69, 25);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // LogBox
            // 
            this.LogBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.LogBox.BackColor = System.Drawing.Color.Gainsboro;
            this.LogBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogBox.Location = new System.Drawing.Point(0, 0);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogBox.Size = new System.Drawing.Size(800, 227);
            this.LogBox.TabIndex = 22;
            this.LogBox.WordWrap = false;
            // 
            // LogsController
            // 
            this.LogsController.Controls.Add(this.CopyLogsToClipboardButon);
            this.LogsController.Controls.Add(this.SaveLogsAsTextButon);
            this.LogsController.Controls.Add(this.ClearLogsButton);
            this.LogsController.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LogsController.Location = new System.Drawing.Point(0, 227);
            this.LogsController.Name = "LogsController";
            this.LogsController.Size = new System.Drawing.Size(800, 28);
            this.LogsController.TabIndex = 23;
            // 
            // CopyLogsToClipboardButon
            // 
            this.CopyLogsToClipboardButon.AutoSize = true;
            this.CopyLogsToClipboardButon.Dock = System.Windows.Forms.DockStyle.Left;
            this.CopyLogsToClipboardButon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CopyLogsToClipboardButon.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
            this.CopyLogsToClipboardButon.ForeColor = System.Drawing.Color.Navy;
            this.CopyLogsToClipboardButon.Location = new System.Drawing.Point(184, 0);
            this.CopyLogsToClipboardButon.Name = "CopyLogsToClipboardButon";
            this.CopyLogsToClipboardButon.Size = new System.Drawing.Size(82, 28);
            this.CopyLogsToClipboardButon.TabIndex = 21;
            this.CopyLogsToClipboardButon.Text = "Copy Logs";
            this.CopyLogsToClipboardButon.UseVisualStyleBackColor = true;
            this.CopyLogsToClipboardButon.Click += new System.EventHandler(this.CopyLogsToClipboardButon_Click);
            // 
            // SaveLogsAsTextButon
            // 
            this.SaveLogsAsTextButon.AutoSize = true;
            this.SaveLogsAsTextButon.Dock = System.Windows.Forms.DockStyle.Left;
            this.SaveLogsAsTextButon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveLogsAsTextButon.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
            this.SaveLogsAsTextButon.ForeColor = System.Drawing.Color.Navy;
            this.SaveLogsAsTextButon.Location = new System.Drawing.Point(92, 0);
            this.SaveLogsAsTextButon.Name = "SaveLogsAsTextButon";
            this.SaveLogsAsTextButon.Size = new System.Drawing.Size(92, 28);
            this.SaveLogsAsTextButon.TabIndex = 20;
            this.SaveLogsAsTextButon.Text = "Save Logs";
            this.SaveLogsAsTextButon.UseVisualStyleBackColor = true;
            this.SaveLogsAsTextButon.Click += new System.EventHandler(this.SaveLogsAsTextButon_Click);
            // 
            // ClearLogsButton
            // 
            this.ClearLogsButton.AutoSize = true;
            this.ClearLogsButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.ClearLogsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearLogsButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
            this.ClearLogsButton.ForeColor = System.Drawing.Color.Navy;
            this.ClearLogsButton.Location = new System.Drawing.Point(0, 0);
            this.ClearLogsButton.Name = "ClearLogsButton";
            this.ClearLogsButton.Size = new System.Drawing.Size(92, 28);
            this.ClearLogsButton.TabIndex = 19;
            this.ClearLogsButton.Text = "Clear Logs";
            this.ClearLogsButton.UseVisualStyleBackColor = true;
            this.ClearLogsButton.Click += new System.EventHandler(this.ClearLogsButton_Click);
            // 
            // SaveRoutingSelector
            // 
            this.SaveRoutingSelector.AutoSize = true;
            this.SaveRoutingSelector.Checked = true;
            this.SaveRoutingSelector.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SaveRoutingSelector.Location = new System.Drawing.Point(208, 16);
            this.SaveRoutingSelector.Name = "SaveRoutingSelector";
            this.SaveRoutingSelector.Size = new System.Drawing.Size(117, 19);
            this.SaveRoutingSelector.TabIndex = 1;
            this.SaveRoutingSelector.Text = "Save Routings";
            this.SaveRoutingSelector.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Text = "Geography TestApp";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.LogsController.ResumeLayout(false);
            this.LogsController.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.Panel LogsController;
        private System.Windows.Forms.Button CopyLogsToClipboardButon;
        private System.Windows.Forms.Button SaveLogsAsTextButon;
        private System.Windows.Forms.Button ClearLogsButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button ExtractRoutesButton;
        private System.Windows.Forms.CheckBox SaveRoutingSelector;
    }
}

