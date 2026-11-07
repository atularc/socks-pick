using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CandyPathGraphic:MaskableGraphic
    {
        float previous,step,startTangent,endTangent;
        public override Texture mainTexture=>ReferenceBArt.Atlas;
        public void Set(float x,float height,float a,float b){previous=x;step=height;startTangent=a;endTangent=b;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();Strip(vh,70,new Color(.45f,.30f,.20f));Strip(vh,61,new Color(1,.95f,.85f));Strip(vh,47,new Color(.8f,.72f,.60f));Strip(vh,39,Color.white);}
        void Strip(VertexHelper vh,float width,Color tint)
        {
            const int segments=20;int offset=vh.currentVertCount;
            var uv=new Vector4(277f/1536f,1-286f/1024f,310f/1536f,1-268f/1024f);
            for(int i=0;i<=segments;i++)
            {
                float t=i/(float)segments;
                float t2=t*t,t3=t2*t;
                float q=1-t;float x=q*q*q*previous+3*q*q*t*startTangent+3*q*t*t*endTangent;
                var p=new Vector2(x,Mathf.Lerp(-step,0,t*t*(3-2*t)));
                float dx=3*q*q*(startTangent-previous)+6*q*t*(endTangent-startTangent)-3*t*t*endTangent;
                var tangent=new Vector2(dx,step*6*t*(1-t)).normalized;
                var n=new Vector2(-tangent.y,tangent.x)*width*.5f;
                var v=UIVertex.simpleVert;v.color=tint;v.position=p+n;v.uv0=new Vector2(Mathf.Lerp(uv.x,uv.z,.35f),Mathf.Lerp(uv.y,uv.w,.3f+t*.4f));vh.AddVert(v);v.position=p-n;v.uv0.x=Mathf.Lerp(uv.x,uv.z,.65f);vh.AddVert(v);
                if(i>0){int a=offset+(i-1)*2;vh.AddTriangle(a,a+1,a+2);vh.AddTriangle(a+1,a+3,a+2);}
            }
        }
    }
}
