using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;    // for PInvoke
using SoundMachine.Properties;

namespace SoundMachine
{
    public partial class MainForm : Form
    {
        public DApp Ap;

        private void UpdateTitle()
        {
            Text = string.Format("{0} - {1} {2}",
                Resources.MainForm_UpdateTitle_SoundMachine,
                (Ap.UseAnd ? "All" : "Any"),
                (Ap.WholeWord ? "Word" : "String")
                );
        }

        readonly Random _random;

        public MainForm()
        {
            InitializeComponent();
            lvMain.Columns.Add("Name", -1);
            WindowState = FormWindowState.Minimized;
            Ap = new DApp();
            UpdateTitle();
            var soundsFolder = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath) ?? ".",
                "Sounds");
            if (!Directory.Exists(soundsFolder))
                soundsFolder = "Sounds";

            Ap.ReadFolder(soundsFolder);
            UpdateMainList();

            _random = new Random();

            try
            {
                // Alt = 1, Ctrl = 2, Shift = 4, Win = 8
                RegisterHotKey(Handle,
                    GetType().GetHashCode(), 3, 'S');
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"{0}: {1}", Resources.MainForm_MainForm_Could_not_register_hotkey, ex.Message);
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
            var listViewItems = Ap.FilteredWavs.Select(ListViewItemForSound);
            lvMain.Items.AddRange(listViewItems.ToArray());
            lvMain.EndUpdate();
        }

        private static ListViewItem ListViewItemForSound(Sound sound)
        {
            return new ListViewItem
            {
                Text = sound.ToString(),
                Font = new Font("Arial Unicode MS", 13F),
                Tag = sound,
                ToolTipText = sound.Path
            };
        }

        private void PlaySound(int index)
        {
            windows7KeepAliveTimer.Enabled = false;
            var sound = lvMain.Items[index].Tag as Sound;
            if (sound != null)
                sound.Play();
            windows7KeepAliveTimer.Enabled = true;
        }

        private void ScrollListTo(int itemno)
        {
            for (var poging = itemno; poging >= 0; poging--)
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
                    UpdateMainList();
                    break;
                case Keys.Subtract:
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    Ap.UseAnd = !Ap.UseAnd;
                    UpdateTitle();
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
                        var count = lvMain.Items.Count;
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
                            int item = _random.Next(lvMain.Items.Count);
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
            _mouseDowned = false;
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z || e.KeyCode == Keys.Space)
            {
                if (e.KeyCode == Keys.Space)
                    tbFilter.Text += @" ";
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

        private bool _mouseDowned;

        private void lvMain_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDowned = true;
        }

        private void lvMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_mouseDowned)
                return;

            _mouseDowned = false;

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
                if (!Visible || WindowState == FormWindowState.Minimized)
                {
                    Visible = true;
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
            if (Visible)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
        }

        private void windows7KeepAliveTimer_Tick(object sender, EventArgs e)
        {
            if (windows7KeepAliveTimer.Interval < 1000)
            {
                windows7KeepAliveTimer.Interval = 30000;
                Visible = false;
            }
            SoundPlaying.MuteAll();
        }

        private void taskBarIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                WindowState = FormWindowState.Minimized;
                return;
            }

            Visible = true;
            WindowState = FormWindowState.Maximized;
            Activate();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Visible = false;
                taskBarIcon.Visible = true;
            }
            else
            {
                Visible = true;
                taskBarIcon.Visible = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                UnregisterHotKey(Handle,
                    GetType().GetHashCode());
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"{0}: {1}", Resources.MainForm_MainForm_FormClosed_Could_not_unregister_hotkey, ex.Message);
            }
        }

        private void taskBarIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                Close();
        }
    }
}
