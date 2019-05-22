using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisyphus.Core
{
    class DepReference
    {
        private Dictionary<string, string> _subAttributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private string GetValueOrNull(string key)
        {
            if (_subAttributes.ContainsKey(key))
            {
                return _subAttributes[key];
            }
            else
            {
                return null;
            }
        }

        public string OriginalString { get; private set; }

        public string Name { get; private set; }

        public string VersionString => GetValueOrNull(nameof(Version)); // This is intentional

        public string Culture => GetValueOrNull(nameof(Culture));

        public string PublicKeyToken => GetValueOrNull(nameof(PublicKeyToken));

        public string ProcessorArchitecture => GetValueOrNull(nameof(ProcessorArchitecture));

        public Version Version { get; set; }

        public DepHintPath HintPath { get; set; }

        public DepReference(string packageIncludeString, string hintPath)
        {
            OriginalString = packageIncludeString;

            var subAttributes = packageIncludeString.Split(", ").ToList();
            if (subAttributes?.Any() == true)
            {
                Name = subAttributes.FirstOrDefault();
                subAttributes.RemoveAt(0);
            }

            foreach (var remainingSubAttr in subAttributes)
            {
                var stuff = remainingSubAttr.Split("=");
                if (stuff.Count() == 2)
                {
                    _subAttributes[stuff[0]] = stuff[1];
                }
            }

            if (VersionString != null)
            {
                Version = new Version(VersionString);
            }

            if (hintPath != null)
            {
                HintPath = new DepHintPath(hintPath);
            }
        }
    }
}
