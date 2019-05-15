using CommandLine;
using Sisyphus.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sisyphus.Commands.Base
{
    public abstract class ProjectFileCommand : BaseCommand
    {
        [Option('i', "input", HelpText = "Input files to be processed.")]
        public IEnumerable<string> Input { get; set; }

        [Option('r', "recursive", HelpText = "Search recursively.")]
        public bool IsRecursive { get; set; }

        public override (bool isSuccess, SError error) Run()
        {
            Vlog("Will sort!");

            var input = Input ?? new List<string>();

            bool noInputs = input.Count() == 0;
            if (noInputs)
            {
                Vlog("No project files specified.");
            }

            if (noInputs || input.Count() == 1 && IsRecursive)
            {
                var searchOption = noInputs ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

                var directory = input.FirstOrDefault() ?? Environment.CurrentDirectory;
                Vlog($"Searching for project files in directory '{directory}'");

                var files = Directory.GetFiles(directory, "*.csproj", searchOption)
                    .Concat(Directory.GetFiles(directory, "*.vbproj", searchOption));
                foreach (var file in files)
                {
                    ActOnProject(file);
                }
            }
            else
            {
                if (File.Exists(input.First()))
                {
                    ActOnProject(input.First());
                }
                else
                {
                    return Error($"File not found: {input.First()}");
                }
            }

            return Success;
        }

        private void ActOnProject(string filePath)
        {
            Vlog(filePath);

            XDocument document = XDocument.Load(filePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            XNamespace msBuildNamespace = document.Root.GetDefaultNamespace();
            XName itemGroupName = XName.Get("ItemGroup", msBuildNamespace.NamespaceName);

            // only consider the top-level item groups, otherwise stuff inside Choose, Targets etc. will be broken
            var itemGroups = document.Root.Elements(itemGroupName).ToArray();

            var shouldSave = ActOnProject(ref itemGroups);
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

        protected abstract bool ActOnProject(ref XElement[] itemGroups);
       
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
