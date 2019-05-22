using Sisyphus.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sisyphus.Helpers
{
    public static class FileHelper
    {
        public static string GetParentDirectory(string path)
        {
            var parentDirectory = Path.GetDirectoryName(path);
            if (parentDirectory == null)
            {
                throw new DirectoryNotFoundException($"No parent directory for '{path}'");
            }

            return parentDirectory;
        }

        public static string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public static FileType? GetFileType(string path)
        {
            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path);
                switch (extension)
                {
                    case ".sln":
                        return FileType.SolutionFile;
                    case ".csproj":
                    case ".vbproj":
                    case ".fsproj": // TODO: Determine if F# is actually supported . . .
                        return FileType.ProjectFile;
                    default:
                        return FileType.Unknown;
                }
            }
            else
            {
                return null;
            }
        }

        public static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static HashSet<string> GetFilesOnDisk(string directoryPath)
        {
            throw new NotImplementedException();
        }

        public static string Join(params string[] paths)
        {
            string path = paths?.FirstOrDefault();
            if (paths?.Length > 1)
            {
                foreach (var p in paths.Skip(1))
                {
                    path = Path.Join(path, p);
                }
            }

            return path;
        }
    }
}
