using System;

namespace SoundMachine
{
    public class Sound : IComparable<Sound>
    {
        public string Path;

        public override string ToString()
        {
            var fn = System.IO.Path.GetFileNameWithoutExtension(Path);
            var fns = fn.Split('[');
            return fns[0].TrimEnd();
        }

        #region IComparable<Sound> Members

        public int CompareTo(Sound other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        #endregion
    }
}