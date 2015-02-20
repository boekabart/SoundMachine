using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
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
            Ap.ReadFolder(System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.ExecutablePath),
                "Sounds"));
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
            windows7KeepAliveTimer.Enabled = true;
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
                        Sound.Mute();
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
            Sound.Mute();
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

    public class Pair
    {
        public Pair(string fn, string n)
        {
            filename = fn;
            name = n;
        }

        string filename;
        string name;

        override public string ToString()
        {
            return name;
        }
    }

    public class Sound : IComparable<Sound>
    {
        public string Path;

        public override string ToString()
        {
            var fn = System.IO.Path.GetFileNameWithoutExtension(Path);
            var fns = fn.Split('[');
            return fns[0].TrimEnd();
        }

        static public void Mute()
        {
            SoundPlayer[] amps;
            lock (m_ActiveMediaPlayers)
            {
                amps = m_ActiveMediaPlayers.ToArray();
                m_ActiveMediaPlayers.Clear();
            }

            foreach (var mediaPlayer in amps)
            {
                mediaPlayer.Stop();
            }
        }

        static private List<SoundPlayer> m_ActiveMediaPlayers = new List<SoundPlayer>();

        public void Play()
        {
            try
            {
                var mediaPlayer = new SoundPlayer(Path);
                mediaPlayer.Play();
                lock (m_ActiveMediaPlayers)
                    m_ActiveMediaPlayers.Add(mediaPlayer);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), e.Message);
            }
        }

        #region IComparable<Sound> Members

        public int CompareTo(Sound other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        #endregion
    }

    public class DApp
    {
        public bool useAnd = true, wholeWord = false;

        public bool WholeWord
        {
            get { return wholeWord; }
            set
            {
                wholeWord = value;
                // If the test gets WEAKER then invalidate the cached list
                if (!wholeWord)
                    InvalidateFilterCache();
            }
        }

        public bool UseAnd
        {
            get { return useAnd; }
            set
            {
                useAnd = value;
                // If the test gets WEAKER then invalidate the cached list
                if (!useAnd)
                    InvalidateFilterCache();
            }
        }
        private string[] filterWords;

        public void InvalidateFileCache()
        {
            cachedWavs = null;
            cachedFilteredWavs = null;
        }

        public void InvalidateFilterCache()
        {
            cachedFilteredWavs = null;
        }

        public IEnumerable<Sound> CachedFilteredWavs
        {
            get
            {
                if (cachedFilteredWavs == null)
                    return Wavs;
                return cachedFilteredWavs;
            }
        }

        public IEnumerable<Sound> FilteredWavs
        {
            get
            {
                // Always cache the result
                cachedFilteredWavs = new List<Sound>(CalcFilteredWavs);
                return cachedFilteredWavs;
            }
        }

        public IEnumerable<Sound> CalcFilteredWavs
        {
            get
            {
                foreach (Sound sound in CachedFilteredWavs)
                {
                    if (filterWords == null || filterWords.Length == 0)
                    {
                        yield return sound;
                        continue;
                    }
                    string fn = Path.GetFileNameWithoutExtension(sound.Path).ToLower();
                    if (useAnd)
                    {
                        bool ok = true;
                        foreach (string w in filterWords)
                        {
                            if (wholeWord)
                            {
                                string[] fnw = fn.Split(haakjes);
                                bool found = false;
                                foreach (string x in fnw)
                                {
                                    if (!x.Equals(w))
                                        continue;
                                    found = true;
                                    break;
                                }
                                if (found)
                                    continue;
                                ok = false;
                                break;
                            }
                            else
                            {
                                if (fn.Contains(w))
                                    continue;
                                ok = false;
                                break;
                            }
                        }
                        if (!ok)
                            continue;
                    }
                    else //or
                    {
                        bool ok = false;
                        foreach (string w in filterWords)
                        {
                            if (wholeWord)
                            {
                                string[] fnw = fn.Split(haakjes);
                                bool found = false;
                                foreach (string x in fnw)
                                {
                                    if (!x.Equals(w))
                                        continue;
                                    found = true;
                                    break;
                                }
                                if (!found)
                                    continue;
                                ok = true;
                                break;
                            }
                            else
                            {
                                if (!fn.Contains(w))
                                    continue;
                                ok = true;
                                break;
                            }
                        }
                        if (!ok)
                            continue;
                    }
                    yield return sound;
                }
            }
        }

        private char[] haakjes = { ' ', '[', ']', '(', ')' };

        public DApp()
        {
        }


        string lastFilter = "";

        public void SetFilter(string filterstring)
        {
            string f = filterstring.Trim().ToLower();

            // If the new filter does not contain the last one
            //  OR we look for entire words only
            //  then we cannot use the last filter to filter again
            if (wholeWord || !f.Contains(lastFilter))
                InvalidateFilterCache();

            lastFilter = f;

            filterWords = f.Split(' ');
        }

        string SoundFolder;
        public void ReadFolder(string folder)
        {
            SoundFolder = System.IO.Path.IsPathRooted(folder) ? folder : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), folder);
        }

        List<Sound> cachedWavs;
        List<Sound> cachedFilteredWavs;

        public IEnumerable<Sound> Wavs
        {
            get
            {
                if (cachedWavs == null)
                {
                    cachedWavs = new List<Sound>(DiskWavs);
                    cachedWavs.Sort();
                }

                return cachedWavs;
            }
        }

        public IEnumerable<Sound> DiskWavs
        {
            get
            {
                return Directory.GetFiles(SoundFolder, "*.wav", SearchOption.AllDirectories).Select(path => new Sound { Path = path });
            }
        }
    }
}
