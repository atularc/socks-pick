using System.IO;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SockPairs.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs.Editor
{
    // Editor-only, file-triggered diagnostics. No runtime/build cost.
    [InitializeOnLoad]
    public static class EditorScreenAudit
    {
        const string Request = "Temp/screen-audit-request.txt";
        static EditorScreenAudit() { EditorApplication.update += Update; }
        static void Update()
        {
            if (!File.Exists(Request)) return;
            string command;try{command=File.ReadAllText(Request).Trim();}catch(IOException){return;}
            if(command=="loading-scene"&&!EditorApplication.isPlaying){File.Delete(Request);BuildTools.RefreshLoadingScreen();return;}
            if(!EditorApplication.isPlaying)return;
            File.Delete(Request);
            if(command=="all"){SockGame.Instance.StartCoroutine(Run(SockGame.Instance));return;}
            if (command != "dump") return;
            var output = new StringBuilder();
            foreach (var graphic in UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None))
            {
                if (!(graphic is ReferenceBArt) && !(graphic is CandyPathGraphic)) continue;
                if(!graphic.GetComponent<CanvasRenderer>()){output.AppendLine("MISSING RENDERER: "+graphic.name);continue;}
                var mesh = graphic.canvasRenderer.GetMesh();
                if(!mesh){output.AppendLine("MISSING MESH: "+graphic.name);continue;}
                output.AppendLine(graphic.name + " type=" + graphic.GetType().Name + " rect=" + graphic.rectTransform.rect + " verts=" + mesh.vertexCount + " bounds=" + mesh.bounds + " color=" + graphic.color + " rendererColor=" + graphic.canvasRenderer.GetColor() + " cull=" + graphic.canvasRenderer.cull + " depth=" + graphic.depth + " texture=" + graphic.mainTexture + " material=" + graphic.materialForRendering);
                if (graphic is ReferenceBArt art) output.AppendLine("  source=" + art.source + " radius=" + art.radius + " localRadius=" + art.localRadius + " frame=" + art.frame);
            }
            Directory.CreateDirectory("Documentation/Editor-Screen-Audit");
            File.WriteAllText("Documentation/Editor-Screen-Audit/graphics.txt", output.ToString());
        }
        static readonly List<string> issues=new List<string>();
        static readonly List<string> shots=new List<string>();
        const string Folder="Documentation/Editor-Screen-Audit";
        static void Call(SockGame game,string name,params object[] args)=>typeof(SockGame).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);
        static IEnumerator Run(SockGame game)
        {
            Directory.CreateDirectory(Folder);
            var original=JsonUtility.ToJson(game.save);
            var bridge=UnityEngine.Object.FindFirstObjectByType<PlatformBridge>();bool ready=bridge.ready;bridge.ready=false;
            File.WriteAllText(Folder+"/save-backup.json",original);
            issues.Clear();shots.Clear();
            try
            {
                game.save=new SaveData();game.save.reduceMotion=true;game.save.sound=false;game.save.music=false;Call(game,"ApplySound");
                Call(game,"NewScreen","boot");game.RefreshLoadingView();yield return Snapshot("loading-screen");
                foreach(int level in new[]{1,2,3,7,25,31,100})
                {
                    game.save.unlocked=level;game.save.active=null;for(int i=0;i<100;i++)game.save.stars[i]=i<level-1?3:0;
                    game.Home();yield return Snapshot("map-"+level);
                    CheckMarkers();
                }
                // Check marker clearance and live button values at every progression state.
                for(int level=1;level<=100;level++)
                {
                    game.save.unlocked=level;game.save.active=null;game.Home();yield return new WaitForEndOfFrame();CheckMarkers();CheckGraphics("map-state-"+level);
                    var label=GameObject.Find("PLAY").GetComponentInChildren<Text>();
                    if(!label||label.text!="Play level "+level)issues.Add("Incorrect live map level "+level);
                }
                game.save.collection[0]=true;Call(game,"Album");yield return Snapshot("album-page-1");
                for(int page=1;page<4;page++){UnityEngine.Object.FindFirstObjectByType<AlbumPager>().GoTo(page,false);yield return Snapshot("album-page-"+(page+1));}
                Call(game,"OpenAlbumItem",0);yield return Snapshot("album-collected");
                Call(game,"OpenAlbumItem",1);yield return Snapshot("album-locked");
                Call(game,"CloseModal");Call(game,"Settings");yield return Snapshot("settings");CheckToggles();
                game.StartLevel(1,true);yield return Snapshot("first-pair-tutorial");
                game.StartLevel(1,false);yield return Snapshot("gameplay-01");
                Call(game,"Pause");yield return Snapshot("pause");CheckToggles();
                Call(game,"CloseModal");
                foreach(int id in game.puzzle.layout.solution.ToArray())
                {Call(game,"Tap",id);yield return new WaitForSecondsRealtime(.05f);}
                yield return new WaitForSecondsRealtime(2);yield return Snapshot("win-actual");
                if(!game.puzzle.Won||!GameObject.Find("Approved B reward artwork"))issues.Add("Level completion did not open reward");
                foreach(int level in new[]{11,21,31,41,51,61,71,81,91})
                {
                    game.StartLevel(level,true);yield return Snapshot("tutorial-"+level);
                    Call(game,"CloseModal");yield return Snapshot("gameplay-"+level);
                }
                game.StartLevel(100,false);yield return Snapshot("gameplay-100");
                foreach(string kind in new[]{"spot","hint","shuffle"}){Call(game,"Offer",kind);yield return Snapshot("helper-"+kind);}
                Call(game,"Lose");yield return Snapshot("basket-full");
                Call(game,"CloseModal");
                var modal=(RectTransform)typeof(SockGame).GetField("modal",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                for(int stars=1;stars<=3;stars++)
                {
                    UI.Clear(modal);yield return new WaitForEndOfFrame();RewardBackdrop.Capture(modal);UI.Box(modal,"Dim",0,0,1080,1920,new Color(.03f,.02f,.01f,.55f),false).gameObject.AddComponent<FullBleed>();ReferenceBArt.Rim(modal);
                    ReferenceReward.Build(modal,stars==3?100:6,stars==3?24:10,stars,stars==3,game.Home);
                    yield return Snapshot("reward-"+stars+"-stars");
                }
                for(int level=1;level<=100;level++)
                {game.StartLevel(level,false);yield return new WaitForEndOfFrame();CheckGraphics("gameplay-"+level);CheckGameplay(game,level);}
                File.WriteAllText(Folder+"/report.txt", "Editor: "+SystemInfo.graphicsDeviceType+" / "+Screen.width+"x"+Screen.height+"\n"+shots.Count+" screens captured using the normal game framebuffer.\n100 map progression states and 100 gameplay layouts checked.\n"+(issues.Count==0?"PASS: no detected missing renderers, empty geometry, clipped labels, escaped socks or map marker overlaps.":string.Join("\n",issues)));
                File.WriteAllText(Folder+"/index.html","<!doctype html><meta charset='utf-8'><title>Unity editor screen audit</title><style>body{background:#173d3b;color:#ffdfb5;font:16px system-ui;margin:28px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:20px}img{width:100%}figure{margin:0}figcaption{padding:10px}</style><h1>Unity editor screen audit</h1><p>Actual editor game-frame captures. "+issues.Count+" detected issues.</p><main>"+string.Join("",shots.ConvertAll(s=>"<figure><img loading='lazy' src='"+s+".png'><figcaption>"+s+"</figcaption></figure>"))+"</main>");
            }
            finally
            {
                Call(game,"NewScreen","map");game.save=SaveData.Read(original);game.Home();Call(game,"ApplySound");bridge.ready=ready;
            }
        }
        static IEnumerator Snapshot(string name)
        {
            yield return new WaitForSecondsRealtime(name=="first-pair-tutorial"?1.1f:.65f);
            yield return new WaitForEndOfFrame();
            CheckGraphics(name);CheckContainers(name);
            var image=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Folder+"/"+name+".png",image.EncodeToPNG());UnityEngine.Object.Destroy(image);shots.Add(name);
            File.WriteAllText(Folder+"/progress.txt",name+" / issues="+issues.Count);
        }
        static void CheckGraphics(string name)
        {
            foreach(var graphic in UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None))
            {
                if(!graphic.isActiveAndEnabled)continue;
                if(!graphic.GetComponent<CanvasRenderer>()){issues.Add(name+": missing renderer "+graphic.name);continue;}
                if(graphic is ReferenceBArt)
                {
                    var mesh=graphic.canvasRenderer.GetMesh();
                    if(!mesh||mesh.vertexCount==0)issues.Add(name+": empty artwork "+graphic.name);
                    else foreach(var p in mesh.vertices)if(float.IsNaN(p.x)||float.IsNaN(p.y)){issues.Add(name+": invalid artwork "+graphic.name);break;}
                }
                if(graphic is Text text&&!string.IsNullOrEmpty(text.text)&&text.preferredHeight>text.rectTransform.rect.height+2)
                    issues.Add(name+": clipped text "+text.text.Replace('\n',' '));
                if(graphic is Image image&&image.sprite&&image.type==Image.Type.Simple&&!image.preserveAspect)
                {
                    // FabricSocks is authored as 6 x 4 square cells (1536 x 1024).
                    // Its compressed POT import rescales the atlas; square UI cells undo that storage resampling.
                    float source=image.sprite.texture.name=="FabricSocks"?1:image.sprite.rect.width/image.sprite.rect.height;
                    float target=image.rectTransform.rect.width/image.rectTransform.rect.height;
                    if(Mathf.Abs(target/source-1)>.025f)issues.Add(name+": distorted image "+image.name);
                }
            }
        }
        static void CheckMarkers()
        {
            var marker=GameObject.Find("Current marker");if(!marker)return;
            var bounds=Bounds((RectTransform)marker.transform);
            foreach(var face in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                if(face.name=="Level"&&bounds.Overlaps(Bounds(face.rectTransform)))issues.Add("Map marker overlaps "+face.transform.parent.name+" at "+SockGame.Instance.save.unlocked);
        }
        static Rect Bounds(RectTransform rect)
        {var p=new Vector3[4];rect.GetWorldCorners(p);return Rect.MinMaxRect(p[0].x,p[0].y,p[2].x,p[2].y);}
        static void CheckToggles()
        {
            foreach(var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                if(button.name=="Toggle"){int id=button.GetInstanceID();button.onClick.Invoke();button.onClick.Invoke();if(!button||button.GetInstanceID()!=id)issues.Add("Toggle rebuilt its screen");}
        }
        static void CheckGameplay(SockGame game,int level)
        {
            var bounds=GameObject.Find("Socks bounds").GetComponent<RectTransform>();
            var mat=GameObject.Find("Cream play surface").GetComponent<RectTransform>();
            foreach(var sock in UnityEngine.Object.FindObjectsByType<SockVisual>(FindObjectsSortMode.None))
            {
                var art=sock.art.rectTransform;var visible=SockArtBounds.Visible[game.puzzle.Socks[int.Parse(sock.name.Substring(5))].design];var r=art.rect;
                var lo=new Vector2(r.xMin+r.width*visible.xMin,r.yMin+r.height*visible.yMin);var hi=new Vector2(r.xMin+r.width*visible.xMax,r.yMin+r.height*visible.yMax);
                var felt=mat.rect;felt.xMin+=32;felt.xMax-=32;felt.yMin+=32;felt.yMax-=32;
                var state=game.puzzle.Socks[int.Parse(sock.name.Substring(5))];
                foreach(var corner in new[]{art.TransformPoint(lo),art.TransformPoint(new Vector2(lo.x,hi.y)),art.TransformPoint(hi),art.TransformPoint(new Vector2(hi.x,lo.y))})
                {
                    if(!bounds.rect.Contains(bounds.InverseTransformPoint(corner))||!felt.Contains(mat.InverseTransformPoint(corner)))issues.Add("Sock outside felt interior: "+level+" / "+sock.name);
                    var local=(Vector2)mat.parent.InverseTransformPoint(corner);
                    if(level>10&&state.place==Place.Pile&&(local.y< -154||local.y>514))issues.Add("Sock overlaps reserved header/footer: "+level+" / "+sock.name+" y="+local.y);
                }
            }
        }
        static void CheckContainers(string screen)
        {
            foreach(var panel in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None))
            {
                if(panel.name!="Panel"&&panel.name!="Album popup")continue;
                var labels=panel.GetComponentsInChildren<Text>();
                for(int a=0;a<labels.Length;a++)for(int b=a+1;b<labels.Length;b++)
                {
                    if(labels[a].canvasRenderer.cull||labels[b].canvasRenderer.cull)continue;
                    var ma=labels[a].canvasRenderer.GetMesh();var mb=labels[b].canvasRenderer.GetMesh();
                    if(!ma||!mb||ma.vertexCount==0||mb.vertexCount==0)continue;
                    var ra=TextBounds(labels[a],ma);var rb=TextBounds(labels[b],mb);
                    if(ra.Overlaps(rb))issues.Add(screen+": overlapping text: "+labels[a].text+" / "+labels[b].text);
                }
                foreach(var graphic in panel.GetComponentsInChildren<Graphic>())
                {
                    if(graphic.transform==panel||!graphic.isActiveAndEnabled||graphic.color.a<.01f||graphic.canvasRenderer.cull)continue;
                    var mesh=graphic.canvasRenderer.GetMesh();if(!mesh)continue;
                    bool failed=false;
                    foreach(var v in mesh.vertices)
                    {
                        var p=panel.InverseTransformPoint(graphic.rectTransform.TransformPoint(v));
                        float x=Mathf.Abs(p.x),ceiling=panel.rect.height*.5f-(x<=140?60:x<260?60+(x-140)*1.2f:230);
                        if(x>390.5f||p.y< -panel.rect.height*.5f+59||p.y>ceiling+1){failed=true;break;}
                    }
                    if(failed)issues.Add(screen+": content crosses panel interior: "+graphic.name);
                }
            }
        }
        static Rect TextBounds(Text text,Mesh mesh)
        {
            Vector2 lo=new Vector2(float.PositiveInfinity,float.PositiveInfinity),hi=-lo;
            foreach(var p in mesh.vertices){var world=(Vector2)text.rectTransform.TransformPoint(p);lo=Vector2.Min(lo,world);hi=Vector2.Max(hi,world);}
            return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);
        }
    }
}
