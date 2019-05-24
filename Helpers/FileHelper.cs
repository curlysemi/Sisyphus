using Sisyphus.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sisyphus.Helpers
{
    public static class FileHelper
    {
        // https://stackoverflow.com/a/340454
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

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
                    path = JoinInner(path, p);
                }
            }

            return path;
        }

        private static string JoinInner(string path, string pathB)
        {
            var result = path;
            if (!result.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                result += Path.DirectorySeparatorChar;
            }
            result += pathB;

            return result;
        }
    }
}
