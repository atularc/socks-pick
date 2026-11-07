using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace SockPairs.Editor
{
    // Keep the developer's Inspector testing switch, but never ship bypassed locks.
    public sealed class ReleaseSceneSettings : IProcessSceneWithReport
    {
        public int callbackOrder => 0;
        public void OnProcessScene(Scene scene,BuildReport report)
        {
            if(report==null||(report.summary.options&UnityEditor.BuildOptions.Development)!=0)return;
            foreach(var root in scene.GetRootGameObjects())
                foreach(var game in root.GetComponentsInChildren<SockGame>(true))game.unlockAllLevelsForTesting=false;
        }
    }
}
