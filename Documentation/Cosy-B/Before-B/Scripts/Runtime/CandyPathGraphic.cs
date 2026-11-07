using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CandyPathGraphic:MaskableGraphic
    {
        float previous,step,startTangent,endTangent;
        public void Set(float x,float height,float a,float b){previous=x;step=height;startTangent=a;endTangent=b;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();Strip(vh,70,UI.Hex("345E53"));Strip(vh,52,UI.Hex("D3B57C"));Strip(vh,32,UI.Hex("F1D69C"));}
        void Strip(VertexHelper vh,float width,Color tint)
        {
            const int segments=20;int offset=vh.currentVertCount;
            for(int i=0;i<=segments;i++)
            {
                float t=i/(float)segments;
                float smooth=t*t*(3-2*t);
                var p=new Vector2(Mathf.Lerp(previous,0,smooth),Mathf.Lerp(-step,0,t));
                var tangent=new Vector2(-previous*6*t*(1-t),step).normalized;
                var n=new Vector2(-tangent.y,tangent.x)*width*.5f;
                var v=UIVertex.simpleVert;v.color=tint;v.position=p+n;vh.AddVert(v);v.position=p-n;vh.AddVert(v);
                if(i>0){int a=offset+(i-1)*2;vh.AddTriangle(a,a+1,a+2);vh.AddTriangle(a+1,a+3,a+2);}
            }
        }
    }
}
