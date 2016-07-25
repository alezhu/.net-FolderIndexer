using System.Collections.Generic;
using System.Diagnostics;

namespace folderindexer
{
    internal class GroupComparer : IComparer<string>
    {
        private readonly string _MainGroup;
        public GroupComparer(string mainGroup)
        {
            Debug.Assert(mainGroup != null, "mainGroup != null");
            _MainGroup = mainGroup;
        }

        public int Compare(string x, string y)
        {
            if (x != y)
            {
                if (x == _MainGroup)
                {
                    return -1;
                }
                if (y == _MainGroup)
                {
                    return 1;
                }
            }

            return x.CompareTo(y);
        }
    }
}
