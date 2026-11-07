from pathlib import Path
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text(encoding='utf-8-sig').replace('static Sprite round,pill,background;','static Sprite round,pill; static Texture2D background;')
a=s.index('        public static void RoomBackground(');b=s.index('        public static void Washer(',a)
s=s[:a]+'''        public static void RoomBackground(Transform parent,bool quiet=false)
        {
            if(!background)background=Resources.Load<Texture2D>("Art/FreshLaundryBackground");
            if(!background){Wood(parent);return;}
            if(quiet)Wood(parent);
            var r=Rect(parent,"Fresh Laundry room",0,0,1080,1920);
            var art=r.gameObject.AddComponent<RawImage>();art.texture=background;art.raycastTarget=false;
            art.color=new Color(1,1,1,quiet?.18f:1f);r.gameObject.AddComponent<FullBleed>();
        }
'''+s[b:]
p.write_text(s)
p=Path('Assets/Editor/ArtImporter.cs');s=p.read_text().replace('return 9','return 10').replace('i.textureType=TextureImporterType.Sprite;i.maxTextureSize=1024;i.alphaSource','i.textureType=TextureImporterType.Default;i.maxTextureSize=1024;i.alphaSource');p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text()
s=s.replace('if(name!="map")UI.Wood(screen,save.theme);','if(name!="map"&&name!="play")UI.RoomBackground(screen,true);')
s=s.replace('UI.Clear(screen);views.Clear();visuals.Clear();UI.Wood(screen,save.theme);','UI.Clear(screen);views.Clear();visuals.Clear();UI.RoomBackground(screen,true);')
# Early basket labels and later mat use a consistent cream inset.
s=s.replace('new Color(.85f,.77f,1f,.35f)','new Color(.87f,.95f,.91f,.55f)')
s=s.replace('basketFrame.color=puzzle.basket.Count>=puzzle.capacity-1?Color.Lerp(Color.white,UI.Pink,.15f+.15f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2)):Color.white;', 'basketFrame.color=puzzle.basket.Count>=puzzle.capacity-1?Color.Lerp(UI.Hex("B9D9CC"),UI.Pink,save.reduceMotion?.4f:.3f+.1f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2)):UI.Hex("B9D9CC");')
# Only active matched effects mutate, all mesh details remain static.
s=s.replace('r.localScale=Vector3.one*(1-u);yield return null;', 'r.localScale=Vector3.one*(1-u);r.localRotation=Quaternion.Euler(0,0,u*90);yield return null;')
s=s.replace('UI.Box(fxLayer,"Sparkle",pos.x,pos.y,10,16,color,false).rectTransform', 'UI.Rect(fxLayer,"Match sparkle",pos.x,pos.y,22,22)')
s=s.replace('float angle=i*Mathf.PI*2/count;StartCoroutine(Particle', 'UI.Glyph(r,"spark",0,0,22,color);float angle=i*Mathf.PI*2/count;StartCoroutine(Particle')
# Stage title and progress are separate to prevent overlap on later levels.
s=s.replace('BuildPlayHeader(c.level,c.pairs,740,100);','BuildPlayHeader(c.level,c.pairs,735,90);')
s=s.replace('UI.Box(screen,"Progress track",0,785,920,9,UI.Line);','UI.Box(screen,"Progress track",0,795,920,8,UI.Line);')
s=s.replace('UI.Box(screen,"Pair progress",-460,785,0,9,UI.Green);','UI.Box(screen,"Pair progress",-460,795,0,8,UI.Green);')
s=s.replace('920f*puzzle.matched/pairs,9','920f*puzzle.matched/pairs,8').replace('920f*puzzle.matched/c.pairs,9','920f*puzzle.matched/c.pairs,8')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/CandyGlyph.cs');s=p.read_text()
s=s.replace('case "album":Rect(-.42f,-.35f,.37f,.70f);Rect(.05f,-.35f,.37f,.70f);break;', '''case "album":
                    Line(new Vector2(0,-.36f),new Vector2(0,.28f),.07f);
                    Line(new Vector2(-.40f,-.27f),new Vector2(-.40f,.37f),.07f);
                    Line(new Vector2(.40f,-.27f),new Vector2(.40f,.37f),.07f);
                    Line(new Vector2(-.40f,.37f),new Vector2(0,.28f),.07f);
                    Line(new Vector2(.40f,.37f),new Vector2(0,.28f),.07f);
                    Line(new Vector2(-.40f,-.27f),new Vector2(0,-.36f),.07f);
                    Line(new Vector2(.40f,-.27f),new Vector2(0,-.36f),.07f);break;''')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/ButtonMotion.cs');s=p.read_text().replace('        Coroutine motion;','        Coroutine motion;\n        void OnDisable(){if(motion!=null)StopCoroutine(motion);motion=null;transform.localScale=Vector3.one;}');p.write_text(s)
