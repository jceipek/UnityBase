namespace ConstantsGenerator {
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;
    using System;

    public static class UnityAnimationConstantsGenerator {

        private static Dictionary<string, AnimatorControllerParameter[]> AnimatorsToParameters {
            get {
                var animatorsToParameters = new Dictionary<string, AnimatorControllerParameter[]>();
                foreach (var file in Directory.GetFiles(Application.dataPath, "*.controller", SearchOption.AllDirectories)) {
                    var relPath = file;
                    if (file.StartsWith(Application.dataPath, StringComparison.InvariantCulture)) {
                        relPath = "Assets" + file.Substring(Application.dataPath.Length);
                    }
                    var animator = AssetDatabase.LoadAssetAtPath(relPath,
                                                                 typeof(UnityEditor.Animations.AnimatorController)) as UnityEditor.Animations.AnimatorController;
                    if (animator != null) {
                        animatorsToParameters[animator.name] = animator.parameters;
                    }
                }
                return animatorsToParameters;
            }
        }

        const string _name = "UnityAnimationConstants";

        [MenuItem("Edit/Generate " + _name + ".cs")]
        public static void Generate () {
            using (var writer = new ConstantsWriter(_name)) {
                writer.WriteLine("namespace UnityAnimationConstants {");
                writer.Indent();
                foreach (var animatorToParams in AnimatorsToParameters) {
                    var animatorName = ConstantsWriter.MakeSafeForCode(animatorToParams.Key);
                    writer.WriteLine("namespace {0} {1}", animatorName, "{");
                    writer.Indent();
                    writer.WriteLine("public static class Parameters {");
                    writer.Indent();
                    foreach (var param in animatorToParams.Value) {
                        writer.WriteLine("public const int {0} = {1};",
                                         ConstantsWriter.MakeSafeForCode(param.name),
                                         param.nameHash);
                    }
                    writer.UnIndent();
                    writer.WriteLine("}");
                    writer.UnIndent();
                    writer.WriteLine("}");
                }
                writer.UnIndent();
                writer.WriteLine("}");
            }

        }

    }
}