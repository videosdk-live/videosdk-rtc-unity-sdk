using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace live.videosdk.Editor
{
    public static class SwiftPostProcess
    {
        [PostProcessBuild(int.MaxValue)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            string projPath = Path.Combine(buildPath, "Unity-Iphone.xcodeproj/project.pbxproj");
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
            string mainTargetGuid = proj.GetUnityMainTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif

            ConfigureBuildSettings(proj, mainTargetGuid);
            proj.WriteToFile(projPath);
        }

        private static void ConfigureBuildSettings(PBXProject proj, string targetGuid)
        {
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/UnityIosPlugin/Source/VideoSDKUnityPlugin.h");
            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "UnityIosPlugin-Swift.h");
            proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
            proj.AddBuildProperty(targetGuid, "FRAMERWORK_SEARCH_PATHS",
                "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            proj.AddBuildProperty(targetGuid, "DYLIB_INSTALL_NAME_BASE", "@rpath");
            proj.AddBuildProperty(targetGuid, "LD_DYLIB_INSTALL_NAME",
                "@executable_path/../Frameworks/$(EXECUTABLE_PATH)");
            proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
            proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "4.0");
            proj.AddBuildProperty(targetGuid, "COREML_CODEGEN_LANGUAGE", "Swift");
        }

    }
}