namespace ConstantsGenerator {
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Collections.Generic;

    public static class SublimeProjectsGenerator {
        [MenuItem("Edit/Generate Sublime Project File")]
        public static void Generate () {

            var projectPath = Directory.GetParent(Application.dataPath); // Strips off Assets
            string projectName = projectPath.Name;
            string projectLocationPath = Directory.GetParent(projectPath.FullName).FullName;
            using (var writer = new ConstantsWriter(projectName,
                                                    extension: ".sublime-project",
                                                    spacesPerIndent: 2,
                                                    path: projectLocationPath)) {
                writer.WriteLine("{"); // open main
                writer.Indent();

                writer.WriteLine("\"folders\":");
                writer.WriteLine("[ {{ \"folder_exclude_patterns\": [ \"{0}/Library\" ]", projectName);
                writer.Indent();
                writer.WriteLine(", \"file_exclude_patterns\": [ \"*.meta\" ]");
                writer.WriteLine(", \"path\": \".\"");
                writer.WriteLine("}"); // close folder_exclude_patterns
                writer.UnIndent();

                writer.WriteLine("]"); // close folders
                writer.WriteLine(", \"solution_file\": \"{0}/{0}.sln\"", projectName);
                writer.UnIndent();
                writer.WriteLine("}"); // close main
            }
        }
    }
}