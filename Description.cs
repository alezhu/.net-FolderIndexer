using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace folderindexer
{
    class Description
    {
        public Description()
        {
            Files = new ConcurrentDictionary<string, String>();
        }

        public void LoadFromStream(Stream stream)
        {
            Lazy<LogWriter> writer = new Lazy<LogWriter>(() => EnterpriseLibraryContainer.Current.GetInstance<LogWriter>());
            Files.Clear();
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        line = line.Trim();
                        string file, desc;
                        if (line.StartsWith("\""))
                        {
                            int pos = line.IndexOf('"', 1);
                            if (pos < 1)
                            {
                                writer.Value.Write(String.Format("Strange line: {0}", line));
                                file = line;
                                desc = string.Empty;
                            }
                            else
                            {
                                file = line.Substring(1, pos - 1);
                                desc = line.Substring(pos + 2);
                            }
                        }
                        else
                        {
                            int pos = line.IndexOf(' ');
                            if (pos >= 0)
                            {
                                file = line.Substring(0, pos);
                                desc = line.Substring(pos + 1);
                            }
                            else
                            {
                                file = line;
                                desc = String.Empty;
                            }
                        }
                        Files[file] = desc;
                    }
                }
            }
        }

        public void SaveToStream(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
            {
                foreach (KeyValuePair<string, string> pair in Files)
                {
                    string file = pair.Key.IndexOf(' ') >= 0 ? String.Format("\"{0}\"", pair.Key) : pair.Key;
                    writer.WriteLine("{0} {1}", file, pair.Value);
                }
            }
        }

        public IDictionary<string, string> Files { get; private set; }
    }
}
