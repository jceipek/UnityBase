namespace ConstantsGenerator {
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;

    public static class UnityConstantsGenerator {
        static Dictionary<int, string> SceneIdsToNames {
            get {
                var idsToNames = new Dictionary<int, string>();
                var scenes = EditorBuildSettings.scenes;
                for (int sceneId = 0; sceneId < scenes.Length; sceneId++) {
                    var scene = scenes[sceneId];
                    var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    idsToNames.Add(sceneId, sceneName);
                }
                return idsToNames;
            }
        }

        const string _name = "UnityConstants";

        [MenuItem("Edit/Generate " + _name + ".cs")]
        public static void Generate () {
            using (var writer = new ConstantsWriter(_name)) {
                writer.WriteLine("namespace " + _name + " {"); // open namespace
                writer.Indent();

                writer.WriteLine("public static class Levels {"); // open levels
                writer.Indent();
                foreach (var sceneIdToName in SceneIdsToNames) {
                    var id = sceneIdToName.Key;
                    var name = sceneIdToName.Value;
                    writer.WriteLine("public const int {0} = {1};",
                                     ConstantsWriter.MakeSafeForCode(name),
                                     id);
                }

                writer.WriteLine("public enum E {"); // open levels enum
                writer.Indent();
                foreach (var sceneIdToName in SceneIdsToNames) {
                    var id = sceneIdToName.Key;
                    var name = sceneIdToName.Value;
                    writer.WriteLine("{0} = {1},",
                                     ConstantsWriter.MakeSafeForCode(name),
                                     id);
                }
                writer.UnIndent();
                writer.WriteLine("};"); // close levels enum
                writer.WriteLine();

                writer.UnIndent();
                writer.WriteLine("}"); // close levels

                writer.WriteLine();



                // Write out the tags
                writer.WriteLine("public static class Tags {");
                writer.Indent();
                foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags) {
                    writer.WriteLine("/// <summary>");
                    writer.WriteLine("/// Name of tag '{0}'.", tag);
                    writer.WriteLine("/// </summary>");
                    writer.WriteLine("public const string {0} = \"{1}\";", ConstantsWriter.MakeSafeForCode(tag), tag);
                }
                writer.UnIndent();
                writer.WriteLine("}");
                writer.WriteLine();

                // Write out sorting layers
                // var sortingLayerNames = UnityToolbag.SortingLayerHelper.sortingLayerNames;
                // if (sortingLayerNames != null) {
                //     writer.WriteLine("public static class SortingLayers {");
                //     writer.Indent();
                //     foreach (var name in sortingLayerNames) {
                //         int id = UnityToolbag.SortingLayerHelper.GetSortingLayerIDForName(name);
                //         writer.WriteLine("/// <summary>");
                //         writer.WriteLine("/// ID of sorting layer '{0}'.", name);
                //         writer.WriteLine("/// </summary>");
                //         writer.WriteLine("public const int {0} = {1};", ConstantsWriter.MakeSafeForCode(name), id);
                //     }
                //     writer.UnIndent();
                //     writer.WriteLine("}");
                //     writer.WriteLine();
                // }

                // Write out layers
                writer.WriteLine("public static class Layers {");
                writer.Indent();
                for (int i = 0; i < 32; i++) {
                    string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
                    if (!string.IsNullOrEmpty(layer)) {
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine("/// Index of layer '{0}'.", layer);
                        writer.WriteLine("/// </summary>");
                        writer.WriteLine("public const int {0} = {1};", ConstantsWriter.MakeSafeForCode(layer), i);
                    }
                }
                writer.WriteLine();
                for (int i = 0; i < 32; i++) {
                    string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
                    if (!string.IsNullOrEmpty(layer)) {
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine("/// Bitmask of layer '{0}'.", layer);
                        writer.WriteLine("/// </summary>");
                        writer.WriteLine("public const int {0}Mask = 1 << {1};", ConstantsWriter.MakeSafeForCode(layer), i);
                    }
                }
                writer.UnIndent();
                writer.WriteLine("}");
                writer.WriteLine();

                writer.UnIndent();
                writer.Write("}"); // close namespace
            }
        }
    }
}