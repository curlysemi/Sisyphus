using System;
using System.Collections.Generic;
using System.Text;

namespace Sisyphus.Core
{
    [Serializable]
    public class Config
    {
        public List<string> IgnorableFiles { get; set; } = new List<string>();
    }
}
