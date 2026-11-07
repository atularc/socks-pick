using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class GradientGraphic : MaskableGraphic
    {
        public Color top,bottom;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var v=UIVertex.simpleVert;
            v.position=new Vector3(r.xMin,r.yMin);v.color=bottom;vh.AddVert(v);
            v.position=new Vector3(r.xMin,r.yMax);v.color=top;vh.AddVert(v);
            v.position=new Vector3(r.xMax,r.yMax);v.color=top;vh.AddVert(v);
            v.position=new Vector3(r.xMax,r.yMin);v.color=bottom;vh.AddVert(v);
            vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public class StarGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(color.a<=0)return;var rect=rectTransform.rect;float radius=Mathf.Min(rect.width,rect.height)*.5f;
            var v=UIVertex.simpleVert;v.color=Color.Lerp(color,Color.white,.35f);v.position=Vector3.zero;vh.AddVert(v);
            for(int i=0;i<10;i++)
            {
                float angle=(90+i*36)*Mathf.Deg2Rad,r=radius*(i%2==0?1:.46f);
                v.position=new Vector3(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r);Color shade=Color.Lerp(color,color*.68f,Mathf.Clamp01(-Mathf.Sin(angle))*.65f);shade.a=color.a;v.color=shade;vh.AddVert(v);
            }
            for(int i=0;i<10;i++)vh.AddTriangle(0,i+1,(i+1)%10+1);
        }
    }
    public class SockVisual : MonoBehaviour
    {
        public Image art,hit,shadow;
        public GameObject knot,hint,selection;
        public Text gate;
        public Coroutine movement;
    }
}
