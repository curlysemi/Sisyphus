using ByteDev.DotNet.Solution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Sisyphus.Helpers
{
    public static class SolutionFileHelper
    {
        public static Dictionary<string, string> GetRelativeProjectFileNamePathMappings(string solutionFilePath)
        {
            var namePathMappings = new Dictionary<string, string>();

            var solution = DotNetSolution.Load(solutionFilePath);

            if (solution != null)
            {
                foreach (var project in solution.Projects)
                {
                    namePathMappings[project.Name] = project.Path;
                }
            }

            return namePathMappings;
        }

        public static List<string> GetRelativeProjectPaths(string solutionFilePath)
        {
            var paths = new List<string>();

            var solution = DotNetSolution.Load(solutionFilePath);

            if (solution?.Projects?.Any() == true)
            {
                paths.AddRange(solution.Projects.Select(s => s.Path));
            }

            return paths;
        }

        public static List<string> GetAbsoluteProjectPaths(string solutionFilePath)
        {
            var paths = new List<string>();

            var parentDir = Path.GetDirectoryName(solutionFilePath);
            var solution = DotNetSolution.Load(solutionFilePath);

            if (solution?.Projects?.Any() == true)
            {
                // These paths are relative to the solution file, which isn't necessarily in the same directory as our git repo . . .
                // So let's just make the paths absolute.
                paths.AddRange(solution.Projects.OrderBy(p => p.Name).Select(s => Path.Join(parentDir, s.Path)));
            }

            return paths;
        }
    }
}
