#if UNITY_EDITOR
using System;
using System.Linq;
using SockPairs.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    public partial class SockGame
    {
        RectTransform authoredScreens,authoredPopups,authoredOverlays;
        static T Named<T>(Transform parent,string name) where T:Component
        {var item=parent.GetComponentsInChildren<T>(true).FirstOrDefault(v=>v.name==name);if(!item)throw new Exception("Missing authoring object: "+name);return item;}
        static Text Caption(Transform parent,string text)
        {var item=parent.GetComponentsInChildren<Text>(true).FirstOrDefault(v=>v.text==text);if(!item)throw new Exception("Missing authoring text: "+text);return item;}
        static Button ButtonNamed(Transform parent,string text)=>Named<Button>(parent,text);
        SceneView Store(Transform source,string id,Transform parent)
        {
            source.SetParent(parent,false);source.name=id;
            var view=source.gameObject.AddComponent<SceneView>();view.id=id;sceneViews.Add(view);
            source.gameObject.SetActive(false);return view;
        }
        SceneView StoreScreen(string id)
        {var view=Store(screen,id,authoredScreens);screen=UI.Rect(root,"Screen",0,0,1080,1920);return view;}
        SceneView StorePopup(string id,RectTransform panel=null)
        {
            var view=Store(modal,id,authoredPopups);
            view.Set("panel",panel?panel:Named<RectTransform>(view.transform,"Panel"));
            view.Set("backdrop",view.GetComponentInChildren<RewardBackdrop>(true));
            var close=view.GetComponentsInChildren<Button>(true).FirstOrDefault(v=>v.name=="x");if(close)view.Set("close",close);
            modal=UI.Rect(root,"Modal",0,0,1080,1920);return view;
        }
        static void BindButton(SceneView view,string key,string name,bool label=false)
        {var button=ButtonNamed(view.transform,name);view.Set(key,button);if(label)view.Set(key+"Label",button.GetComponentInChildren<Text>(true));}
        static SceneToggle MakeToggle(Transform parent)
        {
            var toggle=parent.gameObject.AddComponent<SceneToggle>();toggle.button=parent.GetComponent<Button>();
            toggle.track=Named<Image>(parent,"Track");toggle.knob=Named<RectTransform>(parent,"White knob");return toggle;
        }
        static void BindToggle(SceneView view,string key,int index)
        {var toggle=view.GetComponentsInChildren<RectTransform>(true).Where(r=>r.name=="Toggle").ElementAt(index);view.Set(key,MakeToggle(toggle));}

        // Explicit migration command only. Normal editor opening, Play and builds NEVER regenerate these layouts.
        public void BakeCanvasScreens()
        {
            authoring=true;Instance=this;sceneViews.Clear();
            EnsureSceneUI();UI.Clear(root);screen=modal=toastLayer=null;
            authoredScreens=UI.Rect(root,"Screens",0,0,1080,1920);
            authoredPopups=UI.Rect(root,"Popups",0,0,1080,1920);
            authoredOverlays=UI.Rect(root,"Overlays",0,0,1080,1920);
            screen=UI.Rect(root,"Screen",0,0,1080,1920);modal=UI.Rect(root,"Modal",0,0,1080,1920);toastLayer=authoredOverlays;
            book=JsonUtility.FromJson<LevelBook>(Resources.Load<TextAsset>("Data/levels").text);
            var sheet=Resources.Load<Texture2D>("Art/FabricSocks");
            for(int i=0;i<24;i++){float w=sheet.width/6f,h=sheet.height/4f;sockArt[i]=Sprite.Create(sheet,new Rect(i%6*w,(3-i/6)*h,w,h),Vector2.one*.5f,100);ballArt[i]=sockArt[i];}
            save=new SaveData{unlocked=6,reduceMotion=true};platform=gameObject.AddComponent<PlatformBridge>();platform.youtube=true;platform.rewardedAdsAvailable=true;
            RefreshLoadingView();StoreScreen("Loading");

            Map(-1);var view=StoreScreen("Level map");view.Set("map",view.GetComponent<LevelMapView>());BindButton(view,"play","PLAY",true);BindButton(view,"album","Album");

            StartLevel(1,false);view=StoreScreen("Gameplay simple");BindGameplay(view,true);
            StartLevel(100,false);puzzle.capacity=7;BuildPlay();view=StoreScreen("Gameplay basket");BindGameplay(view,false);puzzle.capacity=5;

            Settings();view=StorePopup("Settings");BindButton(view,"done","Done");BindToggle(view,"sound",0);BindToggle(view,"music",1);BindToggle(view,"haptics",2);BindToggle(view,"motion",3);
            Pause();view=StorePopup("Pause");BindButton(view,"settings","Settings");BindButton(view,"resume","Resume");BindButton(view,"restart","Restart");BindButton(view,"home","Home");BindToggle(view,"sound",0);BindToggle(view,"music",1);
            Lose();view=StorePopup("Basket full");BindButton(view,"buy","+1 Space   100",true);BindButton(view,"retry","Retry");
            foreach(string type in new[]{"spot","hint","shuffle"})
            {
                Offer(type);var panel=Named<RectTransform>(modal,"Panel");
                if(!panel.GetComponentsInChildren<Button>(true).Any(b=>b.name=="Watch ad for helper"))UI.Button(panel,"Watch ad for helper",0,-380,700,135,UI.Purple,()=>{},46);
                view=StorePopup("Helper "+type);BindButton(view,"buy",type=="hint"?"Use 50 coins":"Use 100 coins",true);BindButton(view,"ad","Watch ad for helper");
            }
            var waiting=Modal("One moment...",820,UI.Purple);UI.Label(waiting,"Checking for an available ad",0,-60,600,130,36);StorePopup("Ad waiting");
            foreach(var config in book.levels.Where(c=>c.HasTutorial&&c.level!=1))
            {ShowTutorial(config.level);view=StorePopup("Tutorial "+config.level);BindButton(view,"done","Let's tidy!");}

            Album();var overlay=Named<RectTransform>(screen,"Album overlay");
            var album=Store(overlay,"Sock album",authoredPopups);album.Set("panel",Named<RectTransform>(overlay,"Album popup"));album.Set("backdrop",overlay.GetComponentInChildren<RewardBackdrop>(true));
            BindButton(album,"close","x");album.Set("count",Caption(overlay,"0 / 24 collected"));album.Set("pager",overlay.GetComponentInChildren<AlbumPager>(true));
            BindButton(album,"previous","<");BindButton(album,"next",">");
            for(int i=0;i<4;i++)album.Set("dot"+i,Named<Image>(overlay,"Page "+(i+1)));
            for(int i=0;i<24;i++)
            {
                var item=Named<RectTransform>(overlay,"Collection item "+i);album.Set("item"+i,item.GetComponent<Button>());
                album.Set("sock"+i,Named<Image>(item,"Collected sock"));album.Set("lock"+i,Named<RectTransform>(item,"Lock badge"));album.Set("lockGlyph"+i,Named<RectTransform>(item,"lock"));album.Set("state"+i,Caption(item,"Locked"));
            }
            UI.Clear(screen);
            OpenAlbumItem(0);view=StorePopup("Sock detail");BindButton(view,"done","Done");view.Set("title",Caption(view.transform,"Pink Stripes"));view.Set("sock",Named<Image>(view.transform,"Sock detail"));
            view.Set("lock",Named<RectTransform>(view.transform,"Lock badge"));view.Set("lockGlyph",Named<RectTransform>(view.transform,"lock"));view.Set("state",Caption(view.transform,"Not collected yet"));view.Set("description",Caption(view.transform,"Match this design in a level\nto add it to your album."));

            BakeReward(false);BakeReward(true);
            Shop();view=StoreScreen("Shop");view.Set("coins",coinLabel);BindButton(view,"settings","Settings");BindButton(view,"home","Back to Map");
            for(int i=0;i<3;i++)view.Set("buy"+i,Named<RectTransform>(view.transform,"Offer row "+i).GetComponentInChildren<Button>(true));
            Room();view=StoreScreen("Room themes");view.Set("coins",coinLabel);BindButton(view,"home","<");
            var choices=view.GetComponentsInChildren<Button>(true).Where(b=>b.name!="<").ToArray();for(int i=0;i<4;i++){view.Set("choice"+i,choices[i]);view.Set("label"+i,choices[i].GetComponentInChildren<Text>(true));}

            var toast=UI.Rect(authoredOverlays,"Toast",0,0,1080,1920);var message=UI.Box(toast,"Message",0,700,960,110,UI.Ink).rectTransform;
            var text=UI.Label(message,"Message",0,0,910,95,40,UI.Cream);view=Store(toast,"Toast",authoredOverlays);view.Set("message",text);
            var hint=UI.Rect(authoredOverlays,"First pair hint",0,-550,900,145);UI.Panel(hint,"Bubble",0,0,900,145,UI.Cream);UI.Label(hint,"Tap two identical socks\nto make a pair!",0,0,860,130,40);Store(hint,"First pair hint",authoredOverlays);


            var templates=UI.Rect(root,"Editable item templates",0,0,1080,1920);
            var sock=UI.Rect(templates,"Sock item template",0,0,136,173);sockTemplate=sock.gameObject.AddComponent<SockVisual>();
            var selection=UI.Rect(sock,"Selection glow",0,0,205,205);sockTemplate.selection=selection.gameObject;UI.Circle(selection,"Selected fill",0,0,205,new Color(1,.75f,.3f,.12f));UI.Glyph(selection,"halo",0,0,205,UI.Gold);
            sockTemplate.shadow=UI.Sprite(sock,"Soft shadow",sockArt[0],5,-8,160,160,new Color(.25f,.12f,.42f,.12f));sockTemplate.art=UI.Sprite(sock,"Sock art",sockArt[0],0,0,160,160);
            sockTemplate.hit=UI.Box(sock,"Tap target",0,0,136,173,Color.clear,false);var sockButton=sock.gameObject.AddComponent<Button>();sockButton.targetGraphic=sockTemplate.art;sockButton.transition=Selectable.Transition.None;
            sockTemplate.hole=UI.Box(sock,"Hole",8,-24,31,40,UI.Hex("624A45")).gameObject;UI.Label(sockTemplate.hole.transform,"!",0,0,40,48,34,UI.Cream);
            sockTemplate.knot=UI.Label(sock,"~X~",0,0,140,70,37,UI.Cream).gameObject;sockTemplate.gate=UI.Label(sock,"",0,-45,150,50,34,UI.Ink);UI.Stroke(sockTemplate.gate,UI.Cream,2);
            sockTemplate.hint=UI.Box(sock,"Hint dot",0,95,23,23,UI.Gold).gameObject;sockTemplate.sizeLabel=UI.Label(sock,"S",-45,65,60,50,34,UI.Ink);
            handTemplate=UI.Rect(templates,"Tutorial glove template",65,-80,310,260);UI.Sprite(handTemplate,"White glove",Resources.Load<Sprite>("Art/CandyHand"),0,0,310,260);
            glowTemplate=UI.Rect(templates,"Tutorial glow template",0,0,225,225);UI.Circle(glowTemplate,"Soft gold light",0,0,225,new Color(1,.8f,.2f,.13f));UI.Glyph(glowTemplate,"halo",0,0,225,UI.Gold);
            templates.gameObject.SetActive(false);

            DestroyImmediate(screen.gameObject);DestroyImmediate(modal.gameObject);DestroyImmediate(platform);platform=null;
            screen=(RectTransform)View("Loading").transform;modal=authoredPopups;toastLayer=authoredOverlays;
            screen.gameObject.SetActive(true);root.localScale=Vector3.one;root.anchoredPosition=Vector2.zero;
            playing=false;puzzle=null;save=null;currentScreen="boot";authoring=false;unlockAllLevelsForTesting=false;
            Debug.Log("Authored "+sceneViews.Count+" persistent Canvas screens/overlays.");
        }
        void BindGameplay(SceneView view,bool early)
        {
            view.Set("progress",progressLabel);view.Set("status",statusLabel);view.Set("progressFill",progressFill);view.Set("basket",basketFrame);
            view.Set("drawer",drawerLayer);view.Set("socks",sockLayer);view.Set("effects",fxLayer);view.Set("day",Caption(view.transform,"Day "+puzzle.layout.config.level));BindButton(view,"pause","II");
            if(early)return;
            view.Set("basketLabel",basketLabel);view.Set("pickLabel",pickLabel);view.Set("clothesline",Named<RectTransform>(view.transform,"Clothesline"));
            var washer=new[]{"Washer","Washer shadow","Washer rim","Washer window","Window reflection","3 pairs"};
            for(int i=0;i<washer.Length;i++)view.Set("washer:"+i,Named<RectTransform>(view.transform,washer[i]));
            var bin=new[]{"Odd sock bin","Bin lid","Odd sock\nbin"};for(int i=0;i<bin.Length;i++)view.Set("bin:"+i,Named<RectTransform>(view.transform,bin[i]));
            view.Set("washerText",Caption(view.transform,"3 pairs"));view.Set("washerWindow",Named<Image>(view.transform,"Washer window"));
            BindButton(view,"spot","+1 Slot");BindButton(view,"hint","Hint");BindButton(view,"shuffle","Shuffle");
        }
        void BakeReward(bool last)
        {
            RewardBackdrop.Capture(modal);var dim=UI.Box(modal,"Dim",0,0,1080,1920,new Color(.03f,.02f,.01f,.55f),false);dim.raycastTarget=true;dim.gameObject.AddComponent<FullBleed>();ReferenceBArt.Rim(modal);
            var panel=ReferenceReward.Build(modal,20,24,0,last,()=>{});
            var map=UI.Button(modal,"Back to Map",0,-790,480,100,UI.Purple,()=>{},46);
            map.gameObject.SetActive(!last);
            var view=StorePopup(last?"Campaign complete":"Level complete",panel);
            view.Set("coins",Caption(panel,"+20"));view.Set("collection",Caption(panel,"24 / 24"));view.Set("map",map);
            var fill=Named<RectTransform>(panel,"Album fill");
            var container=UI.Rect(panel,"Album progress bounds",fill.anchoredPosition.x,fill.anchoredPosition.y,fill.sizeDelta.x,fill.sizeDelta.y);
            fill.SetParent(container,false);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;view.Set("fill",fill);
            var stars=panel.GetComponentsInChildren<ReferenceBArt>(true).Where(g=>g.name=="Unearned star").ToArray();for(int i=0;i<3;i++)view.Set("unearned"+i,stars[i]);
            BindButton(view,"next",last?"Back to Map":"Next level");
        }
    }
}
#endif
