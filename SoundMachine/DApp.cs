using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoundMachine
{
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
                /*
                if (filterWords == null || filterWords.Length == 0)
                    return CachedFilteredWavs;

                if (useAnd)
                    return CalcFilteredWavs_And;

                return CalcFilteredWavs_Or;
                */

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