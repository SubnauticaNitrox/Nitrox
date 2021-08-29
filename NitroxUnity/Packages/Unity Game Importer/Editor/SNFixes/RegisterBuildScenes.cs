using PassivePicasso.GameImporter.SN_Fixes;
using UnityEditor;

namespace Packages.ThunderKit.GameImporter.Editor.SNFixes
{
    public class RegisterBuildScenes : ISNFix
    {
        public string GetTaskName() => TASK_NAME;
        public const string TASK_NAME = "Register scenes in build settings";

        public void Run() => RegisterAssetsAsBundle();

        private static readonly EditorBuildSettingsScene[] buildScenesByPath = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/StartScreen.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/XMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/Cleaner.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/Main.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/SubmarineScenes/EscapePod.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/SubmarineScenes/Aurora.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/SubmarineScenes/Cyclops.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/MenuEnvironment.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/Essentials.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/EmptyScene.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/SubmarineScenes/RocketSpace.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/EndCredits.unity", true),
            new EditorBuildSettingsScene("Assets/Subnautica/Scene/Scenes/EndCreditsSceneCleaner.unity", true)
        };

        public static void RegisterAssetsAsBundle()
        {
            EditorBuildSettings.scenes = buildScenesByPath;
        }
    }
}
