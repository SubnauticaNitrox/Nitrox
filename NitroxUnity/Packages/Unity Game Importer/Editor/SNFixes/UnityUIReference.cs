using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PassivePicasso.GameImporter.SN_Fixes;
using uTinyRipper;


namespace Packages.ThunderKit.GameImporter.Editor.SNFixes
{
    public class UnityUIReference : ISNFix
    {
        private const string SN_DEFAULT_GUID = "dc443db3e92b4983b9738c1131f555cb";
        private const string UNITY_DEFAULT_FILE_ID = "11500000";

        private static readonly Dictionary<string, string> fileIdToGuid = new Dictionary<string, string>()
        {
            //Missing: Selectable (maybe)
            { "708705254", "5f7201a12d95ffc409449d95f23cf332" },    // Text
            { "-765806418", "fe87c0e1cc204ed48ad3b37840f39efc" },   // Image
            { "-98529514", "1344c3c82d62a2a41a3576d8abb8e3ea" },    // RawImage
            { "1392445389", "4e29b1a8efbd4b44bb3f3716e73f07ff" },   // Button
            { "575553740", "d199490a83bb2b844b9695cbf13b01ef" },    // InputField
            { "-1200242548", "31a19414c41e5ae4aae2af33fee712f6" },  // Mask
            { "-146154839", "3312d7739989d2b4e91e6319e9a96d76" },   // RectMask2D
            { "-2061169968", "2a4db7a114972834c8e4117be1d82ba3" },  // Scrollbar
            { "1367256648", "1aa08ab6e0800fa44ae55d278d1423e3" },   // ScrollRect
            { "-1184210157", "2fafe2cfe61f6974895a912c3755e8f1" },  // ToggleGroup
            { "853051423", "0d0b652f32a2cc243917e4028fa0f046" },    // Dropdown
            { "-1249906722", "67db9e8f0e2ae9c40bc1e2b64352a6b4" },  // Slider
            { "2109663825", "9085046f02f69544eb97fd06b6048fe2" },   // Toggle

            { "-619905303", "76c392e42b5098c458856cdf6ecaaaa1" },   // EventSystem
            { "-1862395651", "d0b148fe25e99eb48b9724523833bab1" },  // EventTrigger
            { "1077351063", "4f231c4fb786f3946a6b90b886c48677" },   // StandaloneInputModule
            { "1301386320", "dc42784cf147c0c48a680349fa168899" },   // GraphicRaycaster

            { "-1254083943", "86710e43de46f6f4bac7c8e50813a599" },  // AspectRatioFitter
            { "1980459831", "0cd44c1031e13a943bb63640046fad76" },   // CanvasScaler
            { "1741964061", "3245ec927659c4140ac4f8d17403cc18" },   // ContentSizeFitter
            { "-2095666955", "8a8695521f0d02e499659fee002a26c2" },  // GridLayoutGroup
            { "-405508275", "30649d3a9faa99c48a7b1166b86bf2a0" },   // HorizontalLayoutGroup
            { "1679637790", "306cc8c2b49d7114eaa3623786fc2126" },   // LayoutElement
            { "1297475563", "59f8146938fff824cb5fd77236b75775" },   // VerticalLayoutGroup

            { "1573420865", "e19747de3f5aca642ab2be37e372fb86" },   // Outline
            { "-900027084", "cfabb0440166ab443bba8876756fdfa9" },   // Shadow
        };

        public string GetTaskName()=> TASK_NAME;
        public const string TASK_NAME ="Reassigning missing UnityEngine.UI references";

        public void Run() => UnityUIReferenceFix();


        public static void UnityUIReferenceFix()
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(SNFixesUtility.AssetPath, "*.unity", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(SNFixesUtility.AssetPath, "*.prefab", SearchOption.AllDirectories));

            for (int index = 0; index < files.Count; index++)
            {
                string file = files[index];
                SNFixesUtility.ProgressBar.Update(Path.GetFileName(file), null, (float)index / files.Count);
                ApplyFile(file);
            }
        }

        private static string[] lines;
        private static void ApplyFile(string path)
        {
            lines = File.ReadAllLines(path);

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                string lineTrimmed = line.TrimStart();

                if (lineTrimmed.StartsWith("m_Script:"))
                {
                    foreach (KeyValuePair<string, string> keyValuePair in fileIdToGuid)
                    {
                        if (lineTrimmed.StartsWith($"m_Script: {{fileID: {keyValuePair.Key}, guid: {SN_DEFAULT_GUID}, "))
                        {
                            lines[index] = line.Split(new[] { "m_Script" }, StringSplitOptions.None)[0] + $"m_Script: {{fileID: {UNITY_DEFAULT_FILE_ID}, guid: {keyValuePair.Value}, type: 3}}";
                        }
                    }
                }
            }

            File.WriteAllLines(path, lines);
        }
    }
}
