using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CandyGlyph:MaskableGraphic
    {
        public string kind;
        VertexHelper mesh;Vector2 offset;Color shade;
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();mesh=vh;offset=Vector2.zero;shade=color;Draw();}
        void Poly(params Vector2[] points)
        {
            int start=mesh.currentVertCount;var v=UIVertex.simpleVert;v.color=shade;
            foreach(var p in points){v.position=new Vector3((p.x+offset.x)*rectTransform.rect.width,(p.y+offset.y)*rectTransform.rect.height);mesh.AddVert(v);}
            for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);
        }
        void Rect(float x,float y,float w,float h){Poly(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));}
        void Circle(float x,float y,float r)
        {var pts=new Vector2[26];pts[0]=new Vector2(x,y);for(int i=0;i<25;i++){float a=i/24f*Mathf.PI*2;pts[i+1]=new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r);}Poly(pts);}
        void Line(Vector2 a,Vector2 b,float width)
        {var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Poly(a+n,b+n,b-n,a-n);Circle(a.x,a.y,width*.5f);Circle(b.x,b.y,width*.5f);}
        void Arc(float radius,float begin,float end,float width)
        {Vector2 prior=Vector2.zero;for(int i=0;i<=18;i++){float a=Mathf.Lerp(begin,end,i/18f)*Mathf.Deg2Rad;var p=new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius);if(i>0)Line(prior,p,width);prior=p;}}
        void Draw()
        {
            switch(kind)
            {
                case "socks":
                    for(int j=0;j<2;j++)
                    {float x=j==0?-.21f:.18f,y=j==0?-.08f:.06f;var points=new[]{new Vector2(x-.12f,y+.32f),new Vector2(x+.14f,y+.32f),new Vector2(x+.14f,y-.12f),new Vector2(x-.07f,y-.32f),new Vector2(x-.25f,y-.32f),new Vector2(x-.29f,y-.20f),new Vector2(x-.12f,y-.04f),new Vector2(x-.12f,y+.32f)};shade=UI.Cream;Poly(points);shade=color;for(int i=1;i<points.Length;i++)Line(points[i-1],points[i],.045f);Line(new Vector2(x-.12f,y+.20f),new Vector2(x+.14f,y+.20f),.035f);}break;
                case "spark":Poly(new Vector2(0,.45f),new Vector2(.13f,.13f),new Vector2(.45f,0),new Vector2(.13f,-.13f),new Vector2(0,-.45f),new Vector2(-.13f,-.13f),new Vector2(-.45f,0),new Vector2(-.13f,.13f));break;
                case "settings":Arc(.27f,0,360,.12f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.29f,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.43f,.13f);}break;
                case "album":
                    Line(new Vector2(-.43f,-.36f),new Vector2(-.43f,.36f),.08f);
                    Line(new Vector2(-.43f,.36f),new Vector2(.43f,.36f),.08f);
                    Line(new Vector2(.43f,.36f),new Vector2(.43f,-.36f),.08f);
                    Line(new Vector2(.43f,-.36f),new Vector2(-.43f,-.36f),.08f);
                    Circle(-.20f,.15f,.075f);
                    Poly(new Vector2(-.32f,-.24f),new Vector2(-.06f,.06f),new Vector2(.12f,-.10f),new Vector2(.24f,.01f),new Vector2(.33f,-.24f));break;
                case "shop":Rect(-.35f,-.35f,.7f,.5f);Line(new Vector2(-.22f,.1f),new Vector2(-.22f,.37f),.1f);Line(new Vector2(.22f,.1f),new Vector2(.22f,.37f),.1f);Line(new Vector2(-.22f,.37f),new Vector2(.22f,.37f),.1f);break;
                case "lock":Rect(-.31f,-.36f,.62f,.48f);Arc(.21f,0,180,.10f);Line(new Vector2(-.21f,0),new Vector2(-.21f,.16f),.10f);Line(new Vector2(.21f,0),new Vector2(.21f,.16f),.10f);break;
                case "halo":Arc(.44f,0,360,.035f);break;
                case "play":Poly(new Vector2(-.30f,-.42f),new Vector2(.42f,0),new Vector2(-.30f,.42f));break;
                case "back":Line(new Vector2(.18f,.32f),new Vector2(-.16f,0),.14f);Line(new Vector2(-.16f,0),new Vector2(.18f,-.32f),.14f);break;
                case "forward":Line(new Vector2(-.18f,.32f),new Vector2(.16f,0),.14f);Line(new Vector2(.16f,0),new Vector2(-.18f,-.32f),.14f);break;
                case "pause":Line(new Vector2(-.18f,-.32f),new Vector2(-.18f,.32f),.18f);Line(new Vector2(.18f,-.32f),new Vector2(.18f,.32f),.18f);break;
                case "close":Line(new Vector2(-.28f,-.28f),new Vector2(.28f,.28f),.16f);Line(new Vector2(-.28f,.28f),new Vector2(.28f,-.28f),.16f);break;
                case "plus":Line(new Vector2(-.28f,0),new Vector2(.28f,0),.15f);Line(new Vector2(0,-.28f),new Vector2(0,.28f),.15f);break;
                case "home":Poly(new Vector2(-.46f,.03f),new Vector2(0,.44f),new Vector2(.46f,.03f));Rect(-.31f,-.35f,.2f,.45f);Rect(.11f,-.35f,.2f,.45f);Rect(-.11f,-.06f,.22f,.17f);break;
                case "restart":Arc(.31f,-45,245,.13f);Poly(new Vector2(-.37f,-.22f),new Vector2(-.12f,-.43f),new Vector2(-.10f,-.12f));break;
                case "sound":Rect(-.43f,-.14f,.23f,.28f);Poly(new Vector2(-.24f,-.14f),new Vector2(.02f,-.36f),new Vector2(.02f,.36f),new Vector2(-.24f,.14f));Arc(.26f,-55,55,.07f);Arc(.43f,-55,55,.07f);break;
                case "music":Line(new Vector2(-.06f,-.28f),new Vector2(-.06f,.28f),.10f);Line(new Vector2(.30f,-.17f),new Vector2(.30f,.39f),.10f);Line(new Vector2(-.06f,.28f),new Vector2(.30f,.39f),.12f);Circle(-.18f,-.29f,.15f);Circle(.18f,-.18f,.15f);break;
                case "heart":
                    var pts=new Vector2[65];pts[0]=Vector2.zero;
                    for(int i=0;i<64;i++){float t=i/63f*Mathf.PI*2;pts[i+1]=new Vector2(16*Mathf.Pow(Mathf.Sin(t),3)/36,(13*Mathf.Cos(t)-5*Mathf.Cos(2*t)-2*Mathf.Cos(3*t)-Mathf.Cos(4*t))/36+.06f);}Poly(pts);break;
            }
        }
    }
}
