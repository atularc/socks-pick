using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    // Static vertex shading gives every control the same soft clay lighting.
    // One white-texture draw: no shader variants, normal maps, blur or lighting.
    public sealed class ClayImage : Image
    {
        public float roundness=.24f,depth=1f;
        const int Steps=8,Count=36;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float edge=Mathf.Min(r.width,r.height);
            if(edge<=0)return;
            float radius=roundness>=.5f?edge*.5f:Mathf.Min(65,edge*roundness);
            float bevel=Mathf.Min(34,edge*.20f);
            for(int ring=0;ring<5;ring++)
            {
                float inset=ring==0?0:ring==1?1.5f:ring==2?bevel*.3f:ring==3?bevel*.72f:bevel;
                float rad=Mathf.Max(.1f,radius-inset);
                for(int corner=0;corner<4;corner++)for(int step=0;step<=Steps;step++)
                {
                    float angle=(corner*90+step*90f/Steps)*Mathf.Deg2Rad;
                    bool right=corner==0||corner==3,top=corner<2;
                    var centre=new Vector2(right?r.xMax-inset-rad:r.xMin+inset+rad,top?r.yMax-inset-rad:r.yMin+inset+rad);
                    Vector2 pos=centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*rad;
                    float light=Mathf.Clamp((pos.y-r.center.y)/Mathf.Max(1,r.height*.5f)-(pos.x-r.center.x)/Mathf.Max(1,r.width)*.25f,-1,1);
                    float highlight=ring==0?.10f:ring==1?.62f:ring==2?.38f:ring==3?.13f:.015f;
                    float dark=ring==0?.48f:ring==1?.42f:ring==2?.30f:ring==3?.10f:.015f;
                    var tint=light>=0?Color.Lerp(color,Color.white,light*highlight*depth):Color.Lerp(color,UI.Ink,-light*dark*depth);
                    tint.a=ring==0?0:color.a;var v=UIVertex.simpleVert;v.position=pos;v.color=tint;vh.AddVert(v);
                }
                if(ring>0)for(int i=0;i<Count;i++)
                {int a=(ring-1)*Count+i,b=(ring-1)*Count+(i+1)%Count,c=ring*Count+i,d=ring*Count+(i+1)%Count;vh.AddTriangle(a,b,c);vh.AddTriangle(b,d,c);}
            }
            var centreVertex=UIVertex.simpleVert;centreVertex.position=r.center;centreVertex.color=color;
            int centreIndex=vh.currentVertCount;vh.AddVert(centreVertex);
            for(int i=0;i<Count;i++)vh.AddTriangle(centreIndex,4*Count+i,4*Count+(i+1)%Count);
        }
    }
}
