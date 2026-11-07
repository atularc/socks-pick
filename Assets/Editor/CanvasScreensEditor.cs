using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SockPairs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs.Editor
{
    public static class CanvasScreensEditor
    {
        const string Scene="Assets/Scenes/Laundry.unity";
        public static void Create()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before authoring screens.");
            EditorSceneManager.OpenScene(Scene);var game=UnityEngine.Object.FindFirstObjectByType<SockGame>();
            if(UnityEngine.Object.FindFirstObjectByType<Canvas>().GetComponentsInChildren<SceneView>(true).Length>0)throw new InvalidOperationException("Scene screens already exist; edit them directly.");
            game.BakeCanvasScreens();
            ConfigureDynamicContent(UnityEngine.Object.FindFirstObjectByType<Canvas>());
            PersistGeneratedArt();
            Validate();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
        static void PersistGeneratedArt()
        {
            Directory.CreateDirectory("Assets/UI");
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            const string path="Assets/UI/CanvasSprites.asset";if(AssetDatabase.LoadMainAssetAtPath(path))AssetDatabase.DeleteAsset(path);bool created=false;
            var seen=new HashSet<UnityEngine.Object>();int index=0;
            foreach(var image in canvas.GetComponentsInChildren<Image>(true))
            {
                var sprite=image.sprite;if(!sprite||AssetDatabase.Contains(sprite)||!seen.Add(sprite))continue;
                sprite.name="Canvas sprite "+index++;
                if(!created){AssetDatabase.CreateAsset(sprite,path);created=true;}else AssetDatabase.AddObjectToAsset(sprite,path);
            }
            seen.Clear();index=0;
            foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))
            {
                var material=graphic.material;
                if(!material||material==Graphic.defaultGraphicMaterial||AssetDatabase.Contains(material)||!seen.Add(material))continue;
                AssetDatabase.CreateAsset(material,"Assets/UI/CanvasMaterial"+(index++)+".mat");
            }
        }
        [MenuItem("Sock Pairs/Authoring/Validate saved Canvas screens")]
        public static void Validate()
        {
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if(!canvas)throw new InvalidOperationException("Open Assets/Scenes/Laundry.unity before validating its Canvas.");
            var views=canvas.GetComponentsInChildren<SceneView>(true);
            if(views.Length<25)throw new Exception("The saved scene is missing Canvas screens.");
            if(views.Select(v=>v.id).Distinct().Count()!=views.Length)throw new Exception("Duplicate screen identifiers.");
            foreach(var view in views)
            {
                if(view.bindings.Select(b=>b.key).Distinct().Count()!=view.bindings.Count)throw new Exception("Duplicate bindings in "+view.id);
                foreach(var item in view.bindings)if(!item.target)throw new Exception(view.id+": missing "+item.key);
            }
            Debug.Log("CANVAS VALIDATION PASS: "+views.Length+" saved screens; "+views.Sum(v=>v.bindings.Count)+" serialized bindings.");
        }
        public static void CreateAndBuild(){Create();BuildTools.BrowserTargets();}
        public static void ValidateSavedScene(){EditorSceneManager.OpenScene(Scene);Validate();}
        public static void FinalizeAndBuild()
        {
            EditorSceneManager.OpenScene(Scene);
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            ConfigureDynamicContent(canvas);
            Validate();EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);EditorSceneManager.SaveScene(canvas.gameObject.scene);
            BuildTools.BrowserTargets();
        }
        static void ConfigureDynamicContent(Canvas canvas)
        {
            var views=canvas.GetComponentsInChildren<SceneView>(true);
            var final=views.First(v=>v.id=="Campaign complete");
            final.Get<Button>("map").gameObject.SetActive(false); // The final panel already has its primary map action.
            ConfigureMapContent(views.First(v=>v.id=="Level map"));
            foreach(var view in views.Where(v=>v.id=="Level complete"||v.id=="Campaign complete"))
            {var amount=view.Get<Text>("coins");amount.resizeTextForBestFit=true;amount.resizeTextMinSize=40;amount.resizeTextMaxSize=67;}
        }
        static void ConfigureMapContent(SceneView view)
        {
            var button=view.Get<Button>("play");if(button.transform.Find("Editable button content"))return;
            var label=view.Get<Text>("playLabel");var icon=button.GetComponentsInChildren<CandyGlyph>(true).First(g=>g.kind=="play");
            var content=UI.Rect(button.transform,"Editable button content",0,2,760,151);
            var group=content.gameObject.AddComponent<HorizontalLayoutGroup>();group.childAlignment=TextAnchor.MiddleCenter;
            group.childControlWidth=group.childControlHeight=true;group.childForceExpandWidth=group.childForceExpandHeight=false;group.spacing=22;
            icon.transform.SetParent(content,false);label.transform.SetParent(content,false);
            var iconLayout=icon.gameObject.AddComponent<LayoutElement>();iconLayout.preferredWidth=iconLayout.minWidth=60;iconLayout.preferredHeight=iconLayout.minHeight=60;
            var textLayout=label.gameObject.AddComponent<LayoutElement>();textLayout.minWidth=0;label.horizontalOverflow=HorizontalWrapMode.Overflow;
        }
    }
    [CustomEditor(typeof(SceneView))]
    public sealed class SceneViewInspector:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var view=(SceneView)target;
            EditorGUILayout.HelpBox("Edit this screen's child RectTransforms, images and text directly. Runtime uses these saved objects. Dynamic counters and gameplay state are updated by the game.",MessageType.Info);
            if(!Application.isPlaying&&GUILayout.Button("Preview this screen in Canvas"))
            {
                var canvas=view.GetComponentInParent<Canvas>();var all=canvas.GetComponentsInChildren<SceneView>(true);
                Undo.RecordObjects(all.Select(v=>(UnityEngine.Object)v.gameObject).ToArray(),"Preview Canvas screen");
                foreach(var v in all)v.gameObject.SetActive(false);
                if(view.transform.parent.name=="Popups")
                {
                    var background=all.First(v=>v.id==(view.id=="Sock album"||view.id=="Sock detail"?"Level map":"Gameplay basket"));background.gameObject.SetActive(true);
                }
                view.gameObject.SetActive(true);UnityEditor.SceneView.RepaintAll();
            }
            DrawDefaultInspector();
        }
    }
}
