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

			//TODO: Handle LastWriteTimeUtc? 
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

			//TODO: Handle LastWriteTimeUtc ?
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
				return '/';
			}
		}

		public static char DirectorySeparatorChar {
			get {
				return '/';
			}
		}

		public static char AltDirectorySeparatorChar {
			get {
				return '/';
			}
		}

		public static char VolumeSeparatorChar {
			get {
				return '/';
			}
		}

		public static string GetFileNameWithoutExtension(string fullName) 
		{
			return string.Empty;
		}

		public static string Combine(params string[] parts)
		{
			return string.Empty;
		}

		public static string GetDirectoryName(string fullName)
		{
			return string.Empty;
		}

		public static string GetFileName(string fullPath)
		{
			return string.Empty;
		}

		public static bool IsPathRooted(string path)
		{
			return false;
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

