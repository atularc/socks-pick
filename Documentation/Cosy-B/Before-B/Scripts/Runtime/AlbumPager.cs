using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace SockPairs
{
    public sealed class AlbumPager : ScrollRect
    {
        public int pageCount=2;
        public int Page {get;private set;}
        public bool instant;
        public Action<int> PageChanged;
        Vector2 dragStart;
        Coroutine snap;
        public void GoTo(int page,bool animate=true)
        {
            if(snap!=null){StopCoroutine(snap);snap=null;}
            StopMovement();Page=Mathf.Clamp(page,0,pageCount-1);PageChanged?.Invoke(Page);
            float destination=-Page*viewport.rect.width;
            if(!animate||instant)content.anchoredPosition=new Vector2(destination,0);
            else snap=StartCoroutine(Snap(destination));
        }
        public override void OnBeginDrag(PointerEventData data)
        {
            if(snap!=null){StopCoroutine(snap);snap=null;}
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport,data.position,data.pressEventCamera,out dragStart);
            base.OnBeginDrag(data);
        }
        public override void OnEndDrag(PointerEventData data)
        {
            base.OnEndDrag(data);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport,data.position,data.pressEventCamera,out var end);
            float delta=end.x-dragStart.x;
            int target=Mathf.Abs(delta)>viewport.rect.width*.12f?Page+(delta<0?1:-1):Mathf.RoundToInt(-content.anchoredPosition.x/viewport.rect.width);
            GoTo(target);
        }
        IEnumerator Snap(float target)
        {
            float start=content.anchoredPosition.x;
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime)
            {content.anchoredPosition=new Vector2(Mathf.Lerp(start,target,Mathf.SmoothStep(0,1,t/.2f)),0);yield return null;}
            content.anchoredPosition=new Vector2(target,0);snap=null;
        }
        protected override void OnDisable()
        {if(snap!=null){StopCoroutine(snap);snap=null;}base.OnDisable();}
    }
}
