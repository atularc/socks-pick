using System;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    // Twelve recycled rows keep object cost constant across the hundred-day route.
    public class LevelMapView:MonoBehaviour
    {
        const float Step=165,Height=16700;
        [Serializable] class Row {public RectTransform root;public Image face,halo;public Text number;public GameObject locked,bubble;public Button button;public StarGraphic[] stars;public CandyPathGraphic path;public int level;}
        [SerializeField] Row[] rows;[SerializeField] RectTransform content,pathLayer,viewport;SaveData save;int current,lastFirst=-99;bool unlockAll;Action<int> play;
        [Header("Route layout within each seven-level section")]
        [SerializeField] float[] xs={-164,166,-18,247,-178,150,-44};
        [SerializeField] float[] ys={0,34,309,464,634,738,893};
        [Header("Optional node artwork overrides")]
        public Sprite currentArtwork,completedArtwork,lockedArtwork;
        public Color currentText=UI.Cream,otherText=UI.Ink;
        float Y(int index)=>100+(index/7)*1155+ys[index%7];
        public void Build(Transform parent,SaveData data,int next,Action<int> launch,bool unlockAllLevelsForTesting=false)
        {
            save=data;current=next;play=launch;unlockAll=unlockAllLevelsForTesting;lastFirst=-99;
            viewport=UI.Box(parent,"Scrollable level path",0,120,960,1100,Color.clear,false).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.09f;scroll.scrollSensitivity=55;
            content=UI.Rect(viewport,"100 laundry days",0,0,960,Height);content.anchorMin=content.anchorMax=content.pivot=new Vector2(.5f,1);
            pathLayer=UI.Rect(content,"Wooden trail",0,0,960,Height);pathLayer.anchorMin=pathLayer.anchorMax=pathLayer.pivot=new Vector2(.5f,1);
            scroll.content=content;scroll.viewport=viewport;rows=new Row[12];for(int i=0;i<12;i++)rows[i]=CreateRow();
            Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1-Mathf.Clamp01(Mathf.Max(0,next-3)*Step/(Height-1100));
            scroll.onValueChanged.AddListener(_=>Refresh());Refresh();
        }
        public void Bind(SaveData data,int next,Action<int> launch,bool unlockAllLevelsForTesting=false)
        {
            save=data;current=next;play=launch;unlockAll=unlockAllLevelsForTesting;lastFirst=-99;
            var scroll=viewport.GetComponent<ScrollRect>();scroll.StopMovement();
            foreach(var row in rows){var entry=row;entry.button.onClick.RemoveAllListeners();entry.button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();play(entry.level);});}
            scroll.onValueChanged.RemoveAllListeners();scroll.onValueChanged.AddListener(_=>Refresh());
            Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1-Mathf.Clamp01(Mathf.Max(0,next-3)*Step/(Height-viewport.rect.height));Refresh();
        }
        Row CreateRow()
        {
            var row=new Row();row.root=UI.Rect(content,"Recycled level",0,0,960,Step);row.root.anchorMin=row.root.anchorMax=new Vector2(.5f,1);
            var path=UI.Rect(pathLayer,"Wooden path",0,0,960,Step);path.anchorMin=path.anchorMax=new Vector2(.5f,1);
            row.path=path.gameObject.AddComponent<CandyPathGraphic>();row.path.raycastTarget=false;
            row.halo=UI.Circle(row.root,"Current halo",0,0,210,UI.Gold);
            row.face=CosyArt.Image(row.root,"Level","CosyNode",0,0,190,190,false);row.face.raycastTarget=true;
            row.button=row.face.gameObject.AddComponent<Button>();row.button.targetGraphic=row.face;row.face.gameObject.AddComponent<ButtonMotion>();
            var colors=row.button.colors;colors.disabledColor=colors.selectedColor=colors.highlightedColor=Color.white;row.button.colors=colors;
            row.button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();play(row.level);});
            row.number=UI.Label(row.root,"1",0,20,140,90,73,UI.Ink);
            var locked=UI.Rect(row.root,"Lock",0,-43,46,50);row.locked=locked.gameObject;UI.Glyph(locked,"lock",0,0,42,UI.Ink);
            var bubble=UI.Rect(row.root,"Current marker",-245,20,260,140);row.bubble=bubble.gameObject;
            CosyArt.Image(bubble,"Marker","CosySecondary",0,0,260,140);UI.Label(bubble,"YOU ARE\nHERE",0,0,230,115,36,UI.Ink);
            row.stars=new StarGraphic[3];for(int i=0;i<3;i++){var r=UI.Rect(row.root,"Star",(i-1)*40,-43,43,43);row.stars[i]=r.gameObject.AddComponent<StarGraphic>();row.stars[i].raycastTarget=false;}
            return row;
        }
        void Refresh()
        {
            if(!content||rows==null)return;
            int first=Mathf.Clamp(Mathf.FloorToInt(content.anchoredPosition.y/Step)-1,0,88);if(first==lastFirst){Cull();return;}lastFirst=first;
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];int index=first+i;row.level=index+1;float x=xs[index%xs.Length],previousX=xs[(index+xs.Length-1)%xs.Length];
                row.root.anchoredPosition=new Vector2(x,-Y(index));row.path.rectTransform.anchoredPosition=row.root.anchoredPosition;
                float dx=previousX-x;float a=dx*.66f,b=dx*.33f;
                if(index%7==2){a=dx+240;b=340;}
                if(index%7==4){a=200;b=-250;}
                if(index%7==6){a=-60;b=-100;}
                row.path.gameObject.SetActive(index>0);row.path.Set(dx,index>0?-(Y(index)-Y(index-1)):-Step,a,b);
                bool active=row.level==current,locked=row.level>save.unlocked,complete=save.stars[index]>0;
                var artwork=active?currentArtwork:complete?completedArtwork:lockedArtwork;
                row.face.sprite=artwork?artwork:CosyArt.Get(active?"CosyCurrent":complete?"CosyGold":"CosyNode");row.face.color=Color.white;
                row.halo.gameObject.SetActive(active);row.bubble.SetActive(active);row.locked.SetActive(locked);
                row.number.text=row.level.ToString();row.number.color=active?currentText:otherText;row.number.rectTransform.anchoredPosition=new Vector2(0,locked||complete?25:0);
                row.button.interactable=!locked||unlockAll;
                for(int star=0;star<3;star++){row.stars[star].gameObject.SetActive(complete);row.stars[star].color=star<save.stars[index]?UI.Gold:UI.Line;}
            }
            Cull();
        }
        bool TryMarkerPosition(int index,out Vector2 position)
        {
            position=Vector2.zero;
            var origin=new Vector2(xs[index%7],-Y(index));
            float side=origin.x<0?-270:270;
            var candidates=new[]{new Vector2(origin.x< -100?245:-245,25),new Vector2(side-origin.x,-195),new Vector2(-side-origin.x,-195),new Vector2(side-origin.x,195),new Vector2(-side-origin.x,195)};
            foreach(var offset in candidates)
            {
                var p=origin+offset;
                if(p.x-130< -416||p.x+130>416||p.y+70>0)continue;
                float visibleY=content.anchoredPosition.y+p.y;
                if(visibleY+70>0||visibleY-70< -viewport.rect.height)continue;
                bool clear=true;
                for(int other=Mathf.Max(0,index-3);other<=Mathf.Min(99,index+3);other++)
                {
                    if(Mathf.Abs(p.x-xs[other%7])<241&&Mathf.Abs(p.y+Y(other))<181){clear=false;break;}
                }
                if(clear){position=offset;return true;}
            }
            return false;
        }
        void Cull(){foreach(var row in rows){float y=content.anchoredPosition.y+row.root.anchoredPosition.y;row.root.gameObject.SetActive(y<=0&&y>=-viewport.rect.height);if(row.level==current){bool fits=TryMarkerPosition(row.level-1,out var position);row.bubble.SetActive(fits);if(fits)row.bubble.GetComponent<RectTransform>().anchoredPosition=position;}}}
    }
}
