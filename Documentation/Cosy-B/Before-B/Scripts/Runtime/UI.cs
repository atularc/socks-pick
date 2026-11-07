using System;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    public static class UI
    {
        public static readonly Color Ink=Hex("234D4A"), Cream=Hex("F3D9AA"), Green=Hex("36826C"), Blue=Hex("377E80"), Pink=Hex("D95442"), Gold=Hex("F4C453"), Purple=Hex("477D76"), Mint=Hex("BAD8C5"), Muted=Hex("43645B"), Line=Hex("86AA97"), Board=Hex("B6CDB3"), Trail=Hex("56877A");
        static Font font,bodyFont;  static Texture2D background; static Sprite basketArt;
        public static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
        public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
        public static Image Box(Transform parent,string name,float x,float y,float w,float h,Color color,bool rounded=true)
        {
            Image i;
            var r=Rect(parent,name,x,y,w,h);
            if(rounded){var shape=r.gameObject.AddComponent<ClayImage>();shape.depth=0;i=shape;}
            else i=r.gameObject.AddComponent<Image>();
            i.color=color;i.raycastTarget=false;return i;
        }
        public static Image ClayBox(Transform parent,string name,float x,float y,float w,float h,Color color,bool round=false)
        {
            var i=Rect(parent,name,x,y,w,h).gameObject.AddComponent<ClayImage>();
            i.roundness=round?.5f:.24f;i.color=color;i.raycastTarget=false;return i;
        }
        public static Image Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            Box(parent,name+" shadow",x,y-14,w,h,new Color(.10f,.22f,.18f,.20f));
            return ClayBox(parent,name,x,y,w,h,color);
        }
        public static RectTransform ClippedLayer(Transform parent,string name,float top,float bottom)
        {float center=(top+bottom)*.5f;var clip=Rect(parent,name+" bounds",0,center,1040,top-bottom);if(name!="Effects")clip.gameObject.AddComponent<RectMask2D>();var content=Rect(clip,name,0,-center,1080,1920);if(name=="Socks")clip.gameObject.AddComponent<PileBounds>().Configure(content,top,bottom);return content;}
        public static Image Sprite(Transform parent,string name,Sprite sprite,float x,float y,float w,float h,Color? tint=null)
        {var i=Box(parent,name,x,y,w,h,tint??Color.white,false);i.sprite=sprite;i.preserveAspect=true;if(sprite&&sprite.texture.name=="FabricSocks"){i.preserveAspect=false;i.rectTransform.sizeDelta=Vector2.one*Mathf.Min(w,h);}return i;}
        public static Text Label(Transform parent,string value,float x,float y,float w,float h,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleCenter)
        {if(!font)font=Resources.Load<Font>("Fonts/Fredoka700");if(!bodyFont)bodyFont=Resources.Load<Font>("Fonts/Fredoka600");if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");if(!bodyFont)bodyFont=font;var t=Rect(parent,value,x,y,w,h).gameObject.AddComponent<Text>();t.font=size>=46?font:bodyFont;t.text=value;t.fontSize=size;t.color=color??Ink;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        public static Button Button(Transform parent,string title,float x,float y,float w,float h,Color color,Action action,int size=32)
        {
            var r=Rect(parent,title,x,y,w,h);
            string glyph=title=="Resume"||title=="Next"?"play":title=="Restart"||title=="Try again"||title=="Retry"?"restart":title=="Home"?"home":title=="II"?"pause":title=="x"?"close":title=="<"?"back":title==">"?"forward":title=="Settings"?"settings":null;
            bool iconOnly=glyph=="pause"||glyph=="close"||glyph=="back"||glyph=="forward"||glyph=="settings";
            var depth=iconOnly?Circle(r,"Button depth",0,-10,Mathf.Min(w,h),Color.Lerp(color,Ink,.30f)):Box(r,"Button depth",0,-12,w,h,Color.Lerp(color,Ink,.30f));
            var face=ClayBox(r,"Face",0,0,w,h,color,iconOnly);face.raycastTarget=true;
            var label=Label(r,iconOnly?"":title,0,2,w-36,h-12,size,Color.white);label.font=bodyFont;label.gameObject.AddComponent<CenteredGlyphs>();
            if(glyph!=null){float iconSize=iconOnly?h*.43f:h*.32f,gap=h*.12f;float textWidth=Mathf.Min(label.preferredWidth,w-iconSize-gap-48),groupWidth=textWidth+iconSize+gap;label.rectTransform.anchoredPosition=new Vector2(iconOnly?0:(iconSize+gap)*.5f,2);label.rectTransform.sizeDelta=new Vector2(iconOnly?w-24:textWidth+8,h-12);Glyph(r,glyph,iconOnly?0:-groupWidth*.5f+iconSize*.5f,2,iconSize,Color.white);}
            if(w<132||h<132)Box(r,"Touch area",0,0,Mathf.Max(w,132),Mathf.Max(h,132),Color.clear,false).raycastTarget=true;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=face;r.gameObject.AddComponent<ButtonMotion>();var colors=b.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.85f,.91f,.89f);colors.selectedColor=Color.white;colors.disabledColor=new Color(.7f,.75f,.72f,.7f);b.colors=colors;
            b.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();action?.Invoke();});return b;
        }
        public static void Glyph(Transform parent,string kind,float x,float y,float size,Color tint)
        {var g=Rect(parent,kind,x,y,size,size).gameObject.AddComponent<CandyGlyph>();g.kind=kind;g.color=tint;g.raycastTarget=false;}
        public static void Switch(Transform parent,float x,float y,bool on,Action action)
        {var r=Rect(parent,"Toggle",x,y,178,94);var track=ClayBox(r,"Track",0,0,178,94,on?Green:Hex("A7BDB5"));track.raycastTarget=true;var knob=Circle(r,"White knob",on?42:-42,0,77,Cream);Box(r,"Touch area",0,0,178,132,Color.clear,false).raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=track;b.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();on=!on;track.color=on?Green:Hex("A7BDB5");knob.rectTransform.anchoredPosition=new Vector2(on?42:-42,0);action();});}
        public static void Clear(Transform t){for(int i=t.childCount-1;i>=0;i--){var child=t.GetChild(i).gameObject;child.SetActive(false);child.transform.SetParent(null,false);UnityEngine.Object.Destroy(child);}}
        public static void DisposeIcons(){if(basketArt)UnityEngine.Object.Destroy(basketArt);basketArt=null;} // No dynamically created UI sprites to dispose.
        public static void Stars(Transform parent,int count,float x,float y,float size)
        {for(int i=0;i<3;i++){var r=Rect(parent,"Star",x+(i-1)*size*1.15f,y+(i==1?28:0),size,size);var g=r.gameObject.AddComponent<StarGraphic>();g.color=i<count?Gold:Line;g.raycastTarget=false;}}
        public static void Stroke(Text text,Color color,float width=3)
        {var outline=text.gameObject.AddComponent<Outline>();outline.effectColor=color;outline.effectDistance=new Vector2(width,-width);outline.useGraphicAlpha=true;}
        public static Image Circle(Transform parent,string name,float x,float y,float size,Color color)
        {return ClayBox(parent,name,x,y,size,size,color,true);}
        public static Image Medal(Transform parent,string name,float x,float y,float size,Color color){return Circle(parent,name,x,y,size,color);}
        public static void Coin(Transform parent,float x,float y,float size)
        {Circle(parent,"Coin edge",x,y-3,size,Hex("CD9933"));Circle(parent,"Coin",x,y,size*.91f,Gold);Glyph(parent,"spark",x,y,size*.49f,Hex("916626"));}
        public static void Gear(Transform parent){Glyph(parent,"settings",0,0,65,Ink);}
        public static void Gradient(Transform parent,Color top,Color bottom)
        {var r=Rect(parent,"Gradient",0,0,1080,1920);r.gameObject.AddComponent<FullBleed>();var g=r.gameObject.AddComponent<GradientGraphic>();g.top=top;g.bottom=bottom;g.raycastTarget=false;}
        public static Image Inset(Transform parent,string name,float x,float y,float w,float h)
        {Box(parent,name+" rim",x,y-5,w,h,Line);return ClayBox(parent,name,x,y,w,h,Cream);}
        public static void Wood(Transform parent,int theme=0){Gradient(parent,Cream,Hex("EDF5EC"));}
        public static void RoomBackground(Transform parent,bool quiet=false)
        {
            if(!background)background=Resources.Load<Texture2D>("Art/FreshLaundryBackground");
            if(!background){Wood(parent);return;}
            if(quiet)Wood(parent);
            var r=Rect(parent,"Fresh Laundry room",0,0,1080,1920);
            var art=r.gameObject.AddComponent<RawImage>();art.texture=background;art.raycastTarget=false;
            art.color=new Color(1,1,1,quiet?.18f:1f);r.gameObject.AddComponent<FullBleed>();
        }
        public static void Washer(Transform parent,float x,float y,float size,Color color)
        {var r=Rect(parent,"Washing machine",x,y,size,size);Box(r,"Feet",0,-size*.46f,size*.7f,size*.08f,Ink);Box(r,"Cabinet",0,0,size,size,color);Box(r,"Control strip",0,size*.31f,size*.88f,size*.19f,Cream);Circle(r,"Dial",size*.29f,size*.31f,size*.1f,Ink);Circle(r,"Door rim",0,-size*.1f,size*.65f,Ink);Circle(r,"Door glass",0,-size*.1f,size*.53f,Mint);Box(r,"Reflection",-size*.1f,-size*.03f,size*.08f,size*.20f,Color.white);}
        public static Rect BasketImageRect(Image basket)
        {var r=basket.rectTransform.rect;if(basket.preserveAspect&&basket.sprite){float aspect=basket.sprite.rect.width/basket.sprite.rect.height;float w=Mathf.Min(r.width,r.height*aspect),h=w/aspect;r=new Rect(r.center.x-w*.5f,r.center.y-h*.5f,w,h);}return r;}
        public static Vector2 BasketSlot(Image basket,int index,int count=5)
        {var r=BasketImageRect(basket);float x=count<=1?.5f:Mathf.Lerp(.16f,.84f,Mathf.Clamp01(index/(float)(count-1)));return new Vector2(r.xMin+r.width*x,r.yMin+r.height*.65f);}
        public static Vector2 CompletedPairSlot(Image basket,int index,int count)
        {var r=BasketImageRect(basket);return new Vector2(r.center.x+(index-(count-1)*.5f)*r.width*.145f,r.yMin+r.height*.70f);}
        public static void BasketFront(Image basket)
        {
            // Reuse the basket texture, masking its front rim over collected pairs.
            var r=basket.rectTransform.rect;
            var clip=Rect(basket.transform,"Basket front mask",0,r.yMin+r.height*.28f,r.width,r.height*.56f);
            clip.gameObject.AddComponent<RectMask2D>();
            var face=Sprite(clip,"Basket front",basket.sprite,0,-clip.anchoredPosition.y,r.width,r.height);
            face.preserveAspect=false;
        }
        public static Image Basket(Transform parent,float x,float y,float w,float h)
        {
            if(!basketArt){var t=Resources.Load<Texture2D>("Art/FreshBasket");basketArt=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);}
            var image=Sprite(parent,"Lavender basket",basketArt,x,y,w*.88f,h*.8f);image.preserveAspect=false;return image;
        }
    }
}
