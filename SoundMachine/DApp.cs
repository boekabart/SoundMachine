using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoundMachine
{
    public class DApp
    {
        private bool _useAnd = true;
        private bool _wholeWord;

        public bool WholeWord
        {
            get { return _wholeWord; }
            set
            {
                _wholeWord = value;
                // If the test gets WEAKER then invalidate the cached list
                if (!_wholeWord)
                    InvalidateFilterCache();
            }
        }

        public bool UseAnd
        {
            get { return _useAnd; }
            set
            {
                _useAnd = value;
                // If the test gets WEAKER then invalidate the cached list
                if (!_useAnd)
                    InvalidateFilterCache();
            }
        }

        private string[] _filterWords = new string[0];

        public void InvalidateFileCache()
        {
            _cachedWavs = null;
            _cachedFilteredWavs = null;
        }

        public void InvalidateFilterCache()
        {
            _cachedFilteredWavs = null;
        }

        public IEnumerable<Sound> CachedFilteredWavs
        {
            get { return _cachedFilteredWavs ?? Wavs; }
        }

        public IEnumerable<Sound> FilteredWavs
        {
            get
            {
                // Always cache the result
                return _cachedFilteredWavs = CalcFilteredWavs().ToList();
            }
        }

        public IEnumerable<Sound> CalcFilteredWavs()
        {
            if (!_filterWords.Any())
                return CachedFilteredWavs;

            if (WholeWord)
            {
                return _useAnd
                    ? CachedFilteredWavs.Where(
                        sound => _filterWords.Intersect(sound.FilterWords).Count() == _filterWords.Length)
                    : CachedFilteredWavs.Where(sound => _filterWords.Intersect(sound.FilterWords).Any());
            }

            return _useAnd
                ? CachedFilteredWavs.Where(sound => _filterWords.All(sound.FilterString.Contains))
                : CachedFilteredWavs.Where(sound => _filterWords.Any(sound.FilterString.Contains));
        }

        private string _lastFilter = "";

        public void SetFilter(string filterstring)
        {
            var f = filterstring.Trim().ToLower();

            // If the new filter does not contain the last one
            //  OR we look for entire words only
            //  then we cannot use the last filter to filter again
            if (_wholeWord || !f.Contains(_lastFilter))
                InvalidateFilterCache();

            _lastFilter = f;

            _filterWords = f.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        private string _soundFolder;

        public void ReadFolder(string folder)
        {
            _soundFolder = Path.IsPathRooted(folder) ? folder : Path.Combine(Directory.GetCurrentDirectory(), folder);
        }

        private List<Sound> _cachedWavs;
        private List<Sound> _cachedFilteredWavs;

        public IEnumerable<Sound> Wavs
        {
            get { return _cachedWavs ?? (_cachedWavs = DiskWavs.OrderBy(s => s.FilterString).ToList()); }
        }

        public IEnumerable<Sound> DiskWavs
        {
            get
            {
                return
                    new DirectoryInfo(_soundFolder)
                        .EnumerateFiles("*.*", SearchOption.AllDirectories)
                        .Where(SoundPlaying.IsSupported)
                        .Select(fi => new Sound(fi.FullName));
            }
        }
    }
}