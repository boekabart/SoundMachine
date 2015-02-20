using System;

namespace SoundMachine
{
    public class Sound
    {
        public Sound(string path)
        {
            Path = path;
            FilterString = GetFilterString(path);
            FilterWords = GetFilterWords(FilterString);
        }

        private static string[] GetFilterWords(string filterString)
        {
            return filterString.Split(Haakjes, StringSplitOptions.RemoveEmptyEntries);
        }

        private static readonly char[] Haakjes = { ' ', '[', ']', '(', ')' };

        public string Path { get; private set; }

        public override string ToString()
        {
            var fn = System.IO.Path.GetFileNameWithoutExtension(Path) ?? String.Empty;
            var fns = fn.Split('[');
            return fns[0].TrimEnd();
        }

        public string[] FilterWords { get; private set; }
        public string FilterString { get; private set; }

        private static string GetFilterString(string path)
        {
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
            return (fileNameWithoutExtension ?? String.Empty).ToLower();
        }
    }
}