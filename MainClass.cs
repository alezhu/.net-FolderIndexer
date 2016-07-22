using System;
using System.IO;
using System.Collections;

namespace folderindexer	{
	///	<summary>
	///	Summary	description	for	Class1.
	///	</summary>
	class MainClass	{
		///	<summary>
		///	The	main entry point for the application.
		///	</summary>
		[STAThread]
		static void	Main(string[] args)	{
			string fv =	System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion; 
			Console.WriteLine("Folder Indexer ver {0}",fv);	
			if (args.Length	!= 1) {
				Console.WriteLine("You must	provide	a directory	argument at	the	command	line.");
			} else {
				string path= args[0];
				if (path.EndsWith("\\")) {
					path = path.Remove(path.Length-1,1);  
				}
				Console.WriteLine(path);
				DirectoryInfo di = new DirectoryInfo(path);
				if (!di.Exists)	{
					FileInfo fi	= new FileInfo(path);
					//if (!fi.Exists )						 return	;
					di = new DirectoryInfo(fi.DirectoryName);
					if(!di.Exists)	{
						Console.WriteLine("Path	{0}	does not exists");
						return;
					}
				}
				ProcessDirectory(di);
			}
			   
		}

		static void	ProcessDirectory (DirectoryInfo	d) {
			FileInfo[] fs =	d.GetFiles();
			string fmt = "0";
			for(int	c =	fs.Length;c>10;c /=	10)	{
				fmt	+= "0";
			}

			string Ext = null;
			string DirName = d.Name.ToUpper() +	".";
			string Filename	= null;
			int	NeedLen	= DirName.Length + fmt.Length; 
			ArrayList ExtList =	new ArrayList();
			foreach(FileInfo fi	in fs) {
				if (((fi.Attributes	& FileAttributes.Hidden) ==	0) && ((fi.Attributes &	FileAttributes.System)	== 0)){
					Ext	= fi.Extension.ToUpper();
					Filename = fi.Name.ToUpper();
					if ((Filename.IndexOf(DirName) != 0) || (Filename.Length != (NeedLen+Ext.Length) )){
						ExtList.Add(fi); 
					}	
				}  else {
					Console.WriteLine("Skip file: {0}" , fi.Name);
				}
			}

			ExtList.Sort( new FileInfoComparer());
			string path	= d.FullName;
			string name	=d.Name;
			int	i =0;

			foreach(FileInfo fi	in ExtList)	{
				string newname = "";
				do {
					newname	= path+Path.DirectorySeparatorChar + name +	"."	+ i++.ToString(fmt)	+ fi.Extension;
				} while	(File.Exists(newname));	
				Console.WriteLine("{0} => {1}",fi.Name,newname);
				fi.CopyTo(fi.FullName+".bak");
				fi.MoveTo(newname);
			}
		}
	}
}
