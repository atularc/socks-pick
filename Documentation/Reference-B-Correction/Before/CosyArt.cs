using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    public static class CosyArt
    {
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static Sprite Get(string name)
        {
            if(!sprites.TryGetValue(name,out var sprite))
            {var all=Resources.LoadAll<Sprite>("Art/"+name);sprite=all.Length>0?all[0]:null;sprites[name]=sprite;}
            return sprite;
        }
        public static Image Image(Transform parent,string name,string asset,float x,float y,float w,float h,bool sliced=true)
        {
            var image=UI.Rect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();
            image.sprite=Get(asset);bool frame=asset=="CosyBoard"||asset=="CosyShell"||asset=="CosyPopup";
            image.type=sliced&&frame?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;
            image.pixelsPerUnitMultiplier=asset=="CosyPopup"?.60f:.75f;
            image.raycastTarget=false;return image;
        }
    }
}
