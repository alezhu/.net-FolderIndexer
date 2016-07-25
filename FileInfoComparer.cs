using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace folderindexer
{
    /// <summary>
    /// 
    /// </summary>
    public class FileInfoComparer : IComparer<FileInfo>
    {
        private static readonly Regex re = new Regex(@"\D");

        public int Compare(FileInfo x, FileInfo y)
        {
            string sa = re.Replace(x.Name, "");
            string sb = re.Replace(y.Name, "");
            if ((sa.Length == 0) || (sb.Length == 0))
            {
                return x.Name.CompareTo(y.Name);
            }
            else
            {
                Int64 ia, ib;
                int result = 0;
                if (Int64.TryParse(sa, out ia) && Int64.TryParse(sb, out ib))
                {
                    result = ia.CompareTo(ib);
                }
                if (result == 0)
                {
                    result = x.Name.CompareTo(y.Name);
                }
                return result;
            }
        }
    }
}
