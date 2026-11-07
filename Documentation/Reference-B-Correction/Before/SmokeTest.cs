#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using SockPairs.Core;
using UnityEngine;

namespace SockPairs
{
    public class SmokeTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-sockQA")>=0)new GameObject("Smoke test").AddComponent<SmokeTest>();}
        IEnumerator Start()
        {
            while(SockGame.Instance==null||SockGame.Instance.save==null)yield return null;
            Application.logMessageReceived+=(message,stack,type)=>{if(type==LogType.Exception||type==LogType.Error)Application.Quit(1);};
            var game=SockGame.Instance;
            if(QualitySettings.GetQualityLevel()!=0||QualitySettings.antiAliasing!=0||QualitySettings.shadows!=ShadowQuality.Disable)
                throw new Exception("Lowest rendering preset was not applied");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-performanceQA")>=0)
            {yield return Performance(game);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-readabilityQA")>=0)
            {yield return Readability(game);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-layoutQA")>=0)
            {yield return ResponsiveLayout(game);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-controlsQA")>=0)
            {yield return Controls(game);Application.Quit(0);yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-basketQA")>=0)
            {yield return BasketPlacement(game);Application.Quit(0);yield break;}
            if(SaveData.Read(JsonUtility.ToJson(new SaveData())).active!=null)throw new Exception("Empty save must not resume a phantom level");
            game.save=new SaveData();game.Home();
            string output=Path.Combine(Application.dataPath,"..","QA");Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(1);
            Capture(Path.Combine(output,"home.png"));
            yield return new WaitForSecondsRealtime(.3f);
            typeof(SockGame).GetMethod("Shop",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"shop.png"));
            var shopBack=GameObject.Find("Back to Map").GetComponent<UnityEngine.UI.Button>();
            if(!shopBack.interactable)throw new Exception("Shop back button must be clickable");
            shopBack.onClick.Invoke();yield return null;
            if(!GameObject.Find("Map navigation"))throw new Exception("Shop back button did not return to map");
            Debug.Log("SHOP QA PASS: labelled back button returns to map.");
            game.save.collection[0]=true;
            typeof(SockGame).GetMethod("Album",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            var owned=GameObject.Find("Collection item 0").transform.Find("Collected sock").GetComponent<UnityEngine.UI.Image>();
            var locked=GameObject.Find("Collection item 1").transform.Find("Collected sock").GetComponent<UnityEngine.UI.Image>();
            if(owned.color!=Color.white||locked.color.r!=0||locked.color.a>=.3f||!GameObject.Find("Collection item 1").transform.Find("Lock badge"))throw new Exception("Album collected and locked appearances must differ even during level testing");
            Debug.Log("ALBUM QA PASS: collected full colour, uncollected silhouette and lock badge.");
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"album.png"));
            typeof(SockGame).GetMethod("OpenAlbumItem",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{0});
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"album-detail.png"));
            typeof(SockGame).GetMethod("CloseModal",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            typeof(SockGame).GetMethod("Settings",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"settings.png"));
            game.StartLevel(1,true);yield return new WaitForSecondsRealtime(1.3f);
            Capture(Path.Combine(output,"tutorial.png"));
            game.StartLevel(1,false);yield return new WaitForSecondsRealtime(.85f);
            Capture(Path.Combine(output,"level-01.png"));
            yield return new WaitForSecondsRealtime(.3f);
            var tap=typeof(SockGame).GetMethod("Tap",BindingFlags.Instance|BindingFlags.NonPublic);
            var order=game.puzzle.layout.solution.ToArray();
            tap.Invoke(game,new object[]{order[0]});yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"selected-sock.png"));
            foreach(int id in order)
            {
                if(game.puzzle.Socks[id].place==Place.Gone||game.puzzle.selected==id)continue;
                tap.Invoke(game,new object[]{id});yield return new WaitForSecondsRealtime(.1f);
            }
            yield return new WaitForSecondsRealtime(2);
            if(!game.puzzle.Won)throw new Exception("Runtime level 1 failed to clear");
            Capture(Path.Combine(output,"win.png"));
            yield return new WaitForSecondsRealtime(.3f);
            game.StartLevel(11,false);yield return new WaitForSecondsRealtime(.85f);
            Capture(Path.Combine(output,"level-11.png"));
            yield return new WaitForSecondsRealtime(.3f);
            typeof(SockGame).GetMethod("Pause",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"pause.png"));
            typeof(SockGame).GetMethod("Offer",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{"spot"});
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"helper.png"));
            game.StartLevel(100,false);yield return new WaitForSecondsRealtime(.85f);
            Capture(Path.Combine(output,"level-100.png"));
            yield return new WaitForSecondsRealtime(.3f);
            typeof(SockGame).GetMethod("Lose",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"lose.png"));
            foreach(int chapter in new[]{11,21,31,41,51,61,71,81,91})
            {
                game.StartLevel(chapter,true);yield return new WaitForSecondsRealtime(.3f);
                Capture(Path.Combine(output,"tutorial-"+chapter+".png"));
            }
            foreach(string kind in new[]{"hint","shuffle"})
            {
                typeof(SockGame).GetMethod("Offer",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{kind});
                yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"helper-"+kind+".png"));
            }
            game.save.unlocked=3;game.save.stars[0]=game.save.stars[1]=3;game.save.active=null;game.Home();
            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"map-reference-b.png"));
            game.save.unlocked=25;for(int i=0;i<24;i++)game.save.stars[i]=i%3+1;game.save.active=null;game.Home();
            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"map-progress.png"));
            Debug.Log("SOCK PAIRS SMOKE PASS: runtime match/clear flow and screens captured.");
            Application.Quit(0);
        }
        IEnumerator Performance(SockGame game)
        {
            game.save=new SaveData();game.save.reduceMotion=true;game.StartLevel(100,false);
            yield return new WaitForSecondsRealtime(1);
            // Avoid counting warm-up, PNG captures or this audit's allocations.
            System.GC.Collect();long startBytes=System.GC.GetAllocatedBytesForCurrentThread();
            float duration=0,worst=0;int frames=180;
            for(int i=0;i<frames;i++){yield return null;duration+=Time.unscaledDeltaTime;worst=Mathf.Max(worst,Time.unscaledDeltaTime);}
            long allocated=System.GC.GetAllocatedBytesForCurrentThread()-startBytes;
            Debug.Log("GUI PERF: frames="+frames+" meanMs="+(duration/frames*1000).ToString("F2")+" worstMs="+(worst*1000).ToString("F2")+" managedBytesPerFrame="+(allocated/frames));
            foreach(var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
                if(texture.name=="FreshLaundryBackground"||texture.name=="FabricSocks")Debug.Log("GUI TEXTURE: "+texture.name+" "+texture.width+"x"+texture.height+" "+texture.format+" bytes="+UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture));
            Debug.Log("GUI PERF COMPLETE: quality="+QualitySettings.names[QualitySettings.GetQualityLevel()]+" graphics="+UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsSortMode.None).Length);
        }
        IEnumerator Readability(SockGame game)
        {
            string output=Path.Combine(Application.dataPath,"..","QA","Readability-"+Screen.width+"x"+Screen.height);Directory.CreateDirectory(output);
            var tap=typeof(SockGame).GetMethod("Tap",BindingFlags.Instance|BindingFlags.NonPublic);
            game.save.reduceMotion=true;
            for(int level=1;level<=100;level++)
            {
                game.StartLevel(level,false);yield return null;Canvas.ForceUpdateCanvases();
                var bounds=GameObject.Find("Socks bounds").GetComponent<RectTransform>();
                var mat=GameObject.Find("Cream play surface").GetComponent<RectTransform>();
                var title=GameObject.Find("Title ribbon").GetComponent<RectTransform>();
                var titleCorners=new Vector3[4];title.GetWorldCorners(titleCorners);
                var titleShadow=GameObject.Find("Title ribbon shadow").GetComponent<RectTransform>();
                if(Mathf.Abs(title.anchoredPosition.y-titleShadow.anchoredPosition.y-14)>1)throw new Exception("Title shadow detached on level "+level);
                foreach(var sock in FindObjectsByType<SockVisual>(FindObjectsSortMode.None))
                {
                    var corners=new Vector3[4];sock.art.rectTransform.GetWorldCorners(corners);
                    foreach(var corner in corners)
                    {
                        Vector2 local=bounds.InverseTransformPoint(corner);
                        if(!bounds.rect.Contains(local))throw new Exception("Sock artwork clipped on level "+level);
                        if(!mat.rect.Contains(mat.InverseTransformPoint(corner)))throw new Exception("Sock extends outside its cream surface on level "+level);
                        if(level<=10)
                        {
                            var felt=mat.rect;felt.xMin+=40;felt.xMax-=40;felt.yMin+=70;
                            if(!felt.Contains(mat.InverseTransformPoint(corner)))throw new Exception("Sock overlaps the wooden tray bevel on level "+level);
                        }
                        if(corner.y>titleCorners[0].y)throw new Exception("Sock overlaps title on level "+level);
                    }
                }
                if(level>=11)
                {
                    foreach(string name in new[]{"+1 Slot","Hint","Shuffle"})
                    {
                        var button=GameObject.Find(name).GetComponent<RectTransform>();
                        var labels=button.GetComponentsInChildren<UnityEngine.UI.Text>();
                        if(labels.Length!=2)throw new Exception("Helper must have separate title and price");
                        foreach(var label in labels)
                        {
                            var mesh=label.canvasRenderer.GetMesh();
                            if(mesh.vertexCount==0)throw new Exception("Button text is not rendered: "+label.text);
                            foreach(var vertex in mesh.vertices)
                            {
                                Vector2 local=button.InverseTransformPoint(label.rectTransform.TransformPoint(vertex));
                                var inset=button.rect;inset.xMin+=16;inset.xMax-=16;inset.yMin+=16;inset.yMax-=16;
                                if(!inset.Contains(local))throw new Exception("Button glyph lacks padding: "+label.text);
                            }

                        }
                    }
                    var basket=GameObject.Find("Lavender basket").GetComponent<UnityEngine.UI.Image>();
                    var counter=(UnityEngine.UI.Text)typeof(SockGame).GetField("basketLabel",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                    var counterRect=counter.rectTransform;
                    float basketBottom=basket.rectTransform.anchoredPosition.y+UI.BasketImageRect(basket).yMin;
                    float counterTop=counterRect.anchoredPosition.y+counterRect.rect.yMax;
                    var helper=GameObject.Find("Hint").GetComponent<RectTransform>();
                    float counterBottom=counterRect.anchoredPosition.y+counterRect.rect.yMin;
                    if(basketBottom-counterTop<20||counterBottom-(helper.anchoredPosition.y+helper.rect.yMax)<12)
                        throw new Exception("Footer sections have insufficient separation");
                }
                Capture(Path.Combine(output,"audit-"+level.ToString("000")+".png"));
                foreach(var label in FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
                {
                    if(label.isActiveAndEnabled&&!string.IsNullOrEmpty(label.text)&&label.fontSize<32)
                        throw new Exception("Unreadable gameplay label in level "+level+": "+label.text+" size="+label.fontSize);
                }
            }
            Debug.Log("READABILITY AUDIT PASS: all 100 levels captured; text minimum, pile bounds and header separation passed.");
            foreach(int level in new[]{1,11,31,61,81,100})
            {
                game.StartLevel(level,false);yield return new WaitForSecondsRealtime(.85f);
                Capture(Path.Combine(output,"level-"+level+".png"));
                if(level==1)continue;
                // Exercise real selection/matching until the waiting basket is occupied.
                foreach(int id in game.puzzle.layout.solution)
                {
                    if(game.puzzle.Socks[id].place==Place.Gone)continue;
                    tap.Invoke(game,new object[]{id});yield return new WaitForSecondsRealtime(.03f);
                    if(game.puzzle.basket.Count>=2)break;
                }
                yield return new WaitForSecondsRealtime(.4f);
                var views=(System.Collections.Generic.Dictionary<int,RectTransform>)typeof(SockGame).GetField("views",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                foreach(int id in game.puzzle.basket)
                {
                    if(views[id].GetComponentInParent<UnityEngine.UI.RectMask2D>())throw new Exception("Waiting sock is clipped by pile mask");
                    var position=typeof(SockGame).GetMethod("Position",BindingFlags.Instance|BindingFlags.NonPublic);
                    var expected=(Vector2)position.Invoke(game,new object[]{game.puzzle.Socks[id]});
                    if(Vector2.Distance(views[id].anchoredPosition,expected)>1)throw new Exception("Waiting sock is not centred in its slot");
                }
                var label=(UnityEngine.UI.Text)typeof(SockGame).GetField("basketLabel",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if(label.text!="Waiting socks  "+game.puzzle.basket.Count+" / "+game.puzzle.capacity)throw new Exception("Basket count is stale");
                Capture(Path.Combine(output,"level-"+level+"-basket.png"));
                Debug.Log("READABILITY QA PASS: level "+level+" basket visible and count current.");
            }
        }
        IEnumerator ResponsiveLayout(SockGame game)
        {
            string output=Path.Combine(Application.dataPath,"..","QA","Layout");Directory.CreateDirectory(output);
            var canvas=FindFirstObjectByType<Canvas>();
            var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>()??canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(540,1170),new Vector2Int(390,844),new Vector2Int(768,1024)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);game.Home();
                yield return new WaitForSecondsRealtime(.5f);Canvas.ForceUpdateCanvases();
                var bar=GameObject.Find("Map navigation").GetComponent<RectTransform>();
                var hud=GameObject.Find("Wooden screen frame").GetComponent<RectTransform>();
                var corners=new Vector3[4];bar.GetWorldCorners(corners);Rect area=Screen.safeArea;
                if(Mathf.Abs(corners[0].y-area.yMin)>2)throw new Exception("Map footer does not meet safe bottom");
                if(corners[0].x<area.xMin-2||corners[2].x>area.xMax+2)throw new Exception("Map controls clipped horizontally");
                hud.GetWorldCorners(corners);
                if(corners[2].y>area.yMax+2||area.yMax-corners[2].y>30)throw new Exception("Wood frame does not fit safe top");
                // Return within the same frame: Unity has not destroyed the previous
                // screen yet, and its Album heading used to shadow the new button.
                foreach(string from in new[]{"Album","Shop","Album"})
                {
                    typeof(SockGame).GetMethod(from,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
                    if(from=="Shop")
                    {
                        var strip=GameObject.Find("HUD strip").GetComponent<RectTransform>();
                        var shopCorners=new Vector3[4];strip.GetWorldCorners(shopCorners);
                        if(Mathf.Abs(shopCorners[2].y-Screen.safeArea.yMax)>2)throw new Exception("Shop header leaves an unnecessary top gap");
                    }
                    game.Home();
                    var currentBar=GameObject.Find("Map navigation").GetComponent<RectTransform>();
                    foreach(string title in new[]{"PLAY","Album"})
                    {
                        var nav=GameObject.Find(title).GetComponent<RectTransform>();
                        if(Mathf.Abs(nav.anchoredPosition.x)>.01f||nav.anchoredPosition.y-nav.rect.height*.5f<currentBar.anchoredPosition.y-currentBar.rect.height*.5f)
                            throw new Exception("Navigation misaligned returning from "+from+": "+title);
                    }
                }
                yield return null;
                Capture(Path.Combine(output,size.x+"x"+size.y+".png"),Screen.width,Screen.height);
                Debug.Log("LAYOUT QA PASS: "+Screen.width+"x"+Screen.height+" scaler="+canvas.scaleFactor+" HUD/footer fit safe bounds.");
            }
        }
        IEnumerator Controls(SockGame game)
        {
            string output=Path.Combine(Application.dataPath,"..","QA","Controls");Directory.CreateDirectory(output);
            game.save=new SaveData();game.Home();yield return new WaitForSecondsRealtime(.4f);
            if(FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length!=1)throw new Exception("Scene canvas was duplicated at startup");
            var bar=GameObject.Find("Map navigation").GetComponent<RectTransform>();
            foreach(var title in new[]{"PLAY","Album"})
            {
                UnityEngine.UI.Button nav=null;
                foreach(var b in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None))if(b.name==title)nav=b;
                if(!nav)throw new Exception("Missing navigation button "+title);
                var label=nav.GetComponentInChildren<UnityEngine.UI.Text>();
                var face=nav.targetGraphic as UnityEngine.UI.Image;
                if(!label.enabled||!face||!face.sprite||string.IsNullOrEmpty(label.text))
                    throw new Exception("Navigation must use readable live text and a reusable sliced surface");
                var buttonRect=nav.GetComponent<RectTransform>();
                if(buttonRect.rect.height<132)throw new Exception("Navigation hit area is too small");
                float top=bar.anchoredPosition.y+bar.rect.height*.5f-(buttonRect.anchoredPosition.y+buttonRect.rect.height*.5f);
                float bottom=buttonRect.anchoredPosition.y-buttonRect.rect.height*.5f-(bar.anchoredPosition.y-bar.rect.height*.5f);
                if(top<24||bottom<24)throw new Exception("Navigation lacks safe padding");
            }
            var primary=GameObject.Find("PLAY").GetComponent<RectTransform>();
            var secondary=GameObject.Find("Album").GetComponent<RectTransform>();
            if(primary.anchoredPosition.y-primary.rect.height*.5f-(secondary.anchoredPosition.y+secondary.rect.height*.5f)<24)
                throw new Exception("Stacked map actions overlap or lack spacing");
            Capture(Path.Combine(output,"map.png"));
            foreach(var method in new[]{"Settings","Pause"})
            {
                if(method=="Pause")game.StartLevel(1,false);
                typeof(SockGame).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
                yield return new WaitForSecondsRealtime(.4f);
                var modal=(RectTransform)typeof(SockGame).GetField("modal",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                var panel=modal.Find("Panel");int id=panel.GetInstanceID(),children=panel.childCount,count=0;
                foreach(var toggle in panel.GetComponentsInChildren<UnityEngine.UI.Button>())
                {
                    if(toggle.name!="Toggle")continue;count++;
                    var knob=toggle.transform.Find("White knob").GetComponent<RectTransform>();
                    float initial=knob.anchoredPosition.x;
                    for(int click=0;click<2;click++)
                    {
                        toggle.onClick.Invoke();yield return new WaitForSecondsRealtime(.06f);
                        if(!panel||modal.Find("Panel").GetInstanceID()!=id||panel.childCount!=children||Vector3.Distance(panel.localScale,Vector3.one)>.001f)
                            throw new Exception("Toggle rebuilt or animated the popup");
                        float expected=click==0?-initial:initial;
                        if(knob.anchoredPosition.x!=expected)throw new Exception("Toggle knob did not update in place");
                    }
                }
                if(count!=(method=="Settings"?4:2))throw new Exception("Missing toggles");
                Capture(Path.Combine(output,method+".png"));
                Debug.Log("CONTROLS QA PASS: "+method+" toggles preserve popup identity, scale and child hierarchy.");
            }
            Debug.Log("CONTROLS QA COMPLETE: navigation layout and all six toggles.");
        }
        IEnumerator BasketPlacement(SockGame game)
        {
            string output=Path.Combine(Application.dataPath,"..","QA","Basket");Directory.CreateDirectory(output);
            var tap=typeof(SockGame).GetMethod("Tap",BindingFlags.Instance|BindingFlags.NonPublic);
            game.save=new SaveData();game.StartLevel(1,false);yield return new WaitForSecondsRealtime(.4f);
            var order=game.puzzle.layout.solution.ToArray();
            tap.Invoke(game,new object[]{order[0]});tap.Invoke(game,new object[]{order[1]});
            yield return new WaitForSecondsRealtime(1);
            var sizes=new[]{new Vector2Int(540,960),new Vector2Int(390,844),new Vector2Int(768,1024),new Vector2Int(960,540),new Vector2Int(360,640)};
            foreach(var size in sizes)
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
                Canvas.ForceUpdateCanvases();
                var basket=GameObject.Find("Lavender basket").GetComponent<UnityEngine.UI.Image>();
                var pair=GameObject.Find("Rolled pair").GetComponent<RectTransform>();
                Vector2 position=basket.rectTransform.InverseTransformPoint(pair.position);
                if(Vector2.Distance(position,UI.CompletedPairSlot(basket,0,1))>.5f)throw new Exception("Pair moved off its basket slot after resize");
                var bounds=UI.BasketImageRect(basket);
                if(!bounds.Contains(position-pair.sizeDelta*.5f)||!bounds.Contains(position+pair.sizeDelta*.5f))throw new Exception("Pair exceeds basket image bounds");
                Capture(Path.Combine(output,size.x+"x"+size.y+".png"),Screen.width,Screen.height);
                Debug.Log("BASKET QA PASS requested="+size+" actual="+Screen.width+"x"+Screen.height+" slot="+position);
            }
            foreach(int id in order)
            {
                if(game.puzzle.matched>=5)break;
                if(game.puzzle.Socks[id].place==Place.Gone||game.puzzle.selected==id)continue;
                tap.Invoke(game,new object[]{id});yield return new WaitForSecondsRealtime(.15f);
            }
            yield return new WaitForSecondsRealtime(1);
            var filled=GameObject.Find("Finished pairs");
            if(filled.transform.childCount!=5)throw new Exception("Five matched pairs must use exactly five basket slots: matched="+game.puzzle.matched+" children="+filled.transform.childCount);
            Capture(Path.Combine(output,"five-slots.png"),Screen.width,Screen.height);
            Debug.Log("BASKET QA COMPLETE: five aspect ratios and five occupied slots.");
        }
        static void Capture(string path,int width=0,int height=0)
        {
            if(width==0)width=Screen.width;if(height==0)height=Screen.height;
            Canvas.ForceUpdateCanvases();
            foreach(var label in FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
            {
                if(label.isActiveAndEnabled&&!string.IsNullOrEmpty(label.text)&&label.preferredHeight>label.rectTransform.rect.height+2)
                    Debug.LogError("TEXT CLIP: "+Path.GetFileName(path)+" | "+label.text.Replace("\n"," / ")+" needs="+label.preferredHeight+" height="+label.rectTransform.rect.height);
            }
            foreach(var background in FindObjectsByType<FullBleed>(FindObjectsSortMode.None))
            {
                background.Refresh(true);
                var corners=new Vector3[4];((RectTransform)background.transform).GetWorldCorners(corners);
                if(Vector2.Distance(corners[0],Vector2.zero)>1||Vector2.Distance(corners[2],new Vector2(Screen.width,Screen.height))>1)
                    throw new Exception("Background does not cover physical screen: "+background.name);
            }
            var canvas=FindFirstObjectByType<Canvas>();var camera=FindFirstObjectByType<Camera>();
            var texture=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
            var prior=RenderTexture.active;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            camera.targetTexture=texture;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=texture;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);
            camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(texture);
            if(!File.Exists(path))throw new Exception("Screenshot was not written: "+path);
        }
    }
}
#endif
