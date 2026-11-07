using UnityEngine;
namespace SockPairs
{
    public sealed class CosyScreenFrame:MonoBehaviour
    {
        RectTransform rect;float previous=-1;
        void LateUpdate()
        {
            if(!rect)rect=(RectTransform)transform;
            var area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float height=area.height/Mathf.Max(.001f,scale)-40;
            if(height==previous)return;previous=height;rect.sizeDelta=new Vector2(1040,height);
        }
    }
}
