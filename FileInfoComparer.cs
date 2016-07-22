using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;


namespace folderindexer
{
	/// <summary>
	/// 
	/// </summary>
	public class FileInfoComparer:IComparer
	{
		private Regex re = new Regex(@"\D");
		public FileInfoComparer()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

		int IComparer.Compare(Object a, Object b) {
			FileInfo fia , fib;
			fia = (FileInfo)a;
			fib = (FileInfo)b;
			string sa = re.Replace(fia.Name,"");
			string sb = re.Replace(fib.Name,"");
			if ((sa.Length==0)||(sb.Length==0)) {
				return fia.Name.CompareTo(fib.Name);
			} else {
				try {
					Int64 ia = Int64.Parse(sa);
					Int64 ib = Int64.Parse(sb);
					ib = ia-ib;
					return (int) ib;
				}
				catch {
					return fia.Name.CompareTo(fib.Name);
				}
			}
		}
	}
}
