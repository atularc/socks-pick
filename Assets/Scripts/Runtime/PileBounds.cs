using UnityEngine;
namespace SockPairs
{
    [DefaultExecutionOrder(-50)]
    public sealed class PileBounds:MonoBehaviour
    {
        RectTransform rect,content;float top,bottom,last=float.NaN;
        public void Configure(RectTransform child,float upper,float lower){rect=(RectTransform)transform;content=child;top=upper;bottom=lower;LateUpdate();}
        void LateUpdate()
        {
            if(!rect)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=0; // Keep the reference composition intact.
            if(extra==last)return;last=extra;
            float center=(top+extra+bottom)*.5f;
            rect.anchoredPosition=new Vector2(0,center);rect.sizeDelta=new Vector2(1040,top+extra-bottom);
            content.anchoredPosition=new Vector2(0,-center);
        }
    }
}
