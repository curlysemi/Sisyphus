// Copyright 2015–2016 Kirill Osenkov
// https://github.com/KirillOsenkov/CodeCleanupTools/blob/master/SortProjectItems/SortProjectItems.cs
// https://github.com/KirillOsenkov/CodeCleanupTools/blob/master/RemoveDuplicateItems/RemoveDuplicateItems.cs

using Sisyphus.Core;
using Sisyphus.Helpers;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Sisyphus.Commands.Base
{
    internal abstract class PortedProjectFileOrSolutionFileCommand : ProjectFileOrSolutionFileCommand
    {
        protected override (bool isSuccess, SError error) HandleProject(Config config, string repoPath, string projectPath)
        {
            ActOnProject(config, projectPath);

            return Success;
        }

        private void ActOnProject(Config config, string filePath)
        {
            Vlog(filePath);

            var (document, itemGroups) = ProjectFileHelper.LoadProjectXml(filePath);

            var shouldSave = ActOnProject(config, ref itemGroups);
            if (shouldSave)
            {
                var originalBytes = File.ReadAllBytes(filePath);
                byte[] newBytes = null;

                using (var memoryStream = new MemoryStream())
                using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    document.Save(textWriter, SaveOptions.None);
                    newBytes = memoryStream.ToArray();
                }

                newBytes = SyncBOM(originalBytes, newBytes);

                if (!AreEqual(originalBytes, newBytes))
                {
                    File.WriteAllBytes(filePath, newBytes);
                }
            }
        }

        protected abstract bool ActOnProject(Config config, ref XElement[] itemGroups);

        private static byte[] SyncBOM(byte[] originalBytes, byte[] newBytes)
        {
            bool originalHasBOM = HasBOM(originalBytes);
            bool newHasBOM = HasBOM(newBytes);

            if (originalHasBOM && !newHasBOM)
            {
                var extended = new byte[newBytes.Length + 3];
                newBytes.CopyTo(extended, 3);
                BOM.CopyTo(extended, 0);
                newBytes = extended;
            }

            if (!originalHasBOM && newHasBOM)
            {
                var trimmed = new byte[newBytes.Length - 3];
                Array.Copy(newBytes, 3, trimmed, 0, trimmed.Length);
                newBytes = trimmed;
            }

            return newBytes;
        }

        // Byte order mark
        private static readonly byte[] BOM = { 0xEF, 0xBB, 0xBF };

        private static bool HasBOM(byte[] bytes)
        {
            if (bytes.Length >= 3 &&
                bytes[0] == BOM[0] &&
                bytes[1] == BOM[1] &&
                bytes[2] == BOM[2])
            {
                return true;
            }

            return false;
        }

        private bool AreEqual(byte[] left, byte[] right)
        {
            if (left == null)
            {
                return right == null;
            }

            if (right == null)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
