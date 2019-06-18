using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEditor.iOS.Xcode;

public class XcodePostProcessBuild
{
    [PostProcessBuildAttribute(1)]
     public static void OnPostProcessBuild(BuildTarget target, string path)
     {
         if (target == BuildTarget.iOS)
         {
             // Read.
             string projectPath = PBXProject.GetPBXProjectPath(path);
             PBXProject project = new PBXProject();
             project.ReadFromString(File.ReadAllText(projectPath));
             string targetName = PBXProject.GetUnityTargetName(); // note, not "project." ...
             string targetGUID = project.TargetGuidByName(targetName);

             AddFrameworks(project, targetGUID);

             // Write.
             File.WriteAllText(projectPath, project.WriteToString());
         }
     }

     static void AddFrameworks(PBXProject project, string targetGUID)
     {
         project.AddFrameworkToProject(targetGUID, "VideoToolbox.framework", false);
     }
}
