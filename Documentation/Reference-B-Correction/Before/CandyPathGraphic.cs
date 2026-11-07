using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CandyPathGraphic:MaskableGraphic
    {
        float previous,step,startTangent,endTangent;
        public override Texture mainTexture=>CosyArt.Get("CosyShell")?CosyArt.Get("CosyShell").texture:Texture2D.whiteTexture;
        public void Set(float x,float height,float a,float b){previous=x;step=height;startTangent=a;endTangent=b;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();Strip(vh,70,new Color(.45f,.30f,.20f));Strip(vh,61,new Color(1,.95f,.85f));Strip(vh,47,new Color(.8f,.72f,.60f));Strip(vh,39,Color.white);}
        void Strip(VertexHelper vh,float width,Color tint)
        {
            const int segments=20;int offset=vh.currentVertCount;
            var sprite=CosyArt.Get("CosyShell");var uv=sprite?UnityEngine.Sprites.DataUtility.GetOuterUV(sprite):new Vector4(0,0,1,1);
            for(int i=0;i<=segments;i++)
            {
                float t=i/(float)segments;
                float t2=t*t,t3=t2*t;
                float x=(2*t3-3*t2+1)*previous+(t3-2*t2+t)*startTangent+(t3-t2)*endTangent;
                var p=new Vector2(x,Mathf.Lerp(-step,0,t));
                float dx=(6*t2-6*t)*previous+(3*t2-4*t+1)*startTangent+(3*t2-2*t)*endTangent;
                var tangent=new Vector2(dx,step).normalized;
                var n=new Vector2(-tangent.y,tangent.x)*width*.5f;
                var v=UIVertex.simpleVert;v.color=tint;v.position=p+n;v.uv0=new Vector2(Mathf.Lerp(uv.x,uv.z,.35f),Mathf.Lerp(uv.y,uv.w,.3f+t*.4f));vh.AddVert(v);v.position=p-n;v.uv0.x=Mathf.Lerp(uv.x,uv.z,.65f);vh.AddVert(v);
                if(i>0){int a=offset+(i-1)*2;vh.AddTriangle(a,a+1,a+2);vh.AddTriangle(a+1,a+3,a+2);}
            }
        }
    }
}
