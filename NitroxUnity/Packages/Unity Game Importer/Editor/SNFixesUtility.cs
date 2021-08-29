using System;
using Packages.ThunderKit.GameImporter.Editor.SNFixes;
using PassivePicasso.GameImporter.SN_Fixes;
using ThunderKit.Common.Logging;
using UnityEditor;
using UnityEngine;

namespace Packages.ThunderKit.GameImporter.Editor
{
    public static class SNFixesUtility
    {
        public static readonly string AssetPath = Application.dataPath + "/Subnautica";
        public static ProgressBar ProgressBar;

        private static readonly ISNFix[] fixes = { new CleanShader(), new FixShader(), new UnityUIReference(), new RegisterAssetBundle() };

        [MenuItem("Tools/SubnauticaImporter/All SN-Asset Fixes", false, -10)]
        private static void RunFixes()
        {
            ProgressBar = new ProgressBar("Running all SN-Asset fixes");
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (ISNFix fix in fixes)
                {
                    ProgressBar.Update("", fix.GetTaskName());
                    fix.Run();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                ProgressBar.Dispose();
            }
        }
        
        private static void RunSelectedFix(string taskName, Action fix)
        {
            ProgressBar = new ProgressBar(taskName);
            try
            {
                AssetDatabase.StartAssetEditing();

                fix();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                ProgressBar.Dispose();
            }
        }

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/CleanShader", false, 0)]
        private static void CleanShaderFix() => RunSelectedFix(CleanShader.TASK_NAME, CleanShader.CleanShaderFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/InternalDeferredshadingcustomShaderFix", false, 10)]
        private static void ApplyInternalDeferredshadingcustomShaderFix() => RunSelectedFix(FixShader.TASK_NAME, FixShader.ApplyInternalDeferredshadingcustomShaderFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/UWEParticlesUBERFix", false, 11)]
        private static void ApplyUWEParticlesUBERFix() => RunSelectedFix(FixShader.TASK_NAME, FixShader.ApplyUWEParticlesUBERFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/MarmosetUBERFix", false, 12)]
        private static void ApplyMarmosetUBERFix() => RunSelectedFix(FixShader.TASK_NAME, FixShader.ApplyMarmosetUBERFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/GUITextShaderFix", false, 13)]
        private static void ApplyGUITextShaderFix() => RunSelectedFix(FixShader.TASK_NAME, FixShader.ApplyGUITextShaderFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/DefaultHolographicShaderFix", false, 14)]
        private static void DefaultHolographicShaderFix() => RunSelectedFix(FixShader.TASK_NAME, FixShader.ApplyDefaultHolographicShaderFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/OverrideStandardShader", false, 15)]
        private static void OverrideStandardShader() => RunSelectedFix(FixShader.TASK_NAME, FixShader.OverrideStandardShader);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/UnityUIReference", false, 20)]
        private static void UnityUIReferenceFix() => RunSelectedFix(UnityUIReference.TASK_NAME, UnityUIReference.UnityUIReferenceFix);

        [MenuItem("Tools/SubnauticaImporter/Selected Fixes/RegisterAssetsAsBundle", false, 30)]
        private static void RegisterAssetsAsBundle() => RunSelectedFix(RegisterAssetBundle.TASK_NAME, RegisterAssetBundle.RegisterAssetsAsBundle);
    }
}
