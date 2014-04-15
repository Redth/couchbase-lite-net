using System;
using System.Linq;
using PCLStorage;

namespace Couchbase.Lite
{
	public enum SearchOption {
		TopDirectoryOnly,
		AllDirectories
	}

	public class DirectoryInfo
	{
		IFolder root;
		IFolder folder;

		public DirectoryInfo (string name)
		{
			root = FileSystem.Current.LocalStorage;

			folder = root.CreateFolderAsync (name,
				CreationCollisionOption.OpenIfExists).Result;
		}

		public bool Exists {
			get { return true; }
		}

		public string FullName {
			get {
				return "";
			}
		}

		public void Create()
		{
		}

		public void Refresh()
		{
		}

		public void Delete(bool recursive = false)
		{

		}

		public void MoveTo(string destinationPath)
		{

		}

		public FileInfo[] EnumerateFiles(string pattern, SearchOption opt)
		{
			var files = root.GetFilesAsync ().Result;

			return (from f in files
				where System.Text.RegularExpressions.Regex.IsMatch (f.Name, pattern)
				select new FileInfo(f.Name)).ToArray () ?? new FileInfo[] { };
		}
	}

	public class FileInfo 
	{
		public FileInfo(string name)
		{
			Name = name;
		}

		public bool Exists {
			get {
				return true;
			}
		}

		public string Name {
			get;
			private set;
		}

		public string FullName {
			get { return Name; }
		}

		public bool MoveTo(string name)
		{
			return false;
		}


		public string CopyTo(string destination)
		{

		}

	}
}

