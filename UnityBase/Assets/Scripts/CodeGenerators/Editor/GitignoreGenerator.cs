namespace ConstantsGenerator {
    using UnityEditor;
    using UnityEngine;
    using System.IO;

    public static class GitignoreGenerator {
        [MenuItem("Edit/Generate Gitignore File")]
        public static void Generate () {
            var projectPath = Directory.GetParent(Application.dataPath); // Strips off Assets
            string projectLocationPath = Directory.GetParent(projectPath.FullName).FullName;
            using (var writer = new ConstantsWriter("",
                                                    extension: ".gitignore",
                                                    spacesPerIndent: 2,
                                                    path: projectLocationPath,
                                                    includeHeader: false)) {
                writer.WriteLine("# =============== #");
                writer.WriteLine("# Unity generated #");
                writer.WriteLine("# =============== #");
                writer.WriteLine();
                writer.WriteLine("*/[Ll]ibrary/");
                writer.WriteLine("*/[Tt]emp/");
                writer.WriteLine("*/[Oo]bj/");
                writer.WriteLine("*/[Bb]uild/");
                writer.WriteLine();
                writer.WriteLine("#Unity3D Generated File On Crash Reports");
                writer.WriteLine("sysinfo.txt");
                writer.WriteLine();
                writer.WriteLine("# ===================================== #");
                writer.WriteLine("# Visual Studio / MonoDevelop generated #");
                writer.WriteLine("# ===================================== #");
                writer.WriteLine("ExportedObj/");
                writer.WriteLine("*.svd");
                writer.WriteLine("*.userprefs");
                writer.WriteLine("*.csproj");
                writer.WriteLine("*.pidb");
                writer.WriteLine("*.suo");
                writer.WriteLine("*.sln");
                writer.WriteLine("*.user");
                writer.WriteLine("*.unityproj");
                writer.WriteLine("*.booproj");
                writer.WriteLine();
                writer.WriteLine("# ============ #");
                writer.WriteLine("# OS generated #");
                writer.WriteLine("# ============ #");
                writer.WriteLine(".DS_Store");
                writer.WriteLine(".DS_Store?");
                writer.WriteLine("._*");
                writer.WriteLine(".Spotlight-V100");
                writer.WriteLine(".Trashes");
                writer.WriteLine("Icon\r\r");
                writer.WriteLine("ehthumbs.db");
                writer.WriteLine("Thumbs.db");
                writer.WriteLine();
                writer.WriteLine("*.sublime-workspace");
                writer.WriteLine("*.sublime-project");
            }
        }
    }
}
