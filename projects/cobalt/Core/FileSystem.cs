using System;
using System.IO;
using System.Text;

using Path = System.String;

namespace Cobalt.Core
{
    public enum ESaveMode
    {
        Append,
        Truncate
    }

    public class FileSystem
    {
        public static bool Exists(Path path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public static bool IsFile(Path path)
        {
            if (!Exists(path))
                return false;

            FileAttributes attribs = File.GetAttributes(path);
            return !attribs.HasFlag(FileAttributes.Directory);
        }

        public static bool IsDirectory(Path path)
        {
            if (!Exists(path))
                return false;

            FileAttributes attribs = File.GetAttributes(path);
            return attribs.HasFlag(FileAttributes.Directory);
        }

        public static Path[] GetFilePaths(Path directory, bool recursive)
        {
            return GetFilePaths(directory, recursive, "*.*");
        }

        public static Path[] GetFilePaths(Path directory, bool recursive, string searchPattern)
        {
            if (!Exists(directory) || !IsDirectory(directory))
            {
                Logger.Log.Error("Path doest not exist or is not a directory.");
                return null;
            }

            return Directory.GetFiles(directory, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static StreamReader LoadFileToStream(Path file)
        {
            return !Exists(file) ? null : new StreamReader(file);
        }

        public static string LoadFileToString(Path file)
        {
            return !Exists(file) ? "" : File.ReadAllText(file);
        }

        public static byte[] LoadFileToBytes(Path file)
        {
            return !Exists(file) ? null : File.ReadAllBytes(file);
        }
    }
}
