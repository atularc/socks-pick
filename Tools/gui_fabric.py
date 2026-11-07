from pathlib import Path
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text().replace('static Sprite round,pill;', 'static Sprite round,pill,clay;')
s=s.replace('sprite.texture.name=="CandySocks"','sprite.texture.name=="FabricSocks"')
s=s.replace('Box(r,"Button depth",0,-7,w,h,Color.Lerp(color,Ink,.30f));var face=Box(r,"Face",0,0,w,h,color);face.raycastTarget=true;', '''var face=Box(r,"Face",0,0,w,h,color);face.raycastTarget=true;
            face.sprite=Clay();face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=1.2f;''')
s=s.replace('public static void DisposeIcons(){}', 'public static void DisposeIcons(){if(clay)UnityEngine.Object.Destroy(clay);clay=null;}')
s=s.replace('        public static void Stars(', '''        public static Sprite Clay()
        {
            if(clay)return clay;
            var t=Resources.Load<Texture2D>("Art/ClayButton");
            var r=new Rect(t.width*.02f,t.height*.045f,t.width*.96f,t.height*.90f);
            clay=UnityEngine.Sprite.Create(t,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(r.width*.13f,r.height*.38f,r.width*.13f,r.height*.38f));
            return clay;
        }
        public static void Stars(''')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text().replace('Art/CandySocks','Art/FabricSocks').replace('ballArt[i]=Resources.Load<Sprite>("Art/ball_"+i.ToString("00"));','ballArt[i]=sockArt[i]; // Reuse the fabric sheet for folded-pair effects; no extra atlas.')
s=s.replace('UI.Clear(screen);views.Clear();visuals.Clear();UI.RoomBackground(screen,true);','''UI.Clear(screen);views.Clear();visuals.Clear();UI.RoomBackground(screen);
            UI.Panel(screen,"Cream play surface",0,0,1030,1840,UI.Cream);''')
# Leave full-colour edges visible while ensuring late-level readability.
s=s.replace('Color.white,false);face.raycastTarget=true;', 'Color.white,false);face.raycastTarget=true;')
p.write_text(s)
p=Path('Assets/Editor/ArtImporter.cs');s=p.read_text().replace('return 10','return 11').replace('CandySocks','FabricSocks').replace('i.maxTextureSize=2048','i.maxTextureSize=1024')
needle='            if(assetPath.EndsWith("FreshLaundryBackground.png"))'
s=s.replace(needle,'''            if(assetPath.EndsWith("ClayButton.png"))
            {
                i.textureType=TextureImporterType.Default;i.maxTextureSize=512;i.npotScale=TextureImporterNPOTScale.ToNearest;
                i.textureCompression=TextureImporterCompression.CompressedHQ;
                var targets=new[]{"Standalone","WebGL","Android","iPhone"};
                var formats=new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_6x6};
                for(int p=0;p<targets.Length;p++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=targets[p],overridden=true,maxTextureSize=512,format=formats[p],compressionQuality=75});
                i.filterMode=FilterMode.Bilinear;return;
            }
'''+needle);p.write_text(s)
p=Path('Assets/Editor/BuildTools.cs');s=p.read_text().replace('CandySocks','FabricSocks')
s=s.replace('if(file.Contains("FreshLaundryBackground"))','if(file.Contains("FreshLaundryBackground")||file.Contains("ClayButton"))')
s=s.replace('if(!file.Contains("FreshLaundryBackground")', 'if(!file.Contains("ClayButton")&&!file.Contains("FreshLaundryBackground")')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text().replace('texture.name=="CandySocks"','texture.name=="FabricSocks"');p.write_text(s)
