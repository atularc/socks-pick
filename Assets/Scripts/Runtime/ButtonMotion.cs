using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
namespace SockPairs
{
    public class ButtonMotion:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        Coroutine motion;
        void OnDisable(){if(motion!=null)StopCoroutine(motion);motion=null;transform.localScale=Vector3.one;}
        public void OnPointerDown(PointerEventData e){Animate(.93f,.08f);}
        public void OnPointerUp(PointerEventData e){Animate(1,.12f);}
        public void OnPointerExit(PointerEventData e){Animate(1,.12f);}
        void Animate(float target,float duration){if(SockGame.Instance&&SockGame.Instance.save!=null&&SockGame.Instance.save.reduceMotion){transform.localScale=Vector3.one;return;}if(motion!=null)StopCoroutine(motion);motion=StartCoroutine(Scale(target,duration));}
        IEnumerator Scale(float target,float duration)
        {
            float start=transform.localScale.x;
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {float u=t/duration;transform.localScale=Vector3.one*(Mathf.Lerp(start,target,u)+(target==1?Mathf.Sin(u*Mathf.PI)*.045f:0));yield return null;}
            transform.localScale=Vector3.one*target;
        }
    }
}
