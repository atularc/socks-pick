from pathlib import Path
p=Path('Assets/Scripts/Runtime/ClayImage.cs');s=p.read_text().replace('roundness=.18f','roundness=.24f').replace('Mathf.Min(22,edge*.14f)','Mathf.Min(34,edge*.20f)').replace('ring==1?.48f:ring==2?.30f:ring==3?.10f','ring==1?.62f:ring==2?.38f:ring==3?.13f').replace('ring==0?.38f:ring==1?.32f:ring==2?.22f:ring==3?.08f','ring==0?.48f:ring==1?.42f:ring==2?.30f:ring==3?.10f');p.write_text(s)
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text().replace('static Texture2D background;','static Texture2D background; static Sprite basketArt;')
s=s.replace('round?.5f:.18f','round?.5f:.24f').replace('name+" shadow",x,y-9','name+" shadow",x,y-14').replace('new Color(.10f,.22f,.18f,.17f)','new Color(.10f,.22f,.18f,.20f)').replace('"Button depth",0,-6','"Button depth",0,-10').replace('"Button depth",0,-7','"Button depth",0,-12')
s=s.replace('public static void DisposeIcons(){}','public static void DisposeIcons(){if(basketArt)UnityEngine.Object.Destroy(basketArt);basketArt=null;}')
s=s.replace('r.yMin+r.height*.58f','r.yMin+r.height*.65f')
a=s.index('        public static Image Basket(')
s=s[:a]+'''        public static Image Basket(Transform parent,float x,float y,float w,float h)
        {
            if(!basketArt){var t=Resources.Load<Texture2D>("Art/FreshBasket");basketArt=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);}
            var image=Sprite(parent,"Lavender basket",basketArt,x,y,w*.88f,h*.8f);image.preserveAspect=false;return image;
        }
    }
}
'''
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text().replace('FitShopLayout();\n            lastSafe=', 'FitShopLayout();\n            FitSurfaceLayout();\n            lastSafe=')
s=s.replace('            var viewport=screen.Find("Scrollable level path")', '''            foreach(string name in new[]{"Map title card","Map title card shadow","Your laundry adventure"})
            {
                var item=screen.Find(name) as RectTransform;
                if(item)item.anchoredPosition=new Vector2(0,510+extra-(name.EndsWith("shadow")?14:0));
            }
            foreach(string name in new[]{"Map reading surface","Map reading surface shadow"})
            {
                var item=screen.Find(name) as RectTransform;
                if(item){item.sizeDelta=new Vector2(990,1130+extra*2);item.anchoredPosition=new Vector2(0,-110-(name.EndsWith("shadow")?14:0));}
            }
            var viewport=screen.Find("Scrollable level path")''')
pos=s.index('        void PinScreenEdges()')
s=s[:pos]+'''        void FitSurfaceLayout()
        {
            if(!screen)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=Mathf.Max(0,(area.height/Mathf.Max(.0001f,scale)-1920)*.5f);
            string surface=currentScreen=="play"?"Cream play surface":currentScreen=="album"?"Album surface":null;
            if(surface==null)return;
            float top=currentScreen=="play"?(puzzle.layout.config.basket==0?605:705):920;
            float bottom=-930;
            foreach(string name in new[]{surface,surface+" shadow"})
            {
                var item=screen.Find(name) as RectTransform;
                if(item){item.sizeDelta=new Vector2(1030,top-bottom+extra*2);item.anchoredPosition=new Vector2(0,(top+bottom)*.5f-(name.EndsWith("shadow")?14:0));}
            }
        }
'''+s[pos:]
a=s.index('            var mat=screen.Find("Cream play surface")');b=s.index('            if(early)',a)
s=s[:a]+'            FitSurfaceLayout();\n'+s[b:]
s=s.replace('if(!item.GetComponent<FullBleed>())SafeEdgeOffset.Pin(item,1);','''if(!item.GetComponent<FullBleed>()&&!item.name.StartsWith("Album surface"))SafeEdgeOffset.Pin(item,item.anchoredPosition.y< -600?-1:1);''')
s=s.replace('            pager.GoTo(albumPage,false);','            pager.GoTo(albumPage,false);FitSurfaceLayout();')
s=s.replace('UI.Pink,Home,52);\n            UI.Label(screen,"Sock album"','UI.Pink,Home,52);\n            UI.Label(screen,"Sock album"')
# Reset image tint each frame to white: the new basket already has authored mint shading.
s=s.replace('Color.Lerp(UI.Hex("B9D9CC"),UI.Pink,save.reduceMotion?.4f:.3f+.1f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2)):UI.Hex("B9D9CC")','Color.Lerp(Color.white,UI.Pink,save.reduceMotion?.25f:.15f+.1f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2)):Color.white')
p.write_text(s)
p=Path('Assets/Editor/ArtImporter.cs');s=p.read_text().replace('return 11','return 12').replace('if(assetPath.EndsWith("ClayButton.png"))','if(assetPath.EndsWith("FreshBasket.png"))');p.write_text(s)
p=Path('Assets/Editor/BuildTools.cs');s=p.read_text().replace('ClayButton','FreshBasket');p.write_text(s)
p=Path('Assets/Scripts/Runtime/ButtonMotion.cs');s=p.read_text().replace('Animate(.95f,.08f)','Animate(.93f,.08f)').replace('*.025f','*.045f');p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text().replace('                var title=GameObject.Find("Title ribbon")','                var mat=GameObject.Find("Cream play surface").GetComponent<RectTransform>();\n                var title=GameObject.Find("Title ribbon")')
s=s.replace('if(!bounds.rect.Contains(local))throw new Exception("Sock artwork clipped on level "+level);','if(!bounds.rect.Contains(local))throw new Exception("Sock artwork clipped on level "+level);\n                        if(!mat.rect.Contains(mat.InverseTransformPoint(corner)))throw new Exception("Sock extends outside its cream surface on level "+level);')
p.write_text(s)
