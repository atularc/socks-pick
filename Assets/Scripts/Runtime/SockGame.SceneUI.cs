using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SockPairs.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    public partial class SockGame
    {
        [Header("Authored Canvas screens — edit these objects in the scene")]
        [SerializeField] List<SceneView> sceneViews=new List<SceneView>();
        [SerializeField] SockVisual sockTemplate;
        [SerializeField] RectTransform handTemplate,glowTemplate;
        SceneView activePopup;
        bool authoring;
        Coroutine toastRoutine,popupAnimation;
        RectTransform animatedPanel;Vector3 panelScale;
        RectTransform liveConfetti;
        int normalStatusSize;Color normalStatusColor;
        bool HasOpenPopup=>authoring?modal.childCount>0:activePopup;
        SceneView View(string id)
        {
            var result=sceneViews.Find(v=>v&&v.id==id);
            if(!result)throw new InvalidOperationException("Missing authored Canvas screen: "+id+". Open the saved Laundry scene.");
            return result;
        }
        void InitializeSceneViews()
        {
            if(sceneViews.Count==0)throw new InvalidOperationException("Laundry scene needs authored Canvas screens.");
            foreach(var view in sceneViews)view.gameObject.SetActive(false);
            screen=(RectTransform)View("Loading").transform;screen.gameObject.SetActive(true);
        }
        void ClearLiveContent()
        {
            CloseModal();StopAllCoroutines();toastRoutine=null;showingTutorial=false;idleHand=null;
            if(sockLayer)UI.Clear(sockLayer);if(fxLayer)UI.Clear(fxLayer);if(drawerLayer)UI.Clear(drawerLayer);
            sockLayer=fxLayer=drawerLayer=null;views.Clear();visuals.Clear();
            foreach(var view in sceneViews)view.gameObject.SetActive(false);
            activePopup=null;animationEpoch++;playing=false;finishing=false;
        }
        SceneView ShowScreen(string id,string state)
        {
            ClearLiveContent();currentScreen=state;var view=View(id);screen=(RectTransform)view.transform;
            view.gameObject.SetActive(true);Fit();return view;
        }
        SceneView ShowPopup(string id,bool keepCurrent=false)
        {
            if(!keepCurrent)CloseModal();View("Toast").gameObject.SetActive(false);
            var view=View(id);var backdrop=view.Get<RewardBackdrop>("backdrop");backdrop.RefreshCapture();
            activePopup=view;view.gameObject.SetActive(true);
            if(view.bindings.Any(b=>b.key=="close"))view.Button("close",()=>{if(playing&&puzzle!=null&&puzzle.lost)Home();else CloseModal();});
            animatedPanel=view.Get<RectTransform>("panel");panelScale=animatedPanel.localScale;
            if(!save.reduceMotion)popupAnimation=StartCoroutine(PanelPop(animatedPanel));
            return view;
        }
        void BindMap()
        {
            var view=ShowScreen("Level map","map");
            int next=save.active!=null?save.active.level:save.unlocked;
            view.Get<LevelMapView>("map").Bind(save,next,StartLevelFromMap,unlockAllLevelsForTesting);
            view.Text("playLabel","Play level "+next);
            view.Button("play",()=>{if(save.active!=null)Resume();else StartLevel(save.unlocked);});
            view.Button("album",Album);
        }
        void StartLevelFromMap(int level){StartLevel(level);}
        void BindPlay()
        {
            var c=puzzle.layout.config;bool early=c.basket==0;
            var view=ShowScreen(early?"Gameplay simple":"Gameplay basket","play");playing=true;
            progressLabel=view.Get<Text>("progress");statusLabel=view.Get<Text>("status");
            var style=view.TextStyle("status");normalStatusSize=style.size;normalStatusColor=style.color;
            progressFill=view.Get<Image>("progressFill");basketFrame=view.Get<Image>("basket");
            drawerLayer=view.Get<RectTransform>("drawer");sockLayer=view.Get<RectTransform>("socks");fxLayer=view.Get<RectTransform>("effects");
            view.Text("day","Day "+c.level);view.Button("pause",Pause);timeLabel=null;
            if(early)
            {
                basketLabel=null;pickLabel=null;
                for(int i=0;i<earlyGridSlots.Length;i++)earlyGridSlots[i]=-1;
                foreach(var sock in puzzle.Socks)earlyGridSlots[sock.slot]=0;
                int cell=0;for(int i=0;i<earlyGridSlots.Length;i++)if(earlyGridSlots[i]==0)earlyGridSlots[i]=cell++;
            }
            else
            {
                basketLabel=view.Get<Text>("basketLabel");pickLabel=view.Get<Text>("pickLabel");
                foreach(var binding in view.bindings.Where(b=>b.key.StartsWith("washer:",StringComparison.Ordinal)))binding.target.gameObject.SetActive(c.washer>0);
                foreach(var binding in view.bindings.Where(b=>b.key.StartsWith("bin:",StringComparison.Ordinal)))binding.target.gameObject.SetActive(c.odd>0);
                view.Show("clothesline",c.line>0);
                view.Text("washerText",puzzle.matched>=3?"Ready":Mathf.Max(0,3-puzzle.matched)+" pairs");
                view.Get<Image>("washerWindow").color=puzzle.matched>=3?UI.Green:UI.Blue;
                for(int i=0;i<7;i++)
                {
                    var slot=view.Get<RectTransform>("slot"+i);slot.gameObject.SetActive(i<puzzle.capacity);
                    if(i<puzzle.capacity){slot.anchoredPosition=UI.BasketSlot(basketFrame,i,puzzle.capacity);var size=slot.sizeDelta;size.x=UI.BasketImageRect(basketFrame).width*Mathf.Min(.09f,.38f/Mathf.Max(1,puzzle.capacity-1));slot.sizeDelta=size;}
                }
                view.Button("spot",()=>Offer("spot"));view.Button("hint",()=>Offer("hint"));view.Button("shuffle",()=>Offer("shuffle"));
            }
            DrawDrawer();DrawSocks(null);UpdateStatus();
        }
        void BindSettings()
        {
            var view=ShowPopup("Settings");
            view.Get<SceneToggle>("sound").Bind(save.sound,()=>{save.sound=!save.sound;ApplySound();Persist();});
            view.Get<SceneToggle>("music").Bind(save.music,()=>{save.music=!save.music;ApplySound();Persist();});
            view.Get<SceneToggle>("haptics").Bind(save.haptics,()=>{save.haptics=!save.haptics;Persist();});
            view.Get<SceneToggle>("motion").Bind(save.reduceMotion,()=>{save.reduceMotion=!save.reduceMotion;Persist();});
            view.Button("done",()=>{CloseModal();Persist();});
        }
        void BindPause()
        {
            var view=ShowPopup("Pause");view.Button("settings",Settings);
            view.Get<SceneToggle>("sound").Bind(save.sound,()=>{save.sound=!save.sound;ApplySound();Persist();});
            view.Get<SceneToggle>("music").Bind(save.music,()=>{save.music=!save.music;ApplySound();Persist();});
            view.Button("resume",CloseModal);view.Button("restart",()=>StartLevel(puzzle.layout.config.level,false));view.Button("home",Home);Persist();
        }
        void BindLose()
        {
            var view=ShowPopup("Basket full");view.Text("buyLabel",save.spots>0?"+1 Space   Use":"+1 Space   100");
            view.Button("buy",()=>BuyHelper("spot"));view.Button("retry",()=>StartLevel(puzzle.layout.config.level,false));
        }
        void BindOffer(string type)
        {
            var view=ShowPopup("Helper "+type);int cost=type=="hint"?50:100,owned=type=="spot"?save.spots:type=="hint"?save.hints:save.shuffles;
            view.Text("buyLabel",owned>0?"Use helper ("+owned+")":"Use "+cost+" coins");view.Button("buy",()=>BuyHelper(type));
            view.Show("ad",CanRewardHelper(type));view.Button("ad",()=>RewardHelper(type));
        }
        void BindTutorial(int level)
        {
            if(level==1){StartCoroutine(FirstPairTutorial());return;}
            var view=ShowPopup("Tutorial "+level);view.Button("done",CloseModal);
        }
        void BindReward(int reward,int stars,int level)
        {
            bool last=level==100;var view=ShowPopup(last?"Campaign complete":"Level complete");
            view.Text("coins","+"+reward);view.Text("collection",save.collection.Count(v=>v)+" / 24");
            var fill=view.Get<RectTransform>("fill");float progress=save.collection.Count(v=>v)/24f;
            fill.anchorMax=new Vector2(progress,1);fill.gameObject.SetActive(progress>0);
            for(int i=0;i<3;i++)view.Show("unearned"+i,i>=stars);
            view.Button("next",()=>{if(last)Home();else StartLevel(level+1);});view.Button("map",Home);
            StartCoroutine(Confetti());
        }
        void BindAlbum()
        {
            Map(-1);currentScreen="album";var view=ShowPopup("Sock album");view.Button("close",Home);
            view.Text("count",save.collection.Count(v=>v)+" / 24 collected");
            for(int i=0;i<24;i++)
            {
                int design=i;bool found=save.collection[i];view.Get<Image>("sock"+i).color=found?Color.white:new Color(0,0,0,.18f);
                view.Show("lock"+i,!found);view.Show("lockGlyph"+i,!found);view.Text("state"+i,found?"Collected":"Locked");
                view.Get<Text>("state"+i).color=found?UI.Ink:UI.Muted;view.Button("item"+i,()=>OpenAlbumItem(design));
            }
            var pager=view.Get<AlbumPager>("pager");pager.instant=save.reduceMotion;
            var prev=view.Get<Button>("previous");var next=view.Get<Button>("next");
            view.Button("previous",()=>pager.GoTo(pager.Page-1));view.Button("next",()=>pager.GoTo(pager.Page+1));
            pager.PageChanged=page=>{albumPage=page;prev.interactable=page>0;next.interactable=page<3;for(int i=0;i<4;i++)view.Get<Image>("dot"+i).color=i==page?UI.Pink:UI.Purple;};pager.GoTo(albumPage,false);
        }
        void BindAlbumItem(int design)
        {
            // Keep the existing album visible under the detail popup.
            var album=View("Sock album");var view=ShowPopup("Sock detail",true);view.transform.SetAsLastSibling();
            var close=new Action(()=>{view.gameObject.SetActive(false);activePopup=album;});view.Button("close",close);view.Button("done",close);
            string[] colors={"Pink","Blue","Yellow","Green","Purple","Coral"},patterns={"Stripes","Dots","Stars","Zigzags"};
            bool found=save.collection[design];view.Text("title",colors[design%6]+" "+patterns[design/6]);
            var art=view.Get<Image>("sock");art.sprite=sockArt[design];art.color=found?Color.white:new Color(0,0,0,.18f);
            view.Show("lock",!found);view.Show("lockGlyph",!found);view.Text("state",found?"Collected":"Not collected yet");
            view.Text("description",found?"You matched this design!":"Match this design in a level\nto add it to your album.");
        }
        void BindShop()
        {
            if(!CashShopAvailable)return;var view=ShowScreen("Shop","shop");coinLabel=view.Get<Text>("coins");coinLabel.text=save.coins.ToString();
            view.Button("settings",Settings);view.Button("home",Home);for(int i=0;i<3;i++)view.Button("buy"+i,PurchaseUnavailable);
        }
        void BindRoom()
        {
            var view=ShowScreen("Room themes","room");coinLabel=view.Get<Text>("coins");coinLabel.text=save.coins.ToString();view.Button("home",Home);
            for(int i=0;i<4;i++)
            {
                int choice=i;bool owned=(save.decor&(1<<i))!=0||i==0;
                view.Text("label"+i,owned?(save.theme==i?"SELECTED":"Use this room"):"150 coins");
                view.Button("choice"+i,()=>{if(!owned&&save.coins<150){Toast("Earn coins by completing levels.");return;}if(!owned){save.coins-=150;save.decor|=1<<choice;}save.theme=choice;Persist();Room();});
            }
        }
        void ShowSceneToast(string message)
        {
            var view=View("Toast");view.Text("message",message);view.gameObject.SetActive(true);
            if(toastRoutine!=null)StopCoroutine(toastRoutine);toastRoutine=StartCoroutine(HideSceneToast(view));
        }
        IEnumerator HideSceneToast(SceneView view){yield return new WaitForSecondsRealtime(3);view.gameObject.SetActive(false);toastRoutine=null;}
    }
}
