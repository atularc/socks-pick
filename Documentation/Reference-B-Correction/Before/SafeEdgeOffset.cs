using UnityEngine;
namespace SockPairs
{
    // Reposition a group without stretching its artwork or changing internal spacing.
    [DefaultExecutionOrder(-50)]
    public sealed class SafeEdgeOffset : MonoBehaviour
    {
        RectTransform rect;Vector2 origin;float weight,lastExtra=float.NaN;
        public void Configure(float value){rect=(RectTransform)transform;origin=rect.anchoredPosition;weight=value;Apply();}
        void LateUpdate(){Apply();}
        void Apply()
        {
            if(!rect)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=Mathf.Max(0,(area.height/Mathf.Max(.0001f,scale)-1920)*.5f);
            if(extra==lastExtra)return;
            rect.anchoredPosition=origin+Vector2.up*(extra*weight);lastExtra=extra;
        }
        public static void Pin(RectTransform rect,float weight){rect.gameObject.AddComponent<SafeEdgeOffset>().Configure(weight);}
    }
}
