// Inspired by Nick Gravelyn's UnityConstantsGenerator
namespace ConstantsGenerator {
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Text;
    using System.IO;

    public class ConstantsWriter : IDisposable {
        StringBuilder _stringBuilder;
        int _indentLevel;
        string _filePath;
        int _spacesPerIndent;

        string ChooseFilePath (string name, string extension) {
            // Try to find an existing file in the project called name+".cs"
            string filePath = null;
            string extensionWildcard = string.Format("*{0}", extension);
            foreach (var file in Directory.GetFiles(Application.dataPath, extensionWildcard, SearchOption.AllDirectories)) {
                if (Path.GetFileNameWithoutExtension(file) == name) {
                    filePath = file;
                    break;
                }
            }

            string nameWithExtension = string.Format("{0}{1}", name, extension);
            // If no such file exists yet, use a save window to ask the user for a folder in which to save the file.
            if (string.IsNullOrEmpty(_filePath)) {
                string prompt = string.Format("Choose location for {0}", nameWithExtension);
                string directory = EditorUtility.OpenFolderPanel(prompt, Application.dataPath, "");

                // Canceled choose? Do nothing.
                if (string.IsNullOrEmpty(directory)) {
                    return null;
                }

                filePath = Path.Combine(directory, nameWithExtension);
            }
            return filePath;
        }

        public ConstantsWriter (string name, string extension = ".cs", int spacesPerIndent = 4, string path = null, bool includeHeader = true) {
            _stringBuilder = new StringBuilder();

            _spacesPerIndent = spacesPerIndent;
            if (path != null) {
                path = Path.Combine(path, string.Format("{0}{1}", name, extension));
            }
            _filePath = path ?? ChooseFilePath(name, extension);
            if (_filePath == null) { return; }

            _indentLevel = 0;
            if (includeHeader) {
                _stringBuilder.AppendFormat("/* This file ({0}{1}) is auto-generated. Modifications are not saved. */\n", name, extension);
                _stringBuilder.AppendLine();
            }


            AssetDatabase.Refresh();
        }

        public void Dispose () {
            if (_filePath == null) {
                Debug.Log("Aborting!");
                return;
            }

            if (!TabsOK(_indentLevel, isEnd: true)) {
                Debug.Log("Writing Anyway: " + _filePath);
            }

            // Write out our file
            using (var writer = new StreamWriter(_filePath)) {
                writer.Write(_stringBuilder);
            }
        }

        public void WriteLine (string value, params object[] args) {
            AddTabbedFormattedLine(_stringBuilder, _indentLevel, value, args);
        }

        public void Write (string value) {
            AddTabbed(_stringBuilder, _indentLevel, value);
        }

        public void WriteLine (string value) {
            AddTabbedLine(_stringBuilder, _indentLevel, value);
        }

        public void WriteLine () {
            _stringBuilder.AppendLine();
        }

        private void AddTabbedLine (StringBuilder stringBuilder, int tabCount, string value) {
            if (!TabsOK(tabCount)) { return; }
            stringBuilder.Append(' ', tabCount * _spacesPerIndent);
            stringBuilder.AppendLine(value);
        }

        private void AddTabbed (StringBuilder stringBuilder, int tabCount, string value) {
            if (!TabsOK(tabCount)) { return; }
            stringBuilder.Append(' ', tabCount * _spacesPerIndent);
            stringBuilder.Append(value);
        }

        private void AddTabbedFormattedLine (StringBuilder stringBuilder, int tabCount, string value, params object[] args) {
            if (!TabsOK(tabCount)) { return; }
            stringBuilder.Append(' ', tabCount * _spacesPerIndent);
            stringBuilder.AppendFormat(value, args);
            stringBuilder.Append("\n");
        }

        private bool TabsOK (int tabCount, bool isEnd = false) {
            if (tabCount < 0 || (isEnd && tabCount != 0)) {
                Debug.LogError("Tabs unbalanced! TabLevel: " + tabCount);
                return false;
            }
            return true;
        }

        public void Indent () {
            _indentLevel++;
        }

        public void UnIndent () {
            _indentLevel--;
        }

        // Takes in a string and makes it safe to use for a C# variable name.
        // This just means stripping out spaces/? and prefixing with a "_" character
        // if the string starts with a number. It's not the most robust, but should handle most cases just fine.
        public static string MakeSafeForCode (string str) {
            str = str.Replace(" ", "");
            str = str.Replace("?", "");
            if (char.IsDigit(str[0])) {
                str = "_" + str;
            }
            return str;
        }
    }
}