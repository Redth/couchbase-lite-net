using System;
using System.Linq;
using PCLStorage;

namespace Couchbase.Lite
{
	public enum SearchOption {
		TopDirectoryOnly,
		AllDirectories
	}

	public class FileStorage
	{
		static IFolder root;

		public static IFolder Root {
			get {
				if (root == null)
					root = FileSystem.Current.LocalStorage;
				return root;
			}
		}
	}

	public class DirectoryInfo
	{
		IFolder folder;

		public DirectoryInfo (string name)
		{
			folder = FileStorage.Root.CreateFolderAsync (name,
				CreationCollisionOption.OpenIfExists).Result;

			//TODO: Handle LastWriteTimeUtc? 
			LastWriteTimeUtc = DateTime.MinValue;
		}

		public bool Exists {
			get { return true; }
		}

		public string FullName {
			get {
				return "";
			}
		}

		public DateTime LastWriteTimeUtc {
			get;
			set;
		}

		public void Create()
		{
			//Will already be created
		}

		public void Refresh()
		{
		}

		public void Delete(bool recursive = false)
		{
			folder.DeleteAsync ().RunSynchronously();
		}

		public void MoveTo(string destinationPath)
		{
			//TODO: Implement MoveTo
		}

		public FileInfo[] EnumerateFiles(string pattern, SearchOption opt)
		{
			var files = FileStorage.Root.GetFilesAsync ().Result;

			return (from f in files
				where System.Text.RegularExpressions.Regex.IsMatch (f.Name, pattern)
				select new FileInfo(f.Name)).ToArray () ?? new FileInfo[] { };
		}
	}

	public class FileInfo 
	{
		IFile file;

		public FileInfo(string name)
		{
			FullName = name;

			file = FileStorage.Root.GetFileAsync (name).Result;

			//TODO: Handle LastWriteTimeUtc ?
			LastWriteTimeUtc = DateTime.MinValue;
		}

		public bool Exists {
			get {
				return file != null && FileStorage.Root.CheckExistsAsync(Name).Result == ExistenceCheckResult.FileExists;
			}
		}

		public string Name {
			get {
				return Path.GetFileName (FullName);
			}
		}

		public string FullName {
			get;
			private set;
		}
			
		public int Length { 
			get { return 0; }
		}

		public DateTime LastWriteTimeUtc {
			get;
			set;
		}

		public bool MoveTo(string name)
		{
			return false;

		}

		public string CopyTo(string destination)
		{
			return string.Empty;
			//using (var 
		}

	}

	public class Directory
	{
		public static bool Delete(string path, bool recursive = false)
		{
			return true;
		}

		public static bool Exists(string path)
		{
			return true;
		}

		public static void CreateDirectory(string path)
		{

		}

		public static string[] GetFileSystemEntries(string path)
		{
			return null;
		}

		public static string[] GetDirectories(string path)
		{
			return null;
		}

		public static string[] GetFiles(string path)
		{
			return null;
		}
	}

	public class File
	{

		public static bool Create(string path)
		{
			//TODO: Just create new empty file
			return true;
		}

		public static bool Exists(string path)
		{
			return true;
		}

		public static bool Delete(string path)
		{
			return false;
		}

		public static bool Move(string path, string newPath)
		{
			return true;
		}

		public static System.IO.Stream OpenStream(string path, bool write = false)
		{
			return null;
		}
	}

	public class Path
	{
		public static char PathSeparator {
			get {
				return PortablePath.DirectorySeparatorChar;
			}
		}

		public static char DirectorySeparatorChar {
			get {
				return PortablePath.DirectorySeparatorChar;
			}
		}

		public static char AltDirectorySeparatorChar {
			get {
				return PortablePath.DirectorySeparatorChar;
			}
		}

		public static char VolumeSeparatorChar {
			get {
				return PortablePath.DirectorySeparatorChar;
			}
		}

		public static string GetFileNameWithoutExtension(string fullName) 
		{
			var fileName = GetFileName (fullName);

			var ext = "";
			//TODO: Get extension

			return ext;
		}

		public static string Combine(params string[] paths)
		{
			return PortablePath.Combine(paths);
		}

		public static string GetDirectoryName(string fullName)
		{
			return string.Empty;
		}

		public static string GetFileName(string fullPath)
		{
			return string.Empty;
		}
			
		public static string GetFullPath(string path)
		{
			return path;
		}

		public static string GetTempPath()
		{
			return string.Empty;
		}

		public static string GetTempFileName()
		{
			return string.Empty;
		}
	}

}

