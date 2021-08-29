using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PassivePicasso.GameImporter.SN_Fixes;
using Unity.SharpZipLib.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.ThunderKit.GameImporter.Editor.SNFixes
{
    public class FixShader : ISNFix
    {
        public string GetTaskName() => TASK_NAME;
        public const string TASK_NAME = "Fixing Shaders";

        public void Run()
        {
            SNFixesUtility.ProgressBar.Update("", "InternalDeferredshadingcustomShaderFix");
            ApplyInternalDeferredshadingcustomShaderFix();
            SNFixesUtility.ProgressBar.Update("", "MarmosetUBERFix");
            ApplyMarmosetUBERFix();
            SNFixesUtility.ProgressBar.Update("", "UWEParticlesUBERFix");
            ApplyUWEParticlesUBERFix();
            SNFixesUtility.ProgressBar.Update("", "DefaultHolographicFix");
            ApplyDefaultHolographicShaderFix();
            SNFixesUtility.ProgressBar.Update("", "GUITextFix");
            ApplyGUITextShaderFix();
            SNFixesUtility.ProgressBar.Update("", "OverrideStandardShader");
            OverrideStandardShader();
        }

        public static void ApplyInternalDeferredshadingcustomShaderFix()
        {
            string path = SNFixesUtility.AssetPath + @"\Resources\internal-deferredshadingcustom.shader";

            List<string> lines = File.ReadAllLines(path).ToList();

            lines.RemoveAt(7);
            lines.Add("	Fallback \"Hidden/Internal-DeferredShading\"");
            lines.Add("}");

            File.WriteAllLines(path, lines);
        }

        public static void ApplyDefaultHolographicShaderFix()
        {
            string path = SNFixesUtility.AssetPath + @"\Resources\shaders\holographic.shader";

            string[] lines = File.ReadAllLines(path);

            lines[12] = "		_GlitchHeight (\"Height\", Range(0, 0.2)) = 0.001";

            File.WriteAllLines(path, lines);
        }

        public static void ApplyUWEParticlesUBERFix()
        {
            string path = SNFixesUtility.AssetPath + @"\Shader\UWEParticlesUBER.shader";

            List<string> lines = File.ReadAllLines(path).ToList();

            lines[51] = "	Fallback \"Particles/Standard Unlit\"";

            File.WriteAllLines(path, lines);
        }

        public static void ApplyMarmosetUBERFix()
        {
            string path = SNFixesUtility.AssetPath + @"\Shader\MarmosetUBER.shader";

            string text = File.ReadAllText(path);
            text = text.Replace("VertexLit", "Legacy Shaders/VertexLit");
            File.WriteAllText(path, text);
        }

        public static void ApplyGUITextShaderFix()
        {
            string path = SNFixesUtility.AssetPath + @"\Shader\GUIText Shader.shader";

            File.Delete(path);
            File.Delete(path + ".meta");
        }

        public static void OverrideStandardShader()
        {
            string downloadPath = Path.Combine(Environment.CurrentDirectory, "DownloadedBuiltinShaders");
            string extractPath = Path.Combine(downloadPath, "builtInShader-unpacked");
            string downloadFile = Path.Combine(downloadPath, "builtInShader.zip");

            if (Directory.Exists(downloadPath))
            {
                Directory.Delete(downloadPath, true);
            }

            Directory.CreateDirectory(downloadPath);

            DownloadBuiltInShader(downloadFile);

            ZipUtility.UncompressFromZip(downloadFile, null, extractPath);

            Dictionary<string, string> builtInShaderByPath = new Dictionary<string, string>();
            Dictionary<string, string> snShaderByPath = new Dictionary<string, string>();

            RegisterShader(builtInShaderByPath, extractPath);
            RegisterShader(snShaderByPath, SNFixesUtility.AssetPath);

            for (int i = 0; i < snShaderByPath.Count; i++)
            {
                KeyValuePair<string, string> keyValuePair = snShaderByPath.ElementAt(i);

                SNFixesUtility.ProgressBar.Update($"Fixing Shader: {Path.GetFileName(keyValuePair.Value)}", null, (float)i / snShaderByPath.Count);

                if (builtInShaderByPath.ContainsKey(keyValuePair.Key))
                {
                    byte[] byteCache = File.ReadAllBytes(builtInShaderByPath[keyValuePair.Key]);
                    File.WriteAllBytes(keyValuePair.Value, byteCache);
                }
            }

            Directory.Delete(downloadPath, true);
        }

        private static void DownloadBuiltInShader(string filePath)
        {
            UnityWebRequest www = UnityWebRequest.Get("https://download.unity3d.com/download_unity/8e603399ca02/builtin_shaders-2019.2.17f1.zip");
            www.SendWebRequest();

            while (!www.isDone)
            {
                SNFixesUtility.ProgressBar.Update("Downloading Built-In-Shader", null, www.downloadProgress);

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    return;
                }
            }

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                return;
            }

            File.WriteAllBytes(filePath, www.downloadHandler.data);
        }

        private static string[] linesCache;

        private static void RegisterShader(Dictionary<string, string> dictionary, string path)
        {
            string[] builtInFiles = Directory.GetFiles(path, "*.shader", SearchOption.AllDirectories);
            for (int index = 0; index < builtInFiles.Length; index++)
            {
                SNFixesUtility.ProgressBar.Update("Registering Shader", null, (float)index / builtInFiles.Length);
                linesCache = File.ReadAllLines(builtInFiles[index]);

                foreach (string line in linesCache)
                {
                    if (line.TrimStart().StartsWith("Shader \""))
                    {
                        string shaderName = line.Split('\"')[1];
                        if (!dictionary.ContainsKey(shaderName))
                        {
                            dictionary.Add(shaderName, builtInFiles[index]);
                        }
                    }
                }
            }
        }
    }
}
