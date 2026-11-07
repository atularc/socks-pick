using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace SockPairs
{
    // Samples the approved artwork directly. Source rectangles use top-left image coordinates.
    // No generated replacement texture, per-frame readback, or duplicated bitmap slices.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ReferenceBArt:MaskableGraphic
    {
        static Texture2D atlas;static Material accent,unearned;
        public void CutOutTeal(){if(!accent)accent=new Material(Resources.Load<Shader>("Shaders/ReferenceAccent"));material=accent;}
        public void UnearnedStar(){if(!unearned)unearned=new Material(Resources.Load<Shader>("Shaders/ReferenceStarTone"));material=unearned;}
        static readonly Dictionary<Rect,Sprite> slices=new Dictionary<Rect,Sprite>();
        public static Sprite Slice(Rect topLeft){if(!slices.TryGetValue(topLeft,out var sprite)){sprite=Sprite.Create(Atlas,new Rect(topLeft.x,Atlas.height-topLeft.yMax,topLeft.width,topLeft.height),Vector2.one*.5f,100);slices[topLeft]=sprite;}return sprite;}
        public Rect source;
        public float radius,localRadius;
        public Texture texture;
        public bool rewardOutline, frame,tiled;
        public override Texture mainTexture=>texture?texture:Atlas;
        public static Texture2D Atlas {get{if(!atlas)atlas=Resources.Load<Texture2D>("Art/ReferenceB");return atlas;}}
        public static ReferenceBArt Add(Transform parent,string name,Rect source,float x,float y,float w,float h,float radius=0)
        {
            var image=UI.Rect(parent,name,x,y,w,h).gameObject.AddComponent<ReferenceBArt>();
            image.source=source;image.radius=radius;image.raycastTarget=false;image.SetAllDirty();return image;
        }
        void Vertex(VertexHelper vh,Vector2 p,Vector2 uv)
        {var v=UIVertex.simpleVert;v.color=color;v.position=p;v.uv0=new Vector2(uv.x/mainTexture.width,1-uv.y/mainTexture.height);vh.AddVert(v);}
        Vector2 Local(Vector2 p){var r=rectTransform.rect;return new Vector2(r.xMin+p.x/source.width*r.width,r.yMax-p.y/source.height*r.height);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(source.width<=0||source.height<=0||rectTransform.rect.width<=0||rectTransform.rect.height<=0||!mainTexture)return;
            if(tiled)
            {
                var r=rectTransform.rect;float tileW=source.width*2.237f,tileH=source.height*2.237f;
                var outline=new List<Vector2>();float clipRadius=Mathf.Min(localRadius,Mathf.Min(r.width,r.height)*.5f);
                if(clipRadius>0)for(int c=0;c<4;c++)for(int n=0;n<=8;n++)
                {float a=(c*90+n*90f/8)*Mathf.Deg2Rad;outline.Add(new Vector2((c==0||c==3?r.xMax-clipRadius:r.xMin+clipRadius)+Mathf.Cos(a)*clipRadius,(c<2?r.yMax-clipRadius:r.yMin+clipRadius)+Mathf.Sin(a)*clipRadius));}
                int rows=Mathf.CeilToInt(r.height/tileH),cols=Mathf.CeilToInt(r.width/tileW);
                for(int y=0;y<rows;y++)for(int x=0;x<cols;x++)
                {
                    float w=Mathf.Min(tileW,r.width-x*tileW),h=Mathf.Min(tileH,r.height-y*tileH);int i=vh.currentVertCount;
                    float u0=x%2==0?source.x:source.xMax,u1=u0+(x%2==0?1:-1)*source.width*w/tileW;
                    float v0=y%2==0?source.y:source.yMax,v1=v0+(y%2==0?1:-1)*source.height*h/tileH;
                    float left=r.xMin+x*tileW,top=r.yMax-y*tileH;
                    var polygon=new List<Vector2>{new Vector2(left,top),new Vector2(left+w,top),new Vector2(left+w,top-h),new Vector2(left,top-h)};
                    for(int edge=0;edge<outline.Count&&polygon.Count>0;edge++)
                    {
                        var a=outline[edge];var b=outline[(edge+1)%outline.Count];var clipped=new List<Vector2>();
                        var previous=polygon[polygon.Count-1];float before=Cross(b-a,previous-a);
                        foreach(var point in polygon)
                        {float after=Cross(b-a,point-a);if((after>=0)!=(before>=0))clipped.Add(Vector2.Lerp(previous,point,before/(before-after)));if(after>=0)clipped.Add(point);previous=point;before=after;}
                        polygon=clipped;
                    }
                    foreach(var point in polygon)Vertex(vh,point,new Vector2(Mathf.Lerp(u0,u1,(point.x-left)/w),Mathf.Lerp(v0,v1,(top-point.y)/h)));
                    for(int n=1;n+1<polygon.Count;n++)vh.AddTriangle(i,i+n,i+n+1);
                }
                return;
            }
            if(frame){Frame(vh);return;}
            if(rewardOutline)
            {
                var points=new[]{new Vector2(190,0),new Vector2(230,6),new Vector2(260,22),new Vector2(285,50),new Vector2(300,69),new Vector2(310,74),new Vector2(343,74),new Vector2(360,79),new Vector2(374,94),new Vector2(380,115),new Vector2(380,565),new Vector2(375,583),new Vector2(362,597),new Vector2(345,605),new Vector2(36,605),new Vector2(18,600),new Vector2(6,587),new Vector2(0,565),new Vector2(0,115),new Vector2(5,94),new Vector2(18,80),new Vector2(35,74),new Vector2(70,74),new Vector2(81,70),new Vector2(95,50),new Vector2(111,28),new Vector2(136,10),new Vector2(165,2)};
                Vertex(vh,Local(new Vector2(190,300)),source.position+new Vector2(190,300));
                for(int i=0;i<points.Length;i++)Vertex(vh,Local(points[i]),source.position+points[i]);
                for(int i=0;i<points.Length;i++)vh.AddTriangle(0,i+1,(i+1)%points.Length+1);
                return;
            }
            const int segments=8;
            Vertex(vh,Vector2.zero,source.center);
            float rx=localRadius>0?localRadius*source.width/rectTransform.rect.width:radius,ry=localRadius>0?localRadius*source.height/rectTransform.rect.height:radius;
            for(int corner=0;corner<4;corner++)for(int n=0;n<=segments;n++)
            {
                float a=(corner*90+n*90f/segments)*Mathf.Deg2Rad;
                var p=new Vector2((corner==0||corner==3?source.width-rx:rx)+Mathf.Cos(a)*rx,(corner<2?source.height-ry:ry)+Mathf.Sin(a)*ry);
                Vertex(vh,Local(p),source.position+p);
            }
            int count=4*(segments+1);for(int i=0;i<count;i++)vh.AddTriangle(0,i+1,(i+1)%count+1);
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        void Frame(VertexHelper vh)
        {
            // Reuses the exact narrow carved rim from B, including its rounded corners.
            const int steps=12;float sr=43,thickness=16;
            var dst=rectTransform.rect;float scale=dst.width/source.width,dr=sr*scale,dt=thickness*scale;
            for(int c=0;c<4;c++)for(int n=0;n<=steps;n++)
            {
                float a=(c*90+n*90f/steps)*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                var sc=new Vector2(c==0||c==3?source.width-sr:sr,c<2?source.height-sr:sr);
                var dc=new Vector2(c==0||c==3?dst.xMax-dr:dst.xMin+dr,c<2?dst.yMin+dr:dst.yMax-dr);
                Vertex(vh,dc+new Vector2(d.x,-d.y)*dr,source.position+sc+d*sr);
                Vertex(vh,dc+new Vector2(d.x,-d.y)*(dr-dt),source.position+sc+d*(sr-thickness));
            }
            int count=4*(steps+1);for(int i=0;i<count;i++){int a=i*2,b=((i+1)%count)*2;vh.AddTriangle(a,a+1,b);vh.AddTriangle(a+1,b+1,b);}
        }
        public static void Shell(Transform parent,string kind="play")
        {
            if(kind!="plain")
            {
                // The original screen skin supplies the wood at its original scale. Live tray,
                // header and basket layers cover its illustrative content at measured positions.
                Add(parent,"Wooden screen frame",kind=="map"?new Rect(76,115,450,812):new Rect(535,115,465,812),0,0,1040,kind=="map"?1876:1816,43);
                if(kind=="play")
                {
                    var wood=Add(parent,"Empty basket background",new Rect(110,817,58,70),0,-680,1000,440);wood.tiled=true;wood.localRadius=95;
                    Rim(parent);
                }
                return;
            }
            var plain=Add(parent,"Reference wood",new Rect(110,817,58,70),0,0,1000,1780);plain.tiled=true;plain.localRadius=95;
            Rim(parent);
        }
        public static void Rim(Transform parent){var rim=Add(parent,"Wooden screen frame",new Rect(535,115,465,812),0,0,1040,1816);rim.frame=true;}
        public static void Felt(Transform parent,bool framed)
        {
            var sprite=CosyArt.Get("CosyBoard");var uv=sprite.textureRect;
            var source=new Rect(uv.x+uv.width*.2f,sprite.texture.height-uv.y-uv.height*.8f,uv.width*.6f,uv.height*.6f);
            var felt=Add(parent,"Reference teal inset",source,0,0,1,1);felt.texture=sprite.texture;felt.localRadius=framed?95:65;
            Stretch(felt.rectTransform);
            if(framed){var rim=Add(parent,"Reference tray rim",new Rect(551,230,434,502),0,0,1,1);rim.frame=true;Stretch(rim.rectTransform);}
        }
        static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;}
    }
}
