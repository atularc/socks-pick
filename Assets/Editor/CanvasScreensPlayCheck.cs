using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SockPairs.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs.Editor
{
    public static class CanvasScreensPlayCheck
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Laundry.unity");
            PlayerPrefs.DeleteKey("sock-pairs-canvas-qa-v1");
            SessionState.SetBool("SockPairsCanvasCheck",true);
            EditorApplication.EnterPlaymode();
        }
        [InitializeOnLoadMethod] static void Initialize()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("SockPairsCanvasCheck",false))return;
                SessionState.SetBool("SockPairsCanvasCheck",false);new GameObject("Canvas test runner").AddComponent<CanvasCheckRunner>();
            };
        }
    }
    public sealed class CanvasCheckRunner:MonoBehaviour
    {
        readonly List<string> errors=new List<string>();int checks;
        const string Output="Documentation/Scene-Canvas/EditorQA";
        void Awake(){Application.logMessageReceived+=Log;}
        void OnDestroy(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(stack.Contains("UnityEditor.Search."))return;if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
        object Call(SockGame game,string name,params object[] args)=>typeof(SockGame).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);
        SceneView[] Views()=>UnityEngine.Object.FindFirstObjectByType<Canvas>().GetComponentsInChildren<SceneView>(true);
        void Start(){Directory.CreateDirectory(Output);StartCoroutine(Guard(CheckScreens()));}
        IEnumerator Guard(IEnumerator test)
        {
            while(true)
            {
                object current=null;bool move=false;
                try{move=test.MoveNext();if(move)current=test.Current;}catch(Exception e){errors.Add(e.ToString());break;}
                if(!move)break;yield return current;
            }
            var report=(errors.Count==0?"PASS":"FAIL")+": "+checks+" assertions\n"+string.Join("\n",errors);
            File.WriteAllText(Output+"/report.txt",report);Debug.Log(report);EditorApplication.Exit(errors.Count==0?0:1);
        }
        IEnumerator CheckScreens()
        {
            while(!SockGame.Instance||SockGame.Instance.save==null)yield return null;
            var game=SockGame.Instance;var bridge=game.GetComponent<PlatformBridge>();bridge.ready=false;
            game.save.reduceMotion=true;
            var original=Views();var ids=original.Select(v=>v.GetInstanceID()).OrderBy(v=>v).ToArray();
            Check(game.puzzle.layout.config.level==1,"New player starts level 1 directly");
            Check(original.Length==29,"All 29 screens are serialized");
            Check(QualitySettings.GetQualityLevel()==0&&QualitySettings.antiAliasing==0&&QualitySettings.shadows==ShadowQuality.Disable,"Lowest rendering preset, no anti-aliasing or shadows");
            yield return Capture("startup-level-1");

            // Inspector-style edits survive transitions. No regeneration of positions, sizes, fonts or colors.
            var simple=original.First(v=>v.id=="Gameplay simple");var pause=simple.Get<Button>("pause").GetComponent<RectTransform>();
            Vector2 position=pause.anchoredPosition+new Vector2(-19,-11),size=pause.sizeDelta+new Vector2(8,10);pause.anchoredPosition=position;pause.sizeDelta=size;
            var status=simple.Get<Text>("status");var originalStyle=status.fontSize;status.fontSize=originalStyle+2;
            game.Home();game.StartLevel(2,false);
            Check(pause.anchoredPosition==position&&pause.sizeDelta==size,"Inspector layout survives navigation");
            // The style was initialized at first show; test a previously unseen screen's edited style below.
            var settings=original.First(v=>v.id=="Settings");var done=settings.Get<Button>("done").GetComponent<RectTransform>();var donePos=done.anchoredPosition+Vector2.right*13;done.anchoredPosition=donePos;
            Call(game,"Settings");Call(game,"CloseModal");Call(game,"Settings");Check(done.anchoredPosition==donePos,"Popup layout survives reopening");
            Call(game,"CloseModal");pause.anchoredPosition-=new Vector2(-19,-11);pause.sizeDelta-=new Vector2(8,10);status.fontSize=originalStyle;
            var basketView=original.First(v=>v.id=="Gameplay basket");var basketStatus=basketView.Get<Text>("status");
            basketStatus.fontSize=51;basketStatus.color=new Color(.9f,.8f,.4f);
            game.StartLevel(4,false);Check(basketStatus.fontSize==51&&basketStatus.color==new Color(.9f,.8f,.4f),"Authored font and color survive first gameplay binding");
            game.StartLevel(2,false);

            foreach(var name in new[]{"Pause","Settings","Lose"}){Call(game,name);yield return Capture(name.ToLower());Call(game,"CloseModal");}
            game.Home();yield return Capture("map");Call(game,"Album");yield return Capture("album");
            var album=original.First(v=>v.id=="Sock album");album.Get<AlbumPager>("pager").GoTo(3,false);yield return Capture("album-last-page");
            Call(game,"OpenAlbumItem",0);yield return Capture("album-detail");Call(game,"CloseModal");
            foreach(int level in new[]{3,4,21,31,41,51,61,71,81,91})
            {game.StartLevel(level,true);yield return Capture("tutorial-"+level);Call(game,"CloseModal");}
            game.StartLevel(4,false);
            foreach(string type in new[]{"spot","hint","shuffle"}){Call(game,"Offer",type);yield return Capture("helper-"+type);Call(game,"CloseModal");}
            for(int star=1;star<=3;star++){Call(game,"BindReward",20,star,star==3?100:1);yield return Capture("reward-"+star);Call(game,"CloseModal");}

            for(int level=1;level<=100;level++)
            {
                game.StartLevel(level,false);
                Check(game.puzzle.layout.config.level==level,"Level "+level+" initializes");
                Check(game.puzzle.Socks.Count==game.puzzle.layout.config.pairs*2+game.puzzle.layout.config.odd,"Sock count "+level);
                Check(Views().Select(v=>v.GetInstanceID()).OrderBy(v=>v).SequenceEqual(ids),"No screen recreated at level "+level);
                if(new[]{1,3,4,41,51,100}.Contains(level))yield return Capture("gameplay-"+level);
            }
            // Apply a saved session as the platform does on startup, without routing through the map.
            game.save.unlocked=100;game.StartLevel(4,false);Call(game,"Tap",0);Call(game,"Tap",2);Call(game,"Persist");
            var saved=JsonUtility.ToJson(game.save);var expected=SaveData.Read(saved);Call(game,"OnLoaded",saved);
            Check(game.puzzle.layout.config.level==4&&game.puzzle.moves==expected.active.moves,"Returning player resumes moves");
            Check(game.puzzle.basket.SequenceEqual(expected.active.basket)&&game.puzzle.selected==expected.active.selected,"Returning player resumes basket and selected sock");
            var next=new SaveData{unlocked=8,reduceMotion=true};Call(game,"OnLoaded",JsonUtility.ToJson(next));Check(game.puzzle.layout.config.level==8,"Player with no active round opens next unlocked level");
            next.unlocked=100;Call(game,"OnLoaded",JsonUtility.ToJson(next));Check(game.puzzle.layout.config.level==100,"Completed campaign startup remains valid");
            foreach(var image in UnityEngine.Object.FindFirstObjectByType<Canvas>().GetComponentsInChildren<Image>(true))
                if(image.name=="Face"||image.name=="Lavender basket")Check(image.sprite,"Serialized UI sprite retained: "+image.name);
            game.save.reduceMotion=false;Call(game,"BindReward",20,3,1);game.StartLevel(2,false);yield return null;
            Check(!Views().SelectMany(v=>v.GetComponentsInChildren<RectTransform>(true)).Any(r=>r.name=="Confetti"),"Leaving a reward early cleans up transient particles");
            Check(errors.Count==0,"No Unity errors");
        }
        IEnumerator Capture(string name)
        {
            yield return null;
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;var previous=RenderTexture.active;
            var target=RenderTexture.GetTemporary(540,960,24);canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();
            RenderTexture.active=target;var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();File.WriteAllBytes(Output+"/"+name+".png",image.EncodeToPNG());Destroy(image);
            camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);
        }
    }
}
