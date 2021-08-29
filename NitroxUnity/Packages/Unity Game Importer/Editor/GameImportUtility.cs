#define UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.uTinyRipper;
using UnityEditor;
using UnityEngine;
using uTinyRipper;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace PassivePicasso.GameImporter
{
    public class GameImportUtility : ThunderKitSetting
    {
        private ClassIDType[] AllClassIDTypes = (Enum.GetValues(typeof(ClassIDType)) as ClassIDType[]).OrderBy(c => $"{c}").ToArray();
        public ClassIDType[] ClassIDTypes = (Enum.GetValues(typeof(ClassIDType)) as ClassIDType[]).OrderBy(c => $"{c}").ToArray();

        private ListView typeList;
        private ListView addTypeList;
        private string searchValue;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            var importUtilitySo = new SerializedObject(GetOrCreateSettings<GameImportUtility>());

            var importerRoot = new VisualElement();
#if UNITY_2018
            importerRoot.AddStyleSheetPath("Packages/com.passivepicasso.unitygameimporter/Editor/UIToolkit/UnityGameImporter.uss");
#else
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/de.jannify.unitygameimporter/Editor/UIToolkit/UnityGameImporter.uss");
            importerRoot.styleSheets.Add(styleSheet);
#endif

            var typesField = new VisualElement();
            typeList = new ListView(ClassIDTypes, (int)EditorGUIUtility.singleLineHeight, MakeTypesItem, BindTypesItem);
            var typesLabel = new Label($"Exported {ObjectNames.NicifyVariableName(nameof(ClassIDTypes))}");

            typesField.Add(typesLabel);
            typesField.Add(typeList);
            typesField.AddToClassList("grow");
            typesField.AddToClassList("types-field");
            typeList.AddToClassList("types-field-list");
            typeList.AddToClassList("grow");
            typeList.onItemChosen += OnRemoveItem;

            var addTypesField = new VisualElement();
            addTypeList = new ListView(AllClassIDTypes, (int)EditorGUIUtility.singleLineHeight, MakeTypesItem, BindAllTypesItem);
            var addTypesLabel = new Label($"{ObjectNames.NicifyVariableName(nameof(AllClassIDTypes))}");
            var searchElement = new VisualElement();
            searchElement.AddToClassList("searchfield");
            var searchLabel = new Label("Search");
            var searchField = new TextField();
            searchField.AddToClassList("grow");
#if UNITY_2018
            searchField.OnValueChanged(OnSearchChanged);
#else
            searchField.RegisterValueChangedCallback(OnSearchChanged);
#endif
            searchElement.Add(searchLabel);
            searchElement.Add(searchField);

            addTypesField.Add(addTypesLabel);
            addTypesField.Add(searchElement);
            addTypesField.Add(addTypeList);
            addTypesField.AddToClassList("grow");
            addTypesField.AddToClassList("alltypes-field");
            addTypeList.AddToClassList("alltypes-field-list");
            addTypeList.AddToClassList("grow");
            addTypeList.onItemChosen += OnAddItem;

            importerRoot.Add(typesField);
            importerRoot.Add(addTypesField);
            rootElement.Add(importerRoot);

            UpdateAllClassIDTypes();
            rootElement.Bind(importUtilitySo);
        }

        private void UpdateClassIDTypes(object obj, bool remove)
        {
            IEnumerable<ClassIDType> enumer = remove ? ClassIDTypes.Where(cid => !cid.Equals((ClassIDType)obj)) : ClassIDTypes.Append((ClassIDType)obj);
            ClassIDTypes = enumer.OrderBy(cid => $"{cid}").ToArray();

            typeList.itemsSource = ClassIDTypes;
            UpdateAllClassIDTypes();
        }

        private void UpdateAllClassIDTypes()
        {
            var acidt = Enum.GetValues(typeof(ClassIDType))
                            .OfType<ClassIDType>()
                            .Where(cid => !ClassIDTypes.Contains(cid));

            if (!string.IsNullOrWhiteSpace(searchValue))
                acidt = acidt.Where(c => $"{c}".IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) > -1);

            AllClassIDTypes = acidt.OrderBy(c => $"{c}").ToArray();

            addTypeList.itemsSource = AllClassIDTypes;
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            searchValue = evt.newValue;
            UpdateAllClassIDTypes();
        }

        private void OnRemoveItem(object obj) => UpdateClassIDTypes(obj, true);
        private void OnAddItem(object obj) => UpdateClassIDTypes(obj, false);

        private VisualElement MakeTypesItem() => new Label();

        private void BindTypesItem(VisualElement element, int index)
        {
            if (element is Label label)
            {
                label.text = $"{ClassIDTypes[index]}";
            }
        }

        private void BindAllTypesItem(VisualElement element, int index)
        {
            if (!(element is Label label))
                return;

            label.text = $"{AllClassIDTypes[index]}";
        }

        [InitializeOnLoadMethod]
        private static void InitializeImporter()
        {
            GetOrCreateSettings<GameImportUtility>();
        }

        [MenuItem("Tools/SubnauticaImporter/Asset Importer", false, -20)]
        private static void Import()
        {
            SimpleRipperInterface ripper = CreateInstance<SimpleRipperInterface>();
            ThunderKitSettings tkSettings = GetOrCreateSettings<ThunderKitSettings>();
            GameImportUtility importUtility = GetOrCreateSettings<GameImportUtility>();
            ProgressBarLogger progressBarLogger = new ProgressBarLogger();

            try
            {
                AssetDatabase.StartAssetEditing();

                ripper.Load(tkSettings.GamePath, importUtility.ClassIDTypes, Platform.StandaloneWin64Player, TransferInstructionFlags.AllowTextSerialization, progressBarLogger);

                //Moving Assets to Subnautica package
                string destPath = Path.Combine(Environment.CurrentDirectory, "Assets", "Subnautica");
                Directory.CreateDirectory(destPath);

                foreach (string file in Directory.GetFiles(Application.dataPath))
                {
                    string filename = file.Split('\\').Last();
                    if (filename.Equals("Nitrox.meta") || filename.Equals("ThunderKitSettings.meta") || filename.Equals("StreamingAssets.meta") ||
                        filename.Equals("csc.rsp") || filename.Equals("csc.rsp.meta"))
                    {
                        continue;
                    }

                    File.Move(file, Path.Combine(destPath, filename));
                }

                string[] directories = Directory.GetDirectories(Application.dataPath);
                for (int index = 0; index < directories.Length; index++)
                {
                    string directory = directories[index];
                    string dirName = directory.Split('\\').Last();
                    if (dirName.Equals("Nitrox") || dirName.Equals("ThunderKitSettings") || dirName.Equals("StreamingAssets"))
                    {
                        continue;
                    }

                    progressBarLogger.Log(uTinyRipper.LogType.Info, LogCategory.General, "Moving Assets to Package", (float)index / directories.Length);
                    Directory.Move(directory, Path.Combine(destPath, dirName));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                progressBarLogger.Dispose();
            }
        }
    }
}
