using System;
using System.Collections.Generic;
using System.Text;

namespace Sisyphus.Core
{
    public static class Constants
    {
        public const string GIT_DIR = ".git";

        /// <summary>
        /// This file is really intended to be JSON, but we also want to support comments, so we'll call it JS and strip out all comments and then treat as JSON
        /// </summary>
        public const string SISYPHUS_CONFIG_FILENAME = "Sisyphus.js";
    }
}
