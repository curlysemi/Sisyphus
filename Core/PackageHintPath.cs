using System;
using System.Collections.Generic;
using System.Text;

namespace Sisyphus.Core
{
    class PackageHintPath
    {
        string Path { get; set; }
        public PackageHintPath(string path)
        {
            Path = path;
        }
    }
}
