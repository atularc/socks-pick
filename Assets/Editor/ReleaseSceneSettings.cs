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
            int screens=0;foreach(var node in scene.GetRootGameObjects())
                foreach(var view in node.GetComponentsInChildren<SceneView>(true)){screens++;foreach(var binding in view.bindings)if(!binding.target)throw new BuildFailedException("Missing Canvas binding: "+view.id+" / "+binding.key);}
            if(screens<29)throw new BuildFailedException("Build requires the authored Laundry Canvas screens.");
            foreach(var root in scene.GetRootGameObjects())
                foreach(var game in root.GetComponentsInChildren<SockGame>(true))game.unlockAllLevelsForTesting=false;
        }
    }
}
