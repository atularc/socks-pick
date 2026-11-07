using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
namespace SockPairs
{
    public class ButtonMotion:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        Coroutine motion;Vector3 authoredScale;
        void OnEnable(){authoredScale=transform.localScale;}
        void OnDisable(){if(motion!=null)StopCoroutine(motion);motion=null;transform.localScale=authoredScale;}
        public void OnPointerDown(PointerEventData e){Animate(.93f,.08f);}
        public void OnPointerUp(PointerEventData e){Animate(1,.12f);}
        public void OnPointerExit(PointerEventData e){Animate(1,.12f);}
        void Animate(float target,float duration){if(SockGame.Instance&&SockGame.Instance.save!=null&&SockGame.Instance.save.reduceMotion){transform.localScale=authoredScale;return;}if(motion!=null)StopCoroutine(motion);motion=StartCoroutine(Scale(target,duration));}
        IEnumerator Scale(float target,float duration)
        {
            float start=authoredScale.x==0?1:transform.localScale.x/authoredScale.x;
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {float u=t/duration;transform.localScale=authoredScale*(Mathf.Lerp(start,target,u)+(target==1?Mathf.Sin(u*Mathf.PI)*.045f:0));yield return null;}
            transform.localScale=authoredScale*target;
        }
    }
}
