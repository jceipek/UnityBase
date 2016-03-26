namespace ConstantsGenerator {
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Collections.Generic;
    using System;
    using System.Reflection;

    public static class UnityAudioConstantsGenerator {

        private static Dictionary<string, string[]> MixersToParameterNames {
            get {
                // NOTE(JULIAN): Crazy reflection stuff because Unity hides ExposedAudioParameter[] :(
                var assembly = typeof(UnityEditor.Audio.AudioMixerEffectPlugin).Assembly;
                var ExposedAudioParameter = assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");
                var AudioMixerController = assembly.GetType("UnityEditor.Audio.AudioMixerController");

                var mixersToParameterNames = new Dictionary<string, string[]>();
                foreach (var file in Directory.GetFiles(Application.dataPath, "*.mixer", SearchOption.AllDirectories)) {
                    var relPath = file;
                    if (file.StartsWith(Application.dataPath, StringComparison.InvariantCulture)) {
                        relPath = "Assets" + file.Substring(Application.dataPath.Length);
                    }
                    var audioMixerController = AssetDatabase.LoadAssetAtPath(relPath, AudioMixerController);
                    var name = Path.GetFileNameWithoutExtension(file);

                    // NOTE(JULIAN): Some more reflection to grab the names of the ExposedAudioParameters
                    MethodInfo exposedParametersMethod = AudioMixerController.GetMethod("get_exposedParameters");
                    MethodInfo numExposedParametersMethod = AudioMixerController.GetMethod("get_numExposedParameters");
                    if (exposedParametersMethod == null) { continue; }
                    if (numExposedParametersMethod == null) { continue; }
                    var numExposedParameters = (int)numExposedParametersMethod.Invoke(audioMixerController, null);
                    var exposedParameters = (Array)exposedParametersMethod.Invoke(audioMixerController, null); // Actually of internal type ExposedAudioParameter[]
                    FieldInfo nameField = ExposedAudioParameter.GetField("name");
                    var exposedParameterNames = new string[numExposedParameters];
                    int i = 0;
                    foreach (var exposedParameter in exposedParameters) {
                        exposedParameterNames[i] = (string)nameField.GetValue(exposedParameter);
                        i++;
                    }

                    mixersToParameterNames[name] = exposedParameterNames;
                }
                return mixersToParameterNames;
            }
        }

        const string _name = "UnityAudioConstants";

        [MenuItem("Edit/Generate " + _name + ".cs")]
        public static void Generate () {
            using (var writer = new ConstantsWriter(_name)) {
                writer.WriteLine("namespace UnityAudioConstants {");
                writer.Indent();
                foreach (var mixerToParams in MixersToParameterNames) {
                    var mixerName = ConstantsWriter.MakeSafeForCode(mixerToParams.Key);
                    writer.WriteLine("namespace {0} {1}", mixerName, "{");
                    writer.Indent();
                    writer.WriteLine("public static class ExposedAudioParameters {");
                    writer.Indent();
                    foreach (var paramName in mixerToParams.Value) {
                        writer.WriteLine("public const string {0} = \"{1}\";",
                                         ConstantsWriter.MakeSafeForCode(paramName),
                                         paramName);
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