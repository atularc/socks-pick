using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SockPairs.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SockPairs
{
    public class SockGame : MonoBehaviour
    {
        public static SockGame Instance;
        [Header("Testing")]
        [Tooltip("Unlock all 100 map nodes for testing without granting campaign progression. Disable before release.")]
        public bool unlockAllLevelsForTesting = true;
        public SaveData save;
        public Puzzle puzzle;
        LevelBook book;
        PlatformBridge platform;
        SoundBank sound;
        [Header("Scene UI")]
        [SerializeField] Canvas uiCanvas;
        [SerializeField] EventSystem uiEventSystem;
        [SerializeField] RectTransform root,screen,modal,toastLayer;
        RectTransform sockLayer,fxLayer,drawerLayer;
        Text progressLabel,timeLabel,statusLabel,coinLabel,basketLabel,pickLabel;
        Image progressFill,basketFrame;
        readonly int[] earlyGridSlots=new int[36];
        Sprite[] sockArt=new Sprite[24],ballArt=new Sprite[24];
        readonly Dictionary<int,RectTransform> views=new Dictionary<int,RectTransform>();
        readonly Dictionary<int,SockVisual> visuals=new Dictionary<int,SockVisual>();
        readonly List<int> drawer=new List<int>();
        float elapsed,saveClock,lastTap;
        RectTransform idleHand;
        bool showingTutorial;
        int helpers,mapChapter=-1,hintId=-1,animationEpoch;
        bool playing,hostPaused,finishing,adUsed;
        string currentScreen="boot";
        Rect lastSafe;
        float lastWidth,lastHeight,lastCanvasScale=-1;
        int displayedSeconds=-1,newDesigns;

        void Awake()
        {
            Instance=this;gameObject.name="SockPairs";
            #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            unlockAllLevelsForTesting=false;
            #endif
            QualitySettings.SetQualityLevel(0,true);
            Application.targetFrameRate=60;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;QualitySettings.shadows=ShadowQuality.Disable;
            book=JsonUtility.FromJson<LevelBook>(Resources.Load<TextAsset>("Data/levels").text);
            var sheet=Resources.Load<Texture2D>("Art/FabricSocks");
            for(int i=0;i<24;i++)
            {
                float w=sheet.width/6f,h=sheet.height/4f;
                sockArt[i]=Sprite.Create(sheet,new Rect((i%6)*w,(3-i/6)*h,w,h),new Vector2(.5f,.5f),100);
                ballArt[i]=sockArt[i]; // Reuse the fabric sheet for folded-pair effects; no extra atlas.
            }
            EnsureSceneUI();
            sound=gameObject.AddComponent<SoundBank>();sound.Initialize();
            platform=gameObject.AddComponent<PlatformBridge>();
            platform.Loaded=OnLoaded;
            platform.Paused=p=>{hostPaused=p;AudioListener.pause=p;if(p)Persist();};
            platform.AudioChanged=_=>ApplySound();
            platform.Initialize();Fit();
        }
        // Called by scene setup in the Editor. Awake reuses these serialized objects.
        public void EnsureSceneUI()
        {
            if(!uiCanvas)
            {
                var canvasObject=new GameObject("Canvas",typeof(Canvas),typeof(GraphicRaycaster));
                uiCanvas=canvasObject.GetComponent<Canvas>();uiCanvas.renderMode=RenderMode.ScreenSpaceOverlay;
                var surround=UI.Box(canvasObject.transform,"Surround",0,0,0,0,UI.Cream,false).rectTransform;
                surround.anchorMin=Vector2.zero;surround.anchorMax=Vector2.one;surround.sizeDelta=Vector2.zero;
            }
            if(!root)root=UI.Rect(uiCanvas.transform,"Portrait safe area",0,0,1080,1920);
            if(!screen)screen=UI.Rect(root,"Screen",0,0,1080,1920);
            if(!modal)modal=UI.Rect(root,"Modal",0,0,1080,1920);
            if(!toastLayer)toastLayer=UI.Rect(root,"Toast",0,0,1080,1920);
            var loading=screen.Find("Loading background");
            var surroundImage=uiCanvas.transform.Find("Surround");if(surroundImage&&surroundImage.TryGetComponent<Image>(out var surroundFace))surroundFace.color=UI.Board;
            if(!uiEventSystem)
                uiEventSystem=FindFirstObjectByType<EventSystem>()??new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            if(screen.childCount==0||loading)RefreshLoadingView();
        }
        public void RefreshLoadingView()
        {
            if(!screen)return;
            if(Application.isPlaying)UI.Clear(screen);
            else for(int i=screen.childCount-1;i>=0;i--)DestroyImmediate(screen.GetChild(i).gameObject);
            var view=UI.Rect(screen,"Cosy loading screen",0,0,1080,1920);
            var room=UI.Rect(view,"Loading room",0,0,1080,1920).gameObject.AddComponent<RawImage>();
            room.texture=Resources.Load<Texture2D>("Art/CosyRoom");room.raycastTarget=false;room.gameObject.AddComponent<FullBleed>();
            var shade=UI.Box(view,"Loading shade",0,0,1080,1920,new Color(.03f,.06f,.04f,.35f),false);shade.gameObject.AddComponent<FullBleed>();
            var tray=UI.Rect(view,"Loading tray",0,0,900,900);ReferenceBArt.Felt(tray,true);
            ReferenceBArt.Add(tray,"Sock Pairs logo",new Rect(154,150,296,73),0,220,700,173,32);
            UI.Label(tray,"A little mess.\nA lovely match.",0,0,700,150,46,UI.Cream);
            UI.Label(tray,"Unpacking your laundry...",0,-150,720,70,36,UI.Cream);
            UI.Sprite(tray,"Loading basket",CosyArt.Get("CosyBasket"),0,-300,330,172);
            Fit();
        }
        void OnLoaded(string json)
        {
            save=SaveData.Read(json);ApplySound();Home();platform.GameReady();
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-sockLevel"&&int.TryParse(args[i+1],out int level))StartLevel(Mathf.Clamp(level,1,100));
            #endif
        }
        void Fit()
        {
            Rect area=Screen.safeArea;
            float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            // CanvasScaler already converts canvas units to pixels. Compensate once,
            // including safe-area offsets, instead of multiplying both scales.
            float canvasScale=Mathf.Max(.0001f,uiCanvas.scaleFactor);
            root.localScale=Vector3.one*(scale/canvasScale);
            root.anchoredPosition=(area.center-new Vector2(Screen.width*.5f,Screen.height*.5f))/canvasScale;
            lastCanvasScale=canvasScale;
            FitMapLayout();
            FitShopLayout();
            FitSurfaceLayout();
            lastSafe=area;lastWidth=Screen.width;lastHeight=Screen.height;
        }
        void LateUpdate()
        {
            // CanvasScaler updates during Update; read its final value afterwards.
            if(lastWidth!=Screen.width||lastHeight!=Screen.height||lastSafe!=Screen.safeArea||!Mathf.Approximately(lastCanvasScale,uiCanvas.scaleFactor))
            {
                Fit();
                if(playing&&puzzle!=null&&sockLayer)DrawSocks(null);
            }
        }
        void FitMapLayout()
        {
            if(currentScreen!="map"||!screen)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=0; // Reference B uses a fixed-proportion stage inside the safe area.
            foreach(string name in new[]{"Map logo","Map logo title","Settings"})
            {var r=screen.Find(name) as RectTransform;if(r)r.anchoredPosition=new Vector2(r.anchoredPosition.x,745+extra);}
            var viewport=screen.Find("Scrollable level path") as RectTransform;
            if(viewport)viewport.sizeDelta=new Vector2(880,1080);
            var bar=screen.Find("Map navigation") as RectTransform;if(bar)bar.anchoredPosition=new Vector2(0,-680);
            var play=screen.Find("PLAY") as RectTransform;if(play)play.anchoredPosition=new Vector2(0,-584);
            var album=screen.Find("Album") as RectTransform;if(album)album.anchoredPosition=new Vector2(0,-762);
        }
        void FitShopLayout()
        {
            if(currentScreen!="shop"||!screen)return;
            Rect area=Screen.safeArea;
            float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=0; // Reference B uses a fixed-proportion stage inside the safe area.
            foreach(string name in new[]{"Map HUD"})
            {
                var item=screen.Find(name) as RectTransform;
                if(item)item.anchoredPosition=new Vector2(0,extra);
            }
            var back=screen.Find("Back to Map") as RectTransform;
            if(back)back.anchoredPosition=new Vector2(0,-805-extra);
        }
        void FitSurfaceLayout()
        {
            if(!screen)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=0; // Reference B uses a fixed-proportion stage inside the safe area.
            string surface=currentScreen=="play"?"Cream play surface":currentScreen=="album"?"Album surface":null;
            if(surface==null)return;
            float top=currentScreen=="play"?(puzzle.layout.config.basket==0?651:675):920;
            float bottom=currentScreen=="play"?(puzzle.layout.config.basket==0?-472:-370):-930;
            foreach(string name in new[]{surface,surface+" shadow"})
            {
                var item=screen.Find(name) as RectTransform;
                if(item){item.sizeDelta=new Vector2(currentScreen=="play"?(puzzle.layout.config.basket==0?971:960):1000,top-bottom+extra*1.35f);item.anchoredPosition=new Vector2(0,(top+bottom)*.5f+extra*.325f-(name.EndsWith("shadow")?14:0));}
            }
        }
        void PinScreenEdges()
        {
            foreach(RectTransform item in screen)
            {
                if(item.GetComponent<FullBleed>()||item.GetComponent<SafeEdgeOffset>())continue;
                float y=item.anchoredPosition.y;
                // A shadow follows its face's anchoring, even when its depth
                // crosses the threshold between a fixed and pinned element.
                if(item.name.EndsWith(" shadow"))
                {
                    var face=screen.Find(item.name.Substring(0,item.name.Length-7)) as RectTransform;
                    if(face)y=face.anchoredPosition.y;
                }
                if(y>=650)SafeEdgeOffset.Pin(item,1);
                else if(y<=-650||(currentScreen=="play"&&puzzle!=null&&puzzle.capacity>0&&y<=-400))SafeEdgeOffset.Pin(item,-1);
            }
        }
        void Update()
        {
            if(save==null||hostPaused)return;
            if(playing&&!finishing&&modal.childCount==0)
            {
                elapsed+=Time.unscaledDeltaTime;saveClock+=Time.unscaledDeltaTime;
                if(timeLabel&&(int)elapsed!=displayedSeconds){displayedSeconds=(int)elapsed;timeLabel.text=TimeText(elapsed);}
                if(saveClock>8){saveClock=0;Persist();}
                if(!showingTutorial&&!idleHand&&Time.unscaledTime-lastTap>8){lastTap=Time.unscaledTime;StartCoroutine(IdleHint());}
                if(basketFrame&&puzzle.capacity>0)
                    basketFrame.color=puzzle.basket.Count>=puzzle.capacity-1?Color.Lerp(Color.white,UI.Pink,save.reduceMotion?.25f:.15f+.1f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2)):Color.white;
                if(puzzle!=null&&puzzle.layout.config.line>0)
                {
                    foreach(var s in puzzle.Socks)
                        if(s.line&&s.place==Place.Pile&&views.TryGetValue(s.id,out var r)&&r) r.anchoredPosition=Position(s)+new Vector2(Mathf.Sin(Time.time*.65f+s.slot)*13,Mathf.Sin(Time.time*.9f+s.slot)*4);
                }
            }
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(modal.childCount>0){if(playing&&!puzzle.lost)CloseModal();else Home();}else if(playing)Pause();else Home();
            }
        }
        void OnApplicationPause(bool paused){if(paused)Persist();}
        void OnApplicationQuit(){Persist();}
        void OnDestroy(){foreach(var sprite in sockArt)if(sprite)Destroy(sprite);UI.DisposeIcons();}
        void ApplySound(){if(save!=null)sound.Settings(save.sound,save.music,platform.audioAllowed);}
        public void ButtonSound(){if(sound)sound.Play("tap");}
        void Persist()
        {
            if(save==null)return;
            if(playing&&puzzle!=null&&unlockAllLevelsForTesting&&puzzle.layout.config.level>save.unlocked)
                save.active=null; // Do not save a resume point that becomes locked when testing is disabled.
            else if(playing&&puzzle!=null&&!puzzle.Won)
                save.active=new Snapshot{level=puzzle.layout.config.level,levelRevision=puzzle.layout.config.revision,selected=puzzle.selected,matched=puzzle.matched,
                    capacity=puzzle.capacity,moves=puzzle.moves,helpers=helpers,elapsed=elapsed,basket=puzzle.basket.ToArray(),
                    socks=puzzle.Socks.ToArray(),lost=puzzle.lost,adUsed=adUsed};
            platform.Save(JsonUtility.ToJson(save));
        }
        void NewScreen(string name)
        {
            animationEpoch++;UI.Clear(screen);UI.Clear(modal);UI.Clear(toastLayer);views.Clear();visuals.Clear();
            playing=false;finishing=false;currentScreen=name;if(name!="map"&&name!="play")UI.RoomBackground(screen,true);
        }
        void Header(string title,string subtitle)
        {
            UI.Label(screen,title,0,826,930,96,62,UI.Pink);
            UI.Label(screen,subtitle,0,753,930,55,26);
            UI.Button(screen,"<",-451,882,96,80,UI.Blue,Home,40);
        }
        static bool CashShopAvailable
        {
            get
            {
#if UNITY_WEBGL
                return false;
#else
                return true;
#endif
            }
        }
        void Coins(Transform parent,float x=255,float y=850)
        {
            UI.Inset(parent,"Coin counter",x,y,235,95);UI.Coin(parent,x-73,y,65);
            coinLabel=UI.Label(parent,save.coins.ToString(),x+32,y,145,82,45);
        }
        void MapHud()
        {
            var hud=UI.Rect(screen,"Map HUD",0,0,1080,1920);
            UI.Box(hud,"HUD strip",0,880,1080,160,UI.Ink,false);
            UI.Label(hud,currentScreen=="shop"?"Laundry shop":"Sock Pairs",-225,850,540,100,74,UI.Cream,TextAnchor.MiddleLeft);
            Coins(hud);UI.Button(hud,"Settings",455,850,110,100,UI.Purple,Settings,40);
        }

        public void Home()
        {
            Persist(); Map(-1);
        }
        void Map(int chapter)
        {
            NewScreen("map");
            UI.RoomBackground(screen,false,"map");
            int next=save.active!=null&&save.active.level<=save.unlocked?save.active.level:save.unlocked;
            int start=chapter<0?Mathf.Clamp(next-3,1,94):Mathf.Clamp(chapter*10+1,1,94);
            mapChapter=(start-1)/10;
            ReferenceBArt.Add(screen,"Map logo",new Rect(154,150,296,73),0,745,650,160,32);

            var map=screen.gameObject.GetComponent<LevelMapView>()??screen.gameObject.AddComponent<LevelMapView>();
            map.Build(screen,save,next,level=>StartLevel(level),unlockAllLevelsForTesting);
            UI.Box(screen,"Map navigation",0,-680,1040,440,Color.clear,false);
            MapNavigationButton("PLAY",0,830,UI.Pink,1,()=>{if(save.active!=null)Resume();else StartLevel(save.unlocked);});
            MapNavigationButton("Album",0,580,UI.Purple,2,Album);
            FitMapLayout();
        }
        void MapNavigationButton(string title,float x,float width,Color color,int icon,Action action)
        {
            bool primary=title=="PLAY";
            if(primary)
            {
                int level=save.active!=null?save.active.level:save.unlocked;
                var live=UI.Button(screen,"Play level "+level,0,-584,834,187,UI.Pink,action,72);
                live.name="PLAY";return;
            }
            var rect=UI.Rect(screen,title,0,primary?-584:-762,primary?834:578,primary?187:141);
            var face=ReferenceBArt.Add(rect,"Button face",primary?new Rect(122,733,361,81):new Rect(178,820,250,61),0,0,rect.sizeDelta.x,rect.sizeDelta.y,primary?40:30);
            face.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=face;
            rect.gameObject.AddComponent<ButtonMotion>();
            button.onClick.AddListener(()=>{ButtonSound();action();});

        }

        void Shop()
        {
            if(!CashShopAvailable)return;
            NewScreen("shop");MapHud();
            var offers=UI.Rect(screen,"Shop content",0,0,1080,1920);
            var awning=UI.Label(offers,"A little help with the laundry",0,690,950,100,49);SafeEdgeOffset.Pin(awning.rectTransform,1);
            string[] titles={"Coins Pack\nx200","+1 Space","Shuffle"};
            string[] descriptions={"","Add 1 slot\nto the basket.","Mix all socks\non the board."};
            for(int i=0;i<3;i++)
            {
                float y=0;
                var row=UI.Rect(offers,"Offer row "+i,0,360-i*410,1080,330);SafeEdgeOffset.Pin(row,1-i*.7f);
                UI.Inset(row,"Offer card",0,y,990,330);
                if(i==0)UI.Coin(row,-355,y,150);
                else if(i==1){UI.Basket(row,-355,y-15,245,180);UI.Circle(row,"Plus badge",-325,y+55,100,UI.Pink);UI.Label(row,"+1",-325,y+55,100,85,48,Color.white);}
                else UI.Glyph(row,"restart",-335,y,160,UI.Purple);
                UI.Label(row,titles[i],-20,y+(i==0?0:60),390,i==0?175:85,58,UI.Ink,TextAnchor.MiddleLeft);
                UI.Label(row,descriptions[i],-20,y-40,390,115,44,UI.Ink,TextAnchor.MiddleLeft);
                UI.Button(row,i==0?"$1.99":"$0.99",335,y,250,145,UI.Pink,PurchaseUnavailable,54);
            }
            UI.Button(screen,"Back to Map",0,-805,650,135,UI.Purple,Home,54);
            FitShopLayout();
            
        }
        void PurchaseUnavailable(){Toast("Purchases are not enabled in this build. Earn coins by playing.");}
        void BuyPack()
        {
            if(save.coins<800){Toast("You need 800 coins. Earn coins by completing levels.");return;}
            save.coins-=800;save.spots+=3;save.hints+=3;save.shuffles+=3;Persist();Shop();Toast("3 of each helper added!");
        }
        int albumPage;
        void Album()
        {
            Map(-1);currentScreen="album";
            var overlay=UI.Rect(screen,"Album overlay",0,0,1080,1920);RewardBackdrop.Capture(overlay);
            var dim=UI.Box(overlay,"Album dim",0,0,1080,1920,new Color(.03f,.02f,.01f,.55f),false);dim.raycastTarget=true;dim.gameObject.AddComponent<FullBleed>();
            var panel=UI.Panel(overlay,"Album popup",0,0,900,1550,UI.Cream).rectTransform;
            UI.Glyph(panel,"album",0,650,110,UI.Ink);
            UI.Label(panel,"Sock album",-25,460,500,95,60);
            UI.Button(panel,"x",300,460,100,100,UI.Pink,Home,40);
            UI.Label(panel,save.collection.Count(x=>x)+" / 24 collected",0,385,600,70,42);
            UI.Label(panel,"Little finds from every laundry day",0,310,740,60,32);
            var viewport=UI.Box(panel,"Album swipe area",0,-70,800,720,Color.clear,false).rectTransform;
            viewport.GetComponent<Image>().raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
            var content=UI.Rect(viewport,"Album pages",0,0,3200,720);
            content.anchorMin=content.anchorMax=new Vector2(0,.5f);content.pivot=new Vector2(0,.5f);
            var pager=viewport.gameObject.AddComponent<AlbumPager>();pager.viewport=viewport;pager.content=content;pager.pageCount=4;
            pager.horizontal=true;pager.vertical=false;pager.inertia=false;pager.movementType=ScrollRect.MovementType.Clamped;
            pager.instant=save.reduceMotion;
            for(int page=0;page<4;page++)
            {
                var pageRoot=UI.Rect(content,"Album page "+(page+1),page*800+400,0,800,720);
                pageRoot.anchorMin=pageRoot.anchorMax=new Vector2(0,.5f);
                for(int i=0;i<6;i++)
                {
                    int design=page*6+i;float x=(i%3-1)*255,y=180-i/3*360;
                    bool found=save.collection[design];
                    var tile=UI.Rect(pageRoot,"Collection item "+design,x,y,225,280);
                    var face=UI.Inset(tile,"Collection tile",0,0,225,280);face.raycastTarget=true;
                    UI.Sprite(tile,"Collected sock",sockArt[design],0,25,195,215,found?Color.white:new Color(0,0,0,.18f));
                    if(!found)
                    {
                        UI.Circle(tile,"Lock badge",55,-45,78,UI.Cream);
                        UI.Glyph(tile,"lock",55,-45,51,UI.Ink);
                    }
                    UI.Label(tile,found?"Collected":"Locked",0,-100,195,55,34,found?UI.Ink:UI.Muted);
                    var button=tile.gameObject.AddComponent<Button>();button.targetGraphic=face;
                    button.onClick.AddListener(()=>{ButtonSound();OpenAlbumItem(design);});
                }
            }
            var previous=UI.Button(panel,"<",-300,-590,100,100,UI.Purple,()=>pager.GoTo(pager.Page-1),45);
            var next=UI.Button(panel,">",300,-590,100,100,UI.Purple,()=>pager.GoTo(pager.Page+1),45);
            var dots=new Image[4];
            for(int i=0;i<4;i++)
            {
                int page=i;dots[i]=UI.Medal(panel,"Page "+(i+1),(i-1.5f)*65,-590,32,UI.Purple);dots[i].raycastTarget=false;
            }
            pager.PageChanged=page=>{albumPage=page;previous.interactable=page>0;next.interactable=page<3;for(int i=0;i<4;i++)dots[i].color=i==page?UI.Pink:UI.Purple;};
            pager.GoTo(albumPage,false);
        }
        void OpenAlbumItem(int design)
        {
            string[] colors={"Pink","Blue","Yellow","Green","Purple","Coral"};
            string[] patterns={"Stripes","Dots","Stars","Zigzags"};
            var panel=Modal(colors[design%6]+" "+patterns[design/6],1180);
            bool collected=save.collection[design];
            UI.Sprite(panel,"Sock detail",sockArt[design],0,40,360,360,collected?Color.white:new Color(0,0,0,.18f));
            if(!collected){UI.Circle(panel,"Lock badge",100,-15,110,UI.Cream);UI.Glyph(panel,"lock",100,-15,72,UI.Ink);}
            UI.Label(panel,collected?"Collected":"Not collected yet",0,-200,720,70,48,UI.Ink);
            UI.Label(panel,collected?"You matched this design!":"Match this design in a level\nto add it to your album.",0,-295,720,110,40,UI.Ink);
            UI.Button(panel,"Done",0,-435,620,125,UI.Pink,CloseModal,54);
        }

        void Room()
        {
            NewScreen("room");Header("YOUR LAUNDRY", "Make a little space your own");Coins(screen);
            string[] names={"Sunny morning","Mint daydream","Rose cottage","Lavender evening"};
            Color[] colors={UI.Hex("F2DDBB"),UI.Hex("D3EAE0"),UI.Hex("F5D5DE"),UI.Hex("E1D5F1")};
            for(int i=0;i<4;i++)
            {
                int choice=i;float y=500-i*335;
                UI.Panel(screen,"Room sample",0,y,890,275,colors[i]);
                UI.Sprite(screen,"Room sock",sockArt[i*6],-310,y,120,170);
                UI.Label(screen,names[i],90,y+70,530,60,32);
                bool owned=(save.decor&(1<<i))!=0||i==0;
                UI.Button(screen,owned?(save.theme==i?"SELECTED":"Use this room"):"150 coins",100,y-45,460,90,owned?UI.Green:UI.Purple,()=>
                {
                    if(owned){save.theme=choice;Persist();Room();return;}
                    if(save.coins<150){Toast("Earn coins by completing laundry days.");return;}
                    save.coins-=150;save.decor|=1<<choice;save.theme=choice;Persist();Room();sound.Play("win");
                },30);
            }
            UI.Label(screen,"Decorations are earned with play. No purchases.",0,-871,990,65,25);PinScreenEdges();
        }
        void Settings()
        {
            var panel=Modal("Settings",1360);
            Toggle(panel,"Sound",save.sound,250,()=>save.sound=!save.sound);
            Toggle(panel,"Music",save.music,70,()=>save.music=!save.music);
            Toggle(panel,"Vibration",save.haptics,-110,()=>save.haptics=!save.haptics);
            Toggle(panel,"Reduced motion",save.reduceMotion,-290,()=>save.reduceMotion=!save.reduceMotion);
            
            UI.Button(panel,"Done",0,-450,680,135,UI.Pink,()=>{CloseModal();Persist();},64);
        }
        void Toggle(Transform panel,string text,bool on,float y,Action change)
        {
            if(text=="Sound"||text=="Music")UI.Glyph(panel,text=="Sound"?"sound":"music",-355,y,65,UI.Ink);
            UI.Label(panel,text,-90,y,420,90,46,UI.Ink,TextAnchor.MiddleLeft);
            UI.Switch(panel,270,y,on,()=>{change();ApplySound();Persist();});
        }
        void Resume()
        {
            var snap=save.active;if(snap==null){StartLevel(save.unlocked);return;}
            // Changed layouts restart only the active round; campaign progress and rewards are retained.
            int resumeLevel=Mathf.Clamp(snap.level,1,100);
            if(snap.levelRevision!=book.levels[resumeLevel-1].revision){StartLevel(resumeLevel);return;}
            StartLevel(Mathf.Clamp(snap.level,1,100),false);
            if(snap.socks!=null&&snap.socks.Length==puzzle.Socks.Count)
            {
                puzzle.layout.socks=snap.socks.ToList();puzzle.selected=snap.selected;puzzle.matched=snap.matched;
                puzzle.capacity=snap.capacity;puzzle.moves=snap.moves;puzzle.lost=snap.lost;
                puzzle.basket.Clear();if(snap.basket!=null)puzzle.basket.AddRange(snap.basket);
                elapsed=Mathf.Max(0,snap.elapsed);helpers=snap.helpers;adUsed=snap.adUsed;BuildPlay();
                if(puzzle.lost)Lose();
                Persist();
            }
        }
        public void StartLevel(int level,bool tutorial=true)
        {
            if(level<1||level>100)return;
            NewScreen("play");puzzle=new Puzzle(LevelFactory.Create(book.levels[level-1]));
            elapsed=0;helpers=0;newDesigns=0;displayedSeconds=-1;hintId=-1;drawer.Clear();playing=true;finishing=false;adUsed=false;doubling=false;lastTap=Time.unscaledTime;showingTutorial=false;
            BuildPlay();Persist();
            if(!save.reduceMotion)StartCoroutine(DropIn());
            if(tutorial&&puzzle.layout.config.HasTutorial)ShowTutorial(level);
        }
        void BuildPlay()
        {
            animationEpoch++;UI.Clear(screen);views.Clear();visuals.Clear();UI.RoomBackground(screen,puzzle.layout.config.basket>0);
            UI.Panel(screen,"Cream play surface",0,-185,1030,1490,UI.Board);
            var c=puzzle.layout.config;
            bool early=c.basket==0;
            FitSurfaceLayout();
            if(early){BuildEarlyPlay();PinScreenEdges();return;}
            BuildPlayHeader(c.level,c.pairs,575,90);

            if(c.line>0)
            {
                UI.Box(screen,"Clothesline",0,505,800,5,UI.Line,false);

            }
            UI.Panel(screen,"Selected mat",0,-255,220,170,UI.Cream);
            pickLabel=UI.Label(screen,"Your pick",0,-255,180,80,40,UI.Ink);
            pickLabel.gameObject.AddComponent<CenteredGlyphs>();
            if(c.washer>0)
            {
                UI.Panel(screen,"Washer",-320,-255,220,170,UI.Cream);
                UI.Medal(screen,"Washer rim",-320,-220,65,UI.Hex("A0B8C1"));
                UI.Circle(screen,"Washer window",-320,-220,51,puzzle.matched>=3?UI.Green:UI.Blue);
                UI.Circle(screen,"Window reflection",-330,-210,16,new Color(1,1,1,.4f));
                var washerText=UI.Label(screen,puzzle.matched>=3?"Ready":Mathf.Max(0,3-puzzle.matched)+" pairs",-320,-295,180,55,36,UI.Ink);
                washerText.gameObject.AddComponent<CenteredGlyphs>();
            }
            if(c.odd>0)
            {
                UI.Box(screen,"Odd sock bin",320,-255,220,170,UI.Mint);
                UI.Box(screen,"Bin lid",320,-175,220,14,UI.Hex("749C94"));
                var binText=UI.Label(screen,"Odd sock\nbin",320,-255,180,120,36,UI.Ink);
                binText.gameObject.AddComponent<CenteredGlyphs>();
            }
            basketFrame=UI.Basket(screen,0,-505,1000,280);
            if(puzzle.capacity>0)
            {
                int spots=puzzle.capacity;
                for(int i=0;i<spots;i++)
                {
                    var slot=UI.BasketSlot(basketFrame,i,spots);float width=UI.BasketImageRect(basketFrame).width;
                    UI.Box(basketFrame.transform,"Basket space",slot.x,slot.y,width*Mathf.Min(.09f,.38f/Mathf.Max(1,spots-1)),65,new Color(.87f,.95f,.91f,.55f));
                }
                UI.Panel(screen,"Basket counter plate",0,-665,640,70,UI.Cream);
                basketLabel=UI.Label(screen,"Basket  "+puzzle.basket.Count+" / "+puzzle.capacity,0,-665,600,50,40,UI.Ink);
            }
            else UI.Label(screen,"No basket yet. Take your time!",0,-480,890,100,30,UI.Muted);

            drawerLayer=UI.Rect(screen,"Finished pairs",0,0,1080,1920);DrawDrawer();
            HelperButton("+1 Slot","100 coins",-290,UI.Purple,()=>Offer("spot"));
            HelperButton("Hint","50 coins",0,UI.Blue,()=>Offer("hint"));
            HelperButton("Shuffle","100 coins",290,UI.Pink,()=>Offer("shuffle"));
            timeLabel=null; // Time still tracks internally; keep the basket area free of unrelated counters.
            sockLayer=UI.ClippedLayer(screen,"Socks",early?590:695,-615);fxLayer=UI.ClippedLayer(screen,"Effects",early?590:695,-950);
            PinScreenEdges();SafeEdgeOffset.Pin(drawerLayer,-1);
            DrawSocks(null);UpdateStatus();
        }
        void BuildPlayHeader(int level,int pairs,float titleY,float titleHeight)
        {
            float headerY=758;
            UI.Panel(screen,"Level pill",-75,headerY,720,145,UI.Cream);
            UI.Label(screen,"Day "+level,-292,headerY,245,110,52,UI.Ink);
            UI.Box(screen,"Header divider",-150,headerY,3,86,UI.Ink,false);
            progressLabel=UI.Label(screen,"Pairs "+puzzle.matched+" / "+pairs,70,headerY,390,110,52,UI.Ink);
            UI.Button(screen,"II",375,headerY,140,140,UI.Pink,Pause,38);
            if(puzzle.layout.config.basket==0)
            {
                ReferenceBArt.Add(screen,"Header left stud",new Rect(565,172,20,22),-416,headerY,45,49,10);
                ReferenceBArt.Add(screen,"Header right stud",new Rect(874,173,18,19),256,headerY,40,42,9);
                ReferenceBArt.Add(screen,"Instruction left rays",new Rect(630,250,37,52),-266,titleY,83,116).CutOutTeal();
                ReferenceBArt.Add(screen,"Instruction right rays",new Rect(873,250,37,52),277,titleY,83,116).CutOutTeal();
            }
            var titlePlate=UI.Panel(screen,"Title ribbon",0,titleY,titleY<600?450:590,titleHeight,UI.Mint);
            SafeEdgeOffset.Pin(titlePlate.rectTransform,1);
            SafeEdgeOffset.Pin(screen.Find("Title ribbon shadow") as RectTransform,1);
            statusLabel=UI.Label(screen,"Find a pair",0,titleY,titleY<600?420:550,titleHeight-10,earlyTitleSize(titleHeight),UI.Ink);
            SafeEdgeOffset.Pin(statusLabel.rectTransform,1);
            UI.Box(screen,"Progress track",0,795,920,0,UI.Line);
            progressFill=UI.Box(screen,"Pair progress",-460,795,0,0,UI.Green);
            progressFill.rectTransform.pivot=new Vector2(0,.5f);
            progressFill.rectTransform.sizeDelta=new Vector2(920f*puzzle.matched/pairs,0);
        }
        static int earlyTitleSize(float h){return h>100?56:46;}
        void BuildEarlyPlay()
        {
            // Keep the seeded puzzle intact while removing unused visual grid cells.
            for(int i=0;i<earlyGridSlots.Length;i++)earlyGridSlots[i]=-1;
            foreach(var sock in puzzle.Socks)earlyGridSlots[sock.slot]=0;
            int cell=0;for(int i=0;i<earlyGridSlots.Length;i++)if(earlyGridSlots[i]==0)earlyGridSlots[i]=cell++;
            BuildPlayHeader(puzzle.layout.config.level,puzzle.layout.config.pairs,548,110);
            timeLabel=null;basketLabel=null;
            basketFrame=UI.Basket(screen,0,-680,990,480);
            drawerLayer=UI.Rect(basketFrame.transform,"Finished pairs",0,0,1200,400);
            drawerLayer.anchorMin=Vector2.zero;drawerLayer.anchorMax=Vector2.one;drawerLayer.sizeDelta=Vector2.zero;
            DrawDrawer();UI.BasketFront(basketFrame);
            sockLayer=UI.ClippedLayer(screen,"Socks",590,-615);fxLayer=UI.ClippedLayer(screen,"Effects",590,-950);
            DrawSocks(null);
        }
        void DrawDrawer()
        {
            UI.Clear(drawerLayer);
            if(puzzle.layout.config.basket==0)
            {
                int visible=Mathf.Min(5,puzzle.matched),first=Mathf.Max(0,puzzle.matched-visible);
                float size=UI.BasketImageRect(basketFrame).width*.16f;
                for(int i=0;i<visible;i++)
                {
                    Vector2 slot=UI.CompletedPairSlot(basketFrame,i,visible);
                    int entry=first+i,design=drawer.Count>entry?drawer[entry]:entry%ballArt.Length;
                    UI.Sprite(drawerLayer,"Rolled pair",ballArt[design],slot.x,slot.y,size,size);
                }
                return;
            }
            // The header already reports matched pairs; keep the footer dedicated to waiting socks.
        }
        void HelperButton(string title,string price,float x,Color color,Action action)
        {
            var button=UI.Button(screen,title,x,-780,260,140,color,action,38);
            var titleLabel=button.GetComponentInChildren<Text>();
            titleLabel.rectTransform.anchoredPosition=new Vector2(0,26);
            titleLabel.rectTransform.sizeDelta=new Vector2(224,54);
            var priceLabel=UI.Label(button.transform,price,0,-26,224,54,36,titleLabel.color);
            priceLabel.font=titleLabel.font;
            priceLabel.gameObject.AddComponent<CenteredGlyphs>();
            
        }
        float BoardY(float y)
        {
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=0; // Reference B uses a fixed-proportion stage inside the safe area.
            return puzzle!=null&&puzzle.layout.config.basket>0?y*.68f+31:y;
        }
        Vector2 Position(Sock s)
        {
            if(s.place==Place.Selected&&puzzle.layout.config.basket>0)return new Vector2(0,-255);
            if(s.place==Place.Basket)
            {int i=puzzle.basket.IndexOf(s.id);return screen.InverseTransformPoint(basketFrame.rectTransform.TransformPoint(UI.BasketSlot(basketFrame,i,puzzle.capacity)));}
            if(s.line)return new Vector2((s.slot-32.5f)*136,BoardY(570+s.depth*6));
            if(puzzle.layout.config.basket==0)
            {
                int columns=puzzle.layout.config.pairs<=8?4:6;
                float step=columns==4?225:139;
                int rows=Mathf.CeilToInt(puzzle.layout.config.pairs*2f/columns);
                float row=columns==4?(rows>3?210:295):(rows>4?180:210);
                // Preserve a compact, evenly spaced grid on tall screens.
                float center=15;
                float gridTop=center+(rows-1)*row*.5f;
                int cell=earlyGridSlots[s.slot];
                return new Vector2((cell%columns-(columns-1)*.5f)*step,gridTop-cell/columns*row+(BoardY(200)-200)*.35f);
            }
            float top=puzzle.layout.config.line>0?370:540;
            float spacing=puzzle.layout.config.line>0?125:150;
            return new Vector2((s.slot%6-2.5f)*136+(s.depth%2==0?-1:1)*s.depth*3,BoardY(top-s.slot/6*spacing+s.depth*6));
        }
        void DrawSocks(Dictionary<int,Vector2> old)
        {
            foreach(int gone in views.Keys.Where(id=>puzzle.Socks[id].place==Place.Gone).ToArray())
            {if(views[gone])Destroy(views[gone].gameObject);views.Remove(gone);visuals.Remove(gone);}
            foreach(var s in puzzle.Socks.Where(s=>s.place!=Place.Gone).OrderByDescending(s=>s.place==Place.Pile?s.depth:-1))
            {
                int id=s.id;bool exposed=puzzle.Exposed(s),available=puzzle.Available(s);
                Vector2 pos=Position(s);
                float scale=s.size==0?.78f:s.size==2?1.10f:.94f;
                if(s.place==Place.Pile&&puzzle.layout.config.layers==1&&puzzle.layout.config.pairs<=8)scale*=1.25f;
                if(puzzle.layout.config.pairs<=6&&puzzle.layout.config.basket==0)scale=1.75f;
                if(s.place==Place.Pile&&puzzle.layout.config.line>0&&!s.line)scale*=.77f;
                if(s.place==Place.Selected&&puzzle.layout.config.basket==0)scale*=1.08f;
                if(s.place==Place.Basket||s.place==Place.Selected&&puzzle.layout.config.basket>0)scale*=.8f;
                if(s.place==Place.Basket)scale*=Mathf.Min(1,5f/puzzle.capacity);
                if(s.line&&s.place==Place.Pile)scale*=.75f;
                if(!views.TryGetValue(id,out var r)||!r)
                {
                    r=UI.Rect(sockLayer,"Sock "+id,pos.x,pos.y,136,173);views[id]=r;
                    var visual=r.gameObject.AddComponent<SockVisual>();visuals[id]=visual;
                    var selection=UI.Rect(r,"Selection glow",0,0,205,205);visual.selection=selection.gameObject;
                    UI.Circle(selection,"Selected fill",0,0,205,new Color(1,.75f,.3f,.12f));
                    UI.Glyph(selection,"halo",0,0,205,UI.Gold);
                    visual.shadow=UI.Sprite(r,"Soft shadow",sockArt[s.design],5,-8,160*scale,173*scale,new Color(.25f,.12f,.42f,.12f));
                    visual.art=UI.Sprite(r,"Sock art",sockArt[s.design],0,0,160*scale,173*scale);
                    visual.hit=UI.Box(r,"Tap target",0,0,136,173,Color.clear,false);
                    var b=r.gameObject.AddComponent<Button>();b.targetGraphic=visual.art;b.transition=Selectable.Transition.None;b.onClick.AddListener(()=>Tap(id));
                    if(s.odd){UI.Box(r,"Hole",8,-24,31,40,UI.Hex("624A45"));UI.Label(r,"!",8,-24,40,48,34,UI.Cream);}
                    visual.knot=UI.Label(r,"~X~",0,0,140,70,37,UI.Cream).gameObject;
                    visual.gate=UI.Label(r,"",0,-45,150,50,34,UI.Ink);UI.Stroke(visual.gate,UI.Cream,2);
                    visual.hint=UI.Box(r,"Hint dot",0,95,23,23,UI.Gold).gameObject;
                    if(s.size!=1)UI.Label(r,s.size==0?"S":"L",-45,65,60,50,34,UI.Ink);
                }
                r.SetParent(s.place==Place.Basket?fxLayer:sockLayer,false);
                var v=visuals[id];v.art.color=available?Color.white:new Color(.67f,.64f,.61f,.72f);
                if(v.movement!=null){StopCoroutine(v.movement);v.movement=null;}
                v.art.rectTransform.sizeDelta=Vector2.one*(160*scale);
                v.shadow.rectTransform.sizeDelta=v.art.rectTransform.sizeDelta;
                v.selection.transform.localScale=Vector3.one*(160*scale/180f);
                v.hit.raycastTarget=exposed;
                var visible=SockArtBounds.Visible[s.design];
                v.hit.rectTransform.sizeDelta=new Vector2(Mathf.Max(100,160*scale*visible.width),Mathf.Max(110,160*scale*visible.height));
                v.hit.rectTransform.anchoredPosition=(visible.center-Vector2.one*.5f)*(160*scale);
                v.knot.SetActive(s.knot);v.gate.text=!available&&exposed?(s.washer?"WASH":s.wet?"WET":""):"";
                v.selection.SetActive(s.place==Place.Selected);
                r.anchoredPosition=pos;r.SetAsLastSibling();r.localRotation=Quaternion.Euler(0,0,s.place==Place.Pile?(puzzle.layout.config.level==1?0:(s.id%7-3)*4):0);
                v.hint.SetActive(id==hintId);
                if(old!=null&&old.TryGetValue(id,out var start)&&exposed&&!save.reduceMotion&&Vector2.Distance(start,pos)>2)
                    v.movement=StartCoroutine(Move(r,start,pos,.22f,false));
            }
        }
        void Tap(int id)
        {
            if(!playing||finishing||hostPaused||modal.childCount>0)return;
            lastTap=Time.unscaledTime;if(idleHand){Destroy(idleHand.gameObject);idleHand=null;}
            var s=puzzle.Socks[id];
            var old=views.ToDictionary(k=>k.Key,k=>k.Value.anchoredPosition);
            int previous=puzzle.selected,design=s.design,oldMatched=puzzle.matched;
            Vector2 from=old.ContainsKey(id)?old[id]:Position(s),other=previous>=0&&old.ContainsKey(previous)?old[previous]:Vector2.zero;
            var result=puzzle.Tap(id);hintId=-1;
            if(result==TapResult.Blocked)
            {Toast(!puzzle.Exposed(s)?"Pick a sock from the top.":s.washer?"Match 3 pairs to open the washer.":s.wet?"Match 2 pairs to dry the wet socks.":"Find the matching sock.");return;}
            if(result==TapResult.Lost){sound.Play("lose");Lose();Persist();return;}
            if(result==TapResult.Matched)
            {
                drawer.Add(design);if(!save.collection[design])newDesigns++;save.collection[design]=true;sound.Play("tap");
                #if UNITY_ANDROID || UNITY_IOS
                if(save.haptics)Handheld.Vibrate();
                #endif
                StartCoroutine(MatchEffect(design,from,other));
            }
            else if(result==TapResult.Binned){StartCoroutine(BinEffect(design,from));Toast("Odd sock sorted!");}
            else if(result==TapResult.Untied){sound.Play("tap");Spark(from,UI.Cream,7);Toast("Untied! Now find its match.");}
            else sound.Play(puzzle.basket.Count>0?"drop":"tap");
            DrawSocks(old);UpdateStatus();
            if(result==TapResult.Selected&&previous>=0&&previous!=id&&oldMatched==puzzle.matched&&puzzle.layout.config.basket==0)
            {statusLabel.text="Different pair. Try again!";statusLabel.color=UI.Pink;}
            if(oldMatched<3&&puzzle.matched>=3&&puzzle.layout.config.washer>0)Toast("Washer open! Fresh socks are ready.");
            if(oldMatched<2&&puzzle.matched>=2&&puzzle.layout.config.wet>0)Toast("All dry! Wet socks are ready.");
            Persist();if(puzzle.Won){finishing=true;StartCoroutine(CompleteAfterAnimation());}
        }
        void UpdateStatus()
        {
            if(!progressLabel)return;
            var c=puzzle.layout.config;progressLabel.text="Pairs  "+puzzle.matched+" / "+c.pairs;
            if(progressFill)progressFill.rectTransform.sizeDelta=new Vector2(920f*puzzle.matched/c.pairs,0);
            if(pickLabel)pickLabel.gameObject.SetActive(puzzle.selected<0);
            if(basketLabel)basketLabel.text="Waiting socks  "+puzzle.basket.Count+" / "+puzzle.capacity;
            bool full=puzzle.capacity>0&&puzzle.basket.Count>=puzzle.capacity-1;
            statusLabel.text=full?"Basket almost full!":puzzle.selected>=0?"Now find its partner":"Find a pair";
            statusLabel.color=UI.Ink;statusLabel.fontSize=statusLabel.text.Length>16?40:earlyTitleSize(statusLabel.rectTransform.rect.height);
        }

        IEnumerator MatchEffect(int design,Vector2 a,Vector2 b)
        {
            int epoch=animationEpoch,matchedIndex=puzzle.matched-1;
            var left=UI.Sprite(fxLayer,"Matching sock",sockArt[design],a.x,a.y,123,173).rectTransform;
            var right=UI.Sprite(fxLayer,"Matching sock",sockArt[design],b.x,b.y,123,173).rectTransform;
            Vector2 center=(a+b)*.5f;
            float duration=save.reduceMotion?.1f:.22f;
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {
                if(epoch!=animationEpoch||!left||!right)yield break;
                if(hostPaused){yield return null;t-=Time.unscaledDeltaTime;continue;}
                float u=Mathf.SmoothStep(0,1,t/duration);
                left.anchoredPosition=Vector2.Lerp(a,center+Vector2.left*26,u);right.anchoredPosition=Vector2.Lerp(b,center+Vector2.right*26,u);
                left.localRotation=Quaternion.Euler(0,0,-u*40);right.localRotation=Quaternion.Euler(0,0,u*40);
                left.localScale=right.localScale=Vector3.one*(1-u*.25f);yield return null;
            }
            if(!left||!right||epoch!=animationEpoch)yield break;
            Destroy(left.gameObject);Destroy(right.gameObject);
            sound.Play("match");Spark(center,UI.Gold,8);
            var ball=UI.Sprite(fxLayer,"Rolled ball",ballArt[design],center.x,center.y,105,105).rectTransform;
            Vector2 destination=puzzle.layout.config.basket==0
                ?(Vector2)fxLayer.InverseTransformPoint(basketFrame.rectTransform.TransformPoint(UI.CompletedPairSlot(basketFrame,Mathf.Min(4,matchedIndex),Mathf.Min(5,matchedIndex+1))))
                :(Vector2)fxLayer.InverseTransformPoint(progressLabel.rectTransform.position);
            if(puzzle.layout.config.basket==0)ball.sizeDelta=Vector2.one*(UI.BasketImageRect(basketFrame).width*.16f);
            yield return Move(ball,center,destination,save.reduceMotion?.15f:.42f,true);
            if(!ball||epoch!=animationEpoch)yield break;
            Spark(destination,UI.Gold,8);Destroy(ball.gameObject);
            sound.Play("drop");
                DrawDrawer();
        }
        IEnumerator DropIn()
        {
            foreach(var s in puzzle.Socks.Where(s=>s.place==Place.Pile&&puzzle.Exposed(s)))
                if(views.TryGetValue(s.id,out var r))
                {Vector2 destination=Position(s);visuals[s.id].movement=StartCoroutine(Move(r,destination+Vector2.down*32,destination,.6f,false));}
            yield return null;
        }
        IEnumerator BinEffect(int design,Vector2 from)
        {
            var ghost=UI.Sprite(fxLayer,"Odd sock to bin",sockArt[design],from.x,from.y,120,170).rectTransform;
            UI.Box(ghost,"Hole",8,-24,31,40,UI.Hex("624A45"));
            yield return Move(ghost,from,new Vector2(354,-270),save.reduceMotion?.1f:.3f,true);
            if(ghost)Destroy(ghost.gameObject);sound.Play("drop");
        }
        IEnumerator Move(RectTransform r,Vector2 from,Vector2 to,float duration,bool arc)
        {
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {
                if(!r)yield break;
                if(hostPaused){yield return null;t-=Time.unscaledDeltaTime;continue;}
                float u=t/duration,e=u*u*(3-2*u);
                r.anchoredPosition=Vector2.Lerp(from,to,e)+(arc?Vector2.up*Mathf.Sin(u*Mathf.PI)*100:Vector2.zero);
                if(arc)r.localRotation=Quaternion.Euler(0,0,u*300);yield return null;
            }
            if(r){r.anchoredPosition=to;r.localRotation=Quaternion.identity;}
        }
        IEnumerator Pulse(RectTransform r)
        {
            for(float t=0;t<3;t+=Time.unscaledDeltaTime){if(!r)yield break;r.localScale=Vector3.one*(1+Mathf.Sin(t*9)*.045f);yield return null;}
            if(r)r.localScale=Vector3.one;
        }
        void Spark(Vector2 pos,Color color,int count)
        {
            if(save.reduceMotion||!fxLayer)return;
            count=Mathf.Min(count,4);
            for(int i=0;i<count;i++)
            {
                var r=UI.Rect(fxLayer,"Match sparkle",pos.x,pos.y,22,22);
                UI.Glyph(r,"spark",0,0,22,color);float angle=i*Mathf.PI*2/count;StartCoroutine(Particle(r,pos,new Vector2(Mathf.Cos(angle)*90,Mathf.Sin(angle)*80),.45f));
            }
        }
        IEnumerator Particle(RectTransform r,Vector2 from,Vector2 delta,float duration)
        {
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {if(!r)yield break;float u=t/duration;r.anchoredPosition=from+delta*u+Vector2.down*50*u*u;r.localScale=Vector3.one*(1-u);r.localRotation=Quaternion.Euler(0,0,u*90);yield return null;}
            if(r)Destroy(r.gameObject);
        }
        IEnumerator CompleteAfterAnimation()
        {
            yield return new WaitForSecondsRealtime(.75f);
            while(hostPaused)yield return null;
            if(puzzle==null||!puzzle.Won)yield break;
            int stars=helpers==0?(elapsed<=puzzle.layout.config.seconds?3:2):1;
            int level=puzzle.layout.config.level;
            // Replays pay reduced coins; first completion pays the configured level reward.
            int reward=save.stars[level-1]==0?puzzle.layout.config.coins:Mathf.Max(5,puzzle.layout.config.coins/3);
            save.stars[level-1]=Mathf.Max(save.stars[level-1],stars);save.coins+=reward;
            if(!unlockAllLevelsForTesting||level<=save.unlocked)
                save.unlocked=Mathf.Max(save.unlocked,Mathf.Min(100,level+1));
            save.active=null;playing=false;Persist();
            sound.Play("win");
            yield return new WaitForEndOfFrame();
            UI.Clear(toastLayer);UI.Clear(modal);RewardBackdrop.Capture(modal);
            var dim=UI.Box(modal,"Dim",0,0,1080,1920,new Color(.03f,.02f,.01f,.55f),false);dim.raycastTarget=true;dim.gameObject.AddComponent<FullBleed>();
            ReferenceBArt.Rim(modal);
            var panel=ReferenceReward.Build(modal,reward,save.collection.Count(x=>x),stars,level==100,()=>{if(level==100)Home();else StartLevel(level+1);});
            if(!save.reduceMotion)StartCoroutine(PanelPop(panel));
            StartCoroutine(Confetti());
        }
        void Lose()
        {
            var panel=Modal("Basket full!",1240,UI.Pink);
            UI.Basket(panel,0,160,710,250);
            UI.Label(panel,"Add +1 space to\nmake a match!",0,-10,780,180,58,UI.Ink);
            UI.Button(panel,save.spots>0?"+1 Space   Use":"+1 Space   100",0,-235,740,145,UI.Pink,()=>BuyHelper("spot"),53);
            UI.Button(panel,"Retry",0,-410,700,140,UI.Purple,()=>StartLevel(puzzle.layout.config.level,false),62);
        }
        void Pause()
        {
            var panel=Modal("Pause",1320);
            UI.Button(panel,"Settings",-300,345,100,100,UI.Pink,Settings,36);
            UI.Box(panel,"Sound row",0,220,730,140,UI.Cream);
            UI.Glyph(panel,"sound",-290,220,82,UI.Ink);UI.Label(panel,"Sound",-45,220,330,100,62,UI.Ink,TextAnchor.MiddleLeft);
            UI.Switch(panel,270,220,save.sound,()=>{save.sound=!save.sound;ApplySound();Persist();});
            UI.Box(panel,"Music row",0,60,730,140,UI.Cream);
            UI.Glyph(panel,"music",-290,60,82,UI.Ink);UI.Label(panel,"Music",-45,60,330,100,62,UI.Ink,TextAnchor.MiddleLeft);
            UI.Switch(panel,270,60,save.music,()=>{save.music=!save.music;ApplySound();Persist();});
            UI.Button(panel,"Resume",0,-115,720,140,UI.Pink,CloseModal,72);
            UI.Button(panel,"Restart",0,-280,720,140,UI.Purple,()=>StartLevel(puzzle.layout.config.level,false),72);
            UI.Button(panel,"Home",0,-445,720,140,UI.Purple,Home,72);
            Persist();
        }
        void SmallToggle(Transform parent,string title,float x,float y,bool on,Action toggle)
        {
            var bg=UI.Box(parent,title,x,y,135,70,on?UI.Green:UI.Hex("B6B2CC"));bg.raycastTarget=true;
            UI.Circle(parent,"Toggle knob",x+(on?30:-30),y,53,Color.white);
            UI.Label(parent,title,x,y-75,180,55,31,UI.Hex("591995"));
            var button=bg.gameObject.AddComponent<Button>();button.targetGraphic=bg;
            button.onClick.AddListener(()=>{toggle();ApplySound();Persist();Pause();});
        }
        void Offer(string type)
        {
            var panel=Modal(type=="spot"?"Extra Spot":type=="hint"?"Find Match":"Shuffle",1200,UI.Blue);

            UI.Circle(panel,"Helper background",0,120,230,UI.Mint);
            UI.Label(panel,type=="spot"?"+1":type=="hint"?"?":"~",0,120,225,200,105,UI.Ink);
            UI.Label(panel,type=="spot"?"Adds one holding spot\nfor this level":type=="hint"?"Finds your next matching sock":"Makes a fresh, solvable pile",0,-70,780,150,44,UI.Ink);
            int cost=type=="spot"?100:type=="hint"?50:100;
            int owned=type=="spot"?save.spots:type=="hint"?save.hints:save.shuffles;
            UI.Button(panel,owned>0?"Use helper ("+owned+")":"Use "+cost+" coins",0,-205,700,135,UI.Pink,()=>BuyHelper(type),47);
            if(CanRewardHelper(type))
                UI.Button(panel,"Watch ad for helper",0,-380,700,135,UI.Purple,()=>RewardHelper(type),46);
        }
        bool CanRewardHelper(string type)
        {
            return platform.youtube&&platform.rewardedAdsAvailable&&!adUsed&&type!="shuffle"
                &&puzzle!=null&&(type!="spot"||puzzle.capacity<7);
        }
        void RewardHelper(string type)
        {
            if(!CanRewardHelper(type)){Toast("This rewarded helper is not available. Coins and free retries work.");return;}
            if(adUsed){Toast("The rewarded helper was already used for this level.");return;}
            if(type=="shuffle"){Toast("Shuffle is available with coins or a helper pack.");return;}
            var target=puzzle;
            var panel=Modal("One moment...",820,UI.Purple);
            UI.Label(panel,"Checking for an available ad",0,-60,600,130,36);
            platform.Reward(type,earned=>
            {
                if(puzzle!=target)return;
                if(!earned){Offer(type);Toast("No reward earned. You can keep playing.");return;}
                adUsed=true;
                if(type=="spot")save.spots++;else save.hints++;
                BuyHelper(type);
            });
        }
        void BuyHelper(string type)
        {
            int cost=type=="spot"?100:type=="hint"?50:100;
            int owned=type=="spot"?save.spots:type=="hint"?save.hints:save.shuffles;
            if(owned==0&&save.coins<cost){Toast("Not enough coins. You can always retry for free.");return;}
            if(type=="spot"&&puzzle.capacity>=7){Toast("Seven spaces is the basket maximum.");return;}
            if(type=="hint")
            {
                hintId=puzzle.Hint();if(hintId<0){Toast("Try selecting a sock from your basket.");return;}
            }
            if(owned>0){if(type=="spot")save.spots--;else if(type=="hint")save.hints--;else save.shuffles--;}
            else save.coins-=cost;
            helpers++;
            if(type=="spot")puzzle.AddSpot();
            if(type=="shuffle")Reshuffle();
            CloseModal();BuildPlay();Persist();sound.Play("tap");
        }
        void Reshuffle()
        {
            puzzle.Reshuffle(puzzle.layout.config.seed+helpers*701);
        }
        void ShowTutorial(int level)
        {
            if(level==1){StartCoroutine(FirstPairTutorial());return;}
            string[] titles={"A laundry match","Meet your basket","Look a little closer","The odd one out","Wash day","Hanging out","All tangled up","A little damp","Big and little","The grand tidy"};
            string[] tips={"Tap a sock, then tap another with the same color and pattern.\nWatch them roll into a tidy ball!",
                "Only the top sock in each pile can be picked.\nChoose a different sock and your first pick waits in the basket.\nTap waiting socks to match them. Keep a space free!",
                "Some socks share a color.\nCheck their stripes, dots, stars and zigzags too.",
                "A sock with a hole has no partner.\nTap it once to send it straight to the bin.",
                "Some socks are still in the wash.\nMatch three pairs to open the washer.",
                "Some socks sway along the clothesline.\nPick the front socks just like the pile below.",
                "See a knot? Tap once to untie the pair.\nThen match those socks as usual.",
                "Wet socks need a moment to dry.\nMatch two other pairs and they will be ready.",
                "The same design can come in different sizes.\nMatch S with S, L with L, and regular with regular.",
                "All your laundry skills come together.\nPlan your picks, watch the basket, and enjoy the tidy."};
            int ch=(level-1)/10;var config=puzzle.layout.config;
            var panel=Modal(string.IsNullOrEmpty(config.tutorialTitle)?titles[ch]:config.tutorialTitle,1050);
            UI.Sprite(panel,"Example",sockArt[ch%24],-100,110,130,130);UI.Sprite(panel,"Example match",sockArt[ch%24],100,110,130,130);
            UI.Label(panel,string.IsNullOrEmpty(config.tutorialText)?tips[ch]:config.tutorialText,0,-110,710,310,40);
            UI.Button(panel,"Let's tidy!",0,-365,620,130,UI.Green,CloseModal,52);
        }
        RectTransform Modal(string title,float height,Color? ribbon=null)
        {
            UI.Clear(toastLayer);UI.Clear(modal);RewardBackdrop.Capture(modal);
            bool win=title=="Level Complete!";
            // Preserve the live gameplay under all modal overlays.
            var dim=UI.Box(modal,"Dim",0,0,1080,1920,new Color(.06f,.15f,.13f,win?.50f:.65f),false);dim.raycastTarget=true;dim.gameObject.AddComponent<FullBleed>();
            var panel=UI.Panel(modal,win?"Win":"Panel",0,0,900,height,UI.Cream).rectTransform;
            if(win)
            {
                UI.Label(panel,"Lovely\nmatching!",0,405,740,200,80,UI.Ink);
            }
            else
            {
                UI.Glyph(panel,"socks",0,height*.5f-125,110,UI.Ink);
                var heading=UI.Label(panel,title,-25,height*.5f-315,500,95,60,UI.Ink);
                while(heading.preferredWidth>500&&heading.fontSize>36)heading.fontSize--;
                UI.Button(panel,"x",300,height*.5f-315,100,100,UI.Pink,()=>{if(playing&&puzzle!=null&&puzzle.lost)Home();else CloseModal();},46);
            }
            if(!save.reduceMotion)StartCoroutine(PanelPop(panel));
            return panel;
        }
        RectTransform Hand(Vector2 position)
        {
            var r=UI.Rect(fxLayer,"Tutorial glove",position.x+65,position.y-80,310,260);
            UI.Sprite(r,"White glove",Resources.Load<Sprite>("Art/CandyHand"),0,0,310,260);
            return r;
        }
        IEnumerator FirstPairTutorial()
        {
            showingTutorial=true;int epoch=animationEpoch;
            yield return new WaitForSecondsRealtime(.75f);
            if(epoch!=animationEpoch||puzzle.moves!=0){showingTutorial=false;yield break;}
            var bubble=UI.Rect(fxLayer,"Tutorial instruction",0,-550,900,145);SafeEdgeOffset.Pin(bubble,-1);
            UI.Panel(bubble,"Bubble",0,0,900,145,UI.Cream);
            UI.Label(bubble,"Tap two identical socks\nto make a pair!",0,0,860,130,40);
            int[] pair={puzzle.layout.solution[0],puzzle.layout.solution[1]};
            for(int i=0;i<2;i++)
            {
                if(epoch!=animationEpoch||puzzle.moves>i)break;
                int target=i==0?pair[0]:puzzle.Hint();if(target<0)break;
                var targetPosition=Position(puzzle.Socks[target]);
                var glow=UI.Rect(fxLayer,"Tutorial tap glow",targetPosition.x,targetPosition.y,225,225);
                UI.Circle(glow,"Soft gold light",0,0,225,new Color(1,.8f,.2f,.13f));UI.Glyph(glow,"halo",0,0,225,UI.Gold);
                var hand=Hand(targetPosition);idleHand=hand;
                Vector2 start=hand.anchoredPosition;
                while(epoch==animationEpoch&&puzzle.moves==i&&hand)
                {
                    if(!save.reduceMotion)hand.anchoredPosition=start+Vector2.up*Mathf.Sin(Time.unscaledTime*5)*9;
                    yield return null;
                }
                if(hand)Destroy(hand.gameObject);if(glow)Destroy(glow.gameObject);idleHand=null;
            }
            if(bubble)Destroy(bubble.gameObject);showingTutorial=false;lastTap=Time.unscaledTime;
        }
        IEnumerator IdleHint()
        {
            int id=puzzle.Hint();if(id<0||!views.ContainsKey(id))yield break;
            idleHand=Hand(Position(puzzle.Socks[id]));var hand=idleHand;Vector2 start=hand.anchoredPosition;
            for(float t=0;t<2.2f;t+=Time.unscaledDeltaTime)
            {if(!hand)yield break;hand.anchoredPosition=start+Vector2.up*Mathf.Sin(t*5)*14;yield return null;}
            if(hand)Destroy(hand.gameObject);if(idleHand==hand)idleHand=null;
        }
        IEnumerator RevealStars(StarGraphic[] stars)
        {
            if(save.reduceMotion)yield break;
            foreach(var star in stars)star.transform.localScale=Vector3.zero;
            for(float t=0;t<.65f;t+=Time.unscaledDeltaTime)
            {
                for(int i=0;i<stars.Length;i++)
                {if(!stars[i])yield break;float u=Mathf.Clamp01((t-i*.15f)/.3f);stars[i].transform.localScale=Vector3.one*(u+Mathf.Sin(u*Mathf.PI)*.2f);}
                yield return null;
            }
            foreach(var star in stars)if(star)star.transform.localScale=Vector3.one;
        }
        IEnumerator CountReward(Text label,int amount)
        {
            for(float t=0;t<.5f;t+=Time.unscaledDeltaTime)
            {if(!label)yield break;label.text="+"+Mathf.RoundToInt(Mathf.SmoothStep(0,amount,t/.5f))+" Coins";yield return null;}
            if(label)label.text="+"+amount+" Coins";
        }
        IEnumerator PanelPop(RectTransform panel)
        {
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime)
            {if(!panel)yield break;float u=t/.2f;panel.localScale=Vector3.one*(.9f+.1f*u+Mathf.Sin(u*Mathf.PI)*.045f);yield return null;}
            if(panel)panel.localScale=Vector3.one;
        }
        bool doubling;
        void DoubleCoins(int reward)
        {
            if(doubling)return;
            if(!platform.youtube){Toast("Rewarded ads are not configured in this preview.");return;}
            doubling=true;platform.Reward("double-coins",earned=>{if(earned){save.coins+=reward;Persist();Toast("+"+reward+" bonus coins!");}else{doubling=false;Toast("No reward earned. You can keep playing.");}});
        }
        IEnumerator Confetti()
        {
            if(save.reduceMotion)yield break;
            var layer=UI.Rect(modal,"Confetti",0,0,1080,1920);
            var pieces=new List<RectTransform>();var starts=new List<Vector2>();var speeds=new List<Vector2>();
            Color[] colors={UI.Pink,UI.Blue,UI.Gold,UI.Green,UI.Purple,Color.white};
            for(int i=0;i<20;i++)
            {
                Vector2 p=new Vector2(UnityEngine.Random.Range(-520,520),UnityEngine.Random.Range(150,1000));starts.Add(p);
                speeds.Add(new Vector2(UnityEngine.Random.Range(-100,100),UnityEngine.Random.Range(-550,-250)));
                pieces.Add(UI.Box(layer,"Confetti",p.x,p.y,15,23,colors[i%6],false).rectTransform);
            }
            for(float t=0;t<1.5f;t+=Time.unscaledDeltaTime)
            {
                if(!layer)yield break;if(hostPaused){yield return null;t-=Time.unscaledDeltaTime;continue;}
                for(int i=0;i<pieces.Count;i++){pieces[i].anchoredPosition=starts[i]+speeds[i]*t+Vector2.down*200*t*t;pieces[i].localRotation=Quaternion.Euler(0,0,t*150+i*37);}
                yield return null;
            }
            if(layer)Destroy(layer.gameObject);
        }
        void CloseModal(){UI.Clear(modal);}
        void Toast(string message)
        {
            UI.Clear(toastLayer);var r=UI.Box(toastLayer,"Message",0,700,960,110,UI.Ink).rectTransform;
            UI.Label(r,message,0,0,910,95,40,UI.Cream);StartCoroutine(RemoveToast(r));
        }
        IEnumerator RemoveToast(RectTransform r){yield return new WaitForSecondsRealtime(3);if(r)Destroy(r.gameObject);}
        static string TimeText(float time){int s=(int)time;return(s/60).ToString("00")+":"+(s%60).ToString("00");}
        static string Stars(int count){return new string('*',count)+(count>0?" ":"")+new string('-',3-count);}
        static string DesignName(int i)
        {string[] colors={"Rose","Sky","Sunshine","Mint","Lilac","Tangerine"};string[] patterns={"stripes","dots","stars","zigzags"};return colors[i%6]+" "+patterns[i/6];}
    }
}


