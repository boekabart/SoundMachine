namespace SoundMachine
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.lvMain = new System.Windows.Forms.ListView();
            this.windows7KeepAliveTimer = new System.Windows.Forms.Timer(this.components);
            this.taskBarIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // tbFilter
            // 
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbFilter.Location = new System.Drawing.Point(0, 0);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(674, 20);
            this.tbFilter.TabIndex = 0;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            this.tbFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilter_KeyDown);
            // 
            // lvMain
            // 
            this.lvMain.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.lvMain.AutoArrange = false;
            this.lvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMain.Font = new System.Drawing.Font("Arial Narrow", 18F);
            this.lvMain.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lvMain.FullRowSelect = true;
            this.lvMain.GridLines = true;
            this.lvMain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvMain.LabelWrap = false;
            this.lvMain.Location = new System.Drawing.Point(0, 20);
            this.lvMain.MultiSelect = false;
            this.lvMain.Name = "lvMain";
            this.lvMain.Size = new System.Drawing.Size(674, 392);
            this.lvMain.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvMain.TabIndex = 1;
            this.lvMain.TabStop = false;
            this.lvMain.TileSize = new System.Drawing.Size(32, 32);
            this.lvMain.UseCompatibleStateImageBehavior = false;
            this.lvMain.View = System.Windows.Forms.View.List;
            this.lvMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvMain_MouseDown);
            this.lvMain.SelectedIndexChanged += new System.EventHandler(this.lvMain_SelectedIndexChanged);
            this.lvMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lvMain_MouseDown);
            this.lvMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvMain_KeyDown);
            // 
            // windows7KeepAliveTimer
            // 
            this.windows7KeepAliveTimer.Enabled = true;
            this.windows7KeepAliveTimer.Interval = 10;
            this.windows7KeepAliveTimer.Tick += new System.EventHandler(this.windows7KeepAliveTimer_Tick);
            // 
            // taskBarIcon
            // 
            this.taskBarIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("taskBarIcon.Icon")));
            this.taskBarIcon.Text = "SoundMachine";
            this.taskBarIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.taskBarIcon_MouseClick);
            this.taskBarIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.taskBarIcon_MouseDoubleClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(674, 412);
            this.Controls.Add(this.lvMain);
            this.Controls.Add(this.tbFilter);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.ListView lvMain;
        private System.Windows.Forms.Timer windows7KeepAliveTimer;
        private System.Windows.Forms.NotifyIcon taskBarIcon;
    }
}

