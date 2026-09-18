using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace WoollyArena.Editor
{
    public static class PhoneBuild
    {
        [MenuItem("Woolly/Build/Export current iPhone")]
        public static void Export()
        {
            Directory.CreateDirectory("Logs");
            const string reportPath = "Logs/phone-build-report.txt";
            File.WriteAllText(reportPath, "STARTED build 11\n");
            int checks = 0;
            Action<bool, string> check = (ok, name) =>
            {
                if (!ok) throw new InvalidOperationException(name);
                checks++;
            };
            SurvivalBuildChecks.Run(check);
            RunCheckpointChecks.Run(check);
            StartupLoadingSetup.Prepare();
            CombatLobbyChecks.Run();
            KayKitScaleChecks.Run();
            PhonePresentationCheck.Run();
            PlayerSettings.iOS.buildNumber = "11";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            AssetDatabase.SaveAssets();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = StartupLoadingSetup.Scenes,
                locationPathName = "Builds/iOS",
                target = BuildTarget.iOS,
                options = BuildOptions.Development
            });
            File.WriteAllText(reportPath, report.summary.result + "\nBuild: 11\nModel checks: " + checks +
                "\nErrors: " + report.summary.totalErrors + "\nDuration: " + report.summary.totalTime + "\n");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Phone export failed; see Unity build log.");
        }
    }
}
