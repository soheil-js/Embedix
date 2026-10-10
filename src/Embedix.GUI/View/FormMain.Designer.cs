namespace Embedix.GUI.View
{
    partial class FormMain
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
            menuStrip1 = new MenuStrip();
            helpToolStripMenuItem = new ToolStripMenuItem();
            helpLatestRelease = new ToolStripMenuItem();
            helpSourceCode = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            helpAbout = new ToolStripMenuItem();
            txtName = new TextBox();
            btnBrowse = new Button();
            chkAutomatically = new CheckBox();
            chkWritePdb = new CheckBox();
            btnAdd = new Button();
            btnClear = new Button();
            btnRun = new Button();
            lbLibraries = new ListBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(463, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { helpLatestRelease, helpSourceCode, toolStripSeparator1, helpAbout });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            // 
            // helpLatestRelease
            // 
            helpLatestRelease.Name = "helpLatestRelease";
            helpLatestRelease.Size = new Size(186, 26);
            helpLatestRelease.Text = "Latest Release";
            helpLatestRelease.Click += helpLatestRelease_Click;
            // 
            // helpSourceCode
            // 
            helpSourceCode.Name = "helpSourceCode";
            helpSourceCode.Size = new Size(186, 26);
            helpSourceCode.Text = "Source Code";
            helpSourceCode.Click += helpSourceCode_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(183, 6);
            // 
            // helpAbout
            // 
            helpAbout.Name = "helpAbout";
            helpAbout.Size = new Size(186, 26);
            helpAbout.Text = "About";
            helpAbout.Click += helpAbout_Click;
            // 
            // txtName
            // 
            txtName.AllowDrop = true;
            txtName.Location = new Point(12, 31);
            txtName.Name = "txtName";
            txtName.ReadOnly = true;
            txtName.Size = new Size(339, 27);
            txtName.TabIndex = 1;
            txtName.DragDrop += txtName_DragDrop;
            txtName.DragEnter += txtName_DragEnter;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(357, 31);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 29);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // chkAutomatically
            // 
            chkAutomatically.AutoSize = true;
            chkAutomatically.Checked = true;
            chkAutomatically.CheckState = CheckState.Checked;
            chkAutomatically.Location = new Point(12, 64);
            chkAutomatically.Name = "chkAutomatically";
            chkAutomatically.Size = new Size(123, 24);
            chkAutomatically.TabIndex = 3;
            chkAutomatically.Text = "Automatically";
            chkAutomatically.UseVisualStyleBackColor = true;
            chkAutomatically.CheckedChanged += chkAutomatically_CheckedChanged;
            // 
            // chkWritePdb
            // 
            chkWritePdb.AutoSize = true;
            chkWritePdb.Checked = true;
            chkWritePdb.CheckState = CheckState.Checked;
            chkWritePdb.Location = new Point(141, 64);
            chkWritePdb.Name = "chkWritePdb";
            chkWritePdb.Size = new Size(96, 24);
            chkWritePdb.TabIndex = 4;
            chkWritePdb.Text = "Write Pdb";
            chkWritePdb.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Enabled = false;
            btnAdd.Location = new Point(12, 300);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(94, 29);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnClear
            // 
            btnClear.Enabled = false;
            btnClear.Location = new Point(112, 300);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(94, 29);
            btnClear.TabIndex = 6;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(357, 300);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(94, 29);
            btnRun.TabIndex = 7;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // lbLibraries
            // 
            lbLibraries.AllowDrop = true;
            lbLibraries.FormattingEnabled = true;
            lbLibraries.Location = new Point(12, 90);
            lbLibraries.Name = "lbLibraries";
            lbLibraries.Size = new Size(439, 204);
            lbLibraries.TabIndex = 8;
            lbLibraries.DragDrop += lbLibraries_DragDrop;
            lbLibraries.DragEnter += lbLibraries_DragEnter;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(463, 341);
            Controls.Add(lbLibraries);
            Controls.Add(btnRun);
            Controls.Add(btnClear);
            Controls.Add(btnAdd);
            Controls.Add(chkWritePdb);
            Controls.Add(chkAutomatically);
            Controls.Add(btnBrowse);
            Controls.Add(txtName);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Embedix GUI";
            Load += FormMain_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem helpLatestRelease;
        private ToolStripMenuItem helpSourceCode;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem helpAbout;
        private TextBox txtName;
        private Button btnBrowse;
        private CheckBox chkAutomatically;
        private CheckBox chkWritePdb;
        private Button btnAdd;
        private Button btnClear;
        private Button btnRun;
        private ListBox lbLibraries;
    }
}