using System;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    // Twelve recycled rows keep object cost constant across the hundred-day route.
    public class LevelMapView:MonoBehaviour
    {
        const float Step=168,Height=17000;
        class Row {public RectTransform root;public Image face,halo;public Text number;public GameObject locked,bubble;public Button button;public StarGraphic[] stars;public CandyPathGraphic path;public int level;}
        Row[] rows;RectTransform content,pathLayer,viewport;SaveData save;int current,lastFirst=-99;bool unlockAll;Action<int> play;
        readonly float[] xs={-210,150,-20,235,-220,80,-170};
        public void Build(Transform parent,SaveData data,int next,Action<int> launch,bool unlockAllLevelsForTesting=false)
        {
            save=data;current=next;play=launch;unlockAll=unlockAllLevelsForTesting;lastFirst=-99;
            viewport=UI.Box(parent,"Scrollable level path",0,65,960,1210,Color.clear,false).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.09f;scroll.scrollSensitivity=55;
            content=UI.Rect(viewport,"100 laundry days",0,0,960,Height);content.anchorMin=content.anchorMax=content.pivot=new Vector2(.5f,1);
            pathLayer=UI.Rect(content,"Wooden trail",0,0,960,Height);pathLayer.anchorMin=pathLayer.anchorMax=pathLayer.pivot=new Vector2(.5f,1);
            scroll.content=content;scroll.viewport=viewport;rows=new Row[12];for(int i=0;i<12;i++)rows[i]=CreateRow();
            Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1-Mathf.Clamp01(Mathf.Max(0,next-3)*Step/(Height-1210));
            scroll.onValueChanged.AddListener(_=>Refresh());Refresh();
        }
        Row CreateRow()
        {
            var row=new Row();row.root=UI.Rect(content,"Recycled level",0,0,960,Step);row.root.anchorMin=row.root.anchorMax=new Vector2(.5f,1);
            var path=UI.Rect(pathLayer,"Wooden path",0,0,960,Step);path.anchorMin=path.anchorMax=new Vector2(.5f,1);
            row.path=path.gameObject.AddComponent<CandyPathGraphic>();row.path.raycastTarget=false;
            row.halo=UI.Circle(row.root,"Current halo",0,0,198,UI.Gold);
            row.face=CosyArt.Image(row.root,"Level","CosyNode",0,0,176,176,false);row.face.raycastTarget=true;
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
                row.root.anchoredPosition=new Vector2(x,-100-index*Step);row.path.rectTransform.anchoredPosition=row.root.anchoredPosition;
                float beforePrevious=xs[(index+xs.Length-2)%xs.Length],nextX=xs[(index+1)%xs.Length];
                row.path.gameObject.SetActive(index>0);row.path.Set(previousX-x,-Step,(x-beforePrevious)*.65f,(nextX-previousX)*.65f);
                bool active=row.level==current,locked=row.level>save.unlocked,complete=save.stars[index]>0;
                row.face.sprite=CosyArt.Get(active?"CosyCurrent":complete?"CosyGold":"CosyNode");row.face.color=Color.white;
                row.halo.gameObject.SetActive(active);row.bubble.SetActive(active);row.locked.SetActive(locked);
                row.bubble.GetComponent<RectTransform>().anchoredPosition=new Vector2(x< -100?245:-245,25);
                row.number.text=row.level.ToString();row.number.color=active?UI.Cream:UI.Ink;row.number.rectTransform.anchoredPosition=new Vector2(0,locked||complete?25:0);
                row.button.interactable=!locked||unlockAll;
                for(int star=0;star<3;star++){row.stars[star].gameObject.SetActive(complete);row.stars[star].color=star<save.stars[index]?UI.Gold:UI.Line;}
            }
            Cull();
        }
        void Cull(){foreach(var row in rows){float y=content.anchoredPosition.y+row.root.anchoredPosition.y;row.root.gameObject.SetActive(y<=0&&y>=-viewport.rect.height);}}
    }
}
