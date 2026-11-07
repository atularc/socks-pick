using System;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    // Twelve recycled rows cover the viewport and overscan. The map never creates
    // 100 full button hierarchies, so later chapters cost the same as the first.
    public class LevelMapView:MonoBehaviour
    {
        const float Step=236,Height=23750;
        class Row
        {
            public RectTransform root;public Image shadow,face,halo,door;public Text milestone;public Text number;
            public Button button;public GameObject locked;public StarGraphic[] stars;
            public CandyPathGraphic path;public int level;
        }
        Row[] rows;
        RectTransform content,pathLayer;
        SaveData save;int current,lastFirst=-99;
        bool unlockAll;
        Action<int> play;
        readonly Color[] nodeColors={UI.Hex("E46550"),UI.Hex("55A8AD"),UI.Hex("A27CCC"),UI.Hex("6CAC7C"),UI.Hex("E5AE43"),UI.Hex("DA7EA4")};
        readonly float[] xs={0,-235,0,235};
        public void Build(Transform parent,SaveData data,int next,Action<int> launch,bool unlockAllLevelsForTesting=false)
        {
            save=data;current=next;play=launch;lastFirst=-99;unlockAll=unlockAllLevelsForTesting;
            var viewport=UI.Box(parent,"Scrollable level path",0,-125,1080,1120,Color.clear,false).rectTransform;
            viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.09f;scroll.scrollSensitivity=55;
            content=UI.Rect(viewport,"100 laundry days",0,0,1080,Height);content.anchorMin=content.anchorMax=new Vector2(.5f,0);content.pivot=new Vector2(.5f,0);
            scroll.content=content;scroll.viewport=viewport;
            pathLayer=UI.Rect(content,"Trail behind nodes",0,0,1080,Height);pathLayer.anchorMin=pathLayer.anchorMax=new Vector2(.5f,0);pathLayer.pivot=new Vector2(.5f,0);
            rows=new Row[12];
            for(int i=0;i<rows.Length;i++)rows[i]=CreateRow(content);
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition=Mathf.Clamp01(((next-1)*Step-340)/(Height-1120));
            scroll.onValueChanged.AddListener(_=>Refresh());Refresh();
        }
        Row CreateRow(Transform parent)
        {
            var row=new Row();row.root=UI.Rect(parent,"Recycled level",0,0,1080,Step);
            row.root.anchorMin=row.root.anchorMax=new Vector2(.5f,0);
            var path=UI.Rect(pathLayer,"Winding path",0,0,1080,Step);path.anchorMin=path.anchorMax=new Vector2(.5f,0);
            row.path=path.gameObject.AddComponent<CandyPathGraphic>();row.path.raycastTarget=false;
            row.halo=UI.Circle(row.root,"Current halo",0,0,250,UI.Gold);
            row.shadow=UI.Circle(row.root,"Level depth",0,-12,184,UI.Green);
            row.face=UI.ClayBox(row.root,"Level",0,0,184,184,UI.Pink,true);row.face.raycastTarget=true;
            row.door=UI.Circle(row.root,"Soft centre",0,5,161,Color.clear);row.door.gameObject.SetActive(false);
            
            row.button=row.face.gameObject.AddComponent<Button>();row.button.targetGraphic=row.face;
            row.face.gameObject.AddComponent<ButtonMotion>();
            var colors=row.button.colors;colors.disabledColor=Color.white;colors.selectedColor=Color.white;row.button.colors=colors;
            row.button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();play(row.level);});
            row.number=UI.Label(row.root,"1",0,1,140,110,75,Color.white);row.number.gameObject.AddComponent<CenteredGlyphs>();
            var locked=UI.Rect(row.root,"Lock",65,65,53,53);row.locked=locked.gameObject;
            UI.Circle(locked,"Lock chip",0,0,53,UI.Cream);UI.Glyph(locked,"lock",0,0,32,UI.Muted);
            row.milestone=UI.Label(row.root,"",0,153,420,48,32,UI.Cream);
            row.stars=new StarGraphic[3];
            for(int i=0;i<3;i++)
            {
                var r=UI.Rect(row.root,"Star",(i-1)*35,-113,27,27);row.stars[i]=r.gameObject.AddComponent<StarGraphic>();row.stars[i].raycastTarget=false;
            }
            return row;
        }
        void Refresh()
        {
            if(!content||rows==null)return;
            int first=Mathf.Clamp(Mathf.FloorToInt(-content.anchoredPosition.y/Step)-1,0,88);
            if(first==lastFirst)return;lastFirst=first;
            for(int i=0;i<rows.Length;i++)
            {
                var row=rows[i];int index=first+i,level=index+1;row.level=level;
                float x=xs[index%xs.Length],previousX=xs[(index+xs.Length-1)%xs.Length];
                row.root.anchoredPosition=new Vector2(x,190+index*Step);
                row.path.rectTransform.anchoredPosition=row.root.anchoredPosition;row.path.gameObject.SetActive(index>0);row.path.Set(previousX-x,Step,(x-xs[(index+xs.Length-2)%xs.Length])*.5f,(xs[(index+1)%xs.Length]-previousX)*.5f);
                bool locked=level>save.unlocked;
                bool active=level==current;
                Color color=active?UI.Pink:locked?UI.Hex("86AFA3"):UI.Gold;
                row.face.color=color;row.shadow.color=Color.Lerp(color,UI.Ink,.22f);
                row.door.color=Color.Lerp(color,Color.white,.15f);
                row.button.interactable=!locked||unlockAll;row.halo.color=UI.Gold;row.halo.gameObject.SetActive(active);
                row.number.text=level.ToString();row.number.color=active?Color.white:UI.Ink;
                row.number.rectTransform.anchoredPosition=new Vector2(0,1);row.locked.SetActive(locked);
                row.milestone.text=active?"YOU ARE HERE":"";row.milestone.color=UI.Cream;
                for(int star=0;star<3;star++){row.stars[star].gameObject.SetActive(!locked&&save.stars[index]>0);row.stars[star].color=star<save.stars[index]?UI.Gold:UI.Line;}

            }
        }
    }
}
