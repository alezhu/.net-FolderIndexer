using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace folderindexer
{
    ///	<summary>
    ///	Summary	description	for	Class1.
    ///	</summary>
    class MainClass
    {

        static public bool NoBackup { get; set; }


        ///	<summary>
        ///	The	main entry point for the application.
        ///	</summary>
        [STAThread]
        static void Main(string[] args)
        {
//            Debugger.Launch();

            string fv = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            Console.WriteLine("Folder Indexer ver {0}", fv);
            if (args.Length < 1)
            {
                Console.WriteLine("You must	provide	a directory	argument at	the	command	line.");
            }
            else
            {
                var paths = new List<DirectoryInfo>(args.Length);
                foreach (string t in args)
                {
                    if (t.StartsWith("/") ||  t.StartsWith("\\"))
                    {
                        switch (t.ToLower()[1])
                        {
                            case 'b':
                                NoBackup = true;
                                break;
                        }
                    }
                    else
                    {
                        string path = String.Join("", t.Split(Path.GetInvalidPathChars()));

                        if (path.EndsWith("\\"))
                        {
                            path = path.Remove(path.Length - 1, 1);
                        }
                        var di = new DirectoryInfo(path);
                        if (!di.Exists)
                        {
                            var fi = new FileInfo(path);
                            //if (!fi.Exists )						 return	;
                            if (fi.DirectoryName != null) di = new DirectoryInfo(fi.DirectoryName);
                            if (!di.Exists)
                            {
                                Console.WriteLine("Path	{0}	does not exists");
                                continue;
                            }
                        }
                        paths.Add(di);


                    }
                }

                Parallel.ForEach(paths, x =>
                                            {
                                                Console.WriteLine(x.FullName);
                                                ProcessDirectory(x);
                                            });


            }

        }

        private static readonly Lazy<LogWriter> Lazy =
            new Lazy<LogWriter>(() => EnterpriseLibraryContainer.Current.GetInstance<LogWriter>());
        private static LogWriter Writer
        {
            get { return Lazy.Value; }
        }


        static void ProcessDirectory(DirectoryInfo d)
        {
            FileInfo[] fs = d.GetFiles();

            var decCount = (int)Math.Ceiling(Math.Log10(fs.Length));
            string fmt = "D" + decCount.ToString();

            var grouped = from fileInfo in fs
                          where
                              (fileInfo.Attributes & FileAttributes.Hidden) == 0 &&
                              (fileInfo.Attributes & FileAttributes.System) == 0
                          group fileInfo by TryExtractGroup(fileInfo.Name);
            var groupComparer = new GroupComparer(d.Name);
            var sortedGroup = grouped.OrderBy(g => g.Key, groupComparer);
            int i = 0;
            var fileInfoComparer = new FileInfoComparer();
            var sortedList = new Dictionary<string, FileProcessInfo>();
            foreach (IGrouping<string, FileInfo> group in sortedGroup)
            {
                foreach (var fileInfo in group.OrderBy(fileInfo => fileInfo, fileInfoComparer))
                {
                    sortedList[fileInfo.Name] = new FileProcessInfo(fileInfo, fmt, i++);
                }

            }

            string descPath = Path.Combine(d.FullName, "descript.ion");
            Description desc = null;
            if (File.Exists(descPath))
            {
                using (var stream = new FileStream(descPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    desc = new Description();
                    desc.LoadFromStream(stream);
                }
            }

            var renameQueue = new List<FileProcessInfo>(sortedList.Count);
            foreach (var fileProcessInfo in sortedList)
            {
                ProcessFile(sortedList, renameQueue, fileProcessInfo.Value);
            }

            foreach (var fileProcessInfo in renameQueue)
            {
                Rename2(fileProcessInfo,desc);
            }



            ////foreach (KeyValuePair<string, Tuple<FileInfo, int>> pair in sortedList)
            ////{
            ////    ProcessFileInfo(d, sortedList, fmt, pair);
            ////}


            //try
            //{
            //    Parallel.ForEach(sortedList, MakeBackup);
            //}
            //catch (AggregateException ex)
            //{
            //    ex.Handle(x =>
            //    {
            //        Writer.Write(x.Message, "Error", 1, 1, TraceEventType.Error);
            //        return true;
            //    });
            //}


            //try
            //{
            //    Parallel.ForEach(sortedList, pair => Rename(pair, desc));
            //}
            //catch (AggregateException ex)
            //{
            //    ex.Handle(x =>
            //    {
            //        Writer.Write(x.Message, "Error", 1, 1, TraceEventType.Error);
            //        return true;
            //    });
            //}

            if (desc != null)
            {
                using (var stream = new FileStream(descPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    desc.SaveToStream(stream);
                }

            }

        }

        private static void ProcessFile(Dictionary<string, FileProcessInfo> sortedList, List<FileProcessInfo> renameQueue, FileProcessInfo fileProcessInfo)
        {
            if (fileProcessInfo.InQueue)
            {
                return;
            }

            if (fileProcessInfo.OriginalName == fileProcessInfo.NewName)
            {
                fileProcessInfo.InQueue = true; // fake
                Writer.Write(String.Format("Skip file: {0}", fileProcessInfo.OriginalName));
                return;
            }

            if (fileProcessInfo.InRecursion)
            {
                fileProcessInfo.InQueue = true;
                renameQueue.Add(fileProcessInfo);
                return;
            }

            FileProcessInfo info;
            if (sortedList.TryGetValue(fileProcessInfo.NewName, out info) && !info.InQueue)
            {
                fileProcessInfo.InRecursion = true;
                ProcessFile(sortedList, renameQueue, info);
            }
            else
            {
                fileProcessInfo.InQueue = true;
                renameQueue.Add(fileProcessInfo);
            }
        }

/*
        private static void MakeBackup(KeyValuePair<string, FileProcessInfo> pair)
        {
            if (pair.Value.OriginalFullName == pair.Value.NewFullName)
            {
                Writer.Write(String.Format("Skip file: {0}", pair.Key));
                return;
            }

            if (File.Exists(pair.Value.OriginalFullName))
            {
                pair.Value.FileInfo.MoveTo(pair.Value.BakName);
                Writer.Write(String.Format("Backup file: {0}", pair.Key));
            }
        }
*/

/*
        private static void Rename(KeyValuePair<string, FileProcessInfo> pair, Description desc)
        {
            if (File.Exists(pair.Value.BakName))
            {
                Writer.Write(String.Format("{0} => {1}", pair.Key, pair.Value.NewName));

                string descStr;
                if (desc != null && desc.Files.TryGetValue(pair.Value.OriginalName, out descStr))
                {
                    desc.Files[pair.Value.NewName] = descStr;
                    desc.Files.Remove(pair.Value.OriginalName);
                }


                File.Copy(pair.Value.BakName, pair.Value.NewFullName);
            }
        }
*/

        private static void Rename2(FileProcessInfo info, Description desc)
        {
            if (File.Exists(info.OriginalFullName))
            {
                Writer.Write(String.Format("{0} => {1}", info.OriginalName, info.NewName));

                string descStr;
                if (desc != null && desc.Files.TryGetValue(info.OriginalName, out descStr))
                {
                    desc.Files[info.NewName] = descStr;
                    desc.Files.Remove(info.OriginalName);
                }

                if (!NoBackup)
                {
                    File.Copy(info.OriginalFullName, info.BakName);
                }

                File.Move(info.OriginalFullName, info.NewFullName);
            }
        }

        private static readonly Regex DeleteDigitRe = new Regex(@"\.*\d+\.*");

        private static string TryExtractGroup(string fileName)
        {
            string result = String.Empty;
            if (String.IsNullOrEmpty(fileName))
                return result;
            // ReSharper disable AssignNullToNotNullAttribute
            return DeleteDigitRe.Replace(Path.GetFileNameWithoutExtension(fileName), String.Empty).Trim();
            // ReSharper restore AssignNullToNotNullAttribute
        }


    }
}
