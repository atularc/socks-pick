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
        public override Texture mainTexture=>CosyArt.Get("CosyStar")?CosyArt.Get("CosyStar").texture:Texture2D.whiteTexture;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var sprite=CosyArt.Get("CosyStar");if(!sprite)return;
            var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);var r=rectTransform.rect;
            var v=UIVertex.simpleVert;v.color=color==UI.Gold?Color.white:color;
            v.position=new Vector2(r.xMin,r.yMin);v.uv0=new Vector2(uv.x,uv.y);vh.AddVert(v);
            v.position=new Vector2(r.xMin,r.yMax);v.uv0=new Vector2(uv.x,uv.w);vh.AddVert(v);
            v.position=new Vector2(r.xMax,r.yMax);v.uv0=new Vector2(uv.z,uv.w);vh.AddVert(v);
            v.position=new Vector2(r.xMax,r.yMin);v.uv0=new Vector2(uv.z,uv.y);vh.AddVert(v);
            vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
    }

}
