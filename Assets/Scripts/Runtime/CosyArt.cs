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
            {
                var all=Resources.LoadAll<Sprite>("Art/"+name);sprite=all.Length>0?all[0]:null;
                if(sprite&&(name=="CosyPopup"||name=="CosyButton"||name=="CosySecondary"||name=="CosyTeal"))
                {
                    // Keep the illustrated arch, corners and bevels intact. Only the flat centre stretches.
                    var r=sprite.rect;var border=name=="CosyPopup"
                        ?new Vector4(r.width*.14f,r.height*.08f,r.width*.14f,r.height*.23f)
                        :new Vector4(r.height*.5f,r.height*.46f,r.height*.5f,r.height*.46f);
                    // Imported downscaled sprites carry a scaled PPU; use canvas reference units here.
                    sprite=Sprite.Create(sprite.texture,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);
                    sprite.name=name;
                }
                sprites[name]=sprite;
            }
            return sprite;
        }
        public static Image Image(Transform parent,string name,string asset,float x,float y,float w,float h,bool sliced=true)
        {
            var image=UI.Rect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();
            image.sprite=Get(asset);bool frame=asset=="CosyBoard"||asset=="CosyShell"||asset=="CosyPopup";
            bool button=asset=="CosyButton"||asset=="CosySecondary"||asset=="CosyTeal";
            image.type=sliced&&(frame||button)?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect=image.type==UnityEngine.UI.Image.Type.Simple;
            image.pixelsPerUnitMultiplier=asset=="CosyPopup"?image.sprite.rect.width/w:
                button?image.sprite.rect.height/Mathf.Min(h,w*.4f):asset=="CosyBoard"?1.5f:.75f;
            image.raycastTarget=false;return image;
        }
    }
}
