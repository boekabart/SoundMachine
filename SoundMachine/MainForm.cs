using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;    // for PInvoke
using System.Media;

namespace SoundMachine
{
    public partial class MainForm : Form
    {
        public DApp Ap;

        private void UpdateTitle()
        {
            Text = "SoundMachine - " + (Ap.UseAnd ? "All" : "Any") + (Ap.WholeWord ? " Word" : " String");
        }

        System.Random Random;

        public MainForm()
        {
            InitializeComponent();
            lvMain.Columns.Add("Name", -1);
            this.WindowState = FormWindowState.Minimized;
            Ap = new DApp();
            UpdateTitle();
            var soundsFolder = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.ExecutablePath),
                "Sounds");
            if (!Directory.Exists(soundsFolder))
                soundsFolder = "Sounds";

            Ap.ReadFolder(soundsFolder);
            UpdateMainList();

            Random = new Random();

            try
            {
                // Alt = 1, Ctrl = 2, Shift = 4, Win = 8
                RegisterHotKey(this.Handle,
                    this.GetType().GetHashCode(), 3, (int)'S');
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not register hotkey: " + ex.Message);
            }
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            Ap.SetFilter(tbFilter.Text);
            UpdateMainList();
        }

        private void UpdateMainList()
        {
            lvMain.BeginUpdate();
            lvMain.Sorting = SortOrder.None;
            lvMain.Items.Clear();

            foreach (Sound sound in Ap.FilteredWavs)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = sound.ToString();
                lvi.Font = new System.Drawing.Font("Arial Unicode MS", 13F);
                lvi.Tag = sound;
                lvi.ToolTipText = sound.Path;
                lvMain.Items.Add(lvi);
            }
            //			lvMain.ShowItemToolTips = true;
            lvMain.EndUpdate();
        }

        private void PlaySound(int index)
        {
            windows7KeepAliveTimer.Enabled = false;
            Sound sound = (Sound)lvMain.Items[index].Tag;
            sound.Play();
            //windows7KeepAliveTimer.Enabled = true;
        }

        private void ScrollListTo(int itemno)
        {
            for (int poging = itemno; poging >= 0; poging--)
            {
                try
                {
                    lvMain.TopItem = lvMain.Items[poging];
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                break;
            }
        }

        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Add:
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    Ap.WholeWord = !Ap.WholeWord;
                    UpdateTitle();
                    //Ap.SetFilter(tbFilter.Text);
                    UpdateMainList();
                    break;
                case Keys.Subtract:
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    Ap.UseAnd = !Ap.UseAnd;
                    UpdateTitle();
                    //Ap.SetFilter(tbFilter.Text);
                    UpdateMainList();
                    break;
                case Keys.F5:
                    Ap.InvalidateFileCache();
                    UpdateMainList();
                    break;
                case Keys.Escape:
                    if (String.IsNullOrEmpty(tbFilter.Text))
                        SoundPlaying.MuteAll();
                    else
                        tbFilter.Text = "";
                    break;

                case Keys.Up:
                case Keys.Home:
                case Keys.PageUp:
                case Keys.Down:
                    {
                        if (lvMain.Items.Count > 0)
                            SelectItem(0);
                        break;
                    }
                case Keys.End:
                case Keys.PageDown:
                    {
                        int count = lvMain.Items.Count;
                        if (count > 0)
                        {
                            int last = count - 1;
                            SelectItem(last);
                        }
                        break;
                    }
                case Keys.Enter:
                    {
                        if (lvMain.Items.Count == 1)
                        {
                            PlaySound(0);
                        }
                        if (lvMain.Items.Count > 1)
                        {
                            int item = Random.Next(lvMain.Items.Count);
                            SelectItem(item);
                        }
                        break;
                    }
            }
        }

        private void SelectItem(int itemno)
        {
            lvMain.Focus();
            lvMain.BeginUpdate();
            lvMain.SelectedIndices.Clear();
            lvMain.SelectedIndices.Add(itemno);
            lvMain.FocusedItem = lvMain.Items[itemno];
            ScrollListTo(itemno);
            lvMain.EndUpdate();
        }

        private void lvMain_KeyDown(object sender, KeyEventArgs e)
        {
            MouseDowned = false;
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z || e.KeyCode == Keys.Space)
            {
                if (e.KeyCode == Keys.Space)
                    tbFilter.Text += " ";
                else
                    tbFilter.Text += e.KeyData.ToString().Substring(0, 1);

                tbFilter.Focus();
                tbFilter.Select(tbFilter.Text.Length, tbFilter.Text.Length);
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F5:
                    {
                        Ap.InvalidateFileCache();
                        UpdateMainList();
                        break;
                    }
                case Keys.Escape:
                    {
                        tbFilter.Text = "";
                        tbFilter.Focus();
                        break;
                    }
                case Keys.Back:
                    {
                        if (tbFilter.Text.Length > 0)
                            tbFilter.Text = tbFilter.Text.Substring(0, tbFilter.Text.Length - 1);
                        tbFilter.Focus();
                        tbFilter.Select(tbFilter.Text.Length, tbFilter.Text.Length);
                        return;
                    }
                case Keys.Enter:
                    {
                        if (lvMain.SelectedIndices.Count > 0)
                            PlaySound(lvMain.SelectedIndices[0]);
                        return;
                    }
            }
        }

        private bool MouseDowned = false;

        private void lvMain_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDowned = true;
        }

        private void lvMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!MouseDowned)
                return;

            MouseDowned = false;

            if (lvMain.SelectedIndices.Count > 0)
            {
                PlaySound(lvMain.SelectedIndices[0]);
                lvMain.SelectedIndices.Clear();
            }
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd,
          int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                if (!this.Visible || WindowState == FormWindowState.Minimized)
                {
                    this.Visible = true;
                    WindowState = FormWindowState.Maximized;
                    Activate();
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            tbFilter.Focus();
            tbFilter.SelectAll();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Visible)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                return;
            }
        }

        private void windows7KeepAliveTimer_Tick(object sender, EventArgs e)
        {
            if (windows7KeepAliveTimer.Interval < 1000)
            {
                windows7KeepAliveTimer.Interval = 30000;
                this.Visible = false;
            }
            SoundPlaying.MuteAll();
        }

        private void taskBarIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
                return;
            }

            this.Visible = true;
            this.WindowState = FormWindowState.Maximized;
            this.Activate();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                taskBarIcon.Visible = true;
            }
            else
            {
                this.Visible = true;
                taskBarIcon.Visible = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                UnregisterHotKey(this.Handle,
                    this.GetType().GetHashCode());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not unregister hotkey: " + ex.Message);
            }
        }

        private void taskBarIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                Close();
        }
    }
}
