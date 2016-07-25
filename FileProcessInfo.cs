using System;
using System.IO;

namespace folderindexer
{
    internal class FileProcessInfo
    {
        public bool InQueue { get; set; }
        public bool InRecursion { get; set; }

        public readonly FileInfo FileInfo;
        public readonly string IndexFormat;
        public readonly string OriginalFullName;
        public readonly string OriginalName;
        public readonly int NewIndex;
        public string BakName
        {
            get { return String.Format("{0}.bak", OriginalFullName); }
        }
#pragma warning disable 649
        public readonly string NewFullName;
#pragma warning restore 649
#pragma warning disable 649
        public readonly string NewName;
#pragma warning restore 649

        public FileProcessInfo(FileInfo fileInfo, string nameFormat, int newIndex)
        {
            FileInfo = fileInfo;
            IndexFormat = nameFormat;
            NewIndex = newIndex;
            OriginalFullName = FileInfo.FullName;
            OriginalName = FileInfo.Name;
            if (FileInfo.Directory != null)
            {
                NewName = string.Format("{0}.{1}{2}", FileInfo.Directory.Name, NewIndex.ToString(IndexFormat), FileInfo.Extension);
                NewFullName = Path.Combine(FileInfo.Directory.FullName, NewName);
            }
            InQueue = false;
            InRecursion = false;
        }
    }
}
