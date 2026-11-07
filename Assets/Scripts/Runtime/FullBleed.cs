using UnityEngine;

namespace SockPairs
{
    // Backgrounds cover the physical display independently of safe-area controls.
    [DefaultExecutionOrder(1000)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class FullBleed : MonoBehaviour
    {
        RectTransform rect, parent;
        Matrix4x4 lastMatrix;
        int lastWidth=-1,lastHeight=-1;
        void OnEnable(){rect=(RectTransform)transform;parent=transform.parent as RectTransform;Refresh(true);}
        void LateUpdate(){Refresh(false);}
        public void Refresh(bool force=false)
        {
            if(!parent)return;
            var matrix=parent.localToWorldMatrix;
            if(!force&&lastWidth==Screen.width&&lastHeight==Screen.height&&matrix==lastMatrix)return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Vector2.zero,null,out var lower);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(Screen.width,Screen.height),null,out var upper);
            rect.anchorMin=rect.anchorMax=parent.pivot;rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=(lower+upper)*.5f;rect.sizeDelta=upper-lower;
            var image=GetComponent<UnityEngine.UI.RawImage>();
            if(image&&image.texture)
            {
                // Fill the display by cropping, never by squeezing the room illustration.
                float source=image.texture.name=="CosyRoom"?941f/1672f:(float)image.texture.width/image.texture.height;
                float target=rect.rect.width/rect.rect.height;
                float u=Mathf.Min(1,target/source),v=Mathf.Min(1,source/target);
                image.uvRect=new Rect((1-u)*.5f,(1-v)*.5f,u,v);
            }
            lastMatrix=matrix;lastWidth=Screen.width;lastHeight=Screen.height;
        }
    }
}
